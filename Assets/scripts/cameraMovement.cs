using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Gitversion
public class cameraMovement : MonoBehaviour
{
    public float baseDragSpeed = 1.5f;
    public float smoothSpeed = 0.15f;
    public float zoomSpeed = 3.0f;
    public float minZoom = 100.0f;
    public float maxZoom = 600.0f;
    public float dragSpeed = 0.5f;

    private Vector3 dragOrigin;
    private bool isDragging = false;
    private Vector3 targetPosition;
    private float currentDragSpeed;
    private bool lockedMovement = false;

    [SerializeField] private Camera cam;
    [SerializeField] private Transform camDir;
    [SerializeField] private Manager manager;
    private Vector3 zoom;

    float touchDist;
    float lastDist;


    void Start()
    {
        targetPosition = transform.position;
        zoom = camDir.localPosition;
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

    public void SetMovementLock(bool locked)
    {
        StartCoroutine(SetMovementLockDelayed(0.1f, locked));
    }

    IEnumerator SetMovementLockDelayed(float delay, bool locked)
    {
        yield return new WaitForSeconds(delay);
        lockedMovement = locked;
    }

    public bool GetMovementLock()
    {
        return lockedMovement;
    }

    void UpdateDragSpeed()
    {
        float zoomFactor =
            Mathf.InverseLerp(minZoom, maxZoom, Vector3.Distance(camDir.localPosition, transform.position));
        currentDragSpeed = baseDragSpeed;
    }

    void HandleInput()
    {
        if (Input.touchCount == 1)
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
                Vector3 move = new Vector3(-dragDelta.x, 0, -dragDelta.y) *
                               (currentDragSpeed * Time.deltaTime * dragSpeed);
                targetPosition += move;
                dragOrigin = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                isDragging = false;
            }
        }
        else if (Input.touchCount == 2)
        {
            isDragging = false;
        }
        else if (manager.GetPointerDownOnMap())
        {
            dragOrigin = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 dragDelta = Input.mousePosition - dragOrigin;
            Vector3 move = new Vector3(-dragDelta.x, 0, -dragDelta.y) * currentDragSpeed * Time.deltaTime;
            targetPosition += move;
            dragOrigin = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    void SmoothMoveCamera()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
    }

    void HandleZoom()
    {
        float scrollData = Input.GetAxis("Mouse ScrollWheel");
        if (scrollData != 0)
        {
            Zoom(scrollData);
        }

        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            if (touch1.phase == TouchPhase.Began && touch2.phase == TouchPhase.Began)
            {
                lastDist = Vector2.Distance(touch1.position, touch2.position);
            }

            if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved)
            {
                float newDist = Vector2.Distance(touch1.position, touch2.position);
                touchDist = lastDist - newDist;
                lastDist = newDist;
                Zoom(touchDist * -0.001f);
            }
        }
    }

    void Zoom(float increment)
    {
        zoom += cam.transform.forward * increment * zoomSpeed;
        float distance = zoom.magnitude;

        if (distance < minZoom)
        {
            zoom = zoom.normalized * minZoom;
        }
        else if (distance > maxZoom)
        {
            zoom = zoom.normalized * maxZoom;
        }

        camDir.localPosition = zoom;
        UpdateDragSpeed();
    }
}