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
    public I_Unit owner { get => this; set{} }
    protected I_Ammo unitAmmo;
    public GameObject GetUnitAmmoGO() {
        return unitAmmo.gameObject;
    }
    protected I_Damage unitDamageDeco;
    public I_Mod GetUnitDamageDeco() {
        if(unitDamageDeco is I_Mod mod) {
            return mod;
        }
        return null;
    }
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
            AIManager.inst.ClearTargets();
            AIManager.inst.HighlightTargets(this,playerControlled);
        }
    }
    public HexCoord hexPosition;
    public int actionPoints;
    public int maxActionPoints;
    public int firePoints;
    public int maxFirePoints;
    public bool hasBeenHit;
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
        UIManager.inst.SetLayerAllChildren(model.transform,layer);
    }
    public float maxHealth;
    public bool playerControlled = true;
    public int weaponRange;
    public bool indirectFire = false;
    [Header("Debug")]
    public bool prePlacedUnInitiated = false;
    public int genNumberOfItems = 0;
    public void Init() {
        SetHealthBarVis(healthBarActiveState);
        actionPoints = maxActionPoints;
        health = maxHealth;
        firePoints = maxFirePoints;
        transform.position = new Vector3(hexPosition.Position().x,1,hexPosition.Position().y);
        if(playerControlled) {
            PlayerManager.inst.AddUnit(this);
        } else {
            AIManager.inst.AddUnit(this);
        }
        I_Tile currentTile = TileManager.inst.GetTile(hexPosition);
        if (currentTile != null)
        {
            currentTile.unitOnTile = this;
        }
        unitAmmo = Instantiate(ItemManager.inst.basicShell,items).GetComponent<I_Ammo>();
        unitAmmo.owner = this;
        unitDamageDeco = unitAmmo;
        for(int i = 0; i<genNumberOfItems;i++) {
            AddItem(ItemManager.inst.GenerateRandomItem(true));
        }

    }
    public abstract void Move(HexCoord dest);
    public abstract void Fire(I_Unit target);
    
    // TakeDamage function written by Tommy Smith
    public virtual void TakeDamage(I_Damage damage, I_Unit damageSource) {
        DamageContext damageContext = damage.OnDamageEvent();
        
        if (!hasBeenHit)
        {
            Vector3 direction = new Vector3(
                damageSource.transform.position.x - model.transform.position.x, 
                0, 
                damageSource.transform.position.z - model.transform.position.z).normalized;
            model.transform.forward = direction;
        }
        float damageScalar = IsFlanked(damageSource) ? damageContext.flankMultplier: 1f;
        foreach (var damagePart in damageContext.damagePairs)
        {
            health -= damagePart.damage * damageScalar;
            print("Damage is of Type: " + damagePart.damageType+" Multiplied by: "+damageScalar);
        }
        hasBeenHit = true;
    }
    public void SetHealthBarVis(bool state) {
        healthBarContainer.SetActive(state);
    }

    // IsFlanked function written by Tommy Smith
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
    public void RemoveItem(GameObject n_ItemGO) {
        if(n_ItemGO==null) {
            return;
        }
        I_Item n_Item = n_ItemGO.GetComponent<I_Item>(); 
        if(n_Item is I_Mod mod) {
            if((UnityEngine.Object)unitDamageDeco == unitAmmo) {
                return;
            }
            if(mod == (I_Mod)unitDamageDeco) {
                unitDamageDeco = mod.damage;
                ItemManager.inst.TakeBackItem(mod.gameObject);
                return;
            }
            I_DamageDecorator target = ((I_Mod)unitDamageDeco).RemoveDamageDeco(mod);
            if(target==null) {
                return;
            }
            ItemManager.inst.TakeBackItem(mod.gameObject);

        } else if (n_Item is I_Ammo ammo && ammo == unitAmmo) {
            ItemManager.inst.TakeBackItem(unitAmmo.gameObject);
            AddItem(Instantiate(ItemManager.inst.basicShell,items));
        }
    }

    public void AddItem(GameObject n_ItemGO) {
        I_Item n_Item = n_ItemGO.GetComponent<I_Item>(); 
        if(n_Item is I_Mod mod) {
            if(!mod.SetDamageParent(unitDamageDeco)) {
                ItemManager.inst.TakeBackItem(n_ItemGO);
                return;
            }
            n_Item.owner = this;
            mod.transform.parent = items;
            unitDamageDeco = mod;
        } else if (n_Item is I_Ammo ammo) {
            if(unitAmmo is BasicAmmo) {
                Destroy(unitAmmo.gameObject);
                unitAmmo = null;
            } else {
                ItemManager.inst.TakeBackItem(unitAmmo.gameObject);
            }
            if(unitDamageDeco is I_DamageDecorator deco) {
                deco.SetDamageBase(ammo);
            } else {
                unitDamageDeco = ammo;
            }
            n_Item.owner = this;
            unitAmmo = ammo;
            ammo.transform.parent = items;
        }
    }
}
