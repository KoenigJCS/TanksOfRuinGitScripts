using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveSelect : MonoBehaviour
{
    [SerializeField] Button playButton;
    [SerializeField] Button deleteButton;
    [SerializeField] TMPro.TMP_Text text;

    public void SetIndex(int index) {
        int j = index;
        playButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(() => SoundManager.inst.PlayBasicButton());
        deleteButton.onClick.AddListener(() => SoundManager.inst.PlayBadButton());
        playButton.onClick.AddListener(() => SaveManager.inst.LoadPlayerSave(j));
        playButton.onClick.AddListener(() => SaveManager.inst.NextScene("AUTO"));
        deleteButton.onClick.AddListener(() => SaveManager.inst.DeletePlayerSave(j));
        deleteButton.onClick.AddListener(() => Destroy(gameObject));
        text.text = "Save "+index;
        
    }
}
