using UnityEngine;

public class GenericFlankDamageMod : BasicMod
{   
    [SerializeField] float flankMultiplier;

    private void Awake() {
        itemName = "+"+flankMultiplier + " Flank Multiplier";
        itemBlurb = "Extra Flanky";
        description = "Add "+flankMultiplier+ " flank multiplier";
    }

    private void OnValidate()
    {
        itemName = "+"+flankMultiplier + " Flank Multiplier";
        itemBlurb = "Extra Flanky";
        description = "Add "+flankMultiplier+ " flank multiplier";
    }
    public override DamageContext OnDamageEvent()
    {
        DamageContext dc = damage.OnDamageEvent();
        dc.flankMultplier += flankMultiplier;
        for (int i = 0; i < dc.damagePairs.Count; i++) {
            dc.damagePairs[i] = new(dc.damagePairs[i].damage,dc.damagePairs[i].damageType);
        }
        return dc;
    }
}