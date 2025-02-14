using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shredder : Building
{
    public float seedToCompostRatio = 4;
    int seeds = 0;
    private string compostResultTag = "Compost";
    private int compost = 0;

    public override void OnPlaceDown()
    {
        base.OnPlaceDown();
        compostResultTag = GetManager().GetCompostResourceTag();
        StartCoroutine(CompostSeeds());
    }

    IEnumerator CompostSeeds()
    {
        while (true)
        {
            yield return new WaitForSeconds(GetManager().tickTimeSeconds);
            if (seeds < seedToCompostRatio) continue;
            int seedsToConvert = Mathf.FloorToInt(seeds / seedToCompostRatio) * (int)seedToCompostRatio;
            int compostProduced = Mathf.FloorToInt(seedsToConvert / seedToCompostRatio);

            seeds -= seedsToConvert;
            compost += compostProduced;

            print($"Converted {seedsToConvert} seeds into {compostProduced} compost!");
        }
    }

    public int GetSeeds()
    {
        return seeds;
    }

    public void SetSeeds(int amount)
    {
        seeds = amount;
    }

    public string GetCompostResultTag()
    {
        return compostResultTag;
    }

    public void SetCompostResultTag(string tag)
    {
        compostResultTag = tag;
    }

    void AddSeeds(int amount)
    {
        seeds += amount;
        print($"Thanks for {amount} seeds!");
    }

    public override void OnMannequinCollision(Mannequin mannequin)
    {
        string mannequinResourceType = mannequin.GetResourceType();

        if (mannequin.isResourceEmtpy())
        {
            resourceItem resource = new resourceItem(compostResultTag, compost, GetManager().compostColor);
            var (newResource, changed) =
                TransferResourceToMannequin(resource, mannequin, GetManager().GetWaterResourceTag());
            if (changed) compost = newResource.itemAmount;
            return;
        }

        if (mannequinResourceType != GetManager().GetSeedResourceTag()) return;

        AddSeeds(mannequin.GetResourceAmount());
        mannequin.SetResource("", 0);
        mannequin.ChangeColor(GetManager().emptyColor);
    }
}