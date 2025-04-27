using UnityEngine;

public class BasicMod : I_Mod
{   
    public override bool SetDamageParent(I_Damage n_damage) {
        if(n_damage.height>=3) {
            return false;
        }
        height = n_damage.height+1;
        damage = n_damage;
        return true;
    }
    public override DamageContext OnDamageEvent()
    {
        DamageContext dc = damage.OnDamageEvent();
        for (int i = 0; i < dc.damagePairs.Count; i++) {
            dc.damagePairs[i] = new(dc.damagePairs[i].damage*3,dc.damagePairs[i].damageType);
        }
        return dc;
    }

    public override void SetDamageBase(I_Damage n_base)
    {
        I_DamageDecorator deco = this;
        while(deco.damage is I_DamageDecorator temp) {
            deco = temp;
        }
        deco.damage =n_base;
    }

    public override I_DamageDecorator RemoveDamageDeco(I_Damage decorator)
    {
        I_DamageDecorator parent = this;
        I_Damage deco = this;
        while(deco is I_DamageDecorator temp) {
            if(temp == decorator) {
                parent.damage = temp.damage;
                return temp;
            }
            parent=temp;
            height--;
            deco = temp.damage;
        }
        return null;
    }

    public override void OnItemAdded(I_Unit unit)
    {
        return;
    }

    public override void OnItemRemove(I_Unit unit)
    {
        return;
    }
}