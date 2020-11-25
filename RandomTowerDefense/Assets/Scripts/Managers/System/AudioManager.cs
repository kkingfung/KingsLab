﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public List<AudioClip> bgmSource;
    public List<AudioClip> seSource;

    public List<Toggle> bgmUI;
    public List<Toggle> seUI;

    AudioSource[] audioSource;
    Dictionary<string, AudioClip> bgmList;
    Dictionary<string, AudioClip> seList;

    [HideInInspector]
    public bool enabledBGM;
    [HideInInspector]
    public bool enabledSE;

    public void EnableBGM(bool enable) {
        enabledBGM = enable;
        PlayerPrefs.SetInt("BGM", enabledBGM ? 1 : 0);
        foreach (Toggle i in bgmUI)
            i.isOn = enabledBGM;

        if (enabledBGM)
            audioSource[0].Play();
        else
            audioSource[0].Stop();
    }

    public void EnableSE(bool enable)
    {
        enabledSE = enable;
        PlayerPrefs.SetInt("SE", enabledSE ? 1 : 0);
        foreach (Toggle i in seUI)
            i.isOn = enabledSE;


        if (enabledSE)
            audioSource[1].Play();
        else
            audioSource[1].Stop();
    }

    private void OnEnable()
    {
        enabledBGM = PlayerPrefs.GetInt("BGM", 1) == 1;
        enabledSE = PlayerPrefs.GetInt("SE", 1) == 1;
    }

    private void OnDisable()
    {
        PlayerPrefs.SetInt("BGM", enabledBGM ? 1 : 0);
        PlayerPrefs.SetInt("SE", enabledSE ? 1 : 0);
    }

    private void Awake() 
    {
        audioSource = GetComponents<AudioSource>();

        bgmList = new Dictionary<string, AudioClip>();
        seList = new Dictionary<string, AudioClip>();

        int i = 0;
        bgmList.Add("bgm_Battle", bgmSource[i++]);
        bgmList.Add("bgm_Opening", bgmSource[i++]);
        bgmList.Add("bgm_Title", bgmSource[i++]);

        i = 0;
        seList.Add("se_Lighting", seSource[i++]);
        seList.Add("se_Shot", seSource[i++]);
        seList.Add("se_Snail", seSource[i++]);

        seList.Add("se_MagicFire", seSource[i++]);
        seList.Add("se_MagicBlizzard", seSource[i++]);
        seList.Add("se_MagicPetrification", seSource[i++]);
        seList.Add("se_MagicSummon", seSource[i++]);

        seList.Add("se_Button", seSource[i++]);
        seList.Add("se_Clear", seSource[i++]);
        seList.Add("se_Lose", seSource[i++]);

        seList.Add("se_Hitted", seSource[i++]);
    }

    void Record()
    {
        if (audioSource == null) return;
        audioSource[1].clip = Microphone.Start("Built-in Microphone", true, 10, 44100);
        audioSource[1].Play();
    }

    void Release() {
        foreach (var device in Microphone.devices)
        {
            if (Microphone.IsRecording(device))
                Microphone.End(device);
        }
    }

    public float[] GetClipWaveform(string clipname)
    {
        float[] data = new float[bgmList[clipname].samples];
        if (bgmList[clipname].GetData(data, 0) == false)
        {
            seList[clipname].GetData(data, 0);
        }
        for(int i=0;i<10000;i++)
        Debug.Log(data[i]*1000);
        return data;
    }

    float[] GetWaveform(string clipname) 
    {
        float[] data=new float[Microphone.GetPosition("Built-in Microphone")];
        if (bgmList[clipname].GetData(data, 0) == false) {
            seList[clipname].GetData(data, 0);
        }
        return data;
    }

    public void PlayAudio(string clipname,bool isLoop=false)
    {
        if (bgmList.ContainsKey(clipname))
        {
            if (enabledBGM == false) return;
                audioSource[0].pitch = 1;
            audioSource[0].clip = bgmList[clipname];
            audioSource[0].loop = isLoop;
                audioSource[0].Play();
        }
        else {
            if (enabledSE)
                audioSource[1].PlayOneShot(seList[clipname]);
        }
    }

    public AudioClip GetAudio(string clipname)
    {
        if (bgmList.ContainsKey(clipname))
        {
            return bgmList[clipname];
        }
        else
        {
            return seList[clipname];
        }
    }

    public void PlayReverseAudio(string clipname, bool isLoop = false)
    {
        if (bgmList.ContainsKey(clipname))
        {
            if (enabledBGM==false) return;
            audioSource[0].pitch = -1;
            audioSource[0].clip = bgmList[clipname];
            audioSource[0].loop = isLoop;
            audioSource[0].Play();
            StartCoroutine(StopLoop(isLoop,0));
        }
        else
        {
            if (enabledSE == false) return;
            audioSource[1].pitch = -1;
            audioSource[1].clip = seList[clipname];
            audioSource[1].loop = isLoop;
            audioSource[1].Play();
            StartCoroutine(StopLoop(isLoop,1));
        }
        
    }

    public IEnumerator StopLoop(bool isLoop,int SourceID)
    {
        yield return new WaitForSeconds(1f);
        audioSource[SourceID].loop = isLoop;
    }

    public void SetAudioPitch(float pitch, int SourceID) {
        audioSource[SourceID].pitch = pitch;
    }
}
