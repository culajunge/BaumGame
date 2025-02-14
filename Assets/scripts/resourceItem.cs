using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class resourceItem
{
    public string itemNameTag;
    public int itemAmount;
    public int resourceMaxCapacity = Int32.MaxValue;
    Color resourceColor;

    public resourceItem(string itemNameTag, int itemAmount, Color resourceColor,
        int resourceMaxCapacity = Int32.MaxValue)
    {
        this.itemNameTag = itemNameTag;
        this.itemAmount = itemAmount;
        this.resourceMaxCapacity = resourceMaxCapacity;
        this.resourceColor = resourceColor;
    }

    public bool CompareItems(string itemTag)
    {
        return String.Equals(itemNameTag, itemTag);
    }

    public int GetItemAmount()
    {
        return itemAmount;
    }

    public Color GetResourceColor()
    {
        return resourceColor;
    }

    public bool AmountAboveZero()
    {
        return itemAmount > 0;
    }

    public bool TransferToMannequin(Mannequin mannequin)
    {
        Console.WriteLine("Needs to be implemented");
        return false;
    }
}