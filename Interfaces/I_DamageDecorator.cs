using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface I_DamageDecorator : I_Damage
{
    public I_Damage damage {get; set;}
    public bool SetDamageParent(I_Damage n_damage);
    public I_DamageDecorator RemoveDamageDeco(I_Damage decorator);
    public void SetDamageBase(I_Damage n_base);
}
 