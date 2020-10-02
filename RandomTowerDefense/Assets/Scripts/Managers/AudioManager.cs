using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource;

    void Record()
    {
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

    float[] GetWaveform() 
    {
        float[] data=new float[Microphone.GetPosition("Built-in Microphone")];
        audioSource.clip.GetData(data, 0);
        return data;
    }

    public void PlayAudio(AudioClip audioClip)
    {
        audioSource.pitch = 1;
        audioSource.clip = audioClip;
        audioSource.Play();

    }
    public void PlayReverseAudio(AudioClip audioClip)
    {
        audioSource.pitch = -1;
        audioSource.loop = true;
        audioSource.clip = audioClip;
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
