using Settworks.Hexagons;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class GroundTurret : BasicUnit
{
    public override bool Move(HexCoord dest) {
        SoundManager.inst.PlayBadButton();
        return true;
    }
}