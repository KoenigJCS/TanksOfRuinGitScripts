using UnityEngine;

public class GenericDamageMod : BasicMod
{   
    [SerializeField] float boostAmmount;

    private void Awake() {
        itemName = boostAmmount + "x Multiplier";
        itemBlurb = "Extra Punch";
        description = "Multiply all damage by"+ boostAmmount;
    }

    void OnValidate()
    {
        itemName = boostAmmount + "x Multiplier";
        itemBlurb = "Extra Punch";
        description = "Multiply all damage by"+ boostAmmount;
    }
    public override DamageContext OnDamageEvent()
    {
        DamageContext dc = damage.OnDamageEvent();
        for (int i = 0; i < dc.damagePairs.Count; i++) {
            dc.damagePairs[i] = new(dc.damagePairs[i].damage*boostAmmount,dc.damagePairs[i].damageType);
        }
        return dc;
    }
}