using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Random = System.Random;

public class ItemManager : MonoBehaviour
{
    public static ItemManager inst;
    void Awake() {
        inst = this;
    }
    public GameObject basicShell;
    public GameObject basicUnit;
    
    // [SerializeField] SerializedDictionary<Rarity, ListWrapper> itemPrefabStorage;
    [SerializeField] Matrix<GameObject> itemPrefabs;
    [SerializeField] Matrix<GameObject> unitPrefabs;
    [SerializeField] Transform itemBucket;
    [SerializeField] Transform unitBucket;
    Random random;

    public List<GameObject> playerItems = new();
    public List<GameObject> playerUnits = new();
    public List<GameObject> aiItems = new();
    public List<GameObject> aiUnits = new();
    public List<ItemDisplay> unitDisplays = new();
    public List<ItemDisplay> invDisplays = new();
    public List<ItemDisplay> rewardDisplays = new();
    [SerializeField] List<MatGroup> matAssigns = new();
    [SerializeField] Transform invTransform;
    [SerializeField] GameObject invFramePrefab;

    void Start()
    {
        NormalizeRarityWeights();
        LoadItemData();
        for(int i=0;i<40 || i<playerItems.Count;i++) {
            GameObject disp = Instantiate(invFramePrefab,invTransform);
            disp.name = disp.name+" "+i;
            ItemDisplay itemDisplay = disp.GetComponent<ItemDisplay>();
            itemDisplay.order = i;
            invDisplays.Add(itemDisplay);
        }
        random = new(SaveManager.inst.playerSave.tileSeed*(SaveManager.inst.playerSave.locationOrder+1));
        AIArmy army = AIManager.inst.FindAIArmy(random);
        LoadAIArmyData(army);
        if(SaveManager.inst.levelState==1) {
            Invoke(nameof(BeginBattle),.1f);
        } else {
            Debug.Log($"Level state is:{SaveManager.inst.levelState}");
        }
    }

    public void BeginBattle() {
        PlayerManager.inst.StartBattle();
        AIManager.inst.StartBattle();
    }

    public void SetUnitColor(I_Unit unit, bool playerControlled) {
        Color32 color = playerControlled switch
        {
            false => new Color32(255, 0, 0,1),
            true => new Color32(0, 110, 255,1)
        };
        
        bool found = false;
        foreach (MatGroup item in matAssigns)
        {
            if(item.playerControlled == playerControlled && item.itemID == unit.itemID) {
                int q = 0;
                for (int i = 0;i<item.materials.Count;i++) {
                    Material[] tempMat = new Material[unit.meshes[i].materials.Length];
                    for (int j = 0; j< unit.meshes[i].materials.Length;j++) {
                        tempMat[j]=item.materials[i+q];
                        q++;
                    }
                    unit.meshes[i].materials = tempMat;
                    q--;
                }
                found = true;
                break;
            }
        }
        if(found) {return;}
        List<Material> newMats = new();
        for (int i = 0;i<unit.meshes.Count;i++) {
            Material[] tempMat = new Material[unit.meshes[i].materials.Length];
            for (int j = 0; j< unit.meshes[i].materials.Length;j++) {
                Material newMat = new(unit.meshes[i].materials[j]);
                newMat.SetColor("_BaseColor", color);
                newMats.Add(newMat);
                tempMat[j] = newMat;
            }
            unit.meshes[i].materials = tempMat;
        }
        MatGroup temp = new(playerControlled,unit.itemID,newMats);
        matAssigns.Add(temp);
    }

    public void SaveItemData() {
        List<ItemSave> items = new();
        List<UnitSave> units= new();

        foreach (GameObject itemGO in playerItems) {
            I_Item item = itemGO.GetComponent<I_Item>();
            int itemID = item.itemID;
            int unitID = -1;
            if(item.owner) {
                unitID = item.owner.unitID;
            }

            items.Add(new(itemID,unitID));
        }

        foreach (GameObject unitGO in playerUnits) {
            I_Unit unit = unitGO.GetComponent<I_Unit>();
            int itemID = unit.itemID;
            int unitID = unit.unitID;

            units.Add(new(itemID,unitID,unit.health/unit.maxHealth));
        }

        SaveManager.inst.playerSave.items = items;
        SaveManager.inst.playerSave.units = units;
    }

    public void LoadItemData() {
        if(SaveManager.inst.playerSave.units.Count==0 && SaveManager.inst.playerSave.levelCount==0) {
            GenerateUnit(new(0,0,100),true);
            GenerateUnit(new(1,1,100),true);
            GenerateUnit(new(0,2,100),true);
            return;
        }
        foreach (UnitSave unitSave in SaveManager.inst.playerSave.units) {
            GenerateUnit(unitSave);
        }
        foreach (ItemSave itemSave in SaveManager.inst.playerSave.items) {
            GenerateItem(itemSave);
        }
    }

    public void LoadAIArmyData(AIArmy army) {
        foreach (UnitSave unitSave in army.units) {
            GenerateUnit(unitSave,false);
        }
        foreach (ItemSave itemSave in army.items) {
            GenerateItem(itemSave,false);
        }
    }

    public void ObtainRewardItem(GameObject newItem) {
        GameObject item = Instantiate(newItem,itemBucket);
        I_Item temp = item.GetComponent<I_Item>();
        if(temp is I_Unit unit) {
            playerUnits.Add(item);
            unit.unitID=playerUnits.Count > 0 ? playerUnits[^1].GetComponent<I_Unit>().unitID+1 : 0;
            SetUnitColor(unit,true);
            return;
        }
        playerItems.Add(item);
    }

    public void TakeBackItem(GameObject item) {
        if(item==null) {
            return;
        }
        item.transform.parent = itemBucket;
        item.GetComponent<I_Item>().owner = null;
    }

    public void UpdateUnitDisplays() {
        // Debug.Log("Doing This!");
        // Debug.Log($"D Count: {unitDisplays.Count}");
        // Debug.Log($"P Count: {playerUnits.Count}");
        for(int i = 0; i<unitDisplays.Count;i++) {
            unitDisplays[i].SetDisplay(null);
        }
        unitDisplays.Sort();
        for(int i = 0; i<unitDisplays.Count;i++) {
            if(i<playerUnits.Count) {
                unitDisplays[i].SetDisplay(playerUnits[i]);
            } else {
                break;
            }
        }
    }

    [SerializeReference] float[] rarityWeights = new float[5];
    float[] normalizedRarityWeights = new float[5];

    public void NormalizeRarityWeights() {
        float sum = 0f;
        float before = 0f;
        for(int i = 0; i<rarityWeights.Length;i++) {
            sum+=rarityWeights[i]; 
        }
        if(sum == 0) {
            Debug.LogError("Rarity Weights Inccorect");
            return;
        }
        for(int i = 0; i<rarityWeights.Length;i++) {
            normalizedRarityWeights[i]=(rarityWeights[i]/sum)+before; 
            before = normalizedRarityWeights[i];
        }
    }
    
    public GameObject GenerateRandomItem(bool instantiate = false) {
        double luckValue = random.NextDouble();
        // Debug.Log("Rarity Weights Length: "+ rarityWeights.Length);
        // Debug.Log("itemPrefabs[0] Length: "+ itemPrefabs[0].cells.Count);
        // Debug.Log("itemPrefabs Length: "+ itemPrefabs.arrays.Count);
        for(int i = 0; i<rarityWeights.Length;i++) {
            // Debug.Log(luckValue);
            // Debug.Log(normalizedRarityWeights[i]);
            if(luckValue<normalizedRarityWeights[i]) {
                int j = random.Next()%itemPrefabs[i].cells.Count;
                GameObject item = itemPrefabs[i,j];
                if(instantiate){
                    item = Instantiate(item,itemBucket);
                    playerItems.Add(item);
                }
                item.GetComponent<I_Item>().itemID=(i*1000)+j;
                return item;
            }
        }
        Debug.LogError("Item Failed to Generate");
        return null;
    }

    public void GenerateItem(ItemSave itemSave, bool playerControlled = true) {
        GameObject item;
        if(itemSave.itemID==-1) {
            item = Instantiate(basicShell,itemBucket);
        } else {
            int i = itemSave.itemID/1000;
            int j = itemSave.itemID%1000;
            if(i>=itemPrefabs.arrays.Count || j>= itemPrefabs[i].cells.Count) {
                Debug.Log("Invalid Item From Data");
                return;
            }
            item = Instantiate(itemPrefabs[i,j],itemBucket);
        }
        item.GetComponent<I_Item>().itemID=itemSave.itemID;
        if(playerControlled) {            
            if(itemSave.unitID>=0) {
                I_Unit unit = playerUnits.Find((unit) => unit.GetComponent<I_Unit>().unitID == itemSave.unitID).GetComponent<I_Unit>();
                if(unit) {
                    unit.AddItem(item);
                } else {
                    return;
                }
            }
            playerItems.Add(item);
            return;
        } 
        aiItems.Add(item);
        
        if(itemSave.unitID>=0) {
            I_Unit unit = aiUnits.Find((unit) => unit.GetComponent<I_Unit>().unitID == itemSave.unitID).GetComponent<I_Unit>();
            unit.AddItem(item);
        } else {
            Debug.LogWarning("AI Should not have loose item");
        }
    }

    public I_Unit GenerateUnit(UnitSave itemSave, bool playerControlled = true) {
        int i = itemSave.itemID/1000;
        int j = itemSave.itemID%1000;
        if(i>=unitPrefabs.arrays.Count || j>= unitPrefabs[i].cells.Count) {
            Debug.Log("Invalid Unit From Data");
            return null;
        }
        GameObject unitGO = null;
        try {
            unitGO = Instantiate(unitPrefabs[i,j],unitBucket);
        } catch {
            unitGO = basicUnit;
        }
        I_Unit unit = unitGO.GetComponent<I_Unit>();
        unit.itemID=itemSave.itemID;
        List<GameObject> saveList = playerUnits;
        if(!playerControlled) {
            saveList=aiUnits;
        }

        if(itemSave.unitID==-1) {
            if(saveList.Count==0) {
                unit.unitID=0;
            } else {
                unit.unitID = saveList[^1].GetComponent<I_Unit>().unitID+1;
            }
        } else {
            unit.unitID=itemSave.unitID;
        }
        unit.health = itemSave.health*unit.maxHealth;
        unit.playerControlled=playerControlled;
        saveList.Add(unitGO);
        SetUnitColor(unit,playerControlled);
        return unit;
    }

    public GameObject GenerateRandomUnit(bool instantiate = false) {
        double luckValue = random.NextDouble();
        // Debug.Log("Rarity Weights Length: "+ rarityWeights.Length);
        // Debug.Log("itemPrefabs[0] Length: "+ itemPrefabs[0].cells.Count);
        // Debug.Log("itemPrefabs Length: "+ itemPrefabs.arrays.Count);
        for(int i = 0; i<rarityWeights.Length;i++) {
            // Debug.Log(luckValue);
            // Debug.Log(normalizedRarityWeights[i]);
            if(luckValue<normalizedRarityWeights[i]) {
                int j = random.Next()%unitPrefabs[i].cells.Count;
                GameObject unitGO = null;
                try {
                    unitGO = Instantiate(unitPrefabs[i,j],unitBucket);
                } catch {
                    unitGO = basicUnit;
                }
                I_Unit unit = unitGO.GetComponent<I_Unit>();
                if(instantiate){
                    unitGO = Instantiate(unitGO,unitBucket);
                    unit.unitID=playerUnits.Count > 0 ? playerUnits[^1].GetComponent<I_Unit>().unitID+1 : 0;
                    SetUnitColor(unit,true);
                    playerUnits.Add(unitGO);
                }
                unit.itemID=(i*1000)+j;
                return unitGO;
            }
        }
        Debug.LogError("Unit Failed to Generate");
        return null;
    }


    [Serializable]
    struct MatGroup
    {
        public bool playerControlled;
        public int itemID;
        public List<Material> materials;
        public MatGroup(bool playerControlled, int itemID, List<Material> materials) {
            this.playerControlled = playerControlled;
            this.itemID = itemID;
            this.materials = materials;
        }
    }
}
[Serializable]
public class ListWrapper {
    public List<GameObject> objects;
    public ListWrapper() {
        objects = new();
    }
}