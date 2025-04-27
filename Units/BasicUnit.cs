using Settworks.Hexagons;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class BasicUnit : I_Unit
{
    public override void Die()
    {
        isDead=true;
        // Debug.Log($"{name} is dead");
        if(isOnDisplay) {
            PlayerManager.inst.SelectUnit(null);
            UIManager.inst.SetDisplayUnit(null);
        }
        Transform deathPoint = transform;
        if (deathPoint) {
            GameObject explosion = Instantiate(SoundManager.inst.tankExplodeEffect, deathPoint.position, deathPoint.rotation);
            Destroy(explosion, 3.0f);
        }
        if(playerControlled) {
            PlayerManager.inst.RemoveUnit(this);
            ItemManager.inst.playerUnits.Remove(gameObject);
            PlayerManager.inst.CheckGameOver();
        } else {
            AIManager.inst.RemoveUnit(this);
            AIManager.inst.CheckWin();
        }
        I_Tile tile = TileManager.inst.GetTile(hexPosition);
        if(tile) {
            tile.unitOnTile=null;
        }
        hexPosition=new(999,999);
        transform.position = Vector3.up*999;
        // Destroy(this.gameObject);
    }

    public override bool Move(HexCoord dest) {
        if(TileManager.inst.GetTile(hexPosition)==null) {
            return false;
        }
        if(!TileManager.inst.IsOnTile(dest) || TileManager.inst.GetTile(dest).unitOnTile != null) {
            return false;
        }
        TileManager.inst.GetTile(hexPosition).unitOnTile=null;
        transform.position = new Vector3(dest.Position().x,1,dest.Position().y);
        hexPosition = dest;
        I_Tile destTile = TileManager.inst.GetTile(dest);
        destTile.unitOnTile=this;
        destTile.OnStopOn(this);
        
        if(playerControlled) {
            AIManager.inst.ClearTargets();
            if(firePoints>0) {
                AIManager.inst.HighlightTargets(this,true);
            }
        }
        return true;
    }

    void Start() {
        // if(prePlacedUnInitiated) {
        //     Init();
        //     if(playerControlled) {
        //         ItemManager.inst.playerUnits.Add(this.gameObject);
        //         this.unitID=ItemManager.inst.playerUnits.Count-1;
        //     }
        // }
    }

    void Update() {
        if(health<=0 && !isDead) {
            Die();
        }
        if(visHealthBarTimer>0) {
            visHealthBarTimer-=Time.deltaTime;
            if(visHealthBarTimer<=0) {
                SetHealthBarVis(healthBarActiveState);
            }
        }
        if (isRotating) {
            model.transform.rotation = Quaternion.Slerp(model.transform.rotation, targetRotation, 5.0f * Time.deltaTime);

            if (Quaternion.Angle(model.transform.rotation, targetRotation) < 0.1f) {
                model.transform.rotation = targetRotation;
                isRotating = false;
            }
        }
    }

    public override void Fire(I_Unit target) {
        foreach (Transform firePoint in fireTransforms) {
            GameObject explosion = Instantiate(SoundManager.inst.tankFireEfect, firePoint.position, firePoint.rotation);
            Destroy(explosion, 1.0f);
        }
        SoundManager.inst.PlayFire();
        if (smiteUsed)
        {
            smiteUsed = false;
            target.Die();
        }
        else
        {
            target.TakeDamage(CreateDamage(),this);
            if (target.thornsPresent)
                health -= 10;
        }
        target.SetHealthBarVis(true);
        target.visHealthBarTimer = 2.5f;
    }

    public I_Damage CreateDamage() {
        if(unitAmmo == null) {
            Debug.LogError("No Ammo In Unit");
        }
        return unitDamageDeco;
    }

    public override void OnTurnEnd() {
        return;
    }
}