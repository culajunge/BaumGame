using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Gitversion
public class cameraMovement : MonoBehaviour
{
    public float baseDragSpeed = 1.5f; // Speed of the camera movement
    public float maxSpeedMultiplier = 2.5f; // Maximum speed multiplier
    public float smoothSpeed = 0.15f; // Smoothing speed for interpolation
    public float zoomSpeed = 3.0f; // Speed of zooming
    public float minZoom = 100.0f; // Minimum field of view (zoom in limit)
    public float maxZoom = 600.0f; // Maximum field of view (zoom out limit)

    public float dragSpeed = 0.5f;

    private Vector3 dragOrigin; // Where the dragging started
    private bool isDragging = false;
    private bool isZooming = false;
    private Vector3 targetPosition; // The position to smoothly move towards
    private float currentDragSpeed;

    [SerializeField] private Camera cam;
    [SerializeField] private Transform camDir; // Reference to the camera's direction object
    private Vector3 zoom; // Zoom reference variable
    bool lockedMovement = false;
    private bool initialTouchRegistered = false;
    private float lastTouchDeltaMag;
    private Vector2 lastTouchZeroPos;
    private Vector2 lastTouchOnePos;
    private bool skipFirstTwoFingerFrame = true;
    private Coroutine lockMovementCoroutine;

    public void SetMovementLock(bool val)
    {
        lockedMovement = val;
    }

    public bool GetMovementLock()
    {
        return lockedMovement;
    }

    void Start()
    {
        targetPosition = transform.position; // Initialize target position
        zoom = camDir.localPosition; // Initialize zoom with the local position of the camera direction
        UpdateDragSpeed();
    }

    void Update()
    {
        if (!lockedMovement)
        {
            HandleInput();
            SmoothMoveCamera();
            HandleZoom();
        }
    }

    void UpdateDragSpeed()
    {
        float zoomFactor =
            Mathf.InverseLerp(minZoom, maxZoom, Vector3.Distance(camDir.localPosition, transform.position));
        currentDragSpeed = Mathf.Lerp(baseDragSpeed, baseDragSpeed * maxSpeedMultiplier, zoomFactor);
    }

    // Handles the input for mouse or touch
    void HandleInput()
    {
        HandleUniversalInput();
    }

    void HandleUniversalInput()
    {
        if (Input.touchCount == 1 && !isZooming) // Touch input and not zooming
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                dragOrigin = touch.position;
                isDragging = true;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                Vector3 dragDelta = touch.position - (Vector2)dragOrigin;

                Vector3 move = new Vector3(-dragDelta.x, 0, -dragDelta.y) * currentDragSpeed * Time.deltaTime *
                               dragSpeed;
                targetPosition += move; // Add the movement to the target position

                dragOrigin = touch.position;
            }

            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
        else if (Input.GetMouseButtonDown(0)) // Mouse input (left-click)
        {
            dragOrigin = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 dragDelta = Input.mousePosition - dragOrigin;

            Vector3 move = new Vector3(-dragDelta.x, 0, -dragDelta.y) * currentDragSpeed * Time.deltaTime;
            targetPosition += move; // Add the movement to the target position

            dragOrigin = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    // Smoothly moves the camera towards the target position
    void SmoothMoveCamera()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
    }

    // Handles zooming with the scroll wheel or pinch gesture
    void HandleZoom()
    {
        // Mouse Scroll Wheel Zoom
        float scrollData = Input.GetAxis("Mouse ScrollWheel");
        if (scrollData != 0)
        {
            Zoom(scrollData);
        }

        // Pinch Zoom for Touch Input
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            if (touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began)
            {
                isDragging = false;
                isZooming = true;
                skipFirstTwoFingerFrame = true;
                lastTouchZeroPos = touchZero.position;
                lastTouchOnePos = touchOne.position;
                lastTouchDeltaMag = (touchZero.position - touchOne.position).magnitude;
                targetPosition = transform.position; // Reset target position when starting to zoom
                if (lockMovementCoroutine != null)
                    StopCoroutine(lockMovementCoroutine);
                lockMovementCoroutine = StartCoroutine(LockMovementTemporarily());
                return;
            }

            if (!isZooming) return;

            if ((touchZero.phase == TouchPhase.Moved || touchOne.phase == TouchPhase.Moved) && !skipFirstTwoFingerFrame)
            {
                float currentTouchDeltaMag = (touchZero.position - touchOne.position).magnitude;
                float deltaMagnitudeDiff = currentTouchDeltaMag - lastTouchDeltaMag;

                Zoom(deltaMagnitudeDiff * 0.001f);

                lastTouchDeltaMag = currentTouchDeltaMag;
            }

            skipFirstTwoFingerFrame = false;
        }
        else
        {
            isZooming = false;
            skipFirstTwoFingerFrame = true;
        }
    }

    IEnumerator LockMovementTemporarily()
    {
        lockedMovement = true;
        yield return new WaitForSeconds(0.2f); // 100 milliseconds
        lockedMovement = false;
    }

    // Method to handle zooming with limits
    void Zoom(float increment)
    {
        // Update the zoom position
        zoom += cam.transform.forward * increment * zoomSpeed;

        // Calculate the new distance from the camera to the zoom target
        float distance = zoom.magnitude;

        // Clamp the zoom level within the bounds

        if (distance < minZoom)
        {
            zoom = zoom.normalized * minZoom; // Maintain direction but limit distance
        }
        else if (distance > maxZoom)
        {
            zoom = zoom.normalized * maxZoom; // Maintain direction but limit distance
        }

        // Update the camera's local position
        camDir.localPosition = zoom;

        // Update drag speed based on the new zoom level
        UpdateDragSpeed();
    }
}