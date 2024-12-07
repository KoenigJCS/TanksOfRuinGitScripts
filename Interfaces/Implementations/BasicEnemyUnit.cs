using Settworks.Hexagons;
using UnityEngine;

public class BasicEnemyUnit : I_Unit
{
    public override void Die()
    {
        if(isOnDisplay) {
            UIManager.inst.SetDisplayUnit(null);
        }
        AIManager.inst.RemoveUnit(this);
        AIManager.inst.CheckTeamDead();
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
        if(prePlacedUnInitiated) {
            Init();
        }
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
        SoundManager.inst.PlayFire();
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