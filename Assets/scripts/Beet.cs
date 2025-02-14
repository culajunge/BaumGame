using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beet : MonoBehaviour
{
    Manager manager;
    List<TreeBehavior> trees = new List<TreeBehavior>();
    public bool enoughWater = false;
    public BeetConnector beetConnector;

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
        //dem mannequin seeds andrehen
        resourceItem resource = new resourceItem(manager.GetSeedResourceTag(), seeds, manager.seedColor);
        var (newResource, changedResource) =
            beetConnector.TransferResourceToMannequin(resource, mannequin, manager.GetWaterResourceTag());
        if (changedResource)
        {
            seeds = newResource.itemAmount;
            return;
        }

        //dem mannequin water stibitzen
        resource = new resourceItem(manager.GetWaterResourceTag(), water, manager.waterColor);
        (newResource, changedResource) = beetConnector.TransferResourceFromMannequin(resource, mannequin);
        water = newResource.itemAmount;
    }


    public bool IsWaterShortage()
    {
        enoughWater = water >= waterDemand;

        waterShortageInformer.SetActive(!enoughWater);
        return !enoughWater;
    }

    public int GetOxygen()
    {
        if (!enoughWater) return 0;

        int oxygen = 0;
        foreach (TreeBehavior tree in trees)
        {
            oxygen += tree.GetOxygenProduction();
        }

        print($"Beet has {oxygen} oxygen");

        return oxygen;
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