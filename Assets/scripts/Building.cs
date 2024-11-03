using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    
    [SerializeField] Transform[] pathConnectors;

    [HideInInspector] public int buildingIndex;
    [HideInInspector] public int buildingID;
    public virtual void OnPlaceDown()
    {
        print("On PlaceDown");
    }

    public void SetID(int id)
    {
        buildingID = id;
    }

    public virtual void OnDismantle()
    {
        map map = GameObject.FindFirstObjectByType<map>();
        map.RemoveBuilding(buildingIndex);
        
        GameObject.FindFirstObjectByType<Manager>().PaybackBuilding(buildingID);
        
        Destroy(gameObject);
    }
}
