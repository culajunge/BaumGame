using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMovement : MonoBehaviour
{
    public float baseDragSpeed = 2.0f; // Speed of the camera movement
    public float maxSpeedMultiplier = 3.0f; // Maximum speed multiplier
    public float smoothSpeed = 0.125f; // Smoothing speed for interpolation
    public float zoomSpeed = 5.0f; // Speed of zooming
    public float minZoom = 15.0f; // Minimum field of view (zoom in limit)
    public float maxZoom = 60.0f; // Maximum field of view (zoom out limit)

    private Vector3 dragOrigin; // Where the dragging started
    private bool isDragging = false;
    private Vector3 targetPosition; // The position to smoothly move towards
    private float currentDragSpeed;

    [SerializeField] private Camera cam;
    [SerializeField] private Transform camDir; // Reference to the camera's direction object
    private Vector3 zoom; // Zoom reference variable
    bool lockedMovement = false;

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

        //print($"Speed: {currentDragSpeed} \n ZoomFac: {zoomFactor}");
    }

    // Handles the input for mouse or touch
    void HandleInput()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        HandleWebGLInput();
#else
        HandleUniversalInput();
#endif
    }

    void HandleUniversalInput()
    {
        if (Input.touchCount == 1) // Touch input
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

                Vector3 move = new Vector3(-dragDelta.x, 0, -dragDelta.y) * currentDragSpeed * Time.deltaTime;
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

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentTouchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = prevTouchDeltaMag - currentTouchDeltaMag;

            // Zoom based on pinch gesture
            Zoom(deltaMagnitudeDiff * 0.01f); // Adjust multiplier for sensitivity
        }
    }

    // Method to handle zooming
    void Zoom(float increment, bool touch = false)
    {
        zoom += cam.transform.forward * increment * zoomSpeed;
        camDir.localPosition = zoom;
        UpdateDragSpeed();
        /*
        // Ensure the zoom level remains within the desired bounds
        float distance = Vector3.Distance(camDir.localPosition, transform.position);
        if (distance < minZoom)
        {
            zoom = transform.position + (camDir.localPosition - transform.position).normalized * minZoom;
        }
        else if (distance > maxZoom)
        {
            zoom = transform.position + (camDir.localPosition - transform.position).normalized * maxZoom;
        }*/
    }

    void HandleWebGLInput()
    {
        HandleUniversalInput();
    }
}