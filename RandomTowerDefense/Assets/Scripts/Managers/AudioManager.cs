using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AudioManager : MonoBehaviour
{
    private AudioSource audioSource;
    public Dictionary<string, AudioClip> bgmList;
    public Dictionary<string, AudioClip> seList;
    void Start() 
    {
        audioSource = GetComponent<AudioSource>();
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

    public void PlayAudio(string clipname)
    {
        audioSource.pitch = 1;
        audioSource.clip = bgmList[clipname] ? bgmList[clipname] : seList[clipname];
        audioSource.Play();

    }
    public void PlayReverseAudio(string clipname)
    {
        audioSource.pitch = -1;
        audioSource.loop = true;
        audioSource.clip = bgmList[clipname] ? bgmList[clipname] : seList[clipname];
        audioSource.Play();
        StartCoroutine(StopLoop());
    }

    public IEnumerator StopLoop()
    {
        yield return new WaitForSeconds(1f);
        audioSource.loop = false;
    }

    public void SetAudioPitch(float pitch) {
        audioSource.pitch = pitch;
    }
}
