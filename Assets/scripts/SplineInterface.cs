using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

public class SplineInterface : MonoBehaviour
{
    [SerializeField] public SplineContainer splineContainer;
    [SerializeField] map map;
    Spline currentSpline;
    [SerializeField] private float knotGroundOffset = 1f;
    [SerializeField] int mannequinsPerSpline = 3;
    [SerializeField] private GameObject mannequinPrefab;
    private BezierKnot latestKnot;
    private int latestSplineID;
    bool actuallyPlacedPath = false;


    private bool firstPlace = false;
    public void OnPathPlaceMode(bool val)
    {
        firstPlace = true;

        if (!val && actuallyPlacedPath)
        {
            SpawnMannequins(GetSplineIndex(currentSpline), 3);
            actuallyPlacedPath = false;
        }
    }

    public int AddSplineOrKnot(Vector3 position, Vector3 normal, bool newSpline = false, bool offset = false)
    {
        if (newSpline || splineContainer.Splines.Count <= 0 || firstPlace)
        {
            currentSpline = AddSpline();
        }
        
        int splineIndex = GetSplineIndex(currentSpline);
        
        latestKnot = AddKnot(splineIndex, splineContainer.Splines[splineIndex].Count, position, normal, offset);
        firstPlace = false;
        
        return splineIndex;
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

    public void RemoveLatestKnot()
    {
        splineContainer.Splines[latestSplineID].Remove(latestKnot);
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

        var position1 = position;
        if(offset)position1 += normal * knotGroundOffset;
        splineOrientation.transform.position = position1;

        splineOrientation.transform.rotation = knot.Rotation;
        splineOrientation.transform.up = normal;
        
        knot.Rotation = splineOrientation.transform.rotation;
        knot.Position = position1;
        
        
        TangentMode mode = TangentMode.AutoSmooth;

        splineContainer.Splines[splineID].Insert(knotID, knot);
        splineContainer.Splines[splineID].SetTangentMode(knotID, mode);
        
        actuallyPlacedPath = true;
        
        return knot;
    }

    void SpawnMannequins(int splineID, int amount)
    {
        if(splineID == -1) print("NIGGERS");
        
        for (int i = 0; i < amount; i++)
        {
            GameObject mannequinObj = Instantiate(mannequinPrefab);
            Mannequin mannequin = mannequinObj.GetComponent<Mannequin>();
            
            mannequin.spline = splineContainer.Splines[splineID];
            mannequin.startProgress = Random.Range(0f, 1f);
            
            mannequin.OnStart();
        }
    }
}
