using System;
using UnityEngine;

[Serializable]
public struct UnitSave
{
    public int itemID;
    public int unitID;
    public float health;
    public UnitSave(int n_itemID, int n_unitID, float n_health) {
        itemID=n_itemID;
        unitID=n_unitID;
        health=n_health;
    }
}