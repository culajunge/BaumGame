using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class civilTent : Building
{
    [SerializeField] int mannequins = 1;
    SplineInterface splineInterface;

    public int GetAssociatedMannequins()
    {
        return mannequins;
    }

    public override void OnPlaceDown()
    {
        base.OnPlaceDown();
    }

    public override void OnMannequinCollision(Mannequin mannequin)
    {
        //nothing
    }

    public override void OnPathConnect(Spline spline = null, bool isStart = false)
    {
        splineInterface = FindFirstObjectByType<SplineInterface>().GetComponent<SplineInterface>();

        if (splineInterface == null) Debug.LogError("SplineInterface not found");

        splineInterface.SpawnMannequinsSafelyOnCurrentSpline(spline, isStart ? 0f : 1f, mannequins, isStart);
    }
}