using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageSelectOperation : ISceneChange
{
    [Header("Debugger")]
    public bool isDebugging;

    //Camera Start/Stay/End Point
    [Header("MainCamera Settings")]
    public GameObject MainCam;
    public List<Vector3> MainCamStartPt;
    public List<Vector3> MainCamStayPt;
    public List<Vector3> MainCamEndPt;
    [Header("UICamera Settings")]
    public GameObject RightCam;
    public List<Vector3> RightCamStartPt;
    public List<Vector3> RightCamStayPt;
    public List<Vector3> RightCamEndPt;
    public GameObject BottomCam;
    public List<Vector3> BottomCamStartPt;
    public List<Vector3> BottomCamStayPt;
    public List<Vector3> BottomCamEndPt;
    [Header("OtherCamera Settings")]
    public GameObject DarkenCam;
    public GameObject DarkenCamSub;

    int IslandNow = 0;
    int IslandEnabled = 0;
    const int IslandNum = 4;

    //Manager
    AudioManager AudioManager;
    CameraManager CameraManager;
    CanvaManager CanvaManager;
    InputManager InputManager;
    GyroscopeManager GyroscopeManager;

    bool isOption;
    int ButtonWait = 0;
    const int ButtonWaitCnt = 5;

    //CustomizedIslandDetail
    int stageSize = 0;

    //CameraOperation
    enum RecordCameraState
    {
        StarttoStay,
        Stay,
        StaytoExit,
        Exit
    };

    int currentRecStatusMainCam;
    int nextRecStatusMainCam;
    RecordCameraState mainCamState;

    int currentRecStatusRightCam;
    int nextRecStatusRightCam;
    RecordCameraState rightCamState;

    int currentRecStatusBottomCam;
    int nextRecStatusBottomCam;
    RecordCameraState bottomCamState;

    const int maxRecStatus = 4;
    const int maxRecFrame = 20;
    const int maxRecWaitFrame = 120;

    // Start is called before the first frame update
    void Start()
    {
        IslandNow = PlayerPrefs.GetInt("IslandNow");
        if (isDebugging) IslandEnabled = IslandNum;
        else IslandEnabled = PlayerPrefs.GetInt("IslandEnabled");

    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator RecMainCamOperation()
    {
        Vector3 spd;
        int frame;

        //StarttoStay
        mainCamState = RecordCameraState.StarttoStay;
        MainCam.transform.position = MainCamStartPt[currentRecStatusMainCam];

        spd = MainCamStayPt[currentRecStatusMainCam] - MainCamStartPt[currentRecStatusMainCam];
        spd /= maxRecFrame;
        frame = 0;

        while (frame < maxRecFrame)
        {
            frame++;
            MainCam.transform.position += spd;
            yield return new WaitForSeconds(0f);
        }

        //Stay
        mainCamState = RecordCameraState.Stay;
        frame = 0;

            while (frame < maxRecWaitFrame)
            {
                frame++;
                yield return new WaitForSeconds(0f);
            }

        //StaytoExit
        mainCamState = RecordCameraState.StaytoExit;
        spd = MainCamEndPt[currentRecStatusMainCam] - MainCamStayPt[currentRecStatusMainCam];
        spd /= maxRecFrame;
        frame = 0;

        while (frame < maxRecFrame)
        {
            frame++;
            MainCam.transform.position += spd;
            yield return new WaitForSeconds(0f);
        }

        //Exit
        mainCamState = RecordCameraState.Exit;
        currentRecStatusMainCam = nextRecStatusMainCam;
        nextRecStatusMainCam = (currentRecStatusMainCam % maxRecStatus) + 1;
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

        while (frame < maxRecFrame)
        {
            frame++;
            RightCam.transform.position += spd;
            yield return new WaitForSeconds(0f);
        }

        //Stay
        rightCamState = RecordCameraState.Stay;
        frame = 0;

            while (frame < maxRecWaitFrame)
            {
                frame++;
                yield return new WaitForSeconds(0f);
            }

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
        nextRecStatusRightCam = (currentRecStatusRightCam % maxRecStatus) + 1;
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
        frame = 0;

            while (frame < maxRecWaitFrame)
            {
                frame++;
                yield return new WaitForSeconds(0f);
            }

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
