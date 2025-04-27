using Settworks.Hexagons;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Cactus : BasicUnit
{
    public override void TakeDamage(I_Damage damage, I_Unit damageSource)
    {
        base.TakeDamage(damage, damageSource);
        if(damageSource is not Cactus _) { // Stopping Infinite Loop
            baseDamage/=2;
            Fire(damageSource);
            baseDamage*=2;
        }
    }
}