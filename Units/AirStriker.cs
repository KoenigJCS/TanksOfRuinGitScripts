using Settworks.Hexagons;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class AirStriker : BasicUnit
{
    public override void OnTurnEnd() {
        foreach(I_Unit unit in TileManager.inst.GetUnitsOnNeighboringHexes(hexPosition)) {
            if(unit.playerControlled != playerControlled) {
                Fire(unit);
            }
        }
    }
}