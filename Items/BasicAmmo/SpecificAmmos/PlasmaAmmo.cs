using System;
using UnityEngine;

public class PlasmaAmmo : I_Ammo
{
    private int standardRange;
    public override DamageContext OnDamageEvent()
    {
        DamageContext damageContext = new(new());
        damageContext.damagePairs.Add(new(baseDamage,damageType));
        return damageContext;
    }

    public override void OnAmmoAdded(I_Unit unit)
    {
        standardRange = unit.weaponRange;
        unit.weaponRange = 1000;
    }

    public override void OnAmmoRemoved(I_Unit unit)
    {
        unit.weaponRange = standardRange;
    }

    void Start()
    {

    }
}