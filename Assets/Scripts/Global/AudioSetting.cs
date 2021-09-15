using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioSetting : MonoBehaviour
{
    public AudioMixer Mixer;
    public bool MusicVolume,SFXVolume;
    public Image MusicToggleImg,SFXToggleImg;
    public Sprite ToggleOn,ToggleOff;

    private void Awake()
    {
        this.UpdateAudio();
        this.UpdateImgs();
    }

    private void Start()
    {
        this.UpdateAudio();
    }

    public void UpdateAudio()
    {
        MusicVolume = Convert.ToBoolean(PlayerPrefs.GetInt("MusicVolume",1));
        SFXVolume = Convert.ToBoolean(PlayerPrefs.GetInt("SFXVolume",1));

        if(MusicVolume)
        {
            Debug.Log("music on");
            Mixer.SetFloat("Musicvolume",0.0f);
        }
        else
        {
            Debug.Log("music off");
            Mixer.SetFloat("Musicvolume",-80.0f);
        }

        if(SFXVolume)
            Mixer.SetFloat("SFXvolume",0.0f);
        else
            Mixer.SetFloat("SFXvolume",-80.0f);
    }

    public void UpdateImgs()
    {
        MusicVolume = Convert.ToBoolean(PlayerPrefs.GetInt("MusicVolume",1));
        SFXVolume = Convert.ToBoolean(PlayerPrefs.GetInt("SFXVolume",1));

        if(MusicVolume)
            Mixer.SetFloat("Musicvolume",0.0f);
        else
            Mixer.SetFloat("Musicvolume",-80.0f);

        if(MusicVolume)
        {
            if(MusicToggleImg != null)
            MusicToggleImg.sprite = ToggleOn;
        }
        else
        {
            if(MusicToggleImg != null)
            MusicToggleImg.sprite = ToggleOff;
        }

        if(SFXVolume)
        {
            if(SFXToggleImg != null)
            SFXToggleImg.sprite = ToggleOn;
        }
        else
        {
            if(SFXToggleImg != null)
            SFXToggleImg.sprite = ToggleOff;
        }
    }

    public void ToggleAudio()
    {
        PlayerPrefs.SetInt("MusicVolume", Convert.ToInt16(!MusicVolume));
        UpdateImgs();
    }

    public void ToggleSFX()
    {
        PlayerPrefs.SetInt("SFXVolume", Convert.ToInt16(!SFXVolume));
        UpdateImgs();
    }

}
