using UnityEngine;

public class BasicMod : I_Mod
{   
    public override void SetDamageParent(I_Damage n_damage) {
        damage = n_damage;
    }
    public override DamageContext OnDamageEvent()
    {
        DamageContext dc = damage.OnDamageEvent();
        for (int i = 0; i < dc.damagePairs.Count; i++) {
            dc.damagePairs[i] = new(dc.damagePairs[i].damage*3,dc.damagePairs[i].damageType);
        }
        return dc;
    }
}