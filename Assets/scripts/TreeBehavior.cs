using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeBehavior : MonoBehaviour
{
    [HideInInspector] public int treeIndex;
    [HideInInspector] public int treeBeetIndex;

    Manager manager;
    LayerMask raycastLayerMask;
    private float currentGrowth;
    [HideInInspector] public int treeID;

    private float growthFactor = 0;
    private float growLimit = 1;
    private bool growing = false;
    [HideInInspector] public Beet beet;
    public int level;
    private int oxygenProduction;

    public void SetOxygenProduction(int amount)
    {
        oxygenProduction = amount;
    }

    public int GetOxygenProduction()
    {
        print("Oxygen Production: " + oxygenProduction);
        return oxygenProduction;
    }

    public void OnPlant(int id)
    {
        treeID = id;
        manager = GameObject.FindFirstObjectByType<Manager>();
        raycastLayerMask = gameObject.layer;
        tag = "Tree";
        ToggleIgnoreRaycastLayer(false);

        growthFactor = manager.indexer.trees[treeID].growFactor;
        growLimit = manager.indexer.trees[treeID].growLimit;
        growing = true;
    }

    public void OnDismantle()
    {
        map map = GameObject.FindFirstObjectByType<map>();
        map.RemoveTree(gameObject);

        beet.RemoveTree(this);

        Destroy(gameObject);
    }

    public void ToggleIgnoreRaycastLayer(bool ignore)
    {
        gameObject.layer = ignore ? 2 : 0;
    }

    public float GetGrowth()
    {
        return transform.localScale.x;
    }

    public void Grow()
    {
        if (growing && transform.localScale.x < growLimit)
        {
            transform.localScale += Vector3.one * (growthFactor * manager.treeGrowthSmallMaker);
        }
    }
}