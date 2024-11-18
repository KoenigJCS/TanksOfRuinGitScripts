using System.Collections.Generic;
using UnityEngine;

public enum DamageType
{
    Nuclear,
    Railgun,
    HEAT,
    Flame,
    HighExplosive,
    Shrapnel,
    Plasma,
    Cryo,
    Solid,
}

public struct DamagePair {
    public float damage;
    public DamageType damageType;
    public DamagePair(float damage,DamageType damageType) {
        this.damage = damage;
        this.damageType = damageType;
    }
}

public struct DamageContext
{
    public List<DamagePair> damagePairs;

    public DamageContext(List<DamagePair> damagePairs) {
        this.damagePairs = damagePairs;
    }
}

public interface I_Damage
{
    public abstract DamageContext OnDamageEvent();
}