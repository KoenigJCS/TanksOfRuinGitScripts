using Settworks.Hexagons;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class BasicUnit : I_Unit
{
    public override void Die()
    {
        if(isOnDisplay) {
            UIManager.inst.SetDisplayUnit(null);
        }
        Destroy(this.gameObject);
    }

    public override void Move(HexCoord dest) {
        if(TileManager.inst.GetTile(hexPosition)==null) {
            return;
        }
        TileManager.inst.GetTile(hexPosition).unitOnTile=null;
        if(!TileManager.inst.IsOnTile(dest)) {
            return;
        }
        transform.position = new Vector3(dest.Position().x,1,dest.Position().y);
        hexPosition = dest;
        AIManager.inst.ClearTargets();
        AIManager.inst.HighlightTargets(this);
        TileManager.inst.GetTile(dest).unitOnTile=this;
    }

    void Start() {
        SetHealthBarVis(healthBarActiveState);
        playerControlled = true;
        weaponRange = 3;
        actionPoints = 4;
        maxActionPoints = 4;
        health = maxHealth;
        transform.position = new Vector3(hexPosition.Position().x,1,hexPosition.Position().y);
        PlayerManager.inst.AddUnit(this);
        I_Tile currentTile = TileManager.inst.GetTile(hexPosition);
        if (currentTile != null)
        {
            currentTile.unitOnTile = this;
        }
        unitAmmo = Instantiate(ItemManager.inst.basicShell,items).GetComponent<I_Ammo>();
        unitDamageDeco = unitAmmo;
        AddItem(ItemManager.inst.GenerateRandomItem().GetComponent<I_Item>());
    }

    void Update() {
        if(visHealthBarTimer>0) {
            visHealthBarTimer-=Time.deltaTime;
            if(visHealthBarTimer<=0) {
                SetHealthBarVis(healthBarActiveState);
            }
        }
    }

    public override void Fire(I_Unit target) {
        Vector3 enemyDirection = new Vector3(
            target.transform.position.x - transform.position.x, 
            0, 
            target.transform.position.z - transform.position.z).normalized;
        model.transform.forward = enemyDirection;
        target.TakeDamage(CreateDamage(),this);
        target.SetHealthBarVis(true);
        target.visHealthBarTimer = 2.5f;
    }

    public I_Damage CreateDamage() {
        if(unitAmmo == null) {
            Debug.LogError("No Ammo In Unit");
        }
        return unitDamageDeco;
    }

    void OnDestroy()
    {
        PlayerManager.inst.RemoveUnit(this);
    }
}