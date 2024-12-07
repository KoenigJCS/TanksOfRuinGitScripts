using System;
using System.Collections;
using System.Collections.Generic;

using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public abstract class I_Ammo : MonoBehaviour, I_Item, I_Damage
{
    [SerializeField] private float _baseDamage;
    public float baseDamage { get => _baseDamage; set => _baseDamage = value; }
    [SerializeField] private DamageType _damageType;
    public DamageType damageType { get => _damageType; set => _damageType = value; }
    [SerializeField] private Rarity _rarity;
    public Rarity rarity { get => _rarity; set => _rarity = value; }
    [SerializeField] Sprite _spriteImage;
    public Sprite spriteImage { get => _spriteImage; set => _spriteImage = value; }
    [SerializeField] string _description;
    public string description { get => _description; set => _description = value; }
    [SerializeField] string _itemName;
    public string itemName { get => _itemName; set => _itemName = value; }
    [SerializeField] string _itemBlurb;
    public string itemBlurb { get => _itemBlurb; set => _itemBlurb = value; }
    [SerializeField] I_Unit _owner;
    public I_Unit owner { get => _owner; set => _owner = value; }
    public int height { get => 0; set {} }

    public abstract DamageContext OnDamageEvent(); 
}
