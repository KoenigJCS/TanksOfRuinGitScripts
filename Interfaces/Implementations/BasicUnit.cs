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
        PlayerManager.inst.RemoveUnit(this);
        ItemManager.inst.playerItems.Remove(this.gameObject);
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
        AIManager.inst.HighlightTargets(this,true);
        TileManager.inst.GetTile(dest).unitOnTile=this;
    }

    void Start() {
        if(prePlacedUnInitiated) {
            Init();
            ItemManager.inst.playerItems.Add(this.gameObject);
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