using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;


public class SoundManager : MonoBehaviour
{
    public static SoundManager inst;
    [SerializeField] AudioSource basicButtonSound;
    [SerializeField] AudioSource badButtonSound;
    [SerializeField] AudioSource backgroundMusic;
    [SerializeField] AudioSource fireSound;
    [SerializeField] float uIVolume;
    [SerializeField] float musicVolume;
    [SerializeField] float vFXVolume;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider uISlider;
    [SerializeField] Slider vFXSlider;
    [SerializeField] GameObject optionsMenu;

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
        SaveManager.inst.SetSoundSave(new(musicVolume,uIVolume,vFXVolume));
    }


    public void LoadSoundSettings() {
        SoundSave save = SaveManager.inst.GetSoundSave();
        uIVolume = save.uIVolume;
        musicVolume = save.musicVolume;
        vFXVolume = save.vFXVolume;
        SetAllVolumesFromSave();
    }

    public void SetAllVolumesFromSave() {
        basicButtonSound.volume = uIVolume;
        badButtonSound.volume = uIVolume;
        backgroundMusic.volume = musicVolume;
        fireSound.volume = vFXVolume;
        musicSlider.value = musicVolume;
        vFXSlider.value = vFXVolume;
        uISlider.value = uIVolume;
    }

    public void PlayBasicButton () {
        basicButtonSound.Play();
    }

    public void PlayBadButton () {
        badButtonSound.Play();
    }

    public void PlayBackgroundMusic () {
        backgroundMusic.Play();
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
        SaveSoundSettings();
    }
}
