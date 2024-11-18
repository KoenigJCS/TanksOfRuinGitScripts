using System.Collections;
using System.Collections.Generic;
using Settworks.Hexagons;
using UnityEngine;

public abstract class I_Tile : MonoBehaviour
{
    public HexCoord index;
    public int moveCost;
    public I_Unit unitOnTile;
    public abstract void OnMoveOver();
    public abstract void OnStopOn();
}
