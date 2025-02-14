using UnityEngine;

public class DebugTransformCube : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 90f; // Degrees per second
    [SerializeField] private float changeDirectionInterval = 2f; // Time between direction changes
    [SerializeField] private float smoothness = 2f; // Higher value = smoother transitions

    private Vector3 targetRotation;
    private Vector3 currentRotationVelocity;
    private float nextDirectionChange;

    private void Start()
    {
        SetNewRandomRotation();
    }

    private void Update()
    {
        transform.localScale = Input.GetMouseButton(0) ? Vector3.one * 1.8f : Vector3.one * 2f;

        // Check if it's time to change direction
        if (Time.time >= nextDirectionChange)
        {
            SetNewRandomRotation();
        }

        // Smoothly rotate towards target rotation
        transform.eulerAngles = Vector3.SmoothDamp(
            transform.eulerAngles,
            targetRotation,
            ref currentRotationVelocity,
            smoothness,
            rotationSpeed
        );
    }

    private void SetNewRandomRotation()
    {
        // Set random rotation angles
        targetRotation = new Vector3(
            Random.Range(0f, 360f),
            Random.Range(0f, 360f),
            Random.Range(0f, 360f)
        );

        // Schedule next direction change
        nextDirectionChange = Time.time + changeDirectionInterval;
    }
}