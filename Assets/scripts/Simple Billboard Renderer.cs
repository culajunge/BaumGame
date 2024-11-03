using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBillboardRenderer : MonoBehaviour
{
    Transform mainCamera;
    void Start()
    {
        mainCamera = Camera.main.transform;
    }

    void LateUpdate()
    {
        transform.LookAt(mainCamera.transform);
        transform.Rotate(0, 180, 0);
    }
}
