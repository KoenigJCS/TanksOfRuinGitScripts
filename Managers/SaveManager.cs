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
    [SerializeField] string mapBaseName = "/mapSave"; // Should have +id+."txt"
    private void Awake() {
        inst = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        CheckSaveFile();
        CheckMapFile();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NextSceene(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }

    public void CheckSaveFile() {
        string dp = Application.persistentDataPath;
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
