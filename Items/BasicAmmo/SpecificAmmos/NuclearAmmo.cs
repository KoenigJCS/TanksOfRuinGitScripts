using System;
using UnityEngine;

public class NuclearAmmo : I_Ammo
{
    public override DamageContext OnDamageEvent()
    {
        DamageContext damageContext = new(new());
        damageContext.damagePairs.Add(new(baseDamage,damageType));
        return damageContext;
    }

    void Start()
    {

    }
}