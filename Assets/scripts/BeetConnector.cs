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
        base.OnPlaceDown();
        beet.OnPlaceDown();
    }

    public override void OnMannequinCollision(Mannequin mannequin)
    {
        beet.OnMannequinCollsion(mannequin);
    }

    public void AddTree(TreeBehavior treeBehavior)
    {
        beet.AddTree(treeBehavior);
    }

    public override void OnDismantle()
    {
        map map = GameObject.FindFirstObjectByType<map>();
        map.RemoveBeet(beet);

        base.OnDismantle();

        print("Dismantle beet");
    }
}