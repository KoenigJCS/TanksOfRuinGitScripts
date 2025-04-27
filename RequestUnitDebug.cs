using System;
using System.Collections;
using System.Collections.Generic;
using Settworks.Hexagons;
using UnityEngine;

[Serializable]
public class RequestUnitDebug : MonoBehaviour
{
    [SerializeField] int unitItemId;
    [SerializeField] HexCoord unitPosition;
    [SerializeField] bool isPlayerControlled;
    [SerializeField] int pregenItems = 0;
    [SerializeField] bool firstLevelOnly = true;
    // Start is called before the first frame update
    void Start()
    {
        if(SaveManager.inst.playerSave.locationOrder>0 && isPlayerControlled && firstLevelOnly) {
            Destroy(gameObject);
            return;
        }
        I_Unit unit = ItemManager.inst.GenerateUnit(new(unitItemId,-1,1),isPlayerControlled);
        unit.hexPosition=unitPosition;
        for(int i=0;i<pregenItems;i++) {
            ItemManager.inst.GenerateRandomItem(true);
        }
        Destroy(gameObject);
    }

}
