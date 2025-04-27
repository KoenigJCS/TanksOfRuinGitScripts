using System.Collections.Generic;
using Settworks.Hexagons;
using UnityEngine;


public class MineTile : I_Tile
{

    [SerializeField] List<GameObject> mines = new();
    public override float CheckProtection(I_Damage damage, I_Unit damageSource) {
        return 1;
    }
    // Note OnMoveOver Should be triggered even if the unit stops on the tile
    public override void OnMoveOver(I_Unit unit) {
        
    }

    
    public override void OnStopOn(I_Unit unit) {
        if(mines.Count==0) {
            return;
        }
        MineDamage damage = new();
        unit.TakeDamage(damage);
        GameObject explosion = Instantiate(SoundManager.inst.tankExplodeEffect, transform.position+1.0f*Vector3.up, transform.rotation);
        Destroy(explosion, 3.0f);
        GameObject mine = mines[^1];
        mines.RemoveAt(mines.Count-1);
        Destroy(mine);
    }

    void Start()
    {

    }
}

public class MineDamage : I_Damage
{
    public int height { get => 0; set{;}}

    public DamageContext OnDamageEvent()
    {
        DamagePair damage = new(33,DamageType.Solid);
        return new(new(){damage});
    }
}