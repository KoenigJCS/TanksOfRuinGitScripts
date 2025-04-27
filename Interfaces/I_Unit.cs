using System;
using System.Collections;
using System.Collections.Generic;
using Settworks.Hexagons;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public delegate void AbilityInvocation(I_Item item, I_Unit unit);

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
    [SerializeField] int _itemID = -1;
    public int itemID { get => _itemID; set => _itemID = value; }
    [SerializeField] int _unitID = -1;
    public int unitID { get => _unitID; set => _unitID = value; }
    [SerializeField] int _baseDamage = 25;
    public int baseDamage { get => _baseDamage; set => _baseDamage = value; }
    public float damageFalloff = .15f;
    public I_Unit owner { get => this; set{} }
    public List<MeshRenderer> meshes = new();
    public bool isDead = false;
    protected I_Ammo unitAmmo;
    public GameObject GetUnitAmmoGO() {
        if(unitAmmo==null) {
            unitAmmo = Instantiate(ItemManager.inst.basicShell,items).GetComponent<I_Ammo>();
            unitAmmo.owner = this;
            unitDamageDeco = unitAmmo;
        }
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
            if(firePoints>0) {
                AIManager.inst.HighlightTargets(this,playerControlled);
            }
        }
    }
    public List<Transform> fireTransforms;
    public HexCoord hexPosition;
    public int actionPoints;
    public int maxActionPoints;
    public int firePoints;
    public int maxFirePoints;
    public bool reloaderPresent = false;
    public bool repairmanPresent = false;
    public bool extraEyesPresent = false;
    public bool thornsPresent = false;
    public bool frozen = false;
    public int radiationCounter = 0;
    public bool shrapnelJam = false;
    public bool hasBeenHit;
    public Quaternion targetRotation;
    public bool isRotating = false;
    protected bool healthBarActiveState = false;
    public bool NOSTankUsed = false;
    public bool NOSTankEmpty = false;
    public bool roidsEmpty = false;
    public bool smiteEmpty = false;
    public bool smiteUsed = false;
    public float visHealthBarTimer = 0f;
    [SerializeField] float _health;
    public float health {
        get {
            return _health;
        }
        set {
            _health = value;
            if(health <=0){
                visHealthBarTimer=0;
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
        if(unitAmmo==null) {
            unitAmmo = Instantiate(ItemManager.inst.basicShell,items).GetComponent<I_Ammo>();
            unitAmmo.owner = this;
            unitDamageDeco = unitAmmo;
        }
        for(int i = 0; i<genNumberOfItems;i++) {
            AddItem(ItemManager.inst.GenerateRandomItem(true));
        }
    }
    public abstract void OnTurnEnd();
    public abstract bool Move(HexCoord dest);
    public abstract void Fire(I_Unit target);
    
    public void RotateTowards(I_Unit target)
    {
        if (!hasBeenHit)
        {
            Vector3 enemyDirection = new Vector3(
                target.transform.position.x - transform.position.x, 
                0, 
                target.transform.position.z - transform.position.z).normalized;

            targetRotation = Quaternion.LookRotation(enemyDirection);
            isRotating = true;
        }
    }
    public virtual void TakeDamage(I_Damage damage) {
        // This is for mines only atm
        DamageContext dc = damage.OnDamageEvent();
        foreach (var damagePart in dc.damagePairs) {
            health -= damagePart.damage;
        }
    }

    public virtual void TakeDamage(I_Damage damage, I_Unit damageSource) {
        DamageContext damageContext = damage.OnDamageEvent();
        I_Tile tile = TileManager.inst.GetTile(hexPosition);
        float tileProtection = 1f;
        if(tile!=null) {
            tileProtection = tile.CheckProtection(damage,damageSource);
        }
        float damageScalar = IsFlanked(damageSource) ? damageContext.flankMultplier: 1f;
        float damageDiscount = 1-(damageSource.damageFalloff*(HexCoord.Distance(damageSource.hexPosition,hexPosition)-1));
        if(damageDiscount<.1f) {
            damageDiscount=.1f;
        }
        foreach (var damagePart in damageContext.damagePairs)
        {
            if (damagePart.damageType == DamageType.Cryo)
            {
                frozen = true;
            }
            if (damagePart.damageType == DamageType.Nuclear)
            {
                radiationCounter = 3;
            }
            if (damagePart.damageType == DamageType.Shrapnel)
            {
                if (Random.value <= 0.2f)
                {
                    shrapnelJam = true;
                }
            }
            if (damagePart.damageType == DamageType.HighExplosive)
            {
                List<I_Unit> adjacentUnits = TileManager.inst.GetUnitsOnNeighboringHexes(hexPosition);
                foreach (I_Unit neighbor in adjacentUnits)
                {
                    if (neighbor != null && neighbor != this)
                    {
                        neighbor.health -= damagePart.damage * 0.5f;
                        neighbor.SetHealthBarVis(true);
                        neighbor.visHealthBarTimer = 2.5f;
                    }
                }
            }
            float total = damagePart.damage * damageScalar * damageDiscount * damageSource.baseDamage * tileProtection;
            health-= total;
            print("Damage of"+damagePart.damage+" is of Type: " + damagePart.damageType+" Multiplied by: "+damageScalar+" and "+tileProtection+" Discounted by: "+damageDiscount+" Totaling: "+ total);
        }
        hasBeenHit = true;
    }
    public void SetHealthBarVis(bool state) {
        healthBarContainer.SetActive(state);
    }

    public bool IsFlanked(I_Unit attacker) {
        if (!hasBeenHit || extraEyesPresent)
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
                mod.OnItemRemove(this);
                unitItems.Remove(n_Item);
                return;
            }
            I_DamageDecorator target = ((I_Mod)unitDamageDeco).RemoveDamageDeco(mod);
            if(target==null) {
                return;
            }
            ItemManager.inst.TakeBackItem(mod.gameObject);

        } else if (n_Item is I_Ammo ammo && ammo == unitAmmo) {
            unitItems.Remove(n_Item);
            ammo.OnAmmoRemoved(this);
            unitItems.Add(n_Item);
            ItemManager.inst.TakeBackItem(unitAmmo.gameObject);
            AddItem(Instantiate(ItemManager.inst.basicShell,items));
        }
    }

    public List<I_Item> unitItems = new(); 

    public void AddItem(GameObject n_ItemGO) {
        if(unitAmmo==null) {
            unitAmmo = Instantiate(ItemManager.inst.basicShell,items).GetComponent<I_Ammo>();
            unitAmmo.owner = this;
            unitDamageDeco = unitAmmo;
        }
        I_Item n_Item = n_ItemGO.GetComponent<I_Item>(); 
        if(n_Item is I_Mod mod) {
            if(!mod.SetDamageParent(unitDamageDeco)) {
                ItemManager.inst.TakeBackItem(n_ItemGO);
                return;
            }
            n_Item.owner = this;
            mod.transform.parent = items;
            unitItems.Add(n_Item);
            mod.OnItemAdded(this);
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
            ammo.OnAmmoAdded(this);
            unitItems.Add(n_Item);
        }
    }
}
