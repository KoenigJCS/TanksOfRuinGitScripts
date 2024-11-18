using System;
using System.Collections;
using System.Collections.Generic;
using Settworks.Hexagons;
using Unity.VisualScripting;
using UnityEngine;

public abstract class I_Unit : MonoBehaviour, I_Item
{
    [SerializeField] String _unitName = "Default Unit";
    [SerializeField] String _description = "A tank with standard action points and health.";
    [SerializeField] Rarity _rarity = Rarity.Common;
    public string itemName { get => _unitName; set => _unitName = value; }
    public Rarity rarity { get => _rarity; set => _rarity = value; }
    public Sprite spriteImage { get => null; set {} }
    public string description { get => _description; set => _description = value; }
    [SerializeField] string _itemBlurb = "The Most Basic Unit";
    public string itemBlurb { get => _itemBlurb; set => _itemBlurb = value; }
    protected I_Ammo unitAmmo;
    protected I_Damage unitDamageDeco;
    [SerializeField] protected GameObject healthBar;
    [SerializeField] protected GameObject healthBarContainer;
    public GameObject model;
    [SerializeField] protected Transform items;
    const float initalZScale = 0.78f;
    public bool isOnDisplay = false;
    [SerializeField] bool _isSelected;
    public bool isSelected {
        get {
            return _isSelected;
        }
        set {
            _isSelected = value;
            SetModelLayer(value ? 9 : 0); 
            healthBarActiveState = _isSelected;
            SetHealthBarVis(healthBarActiveState);
        }
    }
    public HexCoord hexPosition;
    public int actionPoints;
    public int maxActionPoints;
    public bool hasBeenHit;
    public float flankMultiplier = 1.25f;
    protected bool healthBarActiveState = false;
    public float visHealthBarTimer = 0f;
    [SerializeField] float _health;
    public float health {
        get {
            return _health;
        }
        set {
            _health = value;
            if(health<=0) {
                Die();
                return;
            } else if (_health>maxHealth) {
                _health = maxHealth;
            }
            healthBar.transform.localScale = new(healthBar.transform.localScale.x,healthBar.transform.localScale.y,initalZScale*health/maxHealth);
            healthBar.transform.localPosition = new(healthBar.transform.localPosition.x,healthBar.transform.localPosition.y,-1*(initalZScale/2)*((maxHealth-health)/maxHealth));
        }
    }
    public void SetModelLayer(int layer) {
        model.layer = layer;
    }
    public float maxHealth;
    public bool playerControlled = true;
    public int weaponRange;
    public bool indirectFire = false;
    public abstract void Move(HexCoord dest);
    public abstract void Fire(I_Unit target);
    public virtual void TakeDamage(I_Damage damage, I_Unit damageSource) {
        DamageContext damageContext = damage.OnDamageEvent();
        if (!hasBeenHit)
        {
            Vector3 direction = new Vector3(
                damageSource.transform.position.x - model.transform.position.x, 
                0, 
                damageSource.transform.position.z - model.transform.position.z).normalized;
            model.transform.forward = direction;
            hasBeenHit = true;
            foreach (var damagePart in damageContext.damagePairs)
            {
                health -= damagePart.damage;
                print("Damage is of Type:" + damagePart.damageType);
            }
        }else{
            if (IsFlanked(damageSource))
            {
                foreach (var damagePart in damageContext.damagePairs)
                {
                    health -= damagePart.damage * flankMultiplier;
                    print("FLANKED; Damage is of Type:"+damagePart.damageType);
                }
            }else{
                foreach (var damagePart in damageContext.damagePairs)
                {
                    health -= damagePart.damage;
                    print("Damage is of Type:" + damagePart.damageType);
                } 
            }
        }
    }
    public void SetHealthBarVis(bool state) {
        healthBarContainer.SetActive(state);
    }

    public bool IsFlanked(I_Unit attacker) {
        if (!hasBeenHit)
        {
            return false;
        }
        Vector3 currentTargetAngles = model.transform.eulerAngles;
        Vector3 currentAttackerAngles = attacker.model.transform.eulerAngles;
        float angleOne = Mathf.Max(currentAttackerAngles.y,currentTargetAngles.y);
        float angleTwo = Mathf.Min(currentAttackerAngles.y,currentTargetAngles.y)+180;

        float angleDifference = MathF.Abs(angleOne-angleTwo);
        while (angleDifference > 180) {
            angleDifference -= 180;
        }
        
        // print(angleDifference);

        return angleDifference >= 89.5f;
    }

    public abstract void Die();

    public void AddItem(I_Item n_Item) {
        if(n_Item is I_Mod mod) {
            mod.SetDamageParent(unitDamageDeco);
            mod.transform.parent = items;
            unitDamageDeco = mod;
        }
    }
}
