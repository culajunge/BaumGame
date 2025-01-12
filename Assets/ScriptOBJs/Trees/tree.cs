using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tree", menuName = "ScriptOBJs/Tree", order = 1)]
public class tree : ScriptableObject
{
    public string treeName;
    public int level;

    public Sprite treePreview;

    public int amount;

    public GameObject treeOBJ;
    public float growFactor;
    public float growLimit;
    public int waterConsumption;
    public int O2Emission;
}