﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleOperation: ISceneChange
{
    //Camera Start/Stay/End Point
    [Header("Camera Settings")]
    public GameObject RightCam;
    public List<Vector3> RightCamStartPt;
    public List<Vector3> RightCamStayPt;
    public List<Vector3> RightCamEndPt;
    public GameObject BottomCam;
    public List<Vector3> BottomCamStartPt;
    public List<Vector3> BottomCamStayPt;
    public List<Vector3> BottomCamEndPt;

    [Header("Other Settings")]
    public GameObject BoidSpawn;
    public GameObject LandscapeFadeImg;
    public GameObject PortraitFadeImg;

    FadeEffectUI LandscapeFade;
    FadeEffectUI PortraitFade;

    //Manager
    AudioManager AudioManager;
    CameraManager CameraManager;
    CanvaManager CanvaManager;
    InputManager InputManager;
    bool isOpening;
    bool isOption;
    bool showCredit;

    List<SaveObject> save;

    enum RecordCameraState {
        StarttoStay,
        Stay,
        StaytoExit,
        Exit
    };
    int currentRecStatusRightCam;
    int nextRecStatusRightCam;
    RecordCameraState rightCamState;

    int currentRecStatusBottomCam;
    int nextRecStatusBottomCam;
    RecordCameraState bottomCamState;

    const int maxRecStatus = 4;
    const int maxRecFrame = 20;
    const float maxRecWait = 5f;


    private void OnEnable()
    {
        BoidSpawn.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        isOpening = true;
        isOption = false;
        showCredit = false;

        rightCamState = RecordCameraState.Exit;
        currentRecStatusRightCam = 1;
        nextRecStatusRightCam = (currentRecStatusRightCam % maxRecStatus) + 1;
        bottomCamState = RecordCameraState.Exit;
        currentRecStatusBottomCam = 1;
        nextRecStatusBottomCam = (currentRecStatusBottomCam % maxRecStatus) + 1;

        InputManager = FindObjectOfType<InputManager>();
        AudioManager = FindObjectOfType<AudioManager>();
        CameraManager = FindObjectOfType<CameraManager>();
        CanvaManager = FindObjectOfType<CanvaManager>();

        LandscapeFade = LandscapeFadeImg.GetComponent<FadeEffectUI>();
        PortraitFade = PortraitFadeImg.GetComponent<FadeEffectUI>();

        AudioManager.PlayAudio("bgm_Opening");

        SaveSystem.Init();
        //save = new List<SaveObject>();
        //for (int i = 0; i < 4; i++)
        //{
        //    save.Add(SaveSystem.LoadObject<SaveObject>("Record" + i.ToString()));
        //}
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
        //Change Scene
        if (isSceneFinished && (LandscapeFade && LandscapeFade.isReady) && (PortraitFade && PortraitFade.isReady)) { 
            SceneManager.LoadScene(PlayerPrefs.GetString("nextScene"));
            return;
        }

        if (CameraManager) CameraManager.isOpening = isOpening;
        if (CanvaManager) CanvaManager.isOpening = isOpening;

        if (isOpening && InputManager.GetAnyInput()) BoidSpawnEffect();
        if (showCredit && InputManager.GetAnyInput()) { 
            showCredit = false; 
            nextRecStatusRightCam = 1; 
            nextRecStatusBottomCam = 1;
        }

        if (!isOpening){
            if (rightCamState == RecordCameraState.Exit)
                StartCoroutine(RecRightCamOperation());
            if (bottomCamState == RecordCameraState.Exit)
                StartCoroutine(RecBottomCamOperation());
        }

        if (Input.GetMouseButtonDown(1)) MoveToStageSelection();
    }

    public void MoveToStageSelection() {
        SetNextScene("StageSelection");
        SceneOut();
    }

    void BoidSpawnEffect()
    {
        BoidSpawn.transform.position = Camera.main.transform.position- Camera.main.transform.forward*2.0f;
        GameObject.FindGameObjectWithTag("BoidWall").transform.position = BoidSpawn.transform.position;
        BoidSpawn.SetActive(true);
        isOpening = false;
        AudioManager.PlayAudio("bgm_Title");
        AudioManager.PlayAudio("se_Button");
    }

    public void OptionStatus(bool enabled) {
        isOption = enabled;
        CanvaManager.isOption = enabled;
    }

    public void EnableCredit() {
        showCredit = !showCredit;
        nextRecStatusRightCam = 0;
        Debug.Log("reach");
    }

    private IEnumerator RecRightCamOperation()
    {
        Vector3 spd;
        int frame;

        //StarttoStay
        rightCamState = RecordCameraState.StarttoStay;
        RightCam.transform.position = RightCamStartPt[currentRecStatusRightCam];

        spd = RightCamStayPt[currentRecStatusRightCam] - RightCamStartPt[currentRecStatusRightCam];
        spd /= maxRecFrame;
        frame = 0;

        while (frame<maxRecFrame)
        {
            frame++;
            RightCam.transform.position += spd;
            yield return new WaitForSeconds(0f);
        }

        //Stay
        rightCamState = RecordCameraState.Stay;
        yield return new WaitForSeconds(maxRecWait);

        while (showCredit && currentRecStatusRightCam == 0)
            yield return new WaitForSeconds(0f);


        //StaytoExit
        rightCamState = RecordCameraState.StaytoExit;
        spd = RightCamEndPt[currentRecStatusRightCam] - RightCamStayPt[currentRecStatusRightCam];
        spd /= maxRecFrame;
        frame = 0;

        while (frame < maxRecFrame)
        {
            frame++;
            RightCam.transform.position += spd;
            yield return new WaitForSeconds(0f);
        }

        //Exit
        rightCamState = RecordCameraState.Exit;
        currentRecStatusRightCam = nextRecStatusRightCam;
        nextRecStatusRightCam = (currentRecStatusRightCam% maxRecStatus)+1;
    }

    private IEnumerator RecBottomCamOperation()
    {
        Vector3 spd;
        int frame;

        //StarttoStay
        bottomCamState = RecordCameraState.StarttoStay;
        BottomCam.transform.position = BottomCamStartPt[currentRecStatusBottomCam];

        spd = BottomCamStayPt[currentRecStatusBottomCam] - BottomCamStartPt[currentRecStatusBottomCam];
        spd /= maxRecFrame;
        frame = 0;

        while (frame < maxRecFrame)
        {
            frame++;
            BottomCam.transform.position += spd;
            yield return new WaitForSeconds(0f);
        }

        //Stay
        bottomCamState = RecordCameraState.Stay;
        yield return new WaitForSeconds(maxRecWait);

        while (showCredit && currentRecStatusBottomCam == 0)
            yield return new WaitForSeconds(0f);


        //StaytoExit
        bottomCamState = RecordCameraState.StaytoExit;
        spd = BottomCamEndPt[currentRecStatusBottomCam] - BottomCamStayPt[currentRecStatusBottomCam];
        spd /= maxRecFrame;
        frame = 0;

        while (frame < maxRecFrame)
        {
            frame++;
            BottomCam.transform.position += spd;
            yield return new WaitForSeconds(0f);
        }

        //Exit
        bottomCamState = RecordCameraState.Exit;
        currentRecStatusBottomCam = nextRecStatusBottomCam;
        nextRecStatusBottomCam = (currentRecStatusBottomCam % maxRecStatus) + 1;
    }
}
