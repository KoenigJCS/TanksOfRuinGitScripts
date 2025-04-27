using System;
using UnityEngine;

[Serializable]
public struct GlobalSave
{
    public int currentSave;
    public GlobalSave(int currentSave = -1) {
        this.currentSave=currentSave;
    }
}