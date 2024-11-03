using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeetConnector : Building
{
    public Beet beet;

    public Beet GetBeet()
    {
        return beet;
    }

    public override void OnPlaceDown()
    {
        beet.beetConnector = this;
        beet.OnPlaceDown();
    }

    public void AddTree(TreeBehavior treeBehavior)
    {
        beet.AddTree(treeBehavior);
    }

    public override void OnDismantle()
    {
        base.OnDismantle();
        
        print("Dismantle beet");
    }
}
