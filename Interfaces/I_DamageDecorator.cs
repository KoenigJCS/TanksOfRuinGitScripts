using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface I_DamageDecorator : I_Damage
{
    public I_Damage damage {get; set;}
    public void SetDamageParent(I_Damage n_damage);
}
 