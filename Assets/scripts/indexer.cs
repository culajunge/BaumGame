using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class indexer : MonoBehaviour
{
    public tree[] trees;
    public static tree[] staticTrees;
    
    public building[] buildings;

    void Start()
    {
        staticTrees = trees;
    }
}
