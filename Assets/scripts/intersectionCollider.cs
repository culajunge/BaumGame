using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

public class intersectionCollider : MonoBehaviour
{
    [SerializeField] string mannequinTag = "Mannequin";
    [HideInInspector] public map map;

    private bool activeIntersection = false;

    private Spline endingSpline;
    private Spline intersectingSpline;

    private float endingPointProgress;
    private float intersectingPointProgress;

    int passedMannequins = 0;
    int changingMannequins = 0;

    public void SetSplines(Spline endingSpline, Spline intersectingSpline, float intersectingPointProgress,
        float endingPointProgress)
    {
        this.endingSpline = endingSpline;
        this.intersectingSpline = intersectingSpline;
        this.endingPointProgress = endingPointProgress;
        this.intersectingPointProgress = intersectingPointProgress;
    }

    public void SetMap(map map)
    {
        this.map = map;
    }

    public void FinishSetup()
    {
        activeIntersection = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!activeIntersection || !other.CompareTag(mannequinTag)) return;

        Spline newSpline = passedMannequins % 2 == 0 ? endingSpline : intersectingSpline;
        bool movingForward = Random.Range(0, 2) == 0;

        int mannequinIndex = int.Parse(other.gameObject.name);
        Mannequin mannequin = map.GetMannequin(mannequinIndex);

        Spline mannequinsOlSpline = mannequin.GetCurrentSpline();
        bool mannequinMovingForward = mannequin.IsMovingForward();

        if (mannequinsOlSpline == endingSpline)
        {
            newSpline = intersectingSpline;
            movingForward = changingMannequins % 2 == 0;
            changingMannequins++;
        }
        else if (mannequinsOlSpline != newSpline)
        {
            //changingMannequins++;
        }
        else
        {
            movingForward = mannequinMovingForward;
        }

        if (mannequinsOlSpline == newSpline && mannequinMovingForward == movingForward)
        {
            passedMannequins++;
            return;
        }

        mannequin.ChangeSpline(newSpline, newSpline == endingSpline ? endingPointProgress : intersectingPointProgress,
            movingForward);

        passedMannequins++;
    }
}