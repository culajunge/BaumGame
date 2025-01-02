using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Splines.Examples;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

public class SplineInterface : MonoBehaviour
{
    [SerializeField] public SplineContainer splineContainer;
    [SerializeField] private GameObject firstPathMarkerPrefab;
    [SerializeField] private GameObject intersectionColliderPrefab;
    [SerializeField] map map;
    Spline currentSpline;
    [SerializeField] private float knotGroundOffset = 1f;
    [SerializeField] private int splineConnectionResolution = 10;
    [SerializeField] private int splineConnectionIterations = 2;
    [SerializeField] int mannequinsPerSpline = 3;
    [SerializeField] private GameObject mannequinPrefab;
    [SerializeField] public CustomSplineMesh splineMesh;
    private BezierKnot latestKnot;
    private int latestSplineID;
    bool actuallyPlacedPath = false;
    int knotsPlacedPerPathPlaceMode = 0;
    private bool addColliders = true;


    private bool firstPlace = false;

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

    public void OnPathPlaceMode(bool val)
    {
        firstPlace = true;

        if (!val)
        {
            knotsPlacedPerPathPlaceMode = 0;
            if (actuallyPlacedPath)
            {
                SpawnMannequins(GetSplineIndex(currentSpline), 1);
                actuallyPlacedPath = false;
            }
        }
    }

    public int AddSplineOrKnot(Vector3 position, Vector3 normal, bool newSpline = false, bool offset = false,
        int connectToSplineIndex = -1)
    {
        if (newSpline || splineContainer.Splines.Count <= 0 || firstPlace)
        {
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

                // Calculate the index where to insert the new knot based on the t value
                int insertIndex = Mathf.FloorToInt(t * splineToConnect.Count);

                // Create knot at intersection point on target spline
                BezierKnot connectionKnot = new BezierKnot(nearest);
                splineToConnect.Insert(insertIndex, connectionKnot);

                // Add knot to current spline
                latestKnot = AddKnot(splineIndex, splineContainer.Splines[splineIndex].Count, nearest, normal, offset);

                // Create intersection by linking the knots
                SplineKnotIndex currentKnotIndex = new SplineKnotIndex(splineIndex, currentSpline.Count - 1);
                SplineKnotIndex targetKnotIndex = new SplineKnotIndex(connectToSplineIndex, insertIndex);

                splineContainer.LinkKnots(currentKnotIndex, targetKnotIndex);

                //Add intersection collider
                /*
                intersectionCollider intersectionCol =
                    Instantiate(intersectionColliderPrefab, nearest, Quaternion.identity, transform)
                        .GetComponent<intersectionCollider>();
                
                intersectionCol.SetSplines(currentSpline, splineToConnect, t);*/
            }
        }
        else
        {
            // Normal knot addition without connection
            latestKnot = AddKnot(splineIndex, splineContainer.Splines[splineIndex].Count, position, normal, offset);
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

    Spline GetSplineFromIndex(int splineIndex)
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
        Debug.DrawRay(position, normal.normalized * 10, Color.red, 10000);

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

    void SpawnMannequins(int splineID, int amount)
    {
        if (splineID == -1) print("NIGGERS");

        for (int i = 0; i < amount; i++)
        {
            GameObject mannequinObj = Instantiate(mannequinPrefab);
            Mannequin mannequin = mannequinObj.GetComponent<Mannequin>();

            mannequin.spline = splineContainer.Splines[splineID];
            mannequin.startProgress = Random.Range(0f, 1f);
            mannequin.splineContainer = splineContainer;

            mannequin.OnStart();
        }
    }
}