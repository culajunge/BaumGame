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
    [SerializeField] private int maxSeeds = 25;

    private int beetTypeId;

    int seeds = 0;

    private int water = 0;
    private int waterDemand = 0;

    public List<TreeBehavior> GetTrees()
    {
        return trees;
    }

    public int GetWater()
    {
        return water;
    }

    public void SetSeeds(int amount)
    {
        seeds = amount;
    }

    public void SetWater(int amount)
    {
        water = amount;
    }

    public int GetBeetTypeId()
    {
        return beetTypeId;
    }

    public int GetSeeds()
    {
        return seeds;
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
        print($"{mannequin.GetResourceType()} Mannequin collided");
        int amount = mannequin.GetResourceAmount();
        if ((mannequin.isResourceEmtpy() || mannequin.GetResourceType() == manager.GetSeedResourceTag()) && seeds > 0)
        {
            int mannequinResourceAmount = amount;
            int mannequinResourceCapacity = mannequin.maxResourceCapacity;
            int demand = mannequinResourceCapacity - mannequin.GetResourceAmount();

            int transferAmount = Mathf.Min(demand, seeds);
            seeds -= transferAmount;
            mannequin.SetResource(manager.GetSeedResourceTag(), mannequinResourceAmount + transferAmount);
            mannequin.ChangeColor(manager.seedColor);
            return;
        }

        if (mannequin.GetResourceType() != manager.GetWaterResourceTag()) return;

        AddWater(amount);
        mannequin.SetResource("", 0);
        mannequin.ChangeColor(manager.emptyColor);
        print($"Reset Mannequin, thanks for {amount} water");
        IsWaterShortage();
    }

    public bool IsWaterShortage()
    {
        enoughWater = water >= waterDemand;

        waterShortageInformer.SetActive(!enoughWater);
        return !enoughWater;
    }

    public void GrowTreesAnCollectSeeds()
    {
        if (IsWaterShortage()) return;
        if (!ConsumeWater(waterDemand)) return;

        foreach (TreeBehavior tree in trees)
        {
            if (tree == null) continue;
            tree.Grow();
            seeds += tree.level;
            if (seeds > maxSeeds) seeds = maxSeeds;
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