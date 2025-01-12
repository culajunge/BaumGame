using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

public class Mannequin : MonoBehaviour
{
    [Header("Spline Settings")] [HideInInspector]
    public Spline spline; // Reference to the Spline

    public float speed = 5f; // Speed of movement along the spline
    public float speedVariation = 2f;
    [Range(0f, 1f)] public float startProgress = 0f; // Starting progress on the spline

    private float progress; // Current progress along the spline
    private bool movingForward = true; // Direction of movement (true = forward, false = backward)
    [HideInInspector] public SplineContainer splineContainer;
    private int currentSplineIndex;

    private string mannequinResourceType;
    private int mannequinResourceAmount;
    public int maxResourceCapacity = 2;

    [HideInInspector] public Manager manager;
    [SerializeField] MeshRenderer beanMeshRenderer;

    bool isActive = true;

    #region Resources

    public bool isResourceEmtpy()
    {
        return String.IsNullOrEmpty(mannequinResourceType) && mannequinResourceAmount == 0;
    }

    public string GetResourceType()
    {
        return mannequinResourceType;
    }

    public int GetResourceAmount()
    {
        return mannequinResourceAmount;
    }

    public void SetResource(string resourceType, int resourceAmount)
    {
        mannequinResourceAmount = resourceAmount;
        mannequinResourceType = resourceType;

        print($"Mannequin {gameObject.name} has {mannequinResourceAmount} of {mannequinResourceType}");
    }

    public void ChangeColor(Color color)
    {
        beanMeshRenderer.material.color = color;
    }

    public void AddResource(int amount)
    {
        mannequinResourceAmount += amount;
    }

    #endregion

    public void OnStart()
    {
        if (spline == null)
        {
            Debug.LogError("Spline is not assigned!");
            enabled = false;
            return;
        }

        currentSplineIndex = GetCurrentSplineIndex();
        progress = startProgress;
        speed += Random.Range(-speedVariation, speedVariation);
        GetComponentInChildren<Animator>().speed = Random.Range(0.9f, 1.1f);
    }

    public void CommitSuicide()
    {
        isActive = false;
        Destroy(gameObject);
    }

    public void ChangeSpline(Spline newSpline, float newProgress, bool movingForward)
    {
        spline = newSpline;
        progress = newProgress;
        this.movingForward = movingForward;
    }

    public Spline GetCurrentSpline()
    {
        return spline;
    }

    public bool IsMovingForward()
    {
        return movingForward;
    }

    private void Update()
    {
        if (!isActive || spline == null) return;

        float step = (speed * Time.deltaTime) / spline.GetLength();
        progress += movingForward ? step : -step;

        if (progress >= 1f)
        {
            progress = 1f;
            movingForward = false;
            OnEndReached();
        }
        else if (progress <= 0f)
        {
            progress = 0f;
            movingForward = true;
            OnEndReached();
        }

        Vector3 splinePosition = spline.EvaluatePosition(progress);
        Vector3 splineTangent = spline.EvaluateTangent(progress);

        // Validate position and tangent before applying
        if (!IsValidVector3(splinePosition))
        {
            print("NON VALID POSITION!");
            OnSplineDismantled();
            return;
        }

        if (!IsValidVector3(splineTangent))
        {
            print("NON VALID TANGENT!");
            return;
        }

        transform.position = splinePosition;
        transform.forward = movingForward ? splineTangent : -splineTangent;
    }

    private bool IsValidVector3(Vector3 vector)
    {
        return !float.IsInfinity(vector.x) &&
               !float.IsInfinity(vector.y) &&
               !float.IsInfinity(vector.z) &&
               !float.IsNaN(vector.x) &&
               !float.IsNaN(vector.y) &&
               !float.IsNaN(vector.z) &&
               vector.magnitude > float.Epsilon;
    }

    public void OnSplineDismantled()
    {
        print("MANNEQUIN PERMANENTLY DEACTIVATED");
        isActive = false;
    }

    private void CheckForIntersection()
    {
        int currentKnotIndex = Mathf.FloorToInt(progress * (spline.Count - 1));
        var knotIndex = new SplineKnotIndex(currentSplineIndex, currentKnotIndex);

        var linkedKnots = splineContainer.KnotLinkCollection.GetKnotLinks(knotIndex);

        if (linkedKnots.Count > 0)
        {
            int randomIndex = Random.Range(0, linkedKnots.Count);
            var newKnot = linkedKnots[randomIndex];

            currentSplineIndex = newKnot.Spline;
            spline = splineContainer.Splines[currentSplineIndex];
            progress = (float)newKnot.Knot / (spline.Count - 1);
        }
    }

    private int GetCurrentSplineIndex()
    {
        for (int i = 0; i < splineContainer.Splines.Count; i++)
        {
            if (splineContainer.Splines[i] == spline)
                return i;
        }

        return 0;
    }

    /// <summary>
    /// Rotates the object to face the current tangent direction at the endpoint.
    /// </summary>
    private void RotateTowardsTangent()
    {
        Vector3 tangent = movingForward ? spline.EvaluateTangent(progress) : -spline.EvaluateTangent(progress);
        if (tangent != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(tangent);
        }
    }

    public virtual void OnEndReached()
    {
        //print("End reached");
    }
}