using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class Building : MonoBehaviour
{
    [SerializeField] public Transform[] pathConnectors;

    [HideInInspector] public int buildingIndex;
    [HideInInspector] public int buildingID;

    public virtual void OnPlaceDown()
    {
        print("On PlaceDown");
        ToggleIgnoreRaycastLayer(false);
    }

    void ToggleIgnoreRaycastLayer(bool ignore)
    {
        gameObject.layer = ignore ? 2 : 0;
    }

    public void SetID(int id)
    {
        buildingID = id;
    }

    public virtual void OnDismantle()
    {
        map map = GameObject.FindFirstObjectByType<map>();
        map.RemoveBuilding(gameObject);

        GameObject.FindFirstObjectByType<Manager>().PaybackBuilding(buildingID);

        Destroy(gameObject);
    }

    public Transform GetNearestPathConnector(Vector3 position)
    {
        float minDist = float.MaxValue;
        Transform nearestPathConnector = null;

        foreach (Transform pathConnector in pathConnectors)
        {
            float dist = Vector3.Distance(pathConnector.position, position);
            if (dist < minDist)
            {
                minDist = dist;
                nearestPathConnector = pathConnector;
            }
        }

        return nearestPathConnector;
    }

    public virtual void OnPathConnect(Spline spline = null)
    {
    }
}