using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.VFX;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class StageSelectOperation : ISceneChange
{
    const int IslandNum = 4;
    const float bloomInt_Default = 1.5f;
    const float bloomInt_Max = 60.0f;

    //Camera Start/Stay/End Point
    [Header("MainCamera Settings")]
    public GameObject MainCam;
    public List<Vector3> MainCamStayPt;
    [Header("UICamera Settings")]
    public GameObject RightCam;
    public List<Vector3> RightCamStayPt;
    public GameObject BottomCam;
    public List<Vector3> BottomCamStayPt;
    [Header("OtherCamera Settings")]
    public GameObject DarkenCam;

    [Header("Button Settings")]
    public List<Button> OptionButton;
    public List<GameObject> OtherButtonH;
    public List<GameObject> OtherButtonV;

    [Header("Arrow Settings")]
    public List<GameObject> UILeftArrow;
    public List<GameObject> UIRightArrow;

    [Header("Other Settings")]
    public List<TextMesh> StageCustomText;
    public List<TextMesh> StageCustomTextDirect;
    public List<VisualEffect> SceneChgPS;
    public List<Slider> zoomSlider;
    public GameObject LandscapeFadeImg;
    public GameObject PortraitFadeImg;

    FadeEffect LandscapeFade;
    FadeEffect PortraitFade;

    public bool isDebugging;
    public VolumeProfile volumeProfile;

    int IslandNow = 0;
    int IslandNext = 0;
    int IslandEnabled = 0;

    //Manager
    AudioManager AudioManager;
    //CameraManager CameraManager;
    CanvaManager CanvaManager;
    //InputManager InputManager;
    GyroscopeManager GyroscopeManager;

    TouchScreenKeyboard keyboard;

    float TimeRecord = 0;
    const float TimeWait = 0.5f;

    //CameraOperation
    const int maxRecFrame = 20;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        base.SceneIn();

        IslandNow = PlayerPrefs.GetInt("IslandNow");
        IslandNext = IslandNow;
        MainCam.transform.position = MainCamStayPt[IslandNow];
        RightCam.transform.position = RightCamStayPt[IslandNow];
        BottomCam.transform.position = BottomCamStayPt[IslandNow];

        if (isDebugging) IslandEnabled = IslandNum;
        else IslandEnabled = PlayerPrefs.GetInt("IslandEnabled");

        GameObject[] ClearMarks = GameObject.FindGameObjectsWithTag("ClearMark");
        for (int i = 0; i < ClearMarks.Length; ++i)
            ClearMarks[i].SetActive(i < IslandEnabled);

        for (int i = 0; i < StageCustomText.Count; ++i)
            StageInfoOperation(i, 0);

        LandscapeFade = LandscapeFadeImg.GetComponent<FadeEffect>();
        PortraitFade = PortraitFadeImg.GetComponent<FadeEffect>();

        //InputManager = FindObjectOfType<InputManager>();
        AudioManager = FindObjectOfType<AudioManager>();
        //CameraManager = FindObjectOfType<CameraManager>();
        CanvaManager = FindObjectOfType<CanvaManager>();
        GyroscopeManager = FindObjectOfType<GyroscopeManager>();

        AudioManager.PlayAudio("bgm_Title");

        UnityEngine.Rendering.Universal.Bloom bloom;
        volumeProfile.TryGet<UnityEngine.Rendering.Universal.Bloom>(out bloom);
        bloom.intensity.value = bloomInt_Max;

        StartCoroutine("SceneChgAnimation");
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
        //Change Scene
        if (isSceneFinished && ((LandscapeFade && LandscapeFade.isReady) || (PortraitFade && PortraitFade.isReady)))
        {
            SceneManager.LoadScene("LoadingScene");
            return;
        }

        foreach (Button i in OptionButton)
            i.interactable = !isOption;

        foreach (GameObject i in OtherButtonH)
        {
            i.SetActive(!isOption && !isSceneFinished && IslandNow == IslandNum - 1 && IslandNext == IslandNum - 1 && (Screen.width > Screen.height));
        }

        foreach (GameObject i in OtherButtonV)
        {
            i.SetActive(!isOption && !isSceneFinished && IslandNow == IslandNum - 1 && IslandNext == IslandNum - 1 && (Screen.width <= Screen.height));
        }

        foreach (Slider i in zoomSlider)
        {
            i.interactable = !isOption;
        }

        ArrowOperation();

        if (isSceneFinished) return;

        if (!isOption) ChangeIsland();
        DarkenCam.SetActive(isOption);
    }

    public void ArrowOperation()
    {
        foreach (GameObject i in UILeftArrow)
            i.SetActive(!isSceneFinished && IslandNow > 0);
        foreach (GameObject i in UIRightArrow)
            i.SetActive(!isSceneFinished && IslandNow < IslandEnabled-1);
    }

    public void ChangeIslandByButton(int chgValue)
    {
        IslandNext = Mathf.Clamp(IslandNow + chgValue, 0, IslandEnabled);
    }

    private void ChangeIslandByGyro()
    {
        //Gyroscope Operation
        if (GyroscopeManager.LeftShake)
            IslandNext = Mathf.Clamp(IslandNow - 1, 0, IslandEnabled);
        if (GyroscopeManager.RightShake)
            IslandNext = Mathf.Clamp(IslandNow + 1, 0, IslandEnabled);
    }

    public void ChangeIsland()
    {
        if (Time.time - TimeRecord < TimeWait) return;

        //Gyroscope Operation
        ChangeIslandByGyro();

        //Touch Operation
        switch(toDrag){
            case 0: default: break;
            case 1:
                IslandNext = Mathf.Clamp(IslandNow - 1, 0, IslandEnabled);
                break;
            case 2:
                IslandNext = Mathf.Clamp(IslandNow + 1, 0, IslandEnabled);
                break;
        }

        toDrag = 0;

        if (IslandNext != IslandNow)
        {
            StartCoroutine("RecMainCamOperation");
            StartCoroutine("RecRightCamOperation");
            StartCoroutine("RecBottomCamOperation");
            TimeRecord = Time.time;
        }
    }

    public void MoveToStage()
    {
        if (Time.time - TimeRecord < TimeWait) return;
        SetNextScene("GameScene");
        SceneOut();
        isSceneFinished = true;
        TimeRecord = Time.time;
    }

    public void OptionStatus()
    {
        if (Time.time - TimeRecord < TimeWait) return;
        isOption = !isOption;
        CanvaManager.isOption = isOption;
        GyroscopeManager.isFunctioning = !isOption;
        TimeRecord = Time.time;
    }

    private IEnumerator RecMainCamOperation()
    {
        Vector3 spd;
        int frame;

        MainCam.transform.position = MainCamStayPt[IslandNow];

        spd = MainCamStayPt[IslandNext] - MainCamStayPt[IslandNow];
        spd /= maxRecFrame;
        frame = 0;

        while (frame < maxRecFrame)
        {
            frame++;
            MainCam.transform.position += spd;
            yield return new WaitForSeconds(0f);
        }
      IslandNow = IslandNext;
    }

    private IEnumerator RecRightCamOperation()
    {
        Vector3 spd;
        int frame;

        RightCam.transform.position = RightCamStayPt[IslandNow];

        spd = RightCamStayPt[IslandNext] - RightCamStayPt[IslandNow];
        spd /= maxRecFrame;
        frame = 0;

        while (frame < maxRecFrame)
        {
            frame++;
            RightCam.transform.position += spd;
            yield return new WaitForSeconds(0f);
        }
    }
    private IEnumerator RecBottomCamOperation()
    {
        Vector3 spd;
        int frame;

        BottomCam.transform.position = BottomCamStayPt[IslandNow];

        spd = BottomCamStayPt[IslandNext] - BottomCamStayPt[IslandNow];
        spd /= maxRecFrame;
        frame = 0;

        while (frame < maxRecFrame)
        {
            frame++;
            BottomCam.transform.position += spd;
            yield return new WaitForSeconds(0f);
        }
    }

    public int CurrentIslandNum() { return IslandNow; }
    public int EnabledtIslandNum() { return IslandEnabled; }
    public int NextIslandNum() { return IslandNext; }

    private void StageInfoOperation(int infoID, int chg)
    {
        float result = StageInfo.SaveDataInPrefs(infoID, chg);
        StageCustomText[infoID].text = result.ToString();
    }

    public void StageInfoChgAdd(int infoID)
    {
        StageInfoOperation(infoID, 1);
    }
    public void StageInfoChgSubtract(int infoID)
    {
        StageInfoOperation(infoID, -1);
    }

    public void TouchKeybroad(int infoID)
    {
        keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, true);

        if (keyboard != null)
        {
            keyboard.characterLimit = 2;
            StartCoroutine(TouchScreenInputUpdate(infoID));
        }
    }

    private IEnumerator TouchScreenInputUpdate(int infoID)
    {
        while (keyboard.status == TouchScreenKeyboard.Status.Visible)
        {
            StageCustomText[infoID].text = keyboard.text;
            yield return new WaitForSeconds(0f);
        }

        if (keyboard.status == TouchScreenKeyboard.Status.Done || keyboard.status == TouchScreenKeyboard.Status.Canceled)
            switch (infoID)
            {
                case 0:
                case 1:
                case 4:
                    int Input_int;
                    if (int.TryParse(keyboard.text, out Input_int))
                    {
                        StageCustomText[infoID].text = Input_int.ToString();
                        StageInfo.SaveDataInPrefs_DirectInput(infoID, Input_int);
                    }
                    else
                    {
                        StageCustomText[infoID].text = StageInfo.SaveDataInPrefs(infoID, 0).ToString();
                    }
                    break;
                case 2:
                case 3:
                case 5:
                case 6:
                    float Output_int;
                    if (float.TryParse(keyboard.text, out Output_int))
                    {
                        StageCustomText[infoID].text = Output_int.ToString();
                        StageInfo.SaveDataInPrefs_DirectInput(infoID, Output_int);
                    }
                    else
                    {
                        StageCustomText[infoID].text = StageInfo.SaveDataInPrefs(infoID, 0).ToString();
                    }
                    break;
            }
        keyboard = null;
    }

    private IEnumerator SceneChgAnimation()
    {
        int frame = 200;
        UnityEngine.Rendering.Universal.Bloom bloom;
        volumeProfile.TryGet<UnityEngine.Rendering.Universal.Bloom>(out bloom);
        float bloomValOri = bloom.intensity.value;
        float bloomVal = bloomInt_Default;
        float bloomChg = (bloomVal - bloomValOri) / frame;
        bloomVal = bloomValOri;
        while (frame-- > 0)
        {
            bloomVal += bloomChg;
            bloom.intensity.value = bloomVal;
            yield return new WaitForSeconds(0f);
        }
        foreach (VisualEffect i in SceneChgPS)
            i.Stop();
    }
}