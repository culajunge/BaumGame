using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BucketBoys : Building
{
    [SerializeField] int waterOutputPerTick = 10;
    private int currentWater = 0;
    bool isActive = true;

    public override void OnPlaceDown()
    {
        base.OnPlaceDown();

        StartCoroutine(WaterTicksLoop());
    }

    IEnumerator WaterTicksLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(GetManager().tickTimeSeconds);
            if (isActive) currentWater += waterOutputPerTick;
        }
    }

    public override void OnMannequinCollision(Mannequin mannequin)
    {
        if (!mannequin.isResourceEmtpy() && mannequin.GetResourceType() != GetManager().GetWaterResourceTag()) return;

        int mannequinResourceAmount = mannequin.GetResourceAmount();
        int mannequinResourceCapacity = mannequin.maxResourceCapacity;
        int demand = mannequinResourceCapacity - mannequin.GetResourceAmount();

        int transferAmount = Mathf.Min(demand, currentWater);
        currentWater -= transferAmount;
        mannequin.SetResource(GetManager().GetWaterResourceTag(), mannequinResourceAmount + transferAmount);
    }
}