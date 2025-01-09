using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beet : MonoBehaviour
{
    Manager manager;
    List<TreeBehavior> trees = new List<TreeBehavior>();
    public bool enoughWater = false;
    [HideInInspector] public BeetConnector beetConnector;

    [SerializeField] GameObject waterShortageInformer;

    private int water = 0;
    private int waterDemand = 0;

    public int GetWater()
    {
        return water;
    }

    public void AddWater(int amount)
    {
        water += amount;
    }

    bool ConsumeWater(int amount)
    {
        if (amount > water) return false;

        water -= amount;
        print($"Water: {water + amount} -> {water}");
        return true;
    }

    public void OnPlaceDown()
    {
        manager = GameObject.FindFirstObjectByType<Manager>();
        ToggleIgnoreRaycastLayer(false);
        tag = manager.GetBeetTag();
    }

    public void OnMannequinCollsion(Mannequin mannequin)
    {
        if (mannequin.GetResourceType() != manager.GetWaterResourceTag()) return;
        int amount = mannequin.GetResourceAmount();
        AddWater(amount);
        mannequin.SetResource("", 0);
        print($"Reset Mannequin, thanks for {amount} water");
        IsWaterShortage();
    }

    public bool IsWaterShortage()
    {
        enoughWater = water >= waterDemand;

        waterShortageInformer.SetActive(!enoughWater);
        return !enoughWater;
    }

    public void GrowTrees()
    {
        if (IsWaterShortage()) return;
        if (!ConsumeWater(waterDemand)) return;

        foreach (TreeBehavior tree in trees)
        {
            if (tree == null) continue;
            tree.Grow();
        }
    }

    public int GetWaterDemand()
    {
        int demand = 0;
        foreach (TreeBehavior tree in trees)
        {
            if (tree == null) continue;
            demand += manager.indexer.trees[tree.treeID].waterConsumption;
        }

        return demand;
    }

    void ToggleIgnoreRaycastLayer(bool ignore)
    {
        gameObject.layer = ignore ? 2 : 0;
    }

    public void AddTree(TreeBehavior treeBehavior)
    {
        trees.Add(treeBehavior);
        treeBehavior.treeBeetIndex = GetLastTreeIndex();
        waterDemand = GetWaterDemand();
        IsWaterShortage();
    }

    public void RemoveTree(TreeBehavior tree)
    {
        trees.Remove(tree);
    }

    public void OnDismantle()
    {
        if (trees.Count > 0)
        {
            for (int i = trees.Count - 1; i >= 0; i--)
            {
                trees[i].OnDismantle();
            }
        }

        beetConnector.OnDismantle();
    }

    int GetLastTreeIndex()
    {
        return trees.Count - 1;
    }
}