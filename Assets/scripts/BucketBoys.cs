using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BucketBoys : Building
{
    [SerializeField] int waterOutput = 10;

    public int WaterOutput => waterOutput;
}
