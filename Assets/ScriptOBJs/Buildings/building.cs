using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "ScriptOBJs/Building", order = 1)]
public class building : ScriptableObject
{
    public string buildingName;
    public Sprite buildingPreview;
    public int woodCost;
    public int goldCost;
    
    public GameObject buildingObj;
}
