using UnityEngine;

public class TreadUpgrade : I_Ability
{

    [SerializeField] private int bonusAP = 1;

    
    public override DamageContext OnDamageEvent()
    {
        return damage.OnDamageEvent();
    }
    
    public override bool SetDamageParent(I_Damage n_damage)
    {
        if (n_damage.height >= 3)
            return false;
        
        height = n_damage.height + 1;
        damage = n_damage;

        return true;
    }

    public override void SetDamageBase(I_Damage n_base)
    {
        I_DamageDecorator deco = this;
        while (deco.damage is I_DamageDecorator subDeco)
        {
            deco = subDeco;
        }
        deco.damage = n_base;
    }

    public override I_DamageDecorator RemoveDamageDeco(I_Damage decorator)
    {
        I_DamageDecorator parent = this;
        I_Damage current = this;

        while (current is I_DamageDecorator currentDeco)
        {
            if (currentDeco == decorator)
            {
                parent.damage = currentDeco.damage;
                return currentDeco;
            }
            parent = currentDeco;
            height--;
            current = currentDeco.damage;
        }
        return null;
    }

    public override void OnItemAdded(I_Unit unit)
    {
        unit.maxActionPoints += bonusAP;
        unit.actionPoints += bonusAP;
    }
    
    public override void OnItemRemove(I_Unit unit)
    {
        unit.maxActionPoints -= bonusAP;

        if (unit.actionPoints > unit.maxActionPoints)
            unit.actionPoints = unit.maxActionPoints;
    }

    public override void ActivateAbility(I_Unit unit)
    {
        Debug.Log("Treading they up or something");
    }
}

