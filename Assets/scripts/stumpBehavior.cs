using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class stumpBehavior : MonoBehaviour
{
    [SerializeField] private int amount = 1;
    public int OnCollect()
    {
        Destroy(gameObject, 0.1f);
        return amount;
    }
}
