using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mannequinCollider : MonoBehaviour
{
    [SerializeField] string mannequinTag = "Mannequin";
    [HideInInspector] public map map;

    private bool active = false;
    private Action<Mannequin> callOnCollsion;

    public void SetMap(map map)
    {
        this.map = map;
    }

    public void SetCollisionCallback(Action<Mannequin> callOnCollsionCallback)
    {
        callOnCollsion = callOnCollsionCallback;
    }

    public void FinishSetup()
    {
        active = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!active || !other.CompareTag(mannequinTag)) return;
        int manneqinIndex = int.Parse(other.gameObject.name);

        callOnCollsion(map.GetMannequin(manneqinIndex));
    }
}