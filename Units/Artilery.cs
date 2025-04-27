using Settworks.Hexagons;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Artilery : BasicUnit
{
    public override bool Move(HexCoord dest) {
        firePoints=0;
        return base.Move(dest);
    }
}