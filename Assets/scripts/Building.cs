using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

public class Building : MonoBehaviour
{
    [SerializeField] public PathConnector[] pathConnectors;
    private bool connectMultiple = true;

    [System.Serializable]
    public class PathConnector
    {
        public Transform transform;
        [HideInInspector] public bool isConnected;
    }

    Manager manager;

    [HideInInspector] public int buildingIndex;
    [HideInInspector] public int buildingID;

    public virtual void OnPlaceDown()
    {
        print("On PlaceDown");
        ToggleIgnoreRaycastLayer(false);
        manager = GameObject.FindFirstObjectByType<Manager>();
        CreateMannequinColliders();
    }

    public Manager GetManager()
    {
        return manager;
    }

    void ToggleIgnoreRaycastLayer(bool ignore)
    {
        gameObject.layer = ignore ? 2 : 0;
        print("Toggled Ignore Raycast Layer, ignore: " + ignore);
    }

    void CreateMannequinColliders()
    {
        foreach (PathConnector pathConnector in pathConnectors)
        {
            Transform pathConnectorTransform = pathConnector.transform;
            mannequinCollider mancol =
                Instantiate(manager.mannequinColliderPrefab, pathConnectorTransform).GetComponent<mannequinCollider>();
            mancol.transform.position = pathConnectorTransform.position;

            mancol.SetMap(FindFirstObjectByType<map>());
            mancol.SetCollisionCallback(OnMannequinCollision);
            mancol.FinishSetup();
        }
    }

    public virtual void OnMannequinCollision(Mannequin mannequin)
    {
        print($"{gameObject.name} got hit by {mannequin.name}");
    }

    public void SetID(int id)
    {
        buildingID = id;
    }

    public virtual void OnDismantle()
    {
        print("Wanna dismantle building");
        map map = GameObject.FindFirstObjectByType<map>();
        map.RemoveBuilding(gameObject);

        GameObject.FindFirstObjectByType<Manager>().PaybackBuilding(buildingID);

        Destroy(gameObject);
    }

    public virtual (resourceItem, bool) TransferResourceToMannequin(resourceItem resource, Mannequin mannequin,
        string restistanceResourceType = "")
    {
        bool changedResource = false;

        if (restistanceResourceType != "" && mannequin.GetLastResourceType() == restistanceResourceType)
        {
            return (resource, false);
        }

        if ((mannequin.isResourceEmtpy() || mannequin.GetResourceType() == resource.itemNameTag) &&
            resource.AmountAboveZero())
        {
            int amountMannequin = mannequin.GetResourceAmount();
            int mannequinCapacity = mannequin.maxResourceCapacity;
            int demand = mannequinCapacity - mannequin.GetResourceAmount();

            int transferAmount = Mathf.Min(resource.GetItemAmount(), demand);
            resource.itemAmount -= transferAmount;
            mannequin.SetResource(resource.itemNameTag, amountMannequin + transferAmount);
            mannequin.ChangeColor(resource.GetResourceColor());

            print($"{transferAmount} {resource.itemNameTag} transfered to Mannequin({mannequin.gameObject.name})");
            changedResource = true;
        }

        return (resource, changedResource);
    }

    public virtual (resourceItem, bool) TransferResourceFromMannequin(resourceItem resource, Mannequin mannequin)
    {
        if (!resource.CompareItems(mannequin.GetResourceType())) return (resource, false);

        resource.itemAmount += mannequin.GetResourceAmount();
        print($"{mannequin.GetResourceAmount()} {resource.itemNameTag} transfered to {gameObject.name}");
        mannequin.SetResource("", 0);
        mannequin.ChangeColor(manager.emptyColor);

        return (resource, true);
    }

    [CanBeNull]
    public Transform GetNearestPathConnector(Vector3 position)
    {
        float minDist = float.MaxValue;
        Transform nearestPathConnector = null;

        int index = -1;
        int finalIndex = -1;
        foreach (PathConnector pathConnector in pathConnectors)
        {
            index++;
            if (pathConnector.isConnected && !connectMultiple) continue;
            Transform pathConnectorTransform = pathConnector.transform;
            float dist = Vector3.Distance(pathConnectorTransform.position, position);
            if (dist < minDist)
            {
                minDist = dist;
                nearestPathConnector = pathConnectorTransform;
                finalIndex = index;
            }
        }

        if (nearestPathConnector == null) return null;

        pathConnectors[finalIndex].isConnected = true;
        return nearestPathConnector;
    }

    public virtual void OnPathConnect(Spline spline = null, bool isStart = false)
    {
    }
}