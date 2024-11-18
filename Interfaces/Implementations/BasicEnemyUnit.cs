using Settworks.Hexagons;
using UnityEngine;

public class BasicEnemyUnit : I_Unit
{
    public override void Die()
    {
        if(isOnDisplay) {
            UIManager.inst.SetDisplayUnit(null);
        }
        Destroy(this.gameObject);
    }

    public override void Move(HexCoord dest)
    {
        transform.position = new Vector3(dest.Position().x, 1, dest.Position().y);
        I_Tile newTile = TileManager.inst.GetTile(dest);
        if (newTile == null || newTile.unitOnTile != null)
        {
            return;
        }

        newTile.unitOnTile = this;
        I_Tile currentTile = TileManager.inst.GetTile(hexPosition);
        hexPosition = dest;

        if (currentTile != null)
        {
            currentTile.unitOnTile = null;
        }
    }


    void Start() {
        SetHealthBarVis(healthBarActiveState);
        playerControlled = false;
        weaponRange = 3;
        actionPoints = 4;
        maxActionPoints = 4;
        transform.position = new Vector3(hexPosition.Position().x,1,hexPosition.Position().y);
        I_Tile currentTile = TileManager.inst.GetTile(hexPosition);
        if (currentTile != null)
        {
            currentTile.unitOnTile = this;
        }
        AIManager.inst.AddUnit(this);
        unitAmmo = Instantiate(ItemManager.inst.basicShell,items).GetComponent<I_Ammo>();
        unitDamageDeco = unitAmmo;
        // AddItem(ItemManager.inst.GenerateRandomItem().GetComponent<I_Item>());
    }
    
    void Update() {
        if(visHealthBarTimer>0) {
            visHealthBarTimer-=Time.deltaTime;
            if(visHealthBarTimer<=0) {
                SetHealthBarVis(healthBarActiveState);
            }
        }
    }

    void OnDestroy() {
        AIManager.inst.RemoveUnit(this);
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
}