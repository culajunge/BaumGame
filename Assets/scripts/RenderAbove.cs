using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderAbove : MonoBehaviour
{
    //Not working
    void Start()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        Material material = renderer.material;
        material.renderQueue = 2500; // Geometry queue (2000) + 1
    }
}