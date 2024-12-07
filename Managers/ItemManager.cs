using System;
using System.Collections.Generic;
using System.Security;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Random = System.Random;

public class ItemManager : MonoBehaviour
{
    public static ItemManager inst;
    void Awake() {
        inst = this;
    }
    public GameObject basicShell;
    
    // [SerializeField] SerializedDictionary<Rarity, ListWrapper> itemPrefabStorage;
    [SerializeField] Matrix<GameObject> itemPrefabs;
    [SerializeField] Transform itemBucket;
    Random random = new();

    public List<GameObject> playerItems = new();
    public List<ItemDisplay> unitDisplays = new();
    public List<ItemDisplay> invDisplays = new();
    public List<ItemDisplay> rewardDisplays = new();
    [SerializeField] Transform invTransform;
    [SerializeField] GameObject invFramePrefab;

    void Start()
    {
        NormalizeRarityWeights();
        UpdateUnitDisplays();
        for(int i=0;i<32;i++) {
            GameObject disp = Instantiate(invFramePrefab,invTransform);
            disp.name = disp.name+" "+i;
            ItemDisplay itemDisplay = disp.GetComponent<ItemDisplay>();
            itemDisplay.order = i;
            invDisplays.Add(itemDisplay);

        }
    }

    public void ObtainRewardItem(GameObject newItem) {
        GameObject item = Instantiate(newItem,itemBucket);
        playerItems.Add(item);
    }

    public void TakeBackItem(GameObject item) {
        item.transform.parent = itemBucket;
        item.GetComponent<I_Item>().owner = null;
    }

    public void UpdateUnitDisplays() {
        for(int i = 0; i<unitDisplays.Count;i++) {
            unitDisplays[i].SetDisplay(null);
        }
        List<GameObject> units = new();
        foreach (GameObject item in playerItems)
        {
            //Replace this with tags at some point
            if(item.TryGetComponent<I_Unit>(out _)) {
                units.Add(item);
            }
        }
        unitDisplays.Sort();
        for(int i = 0; i<unitDisplays.Count;i++) {
            if(i<units.Count) {
                unitDisplays[i].SetDisplay(units[i]);
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
        Debug.Log("Rarity Weights Length: "+ rarityWeights.Length);
        Debug.Log("itemPrefabs[0] Length: "+ itemPrefabs[0].cells.Count);
        Debug.Log("itemPrefabs Length: "+ itemPrefabs.arrays.Count);
        for(int i = 0; i<rarityWeights.Length;i++) {
            Debug.Log(luckValue);
            Debug.Log(normalizedRarityWeights[i]);
            if(luckValue<normalizedRarityWeights[i]) {
                GameObject item = itemPrefabs[i,random.Next()%itemPrefabs[i].cells.Count];
                if(instantiate){
                    item = Instantiate(item,itemBucket);
                    playerItems.Add(item);
                }
                return item;
            }
        }
        Debug.LogError("Item Failed to Generate");
        return null;
    }

}
[Serializable]
public class ListWrapper {
    public List<GameObject> objects;
    public ListWrapper() {
        objects = new();
    }
}