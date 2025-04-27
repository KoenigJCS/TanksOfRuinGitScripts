using Settworks.Hexagons;
using UnityEngine;


public class BasicTile : I_Tile
{
    public override float CheckProtection(I_Damage damage, I_Unit damageSource) {
        return 1;
    }

    // Note OnMoveOver Should be triggered even if the unit stops on the tile
    public override void OnMoveOver(I_Unit _) {
        
    }

    
    public override void OnStopOn(I_Unit _) {
        
    }

    void Start()
    {

    }
}