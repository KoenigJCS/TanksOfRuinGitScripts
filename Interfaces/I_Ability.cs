using System;
using System.Collections;
using System.Collections.Generic;

using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public abstract class I_Ability: I_Mod
{
    public override abstract DamageContext OnDamageEvent();
    public override abstract bool SetDamageParent(I_Damage n_damage);
    public override abstract void SetDamageBase(I_Damage n_base);
    public override abstract void OnItemAdded(I_Unit unit);
    public override abstract void OnItemRemove(I_Unit unit);
    public override abstract I_DamageDecorator RemoveDamageDeco(I_Damage decorator);
    public abstract void ActivateAbility(I_Unit unit);
}
