using System;
using System.Collections;
using System.Collections.Generic;

using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public abstract class I_Mod : MonoBehaviour, I_Item, I_DamageDecorator
{
    [SerializeField] private Rarity _rarity;
    public Rarity rarity { get => _rarity; set => _rarity = value; }
    [SerializeField] Sprite _spriteImage;
    public Sprite spriteImage { get => _spriteImage; set => _spriteImage = value; }
    [SerializeField] string _description;
    public string description { get => _description; set => _description = value; }
    [SerializeField] I_Damage _damage;
    public I_Damage damage { get => _damage; set => _damage = value; }
    [SerializeField] string _itemName;
    public string itemName { get => _itemName; set => _itemName = value; }
    [SerializeField] string _itemBlurb;
    public string itemBlurb { get => _itemBlurb; set => _itemBlurb = value; }
    [SerializeField] int _itemID = -1;
    public int itemID { get => _itemID; set => _itemID = value; }
    [SerializeField] I_Unit _owner;
    public I_Unit owner { get => _owner; set => _owner = value; }
    [SerializeField] int _height;
    public int height {get => _height; set => _height = value; }

    public abstract DamageContext OnDamageEvent();
    public abstract bool SetDamageParent(I_Damage n_damage);
    public abstract void SetDamageBase(I_Damage n_base);
    public abstract void OnItemAdded(I_Unit unit);
    public abstract void OnItemRemove(I_Unit unit);
    public abstract I_DamageDecorator RemoveDamageDeco(I_Damage decorator);
}
