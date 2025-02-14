using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class civilTent : Building
{
    [SerializeField] int mannequins = 1;
    SplineInterface splineInterface;
    private bool spawnedManneqins = false;

    public int GetAssociatedMannequins()
    {
        return mannequins;
    }

    public void SetAssociatedMannequins(int amount)
    {
        mannequins = amount;
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
        if (spawnedManneqins) return;
        splineInterface = FindFirstObjectByType<SplineInterface>().GetComponent<SplineInterface>();

        if (splineInterface == null) Debug.LogError("SplineInterface not found");

        StartCoroutine(SpawnMannequinsDelayed(spline, isStart, .2f));
    }

    IEnumerator SpawnMannequinsDelayed(Spline spline, bool isStart, float delaySeconds)
    {
        spawnedManneqins = true;
        yield return new WaitForSeconds(delaySeconds);
        splineInterface.SpawnMannequinsSafelyOnCurrentSpline(ref spline, isStart ? 0f : 1f, mannequins, isStart);
    }
}