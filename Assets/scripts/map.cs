using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class map : MonoBehaviour
{
    
    [Header("Refs")]
    [SerializeField] Manager manager;
    [SerializeField] dayNightCycle dayNightCycle;
    
    List<GameObject> treesList = new List<GameObject>();
    List<int> treeIDs = new List<int>();
    
    List<GameObject> buildingsList = new List<GameObject>();
    List<int> buildingIDs = new List<int>();
    
    List<Beet> beetsList = new List<Beet>();

    public void AddTree(int id, GameObject tree)
    {
        treesList.Add(tree);
        treeIDs.Add(id);
    }

    public void AddBuilding(int id, GameObject building)
    {
        buildingsList.Add(building);
        buildingIDs.Add(id);
    }

    public void AddBeet(Beet beet)
    {
        beetsList.Add(beet);
    }

    public void RemoveTree(int id)
    {
        treesList[id] = null;
        treeIDs[id] = -1;
    }

    public void RemoveBuilding(int id)
    {
        buildingsList[id] = null;
    }

    public void RemoveBeet(int id)
    {
        beetsList[id] = null;
    }

    public int GetO2Emission()
    {
        if (!dayNightCycle.IsDay()) return 0;
        
        float totalO2Emission = 0;

        for (int i = 0; i < treeIDs.Count; i++)
        {
            if (treeIDs[i] == -1) continue;
            
            totalO2Emission += manager.indexer.trees[treeIDs[i]].O2Emission;
        }
        return Mathf.RoundToInt(totalO2Emission);
    }

    public void GrowTrees()
    {
        if(!dayNightCycle.IsDay()) return;
        foreach (Beet beet in beetsList)
        {
            if(beet == null) continue;
            beet.GrowTrees();
        }
    }

    public int GetNextBuildingIndex()
    {
        Building bd = buildingsList[^1].GetComponent<Building>();
        return bd.buildingIndex + 1;
    }

    public int GetNextTreeIndex()
    {
        TreeBehavior tree = treesList[^1].GetComponent<TreeBehavior>();
        return tree.treeIndex + 1;
    }
    
    public int GetLastBuildingIndex()
    {
        Building bd = buildingsList[^1].GetComponent<Building>();
        return bd.buildingIndex;
    }

    public int GetLastTreeIndex()
    {
        TreeBehavior tree = treesList[^1].GetComponent<TreeBehavior>();
        return tree.treeIndex;
    }
}
