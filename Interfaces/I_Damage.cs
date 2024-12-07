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
    public float flankMultplier;

    public DamageContext(List<DamagePair> damagePairs, float n_flankMultiplier = 1.5f) {
        this.damagePairs = damagePairs;
        this.flankMultplier = n_flankMultiplier;
    }
}

public interface I_Damage
{
    public int height {get; set;}
    public abstract DamageContext OnDamageEvent();
}