using Settworks.Hexagons;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PortaNuke : BasicUnit
{
    public override void Fire(I_Unit _) {
        lock (AIManager.inst.Units) {
            foreach (I_Unit unit in AIManager.inst.Units){
            if(HexCoord.Distance(hexPosition,unit.hexPosition)>weaponRange) {
                continue;
            }
            unit.TakeDamage(CreateDamage(),this);
            unit.SetHealthBarVis(true);
            unit.visHealthBarTimer = 2.5f;
        }
        }
        lock (PlayerManager.inst.Units) {
        foreach (I_Unit unit in PlayerManager.inst.Units){
            if(HexCoord.Distance(hexPosition,unit.hexPosition)>weaponRange) {
                continue;
            }
            unit.TakeDamage(CreateDamage(),this);
            unit.SetHealthBarVis(true);
            unit.visHealthBarTimer = 2.5f;
        }
        }
        GameObject explosion = Instantiate(SoundManager.inst.tankExplodeEffect, transform.position+Vector3.up, transform.rotation);
        explosion.transform.localScale= Vector3.one*weaponRange;
        Destroy(explosion, 3.0f);
        health=-1;
    }
}