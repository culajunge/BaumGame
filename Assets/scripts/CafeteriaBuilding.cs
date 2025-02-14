using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CafeteriaBuilding : Building
{
    const int cafeteriaMenuScreenIndex = 6;

    public void OnClick()
    {
        GetManager().ui.ShowOnly(GetManager().ui.screens[cafeteriaMenuScreenIndex]);
    }
}