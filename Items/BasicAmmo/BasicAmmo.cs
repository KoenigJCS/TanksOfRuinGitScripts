using System;
using UnityEngine;

public class BasicAmmo : I_Ammo
{
    public override DamageContext OnDamageEvent()
    {
        DamageContext damageContext = new(new());
        damageContext.damagePairs.Add(new(baseDamage,damageType));
        return damageContext;
    }
    
    public override void OnAmmoAdded(I_Unit unit)
    {
        
    }

    public override void OnAmmoRemoved(I_Unit unit)
    {
        
    }

    void Start()
    {

    }
}