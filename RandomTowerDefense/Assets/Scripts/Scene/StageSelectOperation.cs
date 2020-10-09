using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StageSelectOperation : ISceneChange
{
    const int IslandNum = 4;

    [Header("Debugger")]
    public bool isDebugging;

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
    public List<Button> OtherButton;

    [Header("Arrow Settings")]
    public List<GameObject> UILeftArrow;
    public List<GameObject> UIRightArrow;

    [Header("Other Settings")]
    public GameObject LandscapeFadeImg;
    public GameObject PortraitFadeImg;

    FadeEffectUI LandscapeFade;
    FadeEffectUI PortraitFade;

    int IslandNow = 0;
    int IslandNext = 0;
    int IslandEnabled = 0;

    //Manager
    AudioManager AudioManager;
    CameraManager CameraManager;
    CanvaManager CanvaManager;
    InputManager InputManager;
    GyroscopeManager GyroscopeManager;

    bool isOption;
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

        if (isDebugging) IslandEnabled = IslandNum;
        else IslandEnabled = PlayerPrefs.GetInt("IslandEnabled");

        InputManager = FindObjectOfType<InputManager>();
        AudioManager = FindObjectOfType<AudioManager>();
        CameraManager = FindObjectOfType<CameraManager>();
        CanvaManager = FindObjectOfType<CanvaManager>();
        GyroscopeManager = FindObjectOfType<GyroscopeManager>();

        LandscapeFade = LandscapeFadeImg.GetComponent<FadeEffectUI>();
        PortraitFade = PortraitFadeImg.GetComponent<FadeEffectUI>();

        AudioManager.PlayAudio("bgm_Title");
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

        if (isSceneFinished) return;

        if (!isOption) ChangeIsland();
        ArrowOperation();
        DarkenCam.SetActive(isOption);
        foreach (Button i in OptionButton)
            i.interactable = !isOption;
        foreach (Button i in OtherButton)
            i.interactable = !isOption;
    }

    public void ArrowOperation()
    {
        foreach (GameObject i in UILeftArrow)
            i.SetActive(IslandNow > 0);
        foreach (GameObject i in UIRightArrow)
            i.SetActive(IslandNow < IslandEnabled);
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
        //Update Curr Island ID
        if (IslandNow!= IslandNext && MainCam.transform.position == MainCamStayPt[IslandNext]
            && RightCam.transform.position == RightCamStayPt[IslandNext]
            && BottomCam.transform.position == BottomCamStayPt[IslandNext])
            IslandNow = IslandNext;

        if (Time.time - TimeRecord < TimeWait) return;

        //Gyroscope Operation
        ChangeIslandByGyro();

        if (IslandNext != IslandNow)
        {
            StartCoroutine("RecMainCamOperation");
            StartCoroutine("RecRightCamOperation");
            StartCoroutine("RecBottomCamOperation");
        }

        TimeRecord = Time.time;
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
}
