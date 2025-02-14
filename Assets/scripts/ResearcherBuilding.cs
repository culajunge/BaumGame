using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResearcherBuilding : Building
{
    public float compostToSeedRatio = 12;

    private int inputCompost = 0;
    private int treeSeeds = 0;

    public override void OnPlaceDown()
    {
        base.OnPlaceDown();

        StartCoroutine(ResearchSeeds());
    }

    public override void OnMannequinCollision(Mannequin mannequin)
    {
        resourceItem resource = new resourceItem(GetManager().GetTreeSeedTag(), treeSeeds, GetManager().treeSeedColor);
        var (newResource, changedResource) =
            TransferResourceToMannequin(resource, mannequin, GetManager().GetWaterResourceTag());
        if (changedResource)
        {
            treeSeeds = newResource.itemAmount;
            return;
        }


        resource =
            new resourceItem(GetManager().GetCompostResourceTag(), inputCompost, GetManager().compostColor);
        var (newResource2, changed) = TransferResourceFromMannequin(resource, mannequin);
        inputCompost = newResource2.itemAmount;
    }

    IEnumerator ResearchSeeds()
    {
        while (true)
        {
            yield return new WaitForSeconds(GetManager().tickTimeSeconds);
            if (inputCompost < compostToSeedRatio) continue;
            int compostToConvert = Mathf.FloorToInt(inputCompost / compostToSeedRatio) *
                                   (int)compostToSeedRatio;
            int seedsProduced = Mathf.FloorToInt(compostToConvert / compostToSeedRatio);

            inputCompost -= compostToConvert;
            treeSeeds += seedsProduced;

            print($"Converted {compostToConvert} compost into {seedsProduced} tree seeds!");
        }
    }
}