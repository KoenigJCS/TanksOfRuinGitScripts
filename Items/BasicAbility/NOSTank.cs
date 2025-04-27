using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Settworks.Hexagons;
using System.Linq;


public class NOSTank : I_Ability
{
    public override DamageContext OnDamageEvent() {
        
        return damage.OnDamageEvent();
    }

    public override bool SetDamageParent(I_Damage n_damage) {
        
        if (n_damage.height >= 3)
            return false;

        height = n_damage.height + 1;
        damage = n_damage;
        return true;
    }

    public override void SetDamageBase(I_Damage n_base) {
        
        I_DamageDecorator deco = this;
        while (deco.damage is I_DamageDecorator subDeco) 
        {
            deco = subDeco;
        }
        deco.damage = n_base;
    }

    public override void OnItemAdded(I_Unit unit) {
        
        
    }

    public override void OnItemRemove(I_Unit unit) {
        
        
    }

    public override I_DamageDecorator RemoveDamageDeco(I_Damage decorator) {
        I_DamageDecorator parent = this;
        I_Damage current = this;

        while (current is I_DamageDecorator currentDeco)
        {
            if (currentDeco == decorator)
            {
                parent.damage = currentDeco.damage;
                return currentDeco;
            }
            parent = currentDeco;
            height--;
            current = currentDeco.damage;
        }
        return null;
    }

    public override void ActivateAbility(I_Unit unit) {

        if (!unit.NOSTankEmpty)
        {
            unit.NOSTankUsed = true;
            unit.NOSTankEmpty = true;
            unit.actionPoints = 1000;
            List<HexCoord> tiles = Pathfinding.FindReachableTiles(unit.hexPosition,unit.actionPoints,unit,unit.playerControlled).Keys.ToList();
            if(tiles.Count>1) {
                TileManager.inst.HighlightTiles(tiles);
            } else {
                TileManager.inst.HighlightTiles(null);
            }
        }
    }
}
