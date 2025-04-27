using System;
using UnityEngine;

[Serializable]
public struct ItemSave
{
    public int itemID;
    public int unitID;
    public ItemSave(int n_itemID, int n_unitID) {
        itemID=n_itemID;
        unitID=n_unitID;
    }
}