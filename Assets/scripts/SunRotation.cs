using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunRotation : MonoBehaviour
{
    [SerializeField] private Vector3 rotationAxis;
    
    void Update()
    {
        transform.Rotate(rotationAxis * Time.deltaTime);
    }
}
