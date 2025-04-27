using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveManager : MonoBehaviour
{
    public static SaveManager inst;
    [SerializeField] string soundFilePath = "/SoundSettings.txt";
    [SerializeField] string globalSavePath = "/GlobalSave.txt";
    [SerializeField] string activePlayerSave = "/PlayerData";
    [SerializeField] string mapBaseName = "/mapSave"; // Should have +id+."txt"
    public PlayerSave playerSave;
    public GlobalSave globalSave;
    /* 
    -1 is MM
    0 is Map
    1 is Battle
    */
    public int levelState; 
    private void Awake() {
        inst = this;
    }
    [SerializeField] List<int> captiveIndexes = new();
    // Start is called before the first frame update
    void Start()
    {
        CheckSaveFile();
        GetGlobalSave();
        CheckMapFile();
        CheckPlayerDataFile();
        if(levelState>=0) {
            LoadPlayerSave(globalSave.currentSave);
            playerSave.curentScene=levelState;
        } else {
            LoadAllPlayerSaves();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NewGame() {
        globalSave.currentSave=-1;
        NextScene("BasicBattleMap");
    }

    public void NextScene(string sceneName) {
        if(sceneName=="EXIT") {
            StopAllCoroutines();
            PlayerManager.inst.StopAllCoroutines();
            SceneManager.LoadScene("MMenu");
            return;
        }
        if(levelState>=0) {
            ItemManager.inst.SaveItemData();
            playerSave.levelCount++;
            SetPlayerSave(globalSave.currentSave);
        }
        if(!captiveIndexes.Contains(globalSave.currentSave)) {
            globalSave.currentSave=-1;
        }
  
        if(globalSave.currentSave==-1) {
            int i = 0;
            while(captiveIndexes.Contains(i)) {
                i++;
            }
            globalSave.currentSave=i;
            LoadPlayerSave(i);
        }

        SetGlobalSave();
        StopAllCoroutines();
        if(PlayerManager.inst!=null) {
            PlayerManager.inst.StopAllCoroutines();
        }
        if(sceneName=="AUTO") {
            if(playerSave.curentScene==0) {
                SceneManager.LoadScene("WorldMap");
            } else {
                SceneManager.LoadScene("BasicBattleMap");
            }
            return;
        }
        SceneManager.LoadScene(sceneName);
    }

    public void Retry() {
        StopAllCoroutines();
        if(PlayerManager.inst) {
            PlayerManager.inst.StopAllCoroutines();
        }
        SceneManager.LoadScene("MMenu");
    }

    public void LoadAllPlayerSaves() {
        string dp = Application.persistentDataPath+"/Saves/Player";
        DirectoryInfo info = new(dp);
        FileInfo[] files = info.GetFiles();
        foreach (FileInfo file in files) {
            if(file.Extension!=".txt") {
                continue;
            }
            int index = -1;
            try
            {
                index = int.Parse(file.Name.Substring(activePlayerSave.Length-1,file.Name.Length-(activePlayerSave.Length+3)));
                captiveIndexes.Add(index);
            }
            catch (System.Exception)
            {
                Debug.LogWarning("Failure to parse save"+file.Name+" Got Instead:"+file.Name.Substring(activePlayerSave.Length-1,file.Name.Length-(activePlayerSave.Length+3)));
            }
            if(index==-1) {
                return;
            }
            MainMenuManager.inst.MakeSelect(index);
        }
    }

    public void CheckSaveFile() {
        string dp = Application.persistentDataPath;
        Debug.Log(dp);
        if(Directory.Exists(dp+"/Saves")) {
            return;
        }
        Directory.CreateDirectory(dp+"/Saves");
    }

    public void CheckMapFile() {
        string dp = Application.persistentDataPath;
        if(Directory.Exists(dp+"/Saves/Maps")) {
            return;
        }
        Directory.CreateDirectory(dp+"/Saves/Maps");
    }

    public void CheckPlayerDataFile() {
        string dp = Application.persistentDataPath;
        if(Directory.Exists(dp+"/Saves/Player")) {
            return;
        }
        Directory.CreateDirectory(dp+"/Saves/Player");
    }
    
    public void LoadPlayerSave(int index) {
        globalSave.currentSave = index;
        string dp = Application.persistentDataPath+"/Saves/Player";
        if(!File.Exists(dp+activePlayerSave+index+".txt")) {
            playerSave = new();
            playerSave.curentScene=1;
            playerSave.currentDifficulty=1;
            playerSave.tileSeed= new System.Random().Next();
            SetPlayerSave(index);
            return;
        }
        string json = File.ReadAllText(dp+activePlayerSave+index+".txt");
        playerSave = JsonUtility.FromJson<PlayerSave>(json);
    }

    public void SetPlayerSave(int index) {
        string dp = Application.persistentDataPath+"/Saves/Player";
        if(File.Exists(dp+activePlayerSave+index+".txt")) {
            File.Delete(dp+activePlayerSave+index+".txt");
        }
        
        // File.CreateText(dp+settingsfilePath);
        string json = JsonUtility.ToJson(playerSave);
        File.WriteAllText(dp+activePlayerSave+index+".txt", json);
    }

    public void DeletePlayerSave(int index) {
        string dp = Application.persistentDataPath+"/Saves/Player";
        if(File.Exists(dp+activePlayerSave+index+".txt")) {
            File.Delete(dp+activePlayerSave+index+".txt");
        }
        if(captiveIndexes.Contains(index)) {
            captiveIndexes.Remove(index);
        }
    }

    public SoundSave GetSoundSave() {
        string dp = Application.persistentDataPath+"/Saves";
        if(!File.Exists(dp+soundFilePath)) {
            return new();
        }
        string json = File.ReadAllText(dp+soundFilePath);
        return JsonUtility.FromJson<SoundSave>(json);
    }
    
    public void SetSoundSave(SoundSave newSave) {
        string dp = Application.persistentDataPath+"/Saves";
        if(File.Exists(dp+soundFilePath)) {
            File.Delete(dp+soundFilePath);
        }
        
        // File.CreateText(dp+settingsfilePath);
        string json = JsonUtility.ToJson(newSave);
        File.WriteAllText(dp+soundFilePath, json);
    }

    public void GetGlobalSave() {
        string dp = Application.persistentDataPath+"/Saves";
        if(!File.Exists(dp+globalSavePath)) {
            globalSave = new();
            return;
        }
        string json = File.ReadAllText(dp+globalSavePath);
        globalSave = JsonUtility.FromJson<GlobalSave>(json);
    }
    
    public void SetGlobalSave() {
        string dp = Application.persistentDataPath+"/Saves";
        if(File.Exists(dp+globalSavePath)) {
            File.Delete(dp+globalSavePath);
        }
        
        // File.CreateText(dp+settingsfilePath);
        string json = JsonUtility.ToJson(globalSave);
        File.WriteAllText(dp+globalSavePath, json);
    }

    public TileSetSave GetTileSave(int id) {
        string dp = "Maps";
        string fileName = dp+mapBaseName+id;
        string json = Resources.Load<TextAsset>(fileName).text;
        return JsonUtility.FromJson<TileSetSave>(json);
    }

    public void Quit() {
        Application.Quit();
    }

    public void SetTileSave(TileSetSave newSave, int id) {
        string dp = Application.dataPath+"/Saves/Maps";
        string fileName = dp+mapBaseName+id+".txt";
        if(File.Exists(fileName)) {
            File.Delete(fileName);
        }

        // File.CreateText(dp+settingsfilePath);
        string json = JsonUtility.ToJson(newSave);
        File.WriteAllText(fileName, json);
    }
}
