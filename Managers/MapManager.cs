using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using Random = System.Random;

public class MapManager : MonoBehaviour
{
    public static MapManager inst;
    [SerializeField] List<Location> locations = new();
    [SerializeField] Transform locationContainter;
    [SerializeField] GameObject locationPrefab;
    [SerializeField] Transform pathContainter;
    [SerializeField] GameObject pathprefab;
    [SerializeField] Sprite[] difficultySprites = new Sprite[4];
    [SerializeField] int worldLocationsCount;
    [SerializeField] int worldLocationsCountVarriation;
    [SerializeField] UnityEngine.UI.Image europeImage;
    void Awake() {
        inst = this;
    }
    [Header("Probabilities")]
    [SerializeReference] float[] difficultyWeights = new float[4];
    float[] normalizedDifficultyWeights = new float[4];
    Random random;
    [SerializeField] List<GameObject> locationPositions = new();
    [SerializeField] GameObject startPosition;
    [SerializeField] GameObject endPosition;
    public void NormalizeDifficultyWeights() {
        float sum = 0f;
        float before = 0f;
        for(int i = 0; i<difficultyWeights.Length;i++) {
            sum+=difficultyWeights[i]; 
        }
        if(sum == 0) {
            Debug.LogError("Rarity Weights Inccorect");
            return;
        }
        for(int i = 0; i<difficultyWeights.Length;i++) {
            normalizedDifficultyWeights[i]=(difficultyWeights[i]/sum)+before; 
            before = normalizedDifficultyWeights[i];
        }
    }
    [Header("Game Data")]
    Location _currentLocation;
    public Location currentLocation {get{
        return _currentLocation;
    } set {
        _currentLocation=value;

        if(_currentLocation==null) {
            return;
        }
        
        foreach (Location location in locations) {
            location.GetComponent<RectTransform>().localScale=Vector3.one;
            location.button.interactable=false;
        }
        foreach (Location location in _currentLocation.nextLocations) {
            location.GetComponent<RectTransform>().localScale=Vector3.one*1.75f;
            location.button.interactable=true;
        }

        _currentLocation.GetComponent<RectTransform>().localScale=Vector3.one*1.5f;
        
    }}

    // Start is called before the first frame update
    void Start()
    {
        SaveManager.inst.LoadPlayerSave(SaveManager.inst.globalSave.currentSave);
        random = new(SaveManager.inst.playerSave.tileSeed);
        NormalizeDifficultyWeights();
        //TODO Check if world exists
        if(SaveManager.inst.playerSave.locationOrder==-1 
        || SaveManager.inst.playerSave.mapInfo == null 
        || SaveManager.inst.playerSave.mapInfo.Count == 0 
        || SaveManager.inst.playerSave.locationOrder == SaveManager.inst.playerSave.mapInfo.Count-1) {
            GenerateNewWorld();
            SaveLocationData();
            SaveManager.inst.SetPlayerSave(SaveManager.inst.globalSave.currentSave);
            return;
        }
        GenerateLoadedWorld();
    }

    public void GenerateLoadedWorld() {
        foreach(LocationSave location in SaveManager.inst.playerSave.mapInfo) {
            GameObject tempLocationGO;
            if(location.locationID==0) {
                tempLocationGO = Instantiate(locationPrefab,startPosition.transform.position,Quaternion.identity,locationContainter);
            } else if (location.locationID==999) {
                tempLocationGO = Instantiate(locationPrefab,endPosition.transform.position,Quaternion.identity,locationContainter);
            }else {
                tempLocationGO = Instantiate(locationPrefab,locationPositions[(location.locationID%100)-1].transform.position,Quaternion.identity,locationContainter);
            }
            Location tempLocation = tempLocationGO.GetComponent<Location>();
            tempLocation.stateIcon.sprite = difficultySprites[location.state];
            tempLocation.uniqueID=location.locationID;
            tempLocation.state=location.state;
            tempLocation.button.onClick.AddListener(() => EnterLocation(tempLocation));
            locations.Add(tempLocation);
            tempLocation.order=locations.Count-1;
        }

        foreach(LocationSave location in SaveManager.inst.playerSave.mapInfo) {
            if(location.locationID==999) {
                continue;
            }
            foreach(int order in location.neighborOrder) {
                locations[location.order].nextLocations.Add(locations[order]);
            }
        }
        currentLocation=locations[SaveManager.inst.playerSave.locationOrder];
        GeneratePaths();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void EnterLocation(Location location) {
        if(!currentLocation.nextLocations.Contains(location)) {
            Debug.LogError("Invalid Move");
            return;
        }
        SaveManager.inst.playerSave.currentDifficulty=location.state+1;
        SaveManager.inst.playerSave.locationOrder=location.order;
        SaveLocationData();
        SaveManager.inst.NextScene("BasicBattleMap");
    }

    public void SaveLocationData() {
        List<LocationSave> locationSaves = new();
        foreach (Location location in locations) {
            List<int> ids = new();
            foreach (Location next in location.nextLocations) {
                ids.Add(locations.IndexOf(next));
            }
            LocationSave temp = new(ids,location.uniqueID,location.state,location.order);
            locationSaves.Add(temp);
        }
        SaveManager.inst.playerSave.mapInfo=locationSaves;
    }

    public void GenerateNewWorld() {
        // GenerateValidLocationMap();
        int locationError = ((random.Next() % worldLocationsCountVarriation) +1) * ((random.Next() & 0x1) == 1 ? 1: -1);
        Debug.Log(locationError);
        int zone = 0;
        List<GameObject> tempLocationList = locationPositions;
        List<int> removedLocations= new();
        List<List<Location>> locationMatrix = new();
        for(int i=0;i<6;i++) {
            locationMatrix.Add(new List<Location>());
        }

        for (int i = 0; locationPositions.Count-removedLocations.Count > locationError+worldLocationsCount;i++) {
            int indexGone = random.Next() % locationPositions.Count;
            removedLocations.Add(indexGone);
            tempLocationList.RemoveAt(indexGone);
        }

        GameObject tempLocationGO = Instantiate(locationPrefab,startPosition.transform.position,Quaternion.identity,locationContainter);
        Location startTempLocation = tempLocationGO.GetComponent<Location>();
        startTempLocation.stateIcon.sprite = difficultySprites[0];
        startTempLocation.uniqueID=0;
        startTempLocation.state=0;
        startTempLocation.order=0;
        startTempLocation.button.onClick.AddListener(() => EnterLocation(startTempLocation));
        locations.Add(startTempLocation);
        
        for(int i=0;i<locationPositions.Count;i++) {
            if(removedLocations.Contains(i)) {
                continue;
            }
            zone = i*locationMatrix.Count/tempLocationList.Count;
            // Debug.Log(zone);
            tempLocationGO = Instantiate(locationPrefab,locationPositions[i].transform.position,Quaternion.identity,locationContainter);
            Location tempLocation = tempLocationGO.GetComponent<Location>();
            double luckValue = random.NextDouble();
            int j = 0;
            for(;j<difficultyWeights.Length;j++) {
                if(luckValue<normalizedDifficultyWeights[j]) {
                    break;
                }
            }
            tempLocation.stateIcon.sprite = difficultySprites[j];
            tempLocation.uniqueID=i+1+(zone*100);
            tempLocation.state=j;
            locationMatrix[zone].Add(tempLocation);
            tempLocation.button.onClick.AddListener(() => EnterLocation(tempLocation));
            locations.Add(tempLocation);
            tempLocation.order=locations.Count-1;
        }

        tempLocationGO = Instantiate(locationPrefab,endPosition.transform.position,Quaternion.identity,locationContainter);
        Location endTempLocation = tempLocationGO.GetComponent<Location>();
        endTempLocation.stateIcon.sprite = difficultySprites[^1];
        endTempLocation.uniqueID=999;
        endTempLocation.state=difficultySprites.Length-1;
        endTempLocation.button.onClick.AddListener(() => EnterLocation(endTempLocation));
        locations.Add(endTempLocation);
        endTempLocation.order=locations.Count-1;

        for(int i = 0;i<locationMatrix[0].Count;i++) {
            startTempLocation.nextLocations.Add(locationMatrix[0][i]);
        }

        for(int i = 0;i<locationMatrix.Count-1;i++) {
            for(int j = 0;j<locationMatrix[i].Count;j++) {
                int nextJ = j;
                while(locationMatrix[i+1].Count<=nextJ && nextJ>=0) {
                    nextJ--;
                }
                locationMatrix[i][j].nextLocations.Add(locationMatrix[i+1][nextJ]);
            }
        }

        for(int i = 0;i<locationMatrix[^1].Count;i++) {
            locationMatrix[^1][i].nextLocations.Add(endTempLocation);
        }

        GeneratePaths();
        currentLocation=startTempLocation;
        SaveManager.inst.playerSave.locationOrder=startTempLocation.uniqueID;
    }
    
    void GeneratePaths() {
        foreach (Location location in locations){
            foreach (Location nextLocation in location.nextLocations) {
                
                Vector2 delta = location.GetComponent<RectTransform>().anchoredPosition-nextLocation.GetComponent<RectTransform>().anchoredPosition;
                Vector2 between = (delta/2f) + nextLocation.GetComponent<RectTransform>().anchoredPosition;
                float zRot = Mathf.Atan2(delta.y,delta.x) * Mathf.Rad2Deg;
                GameObject temp  = Instantiate(pathprefab,pathContainter.position,Quaternion.Euler(0,0,zRot),pathContainter);
                temp.GetComponent<RectTransform>().sizeDelta=new(delta.magnitude,20);
                temp.GetComponent<RectTransform>().anchoredPosition=between;
                temp.GetComponent<RectTransform>().localRotation=Quaternion.Euler(0,0,zRot);
            }
        }
    }
}
