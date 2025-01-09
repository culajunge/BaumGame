using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class map : MonoBehaviour
{
    [Header("Refs")] [SerializeField] Terrain terrain;
    [SerializeField] Manager manager;
    [SerializeField] dayNightCycle dayNightCycle;
    [SerializeField] public SplineInterface splineInterface;


    List<GameObject> treesList = new List<GameObject>();
    List<int> treeIDs = new List<int>();

    List<GameObject> buildingsList = new List<GameObject>();
    List<int> buildingIDs = new List<int>();

    List<Beet> beetsList = new List<Beet>();

    List<Mannequin> mannequins = new List<Mannequin>();

    public void AddMannequin(Mannequin mannequin, int index)
    {
        mannequins.Insert(index, mannequin);
    }

    public Mannequin GetMannequin(int index)
    {
        return mannequins[index];
    }

    public int GetMannequinCount()
    {
        return mannequins.Count;
    }

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

    public void RemoveTree(GameObject tree)
    {
        treesList.Remove(tree);
        treeIDs.Remove(treesList.IndexOf(tree));
    }

    public void RemoveBuilding(GameObject building)
    {
        buildingsList.Remove(building);
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
        if (!dayNightCycle.IsDay()) return;
        foreach (Beet beet in beetsList)
        {
            if (beet == null) continue;
            beet.GrowTrees();
        }
    }

    public Vector3 GetTerrainNormal(Vector3 worldPos)
    {
        Terrain terrain = Terrain.activeTerrain;
        TerrainData terrainData = terrain.terrainData;

        // Convert world position to terrain space
        Vector3 terrainPosition = terrain.transform.InverseTransformPoint(worldPos);

        // Get normalized position within terrain
        float normX = terrainPosition.x / terrainData.size.x;
        float normZ = terrainPosition.z / terrainData.size.z;

        // Get terrain normal at this position
        Vector3 normal = terrainData.GetInterpolatedNormal(normX, normZ);

        return normal;
    }

    public float GetTerrainHeight(Vector3 worldPos)
    {
        Terrain terrain = Terrain.activeTerrain;

        // Convert world position to terrain local position
        Vector3 terrainLocalPos = worldPos - terrain.transform.position;

        // Get the height at the specified position
        float height = terrain.SampleHeight(terrainLocalPos);

        return height;
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