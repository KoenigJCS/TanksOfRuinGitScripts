using System;
using UnityEngine;

public class CryoAmmo : I_Ammo
{
    public override DamageContext OnDamageEvent()
    {
        DamageContext damageContext = new(new());
        damageContext.damagePairs.Add(new(baseDamage,damageType));
        return damageContext;
    }
    
    public override void OnAmmoAdded(I_Unit unit)
    {
        // no effect when added
    }

    public override void OnAmmoRemoved(I_Unit unit)
    {
        // no effect when removed
    }

    void Start()
    {

    }
}