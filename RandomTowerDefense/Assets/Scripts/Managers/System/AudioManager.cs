using System.Collections;
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
    bool enabledBGM;
    [HideInInspector]
    bool enabledSE;

    public void EnableBGM(bool enable) {
        enabledBGM = enable;
        PlayerPrefs.SetInt("BGM", enabledBGM ? 1 : 0);
        foreach (Toggle i in bgmUI)
            i.isOn = enabledBGM;
    }

    public void EnableSE(bool enable)
    {
        enabledSE = enable;
        PlayerPrefs.SetInt("SE", enabledSE ? 1 : 0);
        foreach (Toggle i in seUI)
            i.isOn = enabledSE;
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
            audioSource[0].pitch = 1;
            audioSource[0].clip = bgmList[clipname];
            audioSource[0].loop = isLoop;
            audioSource[0].Play();
        }
        else {
            audioSource[1].pitch = 1;
            audioSource[1].clip = seList[clipname];
            audioSource[1].loop = isLoop;
            audioSource[1].Play();
        }
    }
    public void PlayReverseAudio(string clipname, bool isLoop = false)
    {
        if (bgmList.ContainsKey(clipname))
        {
            audioSource[0].pitch = -1;
            audioSource[0].clip = bgmList[clipname];
            audioSource[0].loop = isLoop;
            audioSource[0].Play();
            StartCoroutine(StopLoop(isLoop,0));
        }
        else
        {
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
