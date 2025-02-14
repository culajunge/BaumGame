using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSaveData
{
    public int gold;
    public int wood;
    public int oxygen;

    public List<int> treeInventory = new List<int>();

    public List<BuildingData> buildingData = new List<BuildingData>();
    public List<BeetData> beetData = new List<BeetData>();
    public List<PathData> pathData = new List<PathData>();
}

[System.Serializable]
public class BuildingData
{
    public string buildingTag;
    public int buildingTypeId;
    public WeakTransformData weakTransform;

    //Only populated if applies to type!
    public BucketBoysData bucketBoysData;
    public WorkerTentData workerTentData;
    public ShredderData shredderData;
}

[System.Serializable]
public class BucketBoysData
{
    public int bucketBoyTypeId;
    public int water;
}

[System.Serializable]
public class WorkerTentData
{
    public int associatedMannequins; //needs to be implemented in runtime
}

[System.Serializable]
public class ShredderData
{
    public int seeds;
    public string compostResultType;
}

[System.Serializable]
public class BeetData
{
    public int beetTypeId;
    public Vector3 position;
    public int seeds;
    public int water;
    public List<TreeData> trees = new List<TreeData>();
}

[System.Serializable]
public class TreeData
{
    public int treeSpeciesId;
    public Vector3 position;
    public float growth; //between 0f and 1f
}

[System.Serializable]
public class PathData
{
    public int pathId;
    public List<KnotData> knots = new List<KnotData>();
}

[System.Serializable]
public class KnotData
{
    public Vector3 position;
    public Vector3 normal;

    public KnotData(Vector3 position, Vector3 normal)
    {
        this.position = position;
        this.normal = normal;
    }
}

[System.Serializable]
public class WeakTransformData
{
    public Vector3 position;
    public Quaternion rotation;

    public WeakTransformData(Vector3 position, Quaternion rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }
}