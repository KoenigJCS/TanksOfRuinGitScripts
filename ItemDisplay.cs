using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.VisualScripting;

public enum ItemDisplayType
{
    Reward,
    UnitSelector,
    InventoryDisplay,
    ModSlot,
    None
}

public class ItemDisplay : MonoBehaviour, IComparable<ItemDisplay>
{
    [SerializeField] GameObject holder;
    [SerializeField] GameObject subFrame;
    [SerializeField] TextMeshProUGUI itemName;
    [SerializeField] TextMeshProUGUI itemBlurb;
    [SerializeField] TextMeshProUGUI itemDetailsText;
    [SerializeField] GameObject unitModel;
    [SerializeField] I_Item displayItem;
    public GameObject displayItemGO;
    [SerializeField] bool isUnit = false;
    //Dont Set this
    public ItemDisplayType type = ItemDisplayType.InventoryDisplay;
    [SerializeField] UnityEngine.UI.Image frameImage;
    [SerializeField] UnityEngine.UI.Image rarityImage;
    
    [SerializeField] bool _isSelected;
    public bool isSelected {
        get {
            return _isSelected;
        }
        set {
            _isSelected = value;
            if(value) {
                frameImage.color = new(255,0,0);
            } else {
                frameImage.color = new(255,255,255);
            }
        }
    }
    public int order = 0;
    [Header("Debug")]
    [SerializeField] GameObject debugItem;
    public bool grabRandomItem = false;
    public bool grabRandomUnit = false;
    void Start() {
        Button btn = gameObject.GetComponent<Button>();
        switch (type)
        {
            case ItemDisplayType.Reward:
                btn.onClick.AddListener(() => UIManager.inst.SelectRewardItem(this));
                ItemManager.inst.rewardDisplays.Add(this);
                break;
            case ItemDisplayType.UnitSelector:
                btn.onClick.AddListener(() => UIManager.inst.DisplayUnitEquip(this));
                ItemManager.inst.unitDisplays.Add(this);
                break;
            case ItemDisplayType.InventoryDisplay:
                btn.onClick.AddListener(() => UIManager.inst.equipmentScreen.HandleItemSwap(this));
                break;
            case ItemDisplayType.ModSlot:
                btn.onClick.AddListener(() => UIManager.inst.equipmentScreen.HandleItemSwap(this));
                break;
            default:

                break;
        }

        if(debugItem!=null) {
            SetDisplay(debugItem);
        } else {
            SetDisplay(null);
        }
    }
    public void GenRefresh() {
        if(grabRandomItem) {
            SetDisplay(ItemManager.inst.GenerateRandomItem());
        } else {
            SetDisplay(ItemManager.inst.GenerateRandomUnit());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(unitModel!=null) {
            unitModel.transform.Rotate(90 * Time.deltaTime * Vector3.up);
        }
    }

    public void SetDisplay(GameObject newDisplay) {
        if(newDisplay == null) {
            Cleanup();
            itemName.text = "";
            itemBlurb.text = "";
            itemDetailsText.text = "";
            displayItemGO=null;
            rarityImage.sprite=null;
            rarityImage.color =new(0,0,0,0);
            return;
        }
        displayItemGO = newDisplay;
        I_Item newDisplayItem = newDisplay.GetComponent<I_Item>();
        rarityImage.color =new(255,255,255,255);
        if(newDisplayItem is I_Unit newUnit) {
            SetDisplayUnit(newUnit);
        } else {
            SetDisplayItem(newDisplayItem);
        }
    }

    private void Cleanup() {
        if(!isUnit) {
            holder.GetComponent<UnityEngine.UI.Image>().sprite = null;
            holder.GetComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,0);
            subFrame.GetComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,0);
        } else if (displayItem != null && displayItem is I_Unit unit) {
            unit.isOnDisplay = false;
        }
        if(unitModel != null) {
            Destroy(unitModel);
            unitModel=null;
        }

    }

    //These are private for a reason
    private void SetDisplayItem(I_Item newItem) {
        Cleanup();
        displayItem = newItem;
        isUnit=false;
        holder.GetComponent<UnityEngine.UI.Image>().color = new Color(255,255,255,255);
        if(newItem is I_Mod _) {
            subFrame.GetComponent<UnityEngine.UI.Image>().color = new Color(255,255,255,255);
        } else {
            subFrame.GetComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,0);
        }
        itemName.text = "<color=\""+I_Item.rarityColor[displayItem.rarity]+"\">"+displayItem.itemName+"</color>";
        rarityImage.sprite=UIManager.inst.raritySprites[(int)displayItem.rarity];
        if(type != ItemDisplayType.InventoryDisplay) {
            itemBlurb.text = displayItem.itemBlurb;
        } else {
            itemBlurb.text = "";
        }
        itemDetailsText.text = displayItem.description;
        holder.GetComponent<UnityEngine.UI.Image>().sprite = displayItem.spriteImage;
    }

    //These are priavte for a reason
    private void SetDisplayUnit(I_Unit newDisplayUnit) {
        I_Unit displayUnit = null;
        if(isUnit) {
            displayUnit = (I_Unit)displayItem;
            if(newDisplayUnit == displayUnit && newDisplayUnit != null) {
                return;
            }
        } 
        Cleanup();

        isUnit=true;
        displayUnit = newDisplayUnit;
        newDisplayUnit.isOnDisplay=true;
        unitModel = Instantiate(displayUnit.model,holder.transform);
        // unitModel.layer = 11;
        UIManager.inst.SetLayerAllChildren(unitModel.transform,11);
        unitModel.transform.localScale = Vector3.one*12;
        unitModel.transform.localPosition = unitModel.transform.localPosition+(Vector3.down*9);
        itemName.text = "<color=\""+I_Item.rarityColor[displayUnit.rarity]+"\">"+displayUnit.itemName+"</color>";
        rarityImage.sprite=UIManager.inst.raritySprites[(int)displayUnit.rarity];
        itemBlurb.text = displayUnit.itemBlurb;
        itemDetailsText.text = displayUnit.description;
    }



    public void RefreshText() {
        itemName.text = "<color=\""+I_Item.rarityColor[displayItem.rarity]+"\">"+displayItem.itemName+"</color>";
        itemBlurb.text = displayItem.itemBlurb;
        itemDetailsText.text = displayItem.description;
    }

    public int CompareTo(ItemDisplay comparePart)
    {
          // A null value means that this object is greater.
        if (comparePart == null)
            return 1;
        if(this.order == comparePart.order)
            return 0;
        else
            return this.order > comparePart.order ? 1 : -1;
    }
}
