using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager inst;
    void Awake() {
        inst = this;
    }
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject certaintyMenu;
    [SerializeField] GameObject options;
    [SerializeField] GameObject inventory;
    [SerializeField] GameObject rewardScreen;
    [Header ("Detail Display Things")]
    [SerializeField] GameObject unitDetailDisplay;
    [SerializeField] Transform unitHolder;
    [SerializeField] TextMeshProUGUI unitName;
    [SerializeField] TextMeshProUGUI unitBlurb;
    [SerializeField] TextMeshProUGUI unitDetailsText;
    [SerializeField] GameObject unitModel;
    [SerializeField] I_Unit displayUnit;
    // Start is called before the first frame update
    void Start()
    {
        SetCertantyDisplay(false);
        SetMainMenu(false);
        SetDisplayUnit(null);
    }

    // Update is called once per frame
    void Update()
    {
        if(unitModel) {
            unitModel.transform.Rotate(90 * Time.deltaTime * Vector3.up);
            //This could be a source of lag
            unitDetailsText.text = "Health: " + Mathf.RoundToInt(displayUnit.health/displayUnit.maxHealth*100) +"%\n" +
                                    "Action Points: " + displayUnit.actionPoints+"/"+displayUnit.maxActionPoints;
        }
    }

    public void SetCertantyDisplay(bool value) {
        certaintyMenu.SetActive(value);
    }

    public void SetMainMenu(bool value) {
        mainMenu.SetActive(value);
    }
    public void SetInventory(bool value) {
        inventory.SetActive(value);
    }
    public void SetMainMenu() {
        mainMenu.SetActive(!mainMenu.activeSelf);
    }
    public void SetInventory() {
        inventory.SetActive(!inventory.activeSelf);
    }

    public void SetRewardScreen(bool active) {
        rewardScreen.SetActive(active);
    }

    public void SelectRewardItem(ItemDisplay itemDisplay) {
        if(itemDisplay.displayItemGO != null) {
            ItemManager.inst.ObtainRewardItem(itemDisplay.displayItemGO);
        }
        SetRewardScreen(false);
    }

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
        unitName.text = "<color=\""+I_Item.rarityColor[displayUnit.rarity]+"\">"+displayUnit.itemName+"</color>";
        unitBlurb.text = displayUnit.itemBlurb;
        unitDetailsText.text = "Health: " + Mathf.RoundToInt(displayUnit.health/displayUnit.maxHealth*100) +"%\n" +
                                "Action Points: " + displayUnit.actionPoints+"/"+displayUnit.maxActionPoints;
    }
}
