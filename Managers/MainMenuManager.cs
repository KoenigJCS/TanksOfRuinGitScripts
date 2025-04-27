using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager inst;
    void Awake()
    {
        inst = this;
    }
    [SerializeField] GameObject saveSelect;
    [SerializeField] GameObject savePrefab;
    [SerializeField] Transform saveContentTransform;
    

    public void SetSaveSelect(bool value) {
        saveSelect.SetActive(value);
    } 

    public void MakeSelect(int index) {
        GameObject save = Instantiate(savePrefab,saveContentTransform);
        save.GetComponent<SaveSelect>().SetIndex(index);
    }
}
