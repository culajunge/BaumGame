using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dayNightCycle : MonoBehaviour
{
    public bool IsDay()
    {
        float dotProduct = Vector3.Dot(transform.forward, Vector3.up);
        return dotProduct < 0;
    }
}
