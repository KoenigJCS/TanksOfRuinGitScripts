using Settworks.Hexagons;
using UnityEngine;


public class CoverTile : I_Tile
{
    public float coverCenterDegrees = 0f;
    public override float CheckProtection(I_Damage damage, I_Unit damageSource) {
        float flatAngle = Mathf.Abs(Vector3.SignedAngle(damageSource.transform.position - transform.position, transform.forward,Vector3.up)-90);
        Debug.Log(flatAngle);
        if(flatAngle<=90) {
            return .5f;
        }
        return 1;
    }
    // Note OnMoveOver Should be triggered even if the unit stops on the tile
    public override void OnMoveOver(I_Unit unit)
    {
        
    }

    
    public override void OnStopOn(I_Unit _) {
        
    }

    void Start()
    {

    }
}