using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawmillBuilding : Building
{
    public float compostToWoodRatio = 1;

    int inputCompost = 0;
    int outputWood = 0;

    public override void OnPlaceDown()
    {
        base.OnPlaceDown();
        StartCoroutine(ProduceWood());
    }

    IEnumerator ProduceWood()
    {
        while (true)
        {
            yield return new WaitForSeconds(GetManager().tickTimeSeconds);
            if (inputCompost < compostToWoodRatio) continue;
            int compostToConvert = Mathf.FloorToInt(inputCompost / compostToWoodRatio) * (int)compostToWoodRatio;
            int woodProduced = Mathf.FloorToInt(compostToConvert / compostToConvert);

            inputCompost -= compostToConvert;
            outputWood += woodProduced;

            print($"Converted {compostToConvert} compost into {woodProduced} compost!");
        }
    }

    public override void OnMannequinCollision(Mannequin mannequin)
    {
        if (mannequin.isResourceEmtpy())
        {
            resourceItem resource2 =
                new resourceItem(GetManager().GetWoodResourceTag(), outputWood, GetManager().woodColor);
            var (newResource2, changed2) = TransferResourceToMannequin(resource2, mannequin);
            outputWood = newResource2.itemAmount;
            return;
        }

        if (mannequin.GetResourceType() != GetManager().GetCompostResourceTag()) return;
        resourceItem resource =
            new resourceItem(GetManager().GetCompostResourceTag(), inputCompost, GetManager().compostColor);
        var (newResource, changed) = TransferResourceFromMannequin(resource, mannequin);
        inputCompost = newResource.itemAmount;
    }
}