using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassBehavior : MonoBehaviour
{

    [SerializeField] private int grassTypeID;
    
    public int OnCollect()
    {
        Destroy(gameObject, 0.1f);
        return grassTypeID;
    }
}
