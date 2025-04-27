using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class SoundManager : MonoBehaviour
{
    public static SoundManager inst;
    [SerializeField] AudioSource basicButtonSound;
    [SerializeField] AudioSource badButtonSound;
    [SerializeField] AudioSource backgroundMusic;
    [SerializeField] AudioSource unitSelect;
    [SerializeField] AudioSource unitVoiceLine;
    [SerializeField] List<AudioClip> voiceLines;
    [SerializeField] AudioSource fireSound;
    [SerializeField] float uIVolume;
    [SerializeField] float musicVolume;
    [SerializeField] float vFXVolume;
    [SerializeField] float voiceVolume;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider uISlider;
    [SerializeField] Slider vFXSlider;
    [SerializeField] Slider voiceSlider;
    [SerializeField] GameObject optionsMenu;
    public GameObject tankFireEfect;
    public GameObject tankExplodeEffect;

    void Awake() {
        inst = this;
    }
    // Start is called before the first frame update
    void Start() {
        LoadSoundSettings();
        PlayBackgroundMusic();
    }

    // Update is called once per frame
    void Update() {
        
    }

    public void SetOptionsMenu(bool state) {
        optionsMenu.SetActive(state);
        if(state) {
            SetAllVolumesFromSave();
        }
    }

    public void SaveSoundSettings() {
        SaveManager.inst.SetSoundSave(new(musicVolume,uIVolume,vFXVolume,voiceVolume));
    }


    public void LoadSoundSettings() {
        SoundSave save = SaveManager.inst.GetSoundSave();
        uIVolume = save.uIVolume;
        musicVolume = save.musicVolume;
        vFXVolume = save.vFXVolume;
        voiceVolume = save.voiceVolume;
        musicSlider.onValueChanged.AddListener(setMusicVolume);
        uISlider.onValueChanged.AddListener(setUIVolume);
        vFXSlider.onValueChanged.AddListener(setVFXVolume);
        voiceSlider.onValueChanged.AddListener(setVoiceVolume);
        musicSlider.onValueChanged.AddListener(_ => PlayBasicButton());
        uISlider.onValueChanged.AddListener(_ => PlayBasicButton());
        vFXSlider.onValueChanged.AddListener(_ => PlayBasicButton());
        voiceSlider.onValueChanged.AddListener(_ => PlayBasicButton());
        if(debugIsOptionsAvalible) {
            SetAllVolumesFromSave();
        }
    }

    [SerializeField] bool debugIsOptionsAvalible = true;

    public void SetAllVolumesFromSave() {
        basicButtonSound.volume = uIVolume;
        badButtonSound.volume = uIVolume;
        backgroundMusic.volume = musicVolume;
        fireSound.volume = vFXVolume;
        unitVoiceLine.volume=voiceVolume;
        unitSelect.volume = vFXVolume;
        musicSlider.value = musicVolume;
        vFXSlider.value = vFXVolume;
        uISlider.value = uIVolume;
        voiceSlider.value = voiceVolume;
    }

    public void PlayBasicButton () {
        basicButtonSound.Play();
    }

    public void PlayBadButton () {
        badButtonSound.Play();
    }

    public void PlayUnitVoiceLine() {
        if(voiceLines.Count==0) {
            return;
        }

        unitVoiceLine.clip = voiceLines[Random.Range(0,voiceLines.Count)];
        unitVoiceLine.Play();
    }

    public void PlayBackgroundMusic () {
        backgroundMusic.Play();
    }

    public void PlayUnitSelect () {
        unitSelect.Play();
    }

    public void PlayFire() {
        fireSound.Play();
    }

    public void setUIVolume(Single newVolume) {
        uIVolume = newVolume;
        basicButtonSound.volume = newVolume;
        badButtonSound.volume = newVolume;
        SaveSoundSettings();
    }

    public void setMusicVolume(Single newVolume) {
        musicVolume = newVolume;
        backgroundMusic.volume = newVolume;
        SaveSoundSettings();
    }

    public void setVFXVolume(Single newVolume) {
        vFXVolume = newVolume;
        fireSound.volume= newVolume;
        unitVoiceLine.volume = newVolume;
        unitSelect.volume = newVolume;
        SaveSoundSettings();
    }

    public void setVoiceVolume(Single newVolume) {
        voiceVolume = newVolume;
        unitVoiceLine.volume = newVolume;
        SaveSoundSettings();
    }
}
