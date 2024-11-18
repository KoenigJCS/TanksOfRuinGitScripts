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
    
    [SerializeField] SerializedDictionary<Rarity, ListWrapper> itemPrefabStorage;
    public List<GameObject> rewardItems = new();
    [SerializeField] Transform itemBucket;
    Random random = new();

    public List<I_Item> playerItems = new();

    void OnValidate()
    {
        if(itemPrefabStorage.IsUnityNull()) {
            itemPrefabStorage = new()
            {
                { Rarity.Common, new() },
                { Rarity.Uncommon, new() },
                { Rarity.Rare, new() },
                { Rarity.Mythic, new() },
                { Rarity.Legendary, new() }
            };
        }
    }

    public void ObtainRewardItem(GameObject newItem) {
        if(rewardItems.Contains(newItem)) {
            rewardItems.Remove(newItem);
        }
        for (int i = 0; i < rewardItems.Count; i++)
        {
            Destroy(rewardItems[i]);
        }
        rewardItems.Clear();
        playerItems.Add(newItem.GetComponent<I_Item>());
    }

    [SerializeReference] float[] rarityWeights = new float[5];
    
    public GameObject GenerateRandomItem(bool rewardFlag= true) {
        float sum = 0f;
        float before = 0f;
        float[] normalizedRarityWeights = new float[5];
        for(int i = 0; i<rarityWeights.Length;i++) {
            sum+=rarityWeights[i]; 
        }
        if(sum!=0) {
            for(int i = 0; i<rarityWeights.Length;i++) {
                normalizedRarityWeights[i]=(rarityWeights[i]/sum)+before; 
                before = normalizedRarityWeights[i];
            }
            double luckValue = random.NextDouble();
            for(int i = 0; i<rarityWeights.Length;i++) {
                if(luckValue<normalizedRarityWeights[i]) {
                    GameObject prefabItem = itemPrefabStorage[(Rarity)i].objects[random.Next()%itemPrefabStorage[(Rarity)i].objects.Count];
                    GameObject newItem = Instantiate(prefabItem,itemBucket);
                    if(rewardFlag) {
                        rewardItems.Add(newItem);
                    }
                    return newItem;
                }
            }
        }
        Debug.LogError("What");
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