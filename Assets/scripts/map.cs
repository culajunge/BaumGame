using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Timers;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Splines;
using Debug = System.Diagnostics.Debug;

public class map : MonoBehaviour
{
    [Header("Refs")] [SerializeField] Terrain terrain;
    [SerializeField] public Manager manager;
    [SerializeField] dayNightCycle dayNightCycle;
    [SerializeField] public SplineInterface splineInterface;


    List<GameObject> treesList = new List<GameObject>();
    List<int> treeIDs = new List<int>();

    List<GameObject> buildingsList = new List<GameObject>();
    List<int> buildingIDs = new List<int>();

    List<Beet> beetsList = new List<Beet>();

    List<Mannequin> mannequins = new List<Mannequin>();

    public List<int> GetTreeInventory()
    {
        List<int> treeInventory = new List<int>();
        indexer indexer = manager.indexer;

        foreach (tree tree in indexer.trees)
        {
            treeInventory.Add(tree.amount);
        }

        return treeInventory;
    }

    public void AddMannequin(Mannequin mannequin, int index)
    {
        mannequins.Insert(index, mannequin);
    }

    public Mannequin GetMannequin(int index)
    {
        return mannequins[index];
    }

    public List<Mannequin> GetMannequins()
    {
        return mannequins;
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

    public void RemoveBeet(Beet beet)
    {
        beetsList.Remove(beet);
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
            beet.GrowTreesAnCollectSeeds();
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

    #region Save Data

    List<KnotData> SplineKnotsToWeakTransforms(Spline spline)
    {
        List<KnotData> transforms = new List<KnotData>();

        foreach (BezierKnot knot in spline.Knots)
        {
            KnotData knotData = new KnotData(knot.Position, splineInterface.GetBezierKnotNormal(knot, Vector3.up));
            transforms.Add(knotData);
        }

        return transforms;
    }

    public void SaveGame()
    {
        Stopwatch timer = new Stopwatch();
        timer.Start();
        print("Saving game...");

        GameSaveData saveData = new GameSaveData();

        // Save resources
        saveData.gold = manager.GetGold();
        saveData.wood = manager.GetWood();
        saveData.oxygen = manager.GetOxygen();

        // Save tree inventory
        saveData.treeInventory = GetTreeInventory();

        // Save buildings
        foreach (GameObject buildingObj in buildingsList)
        {
            Building building = buildingObj.GetComponent<Building>();

            if (building is BeetConnector beetConnector) continue;

            BuildingData data = new BuildingData
            {
                buildingTag = building.transform.tag,
                buildingTypeId = building.buildingID,
                weakTransform = new WeakTransformData(building.transform.position, building.transform.rotation)
            };

            // Handle specific building types
            if (building is BucketBoys bucketBoys)
            {
                data.bucketBoysData = new BucketBoysData
                {
                    bucketBoyTypeId = bucketBoys.buildingID,
                    water = bucketBoys.GetCurrentWater()
                };
            }
            else if (building is civilTent workerTent)
            {
                data.workerTentData = new WorkerTentData
                {
                    associatedMannequins = workerTent.GetAssociatedMannequins(),
                };
            }
            else if (building is shredder shredder)
            {
                data.shredderData = new ShredderData
                {
                    seeds = shredder.GetSeeds(),
                    compostResultType = shredder.GetCompostResultTag(),
                };
            }

            saveData.buildingData.Add(data);
        }

        // Save beets
        foreach (Beet beet in beetsList)
        {
            BeetData beetData = new BeetData
            {
                beetTypeId = beet.GetBeetTypeId(),
                position = beet.beetConnector.transform.position,
                seeds = beet.GetSeeds(),
                water = beet.GetWater()
            };

            // Save trees on this beet
            foreach (TreeBehavior tree in beet.GetTrees())
            {
                TreeData treeData = new TreeData
                {
                    treeSpeciesId = tree.treeID,
                    position = tree.transform.position,
                    growth = tree.GetGrowth()
                };
                beetData.trees.Add(treeData);
            }

            saveData.beetData.Add(beetData);
        }

        int tempIndex = 0;
        // Save paths
        foreach (Spline path in splineInterface.splineContainer.Splines)
        {
            PathData pathData = new PathData
            {
                pathId = tempIndex,
                knots = SplineKnotsToWeakTransforms(path)
            };
            saveData.pathData.Add(pathData);

            tempIndex++;
        }

        // Save to file
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(Application.persistentDataPath + "/gamesave.json", json);

        timer.Stop();
        print($"Game saved in {timer.ElapsedMilliseconds}ms to {Application.persistentDataPath}/gamesave.json");
    }

    public void LoadGame()
    {
        Stopwatch timer = new Stopwatch();
        timer.Start();
        print("Loading game...");

        ClearAllExistingObjects();

        string path = Application.persistentDataPath + "/gamesave.json";
        if (!File.Exists(path))
        {
            print("No save file found!");
            return;
        }

        string json = File.ReadAllText(path);
        GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);

        foreach (BeetData beet in saveData.beetData)
        {
            manager.PlaceBeetByData(beet);
        }

        foreach (BuildingData building in saveData.buildingData)
        {
            manager.PlaceBuildingByData(building);
        }

        foreach (PathData pathData in saveData.pathData)
        {
            manager.PlaceSplineByData(pathData);
        }

        manager.SetInventory(saveData.wood, saveData.oxygen, saveData.gold, saveData.treeInventory);

        timer.Stop();
        print($"Game loaded in {timer.ElapsedMilliseconds}ms from {path}");
    }

    private void ClearAllExistingObjects()
    {
        //manager.SetInventory(0, 0, 0, new List<int>(manager.indexer.trees.Length));

        for (int i = buildingsList.Count - 1; i >= 0; i--)
        {
            GameObject bd = buildingsList[i];
            if (bd.CompareTag(manager.GetBeetTag())) continue;
            bd.GetComponent<Building>().OnDismantle();
        }

        for (int i = beetsList.Count - 1; i >= 0; i--)
        {
            Beet bt = beetsList[i];
            bt.beetConnector.OnDismantle();
        }

        foreach (Mannequin mannequin in mannequins)
        {
            mannequin.CommitSuicide();
        }

        splineInterface.ClearAllSplines();
    }

    #endregion
}