using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.Rendering;

public class EquipmentScreen : MonoBehaviour
{
    public I_Unit equipUnit;
    [SerializeField] ItemDisplay mainDisplay;
    [SerializeField] ItemDisplay baseAmmoFrame;
    [SerializeReference ]List<ItemDisplay> modFrames = new();
    [SerializeField] ItemDisplay gloablInvItem;
    public void HandleItemSwap(ItemDisplay unitInvItem) {
        CleanInvScreen();
        void EndItemSwap() {
            gloablInvItem.isSelected=false;
            unitInvItem.isSelected=false;
            gloablInvItem=null;
            CleanInvScreen();
        }
        if(gloablInvItem==null) {
            gloablInvItem = unitInvItem;
            gloablInvItem.isSelected=true;
            return;
        }
        if(gloablInvItem.type == ItemDisplayType.ModSlot) {
            ItemDisplay temp = gloablInvItem;
            gloablInvItem = unitInvItem;
            unitInvItem = temp;
        }
        // try {
        //     print("Gobal: "+gloablInvItem.displayItemGO.name);
        // } catch (System.Exception exept) {
        //     print("Global Null+"+exept.ToString());
        // }

        // try {
        //     print(" Unit: "+unitInvItem.displayItemGO.name);
        // } catch (System.Exception exept) {
        //     print("Unit Null"+exept.ToString());
        // }
        
        if(equipUnit==null || gloablInvItem.type == unitInvItem.type) {
            EndItemSwap();
            return;
        } else if(unitInvItem.displayItemGO && gloablInvItem.displayItemGO==null && unitInvItem.displayItemGO.TryGetComponent<BasicAmmo>(out _) ) {
            EndItemSwap();
            return;
        } else if(gloablInvItem.displayItemGO && ((modFrames.Contains(unitInvItem) && gloablInvItem.displayItemGO.TryGetComponent<I_Ammo>(out _)) || (baseAmmoFrame == unitInvItem && gloablInvItem.displayItemGO.TryGetComponent<I_Mod>(out _)))) {
            EndItemSwap();
            return;
        }

        if(unitInvItem.displayItemGO!=null) {
            equipUnit.RemoveItem(unitInvItem.displayItemGO);
        }
        if(gloablInvItem.displayItemGO!=null) {
            equipUnit.AddItem(gloablInvItem.displayItemGO);
        }
        EndItemSwap();
    }

    public void CleanInvScreen() {
        foreach(ItemDisplay itemFrame in ItemManager.inst.invDisplays) {
            itemFrame.SetDisplay(null);
        }

        
        // Debug.Log($"D Count: {ItemManager.inst.invDisplays.Count}");
        // Debug.Log($"P Count: {ItemManager.inst.playerItems.Count}");
        for (int i=0,j=0 ;i<ItemManager.inst.playerItems.Count && j<ItemManager.inst.invDisplays.Count;i++) {
            //This is laggy and bad
            if(ItemManager.inst.playerItems[i].GetComponent<I_Item>().owner==null) {
                ItemManager.inst.invDisplays[j++].SetDisplay(ItemManager.inst.playerItems[i]);
            }
        }
        if(equipUnit==null) {
            return;
        } 
        mainDisplay.SetDisplay(equipUnit.gameObject);
        baseAmmoFrame.SetDisplay(equipUnit.GetUnitAmmoGO());
        I_Mod unitDeco = equipUnit.GetUnitDamageDeco();
        List<I_Mod> mods = new();
        if(unitDeco!=null && unitDeco is I_Mod subMod) {
            mods.Add(subMod);
            while(subMod.damage is I_Mod temp) {
                subMod= temp;
                mods.Add(temp);
            }
        }
        foreach(ItemDisplay modFrame in modFrames) {
            modFrame.SetDisplay(null);
        }

        for (int i =0; i<mods.Count && i<modFrames.Count; i++ )
        {
            modFrames[i].SetDisplay(mods[i].gameObject);
        }
    }

    public void SetEquipUnit(I_Unit newUnit) {
        if(newUnit==null) {
            return;
        }
        equipUnit = newUnit;
        CleanInvScreen();
    }
}
