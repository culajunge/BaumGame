using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BucketBoys : Building
{
    [SerializeField] int waterOutputPerTick = 10;
    [SerializeField] private Transform waterLevel;

    float waterLevelMin = 0;
    float waterLevelMax = 1.2f;
    private int currentWater = 0;
    bool isActive = true;


    public int GetCurrentWater()
    {
        return currentWater;
    }

    public void SetWater(int amount)
    {
        currentWater = amount;
    }

    public override void OnPlaceDown()
    {
        base.OnPlaceDown();

        StartCoroutine(WaterTicksLoop());
    }

    void UpdateWaterLevel()
    {
        float currentWaterAsFloat = (float)currentWater / 4f;
        float height = Mathf.Lerp(waterLevelMin, waterLevelMax, currentWaterAsFloat);

        waterLevel.localPosition = new Vector3(0, height, 0);
    }

    IEnumerator WaterTicksLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(GetManager().tickTimeSeconds);
            if (isActive) currentWater += waterOutputPerTick;
            UpdateWaterLevel();
        }
    }

    public override void OnMannequinCollision(Mannequin mannequin)
    {
        resourceItem resource =
            new resourceItem(GetManager().GetWaterResourceTag(), currentWater, GetManager().waterColor);
        var (newResource, changedResource) = TransferResourceToMannequin(resource, mannequin);
        if (changedResource)
        {
            currentWater = newResource.itemAmount;
        }

        UpdateWaterLevel();
    }
}