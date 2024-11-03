using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SplineInterface : Building
{
    [SerializeField] SplineContainer splineContainer;

    public override void OnPlaceDown()
    {
        //TODO do smth here ig
    }

    public void AddKnot(int id, Vector3 position)
    {
        BezierKnot knot = new BezierKnot(position);
        TangentMode mode = TangentMode.AutoSmooth;

        splineContainer.Spline.Insert(id, knot);
        splineContainer.Spline.SetTangentMode(id, mode);
    }
}
