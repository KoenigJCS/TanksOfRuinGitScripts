using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public struct AIArmy
{
    public float difficulty;
    public List<UnitSave> units;
    public List<ItemSave> items;

    public AIArmy(float difficulty) {
        this.difficulty = difficulty;
        units = new();
        items = new();
    }
}