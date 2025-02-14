using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mainBuilding : Building
{
    void Start()
    {
        base.OnPlaceDown();
    }

    public override void OnDismantle()
    {
        print(
            "Why you try to destory main building? You will need it to get wood and tree seeds! Be more careful next time!");
    }

    public override void OnMannequinCollision(Mannequin mannequin)
    {
        if (mannequin.GetResourceType() == GetManager().GetWoodResourceTag())
        {
            resourceItem resource = new resourceItem(GetManager().GetWoodResourceTag(), 0, GetManager().woodColor);
            var (newResource, changed) = TransferResourceFromMannequin(resource, mannequin);
            if (changed) GetManager().AddWood(newResource.itemAmount);
        }else if (mannequin.GetResourceType() == GetManager().GetTreeSeedTag())
        {
            resourceItem resource = new resourceItem(GetManager().GetTreeSeedTag(), 0, GetManager().treeSeedColor);
            var (newResource, changed) = TransferResourceFromMannequin(resource, mannequin);
            if (changed) GetManager().AddSeeds(newResource.itemAmount);
        }
    }
}