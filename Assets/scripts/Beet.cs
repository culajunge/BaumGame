using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beet : MonoBehaviour
{
    Manager manager;
    List<TreeBehavior> trees = new List<TreeBehavior>();
    int waterinput = 0;
    [HideInInspector] public BeetConnector beetConnector;
    
    [SerializeField] GameObject waterShortageInformer;
    public void OnPlaceDown()
    {
        manager = GameObject.FindFirstObjectByType<Manager>();
        ToggleIgnoreRaycastLayer(false);
        tag = manager.GetBeetTag();
    }

    public bool isWaterShortage()
    {
        bool shortage = waterinput < GetWaterDemand();
        waterShortageInformer.SetActive(shortage);
        print($"Water shortage: {shortage}");
        return shortage;
    }

    public void GrowTrees()
    {
        if(isWaterShortage()) return;

        foreach (TreeBehavior tree in trees)
        {
            if(tree == null) continue;
            tree.Grow();
        }
    }

    public int GetWaterDemand()
    {
        int demand = 0;
        foreach (TreeBehavior tree in trees)
        {
            if(tree == null) continue;
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
        isWaterShortage();
    }

    public void RemoveTree(int index)
    {
        trees[index] = null;
    }

    public void OnDismantle()
    {
        foreach (TreeBehavior tree in trees)
        {
            if(tree == null) continue;
            tree.OnDismantle();
        }
        
        beetConnector.OnDismantle();
    }

    int GetLastTreeIndex()
    {
        return trees.Count - 1;
    }
}
