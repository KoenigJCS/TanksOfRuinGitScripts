using Settworks.Hexagons;
using UnityEngine;

public class ForestTile : I_Tile
{
    // Note OnMoveOver Should be triggered even if the unit stops on the tile
    public override void OnMoveOver()
    {
        
    }

    
    public override void OnStopOn()
    {
        print("Stopped on: "+transform.position);
    }

    void Start()
    {

    }
}