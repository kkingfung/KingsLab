using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public List<AudioClip> bgmSource;
    public List<AudioClip> seSource;

    AudioSource audioSource;
    Dictionary<string, AudioClip> bgmList;
    Dictionary<string, AudioClip> seList;


    void Start() 
    {
        audioSource = GetComponent<AudioSource>();

        int i = 0;
        bgmList.Add("bgm_Battle", bgmSource[i++]);
        bgmList.Add("bgm_Opening", bgmSource[i++]);
        bgmList.Add("bgm_Title", bgmSource[i++]);

        i = 0;
        seList.Add("se_Katana", seSource[i++]);
        seList.Add("se_LongDist", seSource[i++]);
        seList.Add("se_Punch", seSource[i++]);

        seList.Add("se_MagicBuff", seSource[i++]);
        seList.Add("se_MagicFire", seSource[i++]);
        seList.Add("se_MagicGolem", seSource[i++]);
        seList.Add("se_MagicIce1", seSource[i++]);
        seList.Add("se_MagicIce2", seSource[i++]);
        seList.Add("se_MagicTornado", seSource[i++]);

        seList.Add("se_Button", seSource[i++]);
        seList.Add("se_Clear", seSource[i++]);
        seList.Add("se_Lose", seSource[i++]);
    }

    void Record()
    {
        if (audioSource == null) return;
        audioSource.clip = Microphone.Start("Built-in Microphone", true, 10, 44100);
        audioSource.Play();
    }

    void Release() {
        foreach (var device in Microphone.devices)
        {
            if (Microphone.IsRecording(device))
                Microphone.End(device);
        }
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
        audioSource.pitch = 1;
        audioSource.clip = bgmList[clipname] ? bgmList[clipname] : seList[clipname];
        audioSource.loop = isLoop;
        audioSource.Play();

    }
    public void PlayReverseAudio(string clipname, bool isLoop = false)
    {
        audioSource.pitch = -1;
        audioSource.clip = bgmList[clipname] ? bgmList[clipname] : seList[clipname];
        audioSource.Play();
        StartCoroutine(StopLoop(isLoop));
    }

    public IEnumerator StopLoop(bool isLoop)
    {
        yield return new WaitForSeconds(1f);
        audioSource.loop = isLoop;
    }

    public void SetAudioPitch(float pitch) {
        audioSource.pitch = pitch;
    }
}
