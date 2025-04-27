using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;
using System.Linq;

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
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] GameObject winScreen;
    public Sprite[] raritySprites; 
    [SerializeField] bool battleDone = false;
    
    [Header ("Detail Display Things")]
    [SerializeField] GameObject unitDetailDisplay;
    [SerializeField] Transform unitHolder;
    [SerializeField] TextMeshProUGUI unitName;
    [SerializeField] TextMeshProUGUI unitBlurb;
    [SerializeField] TextMeshProUGUI unitDetailsText;
    [SerializeField] GameObject unitModel;
    [SerializeField] I_Unit displayUnit;
    public EquipmentScreen equipmentScreen;
    [Header("Wiki")]
    [SerializeField] GameObject wikiMain;
    [SerializeField] GameObject defaultPage;
    [SerializeField] List<GameObject> wikiPages;
    [Header("Reward")]
    [SerializeField] Transform rewardContent;
    [SerializeField] GameObject itemRewardButton;
    [SerializeField] GameObject unitRewardButton;
    
    // Start is called before the first frame update
    void Start()
    {
        GUIStyle style = new GUIStyle ();
        style.richText = true;
        if(certaintyMenu) SetCertantyDisplay(false);
        if(mainMenu) SetMainMenu(false);
        if(unitDetailDisplay) SetDisplayUnit(null);
        if(equipmentScreen) equipmentScreen.CleanInvScreen();
        if(equipInventory) SetEquipInventory(false);
        if(unitInventory && equipInventory) SetInventory(false);
        if(wikiMain) {
            SetWiki(true); // This works ok
            ShowWikiPage("DefaultPage");
            SetWiki(false);
        }
        if(gameOverScreen) gameOverScreen.SetActive(false);
        if(winScreen) winScreen.SetActive(false);
        if(rewardScreen) rewardScreen.SetActive(false);
        // if(unitInventory && equipInventory) Invoke(nameof(FlickerStupidThingsIDKOk),1f);
    }

    void FlickerStupidThingsIDKOk() {
        SetInventory(true);
        ItemManager.inst.UpdateUnitDisplays();
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

            unitDetailsText.text = "Health: " + Mathf.RoundToInt(displayUnit.health) +"/"+ Mathf.RoundToInt(displayUnit.maxHealth) +"\n" +
                                    "Move: " + displayUnit.actionPoints+"/"+displayUnit.maxActionPoints + "\n"
                                    +"Fire: " + displayUnit.firePoints+"/"+displayUnit.maxFirePoints;
        }
        
    }

    [SerializeField] I_Item[] boundAbiitiyItems = new I_Item[3]{null,null,null};
    [SerializeField] I_Ability[] boundAbiities = new I_Ability[3]{null,null,null};
    [SerializeField] GameObject[] abilityButtons = new GameObject[3];
    [SerializeField] Image[] itemSprite = new Image[3];

    public void ActivateAbility(int id) {
        if(displayUnit==null || boundAbiities[id] == null) {
            return;
        }
        boundAbiities[id].ActivateAbility(displayUnit);
    }

    public void SetAbility(I_Item item) {
        if(boundAbiitiyItems.Contains(item)) {
            return;
        }
        int i = 0;
        for(;i<3;i++) {
            if(boundAbiitiyItems[i]==null) {
                break;
            }
        }
        if(i==3) {
            Debug.LogWarning("Slots Full");
            return;
        }
        
        boundAbiitiyItems[i] =item;
        try {
            boundAbiities[i] = (I_Ability)item;
        } catch (System.Exception) {
            Debug.LogError($"Item {item.itemName} is not abiltiy");
            return;
        }
        
        Image abilityImage = itemSprite[i];
        abilityImage.color = new Color(255,255,255,255);
        abilityImage.sprite = ((I_Mod)item).spriteImage;
    }

    public void RemoveAbility(I_Item item) {
        if(!boundAbiitiyItems.Contains(item)) {
            return;
        }
        int i = 0;
        for(;i<3;i++) {
            if(boundAbiitiyItems[i]==item) {
                break;
            }
        }
        
        boundAbiitiyItems[i] = null;
        boundAbiities[i] = null;
        Image abilityImage = itemSprite[i];
        abilityImage.color = new Color(0,0,0,0);
        abilityImage.sprite = null;
    }

    public void ClearAllAbilityIcons() {
        
    }
    
    public void SetCertantyDisplay(bool value) {
        if(battleDone) {
            return;
        }
        certaintyMenu.SetActive(value);
    }

    public void SetMainMenu(bool value) {
        if(battleDone) {
            return;
        }
        mainMenu.SetActive(value);
    }
    public void SetInventory(bool value) {
        if(battleDone) {
            return;
        }
        unitInventory.SetActive(value);
        if(value) {
            if(PlayerManager.inst) PlayerManager.inst.uILock = true;
            if(unitDetailDisplay) SetDisplayUnit(null);
            SetEquipInventory(false);
            ItemManager.inst.UpdateUnitDisplays();
        } else if(PlayerManager.inst){
            PlayerManager.inst.uILock = false;
        }
    }

    public void SetEquipInventory(bool value) {
        if(battleDone) {
            return;
        }
        equipInventory.SetActive(value);
        if(value) {
            unitInventory.SetActive(false);
            Invoke(nameof(StupidWrapper),.1f);
        } else {
            unitInventory.SetActive(true);
        }
    }

    void StupidWrapper() {
        equipmentScreen.CleanInvScreen();
    }
    public void SetMainMenu() {
        if(battleDone) {
            return;
        }
        SetInventory(false);
        mainMenu.SetActive(!mainMenu.activeSelf);
    }

    public void SetOptions() {
        options.SetActive(!options.activeSelf);
    }

    public void SetOptions(bool value) {
        options.SetActive(value);
    }

    public void SetWiki() {
        if(battleDone) {
            return;
        }
        wikiMain.SetActive(!wikiMain.activeSelf);
    }

    public void AddWiki(GameObject newWiki) {
        wikiPages.Add(newWiki);
    }

    public void SetWiki(bool value) {
        wikiMain.SetActive(value);
    }
    
    public void ShowWikiPage(string wikiID)
    {
        if(battleDone) {
            return;
        }
        defaultPage.SetActive(false);
        wikiPages.ForEach((page) => page.SetActive(false));

        GameObject newPage = wikiPages.Find(page => page.name == wikiID);
        if(newPage) {
            newPage.SetActive(true);
            return;
        }

        defaultPage.SetActive(true);
    }

    public void SetInventory() {
        SetInventory(!unitInventory.activeSelf);
    }

    public void GameOver() {
        SetInventory(false);
        SetWiki(false);
        SetCertantyDisplay(false);
        gameOverScreen.SetActive(true);
        battleDone=true;
    }

    public void GameWin() {

        SetInventory(false);
        SetWiki(false);
        SetCertantyDisplay(false);
        if(SaveManager.inst.playerSave.locationOrder<17) {
            SetupWinScreen();
        } else {
            SaveManager.inst.DeletePlayerSave(SaveManager.inst.globalSave.currentSave);
            GameDoneWin();
        }
        battleDone=true;
    }

    [SerializeField] GameObject gameDoneWin;

    public void GameDoneWin() {
        gameDoneWin.SetActive(true);
    }

    public void SetupWinScreen() {
        winScreen.SetActive(true);
        for(int i=0;i<SaveManager.inst.playerSave.currentDifficulty;i++) {
            Button temp1 = Instantiate(itemRewardButton,rewardContent).GetComponent<Button>();
            temp1.onClick.AddListener(() => SetItemRewardScreen(true));
            temp1.onClick.AddListener(() => Destroy(temp1.gameObject));
        }
        Button temp2 = Instantiate(unitRewardButton,rewardContent).GetComponent<Button>();
        temp2.onClick.AddListener(() => SetUnitRewardScreen(true));
        temp2.onClick.AddListener(() => Destroy(temp2.gameObject));
    }


    public void SetItemRewardScreen(bool active) {
        if(active) {
            foreach (ItemDisplay rewardDisplay in ItemManager.inst.rewardDisplays)
            {
                rewardDisplay.grabRandomItem=true;
                rewardDisplay.grabRandomUnit=false;
                rewardDisplay.GenRefresh();
            }
        }
        rewardScreen.SetActive(active);
    }
    public void SetUnitRewardScreen(bool active) {
        if(active) {
            foreach (ItemDisplay rewardDisplay in ItemManager.inst.rewardDisplays)
            {
                rewardDisplay.grabRandomItem=false;
                rewardDisplay.grabRandomUnit=true;
                rewardDisplay.GenRefresh();
            }
        }
        rewardScreen.SetActive(active);
    }

    public void SelectRewardItem(ItemDisplay itemDisplay) {
        if(itemDisplay.displayItemGO != null) {
            ItemManager.inst.ObtainRewardItem(itemDisplay.displayItemGO);
        }
        SetItemRewardScreen(false);
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
        if(!unitDetailDisplay) {
            return;
        }
        if(newDisplayUnit == displayUnit && newDisplayUnit != null) {
            return;
        }
        foreach (I_Item ability in boundAbiitiyItems) {
            if(ability!=null) RemoveAbility(ability);
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

        foreach (I_Item item in newDisplayUnit.unitItems) {
            if(item!=null && item is I_Ability) {
                SetAbility(item);
            }
        }

        newDisplayUnit.isOnDisplay=true;
        unitDetailDisplay.SetActive(true);
        unitModel = Instantiate(displayUnit.model,unitHolder);
        // unitModel.layer = 11;
        if(newDisplayUnit.playerControlled) {
            SoundManager.inst.PlayUnitVoiceLine();
        }
        SoundManager.inst.PlayUnitSelect();
        UIManager.inst.SetLayerAllChildren(unitModel.transform,11);
        unitModel.transform.localScale = Vector3.one*16;
        unitModel.transform.localPosition = unitModel.transform.localPosition+(Vector3.down*9);
        pivotpos = unitModel.transform.localPosition;
        unitName.text = "<color=\""+I_Item.rarityColor[displayUnit.rarity]+"\">"+displayUnit.itemName+"</color>";
        unitBlurb.text = displayUnit.itemBlurb;
        unitDetailsText.text = "Health: " + Mathf.RoundToInt(displayUnit.health) +"/"+ Mathf.RoundToInt(displayUnit.maxHealth) +"\n" +
                                "Move: " + displayUnit.actionPoints+"/"+displayUnit.maxActionPoints + "\n"
                                +"Fire: " + displayUnit.firePoints+"/"+displayUnit.maxFirePoints;
    }

    public void SetLayerAllChildren(Transform root, int layer)
    {
        var children = root.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (var child in children)
        {
//            Debug.Log(child.name);
            child.gameObject.layer = layer;
        }
    }
}
