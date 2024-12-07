using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class UIManager : MonoBehaviour
{
    public static UIManager inst;
    void Awake() {
        inst = this;
    }
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject certaintyMenu;
    [SerializeField] GameObject options;
    [SerializeField] GameObject unitInventory;
    [SerializeField] GameObject equipInventory;
    [SerializeField] GameObject rewardScreen;
    [Header ("Detail Display Things")]
    [SerializeField] GameObject unitDetailDisplay;
    [SerializeField] Transform unitHolder;
    [SerializeField] TextMeshProUGUI unitName;
    [SerializeField] TextMeshProUGUI unitBlurb;
    [SerializeField] TextMeshProUGUI unitDetailsText;
    [SerializeField] GameObject unitModel;
    [SerializeField] I_Unit displayUnit;
    public EquipmentScreen equipmentScreen;
    // Start is called before the first frame update
    void Start()
    {
        SetCertantyDisplay(false);
        SetMainMenu(false);
        SetDisplayUnit(null);
        SetEquipInventory(false);
        SetInventory(false);
    }

    // Update is called once per frame
    void Update()
    {
        // float t = 0;
        if(unitModel) {
            unitModel.transform.Rotate(90 * Time.deltaTime * Vector3.up);
            // t+=Time.deltaTime*75;
            // if(t>360f) {
            //     t=0;
            // }
            // unitModel.transform.localPosition = pivotpos + (Vector3.up*Mathf.Sin(Mathf.Deg2Rad*t)*20);
            //This could be a source of lag
                    unitDetailsText.text = "Health: " + Mathf.RoundToInt(displayUnit.health/displayUnit.maxHealth*100) +"%\n" +
                                "Action Points: " + displayUnit.actionPoints+"/"+displayUnit.maxActionPoints + "\n"
                                +"Fire Points: " + displayUnit.firePoints+"/"+displayUnit.maxFirePoints;
        }
    }

    public void EndSceneVictory() {
        SetRewardScreen(true);
    }

    public void SetCertantyDisplay(bool value) {
        certaintyMenu.SetActive(value);
    }

    public void SetMainMenu(bool value) {
        mainMenu.SetActive(value);
    }
    public void SetInventory(bool value) {
        if(value) {
            PlayerManager.inst.uILock = true;
            SetDisplayUnit(null);
            ItemManager.inst.UpdateUnitDisplays();
        } else {
            PlayerManager.inst.uILock = false;
        }
        unitInventory.SetActive(value);
    }

    private void SetEquipInventory(bool value) {
        equipInventory.SetActive(value);
        if(value) {
            unitInventory.SetActive(false);
        } else {
            unitInventory.SetActive(true);
        }
    }

    public void SetMainMenu() {
        mainMenu.SetActive(!mainMenu.activeSelf);
    }

    public void SetOptions() {
        options.SetActive(!options.activeSelf);
    }

    public void SetOptions(bool value) {
        options.SetActive(value);
    }
    public void SetInventory() {
        SetInventory(!unitInventory.activeSelf);
    }

    public void SetRewardScreen(bool active) {
        if(active) {
            foreach (ItemDisplay rewardDisplay in ItemManager.inst.rewardDisplays)
            {
                rewardDisplay.RefreshText();
            }
        }
        rewardScreen.SetActive(active);
    }

    public void SelectRewardItem(ItemDisplay itemDisplay) {
        if(itemDisplay.displayItemGO != null) {
            ItemManager.inst.ObtainRewardItem(itemDisplay.displayItemGO);
        }
        SetRewardScreen(false);
    }

    public void DisplayUnitEquip(ItemDisplay itemDisplay) {
        if(itemDisplay.displayItemGO == null) {
            return;
        }
        if(itemDisplay.displayItemGO.GetComponent<I_Item>() is I_Unit unit) {
            SetEquipInventory(true);
            equipmentScreen.SetEquipUnit(unit);
        }
    }
    Vector3 pivotpos;

    public void SetDisplayUnit(I_Unit newDisplayUnit) {
        if(newDisplayUnit == displayUnit && newDisplayUnit != null) {
            return;
        }
        if(displayUnit) {
            displayUnit.isOnDisplay = false;
            Destroy(unitModel);
            unitModel = null;
        }
        displayUnit = newDisplayUnit;
        if(!newDisplayUnit) {
            unitDetailDisplay.SetActive(false);
            return;
        } 
        newDisplayUnit.isOnDisplay=true;
        unitDetailDisplay.SetActive(true);
        unitModel = Instantiate(displayUnit.model,unitHolder);
        unitModel.layer = 11;
        unitModel.transform.localScale = Vector3.one*16;
        unitModel.transform.localPosition = unitModel.transform.localPosition+(Vector3.down*9);
        pivotpos = unitModel.transform.localPosition;
        unitName.text = "<color=\""+I_Item.rarityColor[displayUnit.rarity]+"\">"+displayUnit.itemName+"</color>";
        unitBlurb.text = displayUnit.itemBlurb;
        unitDetailsText.text = "Health: " + Mathf.RoundToInt(displayUnit.health/displayUnit.maxHealth*100) +"%\n" +
                                "Action Points: " + displayUnit.actionPoints+"/"+displayUnit.maxActionPoints + "\n"
                                +"Fire Points: " + displayUnit.firePoints+"/"+displayUnit.maxFirePoints;
    }
}
