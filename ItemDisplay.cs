using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Microsoft.Unity.VisualStudio.Editor;

public class ItemDisplay : MonoBehaviour
{
    [SerializeField] GameObject holder;
    [SerializeField] TextMeshProUGUI itemName;
    [SerializeField] TextMeshProUGUI itemBlurb;
    [SerializeField] TextMeshProUGUI itemDetailsText;
    [SerializeField] GameObject unitModel;
    [SerializeField] I_Item displayItem;
    public GameObject displayItemGO;
    [SerializeField] bool isUnit = false;

    [Header("Debug")]
    [SerializeField] GameObject debugItem;
    // Start is called before the first frame update
    void Start()
    {
        Button btn = gameObject.GetComponent<Button>();
        btn.onClick.AddListener(() => UIManager.inst.SelectRewardItem(this));
        if(debugItem!=null) {
            SetDisplay(debugItem);
        } else {
            SetDisplay(ItemManager.inst.GenerateRandomItem());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isUnit) {
            unitModel.transform.Rotate(90 * Time.deltaTime * Vector3.up);
        }
    }

    public void SetDisplay(GameObject newDisplay) {
        displayItemGO = newDisplay;
        I_Item newDisplayItem = newDisplay.GetComponent<I_Item>();
        if(newDisplayItem is I_Unit newUnit) {
            SetDisplayUnit(newUnit);
        } else {
            SetDisplayItem(newDisplayItem);
        }
    }

    public void SetDisplayItem(I_Item newItem) {
        if(isUnit) {
            if(unitModel != null) {
                Destroy(unitModel);
            }
        } else {
            holder.GetComponent<UnityEngine.UI.Image>().sprite = null;
        }
        displayItem = newItem;
        isUnit=false;
        holder.GetComponent<UnityEngine.UI.Image>().color = new Color(255,255,255,255);
        itemName.text = "<color=\""+I_Item.rarityColor[displayItem.rarity]+"\">"+displayItem.itemName+"</color>";
        itemBlurb.text = displayItem.itemBlurb;
        itemDetailsText.text = displayItem.description;
        holder.GetComponent<UnityEngine.UI.Image>().sprite = displayItem.spriteImage;
    }

    public void SetDisplayUnit(I_Unit newDisplayUnit) {
        I_Unit displayUnit = null;
        if(isUnit) {
            displayUnit = (I_Unit)displayItem;
            if(newDisplayUnit == displayUnit && newDisplayUnit != null) {
                return;
            }
            if(displayUnit != null) {
                displayUnit.isOnDisplay = false;
                Destroy(unitModel);
            }
        } else {
            holder.GetComponent<UnityEngine.UI.Image>().sprite = null;
            holder.GetComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,0);
        }
        isUnit=true;
        displayUnit = newDisplayUnit;
        newDisplayUnit.isOnDisplay=true;
        unitModel = Instantiate(displayUnit.model,holder.transform);
        unitModel.layer = 11;
        unitModel.transform.localScale = Vector3.one*12;
        unitModel.transform.localPosition = unitModel.transform.localPosition+(Vector3.down*9);
        itemName.text = "<color=\""+I_Item.rarityColor[displayUnit.rarity]+"\">"+displayUnit.itemName+"</color>";
        itemBlurb.text = displayUnit.itemBlurb;
        itemDetailsText.text = displayUnit.description;
    }
}
