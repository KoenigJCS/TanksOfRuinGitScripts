using System;
using System.Collections;
using System.Collections.Generic;
using Settworks.Hexagons;
using UnityEngine;

[Serializable]
public abstract class I_Tile : MonoBehaviour
{
    public HexCoord index;
    public int moveCost;
    public I_Unit unitOnTile;
    public bool onFire = false;
    public int type = -1;
    public float height = 0;
    public abstract void OnMoveOver(I_Unit unit);
    public abstract void OnStopOn(I_Unit unit);
    public abstract float CheckProtection(I_Damage damage, I_Unit damageSource);
}
