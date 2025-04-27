using System;
using UnityEngine;

[Serializable]
public struct SoundSave
{
    public float uIVolume;
    public float musicVolume;
    public float vFXVolume;
    public float voiceVolume;

    public SoundSave(float n_music = 1.0f, float n_UI = 1.0f, float n_vfx = 1.0f, float n_voice = 1.0f) {
        musicVolume = n_music;
        uIVolume = n_UI;
        vFXVolume = n_vfx;
        voiceVolume = n_voice;
    }
}