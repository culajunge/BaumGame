using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Splines.Examples;
using Unity.VisualScripting;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

public class SplineInterface : MonoBehaviour
{
    [SerializeField] public SplineContainer splineContainer;
    [SerializeField] private GameObject firstPathMarkerPrefab;
    [SerializeField] private GameObject intersectionColliderPrefab;
    [SerializeField] map map;
    [HideInInspector] public Spline currentSpline;
    [SerializeField] private float knotGroundOffset = 1f;
    [SerializeField] private int splineConnectionResolution = 10;
    [SerializeField] private int splineConnectionIterations = 2;
    [SerializeField] int mannequinsPerSpline = 3;
    [SerializeField] private GameObject mannequinPrefab;
    [SerializeField] public CustomSplineMesh splineMesh;
    [SerializeField] float snapThreshold = 0.1f;
    private BezierKnot latestKnot;
    private int latestSplineID;
    bool actuallyPlacedPath = false;
    int knotsPlacedPerPathPlaceMode = 0;
    private bool addColliders = true;


    private bool firstPlace = false;
    [HideInInspector] public bool publicFirstPlace = false;

    void Start()
    {
        UpdateCollision();
    }

    void UpdateCollision()
    {
        MeshCollider meshCollider = GetComponent<MeshCollider>();

        // Assume LoftRoad is a component that provides the mesh
        var csplinemesh = GetComponent<CustomSplineMesh>();
        if (csplinemesh != null)
        {
            meshCollider.sharedMesh = csplinemesh.LoftMesh;
        }
        else
        {
            Debug.LogWarning("LoftRoad component not found.");
        }
    }

    public bool isFirstPlace()
    {
        return firstPlace;
    }

    public void OnPathPlaceMode(bool val)
    {
        firstPlace = true;

        if (!val)
        {
            knotsPlacedPerPathPlaceMode = 0;
            if (actuallyPlacedPath)
            {
                actuallyPlacedPath = false;
            }
        }
    }

    public int AddSplineOrKnot(Vector3 position, Vector3 normal, bool newSpline = false, bool offset = false,
        int connectToSplineIndex = -1)
    {
        publicFirstPlace = false;
        if (newSpline || splineContainer.Splines.Count <= 0 || firstPlace)
        {
            publicFirstPlace = true;
            currentSpline = AddSpline();
        }

        int splineIndex = GetSplineIndex(currentSpline);

        // Handle connection to existing spline
        if (connectToSplineIndex >= 0)
        {
            Spline splineToConnect = GetSplineFromIndex(connectToSplineIndex);
            if (splineToConnect != null)
            {
                // Find closest point on target spline
                float t;
                float3 nearest;
                SplineUtility.GetNearestPoint(
                    splineToConnect,
                    position,
                    out nearest,
                    out t,
                    resolution: splineConnectionResolution,
                    iterations: splineConnectionIterations
                );
                Debug.DrawRay(position, Vector3.up * 5f, Color.cyan, 1000f);
                Debug.DrawRay((Vector3)nearest, Vector3.up * 10f, Color.blue, 1000f);

                #region find best index

                // AREA: Find the correct segment index for insertion

                int segmentCount = splineToConnect.Count - 1;

                // Check distances to existing knots first
                float minKnotDistance = float.MaxValue;
                int closestKnotIndex = -1;
                for (int i = 0; i < splineToConnect.Count; i++)
                {
                    float distance = Vector3.Distance(splineToConnect[i].Position, nearest);
                    if (distance < minKnotDistance)
                    {
                        minKnotDistance = distance;
                        closestKnotIndex = i;
                    }
                }

                // If we're very close to an existing knot, use that index
                int insertIndex;
                if (minKnotDistance < snapThreshold)
                {
                    insertIndex = closestKnotIndex;
                }
                else
                {
                    insertIndex = Mathf.Clamp(Mathf.CeilToInt(t * segmentCount), 0, segmentCount);
                }

                print(
                    $"Insert index: {insertIndex.ToString()}/{(segmentCount + 2).ToString()}, Distance to nearest knot: {minKnotDistance}");

                #endregion

                // Create knot at intersection point on target spline
                BezierKnot connectionKnot = new BezierKnot(nearest);
                splineToConnect.Insert(insertIndex, connectionKnot);


                // Add knot to current spline
                latestKnot = AddKnot(splineIndex, splineContainer.Splines[splineIndex].Count, nearest, normal, offset);

                // Create intersection by linking the knots
                SplineKnotIndex currentKnotIndex = new SplineKnotIndex(splineIndex, currentSpline.Count - 1);
                SplineKnotIndex targetKnotIndex = new SplineKnotIndex(connectToSplineIndex, insertIndex);

                splineContainer.LinkKnots(currentKnotIndex, targetKnotIndex);

                intersectionCollider intersectionCol =
                    Instantiate(intersectionColliderPrefab, nearest, Quaternion.identity, transform)
                        .GetComponent<intersectionCollider>();

                float endingPointProgress = firstPlace ? 0f : 1f;

                intersectionCol.SetMap(map);
                intersectionCol.SetSplines(currentSpline, splineToConnect, t, endingPointProgress);
                intersectionCol.FinishSetup();
            }
        }
        else
        {
            // Normal knot addition without connection
            latestKnot = AddKnot(splineIndex, splineContainer.Splines[splineIndex].Count, position, normal, offset);
        }

        if (SpawnMannequinsDelayed && knotsPlacedPerPathPlaceMode == 1)
        {
            SpawnMannequinsDelayed = false;
            SpawnMannequins(currentSpline, delayedMannequinProgress, delayedMannequinAmount);
            delayedMannequinSpline = null;
            delayedMannequinProgress = 0f;
            delayedMannequinAmount = 0;
        }

        knotsPlacedPerPathPlaceMode++;
        firstPlace = false;
        UpdateCollision();

        HandleFirstPlaceMarker(position, normal, offset);

        return splineIndex;
    }

    GameObject FirstPlaceMarker;

    void HandleFirstPlaceMarker(Vector3 position, Vector3 normal, bool offset = false)
    {
        if (FirstPlaceMarker == null)
        {
            FirstPlaceMarker = Instantiate(firstPathMarkerPrefab, position, Quaternion.Euler(90, 0, 0));
            FirstPlaceMarker.SetActive(false);
            FirstPlaceMarker.transform.localScale = Vector3.one * (2 * splineMesh.roadWidth);
        }

        if (knotsPlacedPerPathPlaceMode <= 1)
        {
            FirstPlaceMarker.transform.position = GetOffsetPosition(position, normal, offset);
            FirstPlaceMarker.transform.forward = normal;
            FirstPlaceMarker.SetActive(true);
        }
        else
        {
            FirstPlaceMarker.SetActive(false);
        }
    }

    public Spline GetSplineFromIndex(int splineIndex)
    {
        return splineContainer.Splines[splineIndex];
    }

    int GetSplineIndex(Spline spline)
    {
        for (int i = 0; i < splineContainer.Splines.Count; i++)
        {
            if (splineContainer.Splines[i] == spline)
            {
                return i;
            }
        }

        return -1;
    }

    public Spline AddSpline()
    {
        Spline spline = splineContainer.AddSpline();

        //SpawnMannequins(GetSplineIndex(spline), mannequinsPerSpline);

        return spline;
    }

    public void RemoveSpline(Spline spline)
    {
        splineContainer.RemoveSpline(spline);
    }

    public void RemoveSplineSilent(int id)
    {
        splineContainer.Splines[id].Clear();
    }

    public void RemoveLatestKnot()
    {
        splineContainer.Splines[latestSplineID].Remove(latestKnot);
    }

    Vector3 GetOffsetPosition(Vector3 position, Vector3 normal, bool offset)
    {
        if (!offset) return position;

        var position1 = position;
        if (offset) position1 += normal * knotGroundOffset;
        return position1;
    }

    public BezierKnot AddKnot(int splineID, int knotID, Vector3 position, Vector3 normal, bool offset)
    {
        latestSplineID = splineID;
        Debug.DrawRay(position, normal.normalized * 2, Color.red, 10000);

        /*
        
        Vector3 tangentIn = position - normal * 0.5f;
        Vector3 tangentOut = position + normal * 0.5f;
        
        quaternion normalRotation = quaternion.LookRotationSafe(normal, Vector3.up);*/


        BezierKnot knot = new BezierKnot(position: position);

        GameObject splineOrientation = new GameObject();

        Vector3 position1 = GetOffsetPosition(position, normal, offset);
        splineOrientation.transform.position = position1;

        splineOrientation.transform.rotation = knot.Rotation;
        splineOrientation.transform.up = normal;

        knot.Rotation = splineOrientation.transform.rotation;
        knot.Position = position1;


        TangentMode mode = TangentMode.AutoSmooth;

        splineContainer.Splines[splineID].Insert(knotID, knot);
        splineContainer.Splines[splineID].SetTangentMode(knotID, mode);

        actuallyPlacedPath = true;
        Destroy(splineOrientation);
        return knot;
    }

    private Spline delayedMannequinSpline;
    float delayedMannequinProgress;
    int delayedMannequinAmount;
    private bool SpawnMannequinsDelayed = false;

    public int? SpawnMannequinsSafelyOnCurrentSpline(Spline spline, float progress, int amount, bool isStart)
    {
        if (isStart)
        {
            delayedMannequinSpline = spline;
            delayedMannequinProgress = progress;
            delayedMannequinAmount = amount;
            SpawnMannequinsDelayed = true;
            return null;
        }

        return SpawnMannequins(spline, progress, amount);
    }

    public int SpawnMannequins(Spline spline, float progress, int amount)
    {
        int firstMannequinIndex = -1;
        for (int i = 0; i < amount; i++)
        {
            GameObject mannequinObj = Instantiate(mannequinPrefab);

            int index = map.GetMannequinCount();
            mannequinObj.name = index.ToString();
            if (firstMannequinIndex == -1) firstMannequinIndex = index;

            Mannequin mannequin = mannequinObj.GetComponent<Mannequin>();

            mannequin.spline = spline;
            mannequin.startProgress = progress;
            mannequin.splineContainer = splineContainer;

            map.AddMannequin(mannequin, index);

            mannequin.OnStart();
        }

        return firstMannequinIndex;
    }
}