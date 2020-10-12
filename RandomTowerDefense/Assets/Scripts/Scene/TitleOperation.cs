using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.Experimental.TerrainAPI;

public class TitleOperation: ISceneChange
{
    //Camera Start/Stay/End Point
    [Header("Camera Settings")]
    public GameObject DarkenCam;
    public GameObject DarkenCamSub;

    public GameObject RightCam;
    public List<Vector3> RightCamStartPt;
    public List<Vector3> RightCamStayPt;
    public List<Vector3> RightCamEndPt;
    public GameObject BottomCam;
    public List<Vector3> BottomCamStartPt;
    public List<Vector3> BottomCamStayPt;
    public List<Vector3> BottomCamEndPt;

    [Header("Button Settings")]
    public List<Button> StartButton;
    public List<Button> OptionButton;
    public List<Button> CreditButton;

    [Header("Other Settings")]
    public List<GameObject> TitleImg;
    public GameObject BoidSpawn;
    public GameObject LandscapeFadeImg;
    public GameObject PortraitFadeImg;

    FadeEffect LandscapeFade;
    FadeEffect PortraitFade;

    //Manager
    AudioManager AudioManager;
    CameraManager CameraManager;
    CanvaManager CanvaManager;
    InputManager InputManager;
    GyroscopeManager GyroscopeManager;

    bool isOpening;
    bool isWaiting;
    bool showCredit;
    float TimeRecord = 0;
    const float TimeWait = 0.2f;

    //CameraAnimation
    const float targetCamAngle = -45f;

    enum RecordCameraState
    {
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
    const int maxRecWaitFrame = 120;


    private void OnEnable()
    {
        BoidSpawn.SetActive(true);
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
        GyroscopeManager = FindObjectOfType<GyroscopeManager>();

        LandscapeFade = LandscapeFadeImg.GetComponent<FadeEffect>();
        PortraitFade = PortraitFadeImg.GetComponent<FadeEffect>();

        AudioManager.PlayAudio("bgm_Opening");
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
        if (isWaiting) return;
        //Change Scene
        if (isSceneFinished && ((LandscapeFade && LandscapeFade.isReady) || (PortraitFade && PortraitFade.isReady)))
        {
            SceneManager.LoadScene("LoadingScene");
            return;
        }

        if (isSceneFinished) return;

        if (isOpening && InputManager.GetAnyInput()) {
            isWaiting = true;
            StartCoroutine(PreparationToMain());
        }

        if (showCredit && InputManager.GetAnyInput()) { 
            showCredit = false; 
            nextRecStatusRightCam = 1; 
            nextRecStatusBottomCam = 1;
            TimeRecord = Time.time;
        }

        if (!isOpening){
            if (rightCamState == RecordCameraState.Exit)
                StartCoroutine(RecRightCamOperation());
            if (bottomCamState == RecordCameraState.Exit)
                StartCoroutine(RecBottomCamOperation());
        }

        DarkenCam.SetActive(isOption);
        DarkenCamSub.SetActive(showCredit);

            foreach (Button i in StartButton)
                i.interactable = !showCredit && !isOption;
            foreach (Button i in OptionButton)
                i.interactable = !showCredit;
            foreach (Button i in CreditButton)
                i.interactable = !isOption;
    }

    public void MoveToStageSelection()
    {
        if (Time.time - TimeRecord < TimeWait) return;
        SetNextScene("StageSelection");
        SceneOut();
        isSceneFinished = true;
        TimeRecord = Time.time;
    }

    void OpeningAnimation()
    {
        CameraManager.RotateCam(targetCamAngle);

        GameObject[] Upper = GameObject.FindGameObjectsWithTag("UpperEgg");
        StartCoroutine(UpperEggAnimation(Upper[0], 0.4f));
        StartCoroutine(UpperEggAnimation(Upper[1], 0.4f));

        GameObject[] Lower = GameObject.FindGameObjectsWithTag("LowerEgg");
        StartCoroutine(LowerEggAnimation(Lower[0], -0.4f));
        StartCoroutine(LowerEggAnimation(Lower[1], -0.15f));

        isOpening = false;

        AudioManager.PlayAudio("bgm_Title");
        AudioManager.PlayAudio("se_Button");
    }

    public void OptionStatus() 
    {
        if (Time.time - TimeRecord < TimeWait) return;
        isOption = !isOption;
        CanvaManager.isOption = isOption;
        GyroscopeManager.isFunctioning = !isOption;
        TimeRecord = Time.time;
    }

    public void EnableCredit() 
    {
        if (Time.time - TimeRecord < TimeWait) return;
        showCredit = !showCredit;
        nextRecStatusRightCam = 0;
        nextRecStatusBottomCam = 0;
        TimeRecord = Time.time;
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
        frame = 0;
        if (showCredit)
        {
            while (showCredit && currentRecStatusRightCam == 0)
                yield return new WaitForSeconds(0f);
        }
        else
        {
            while (frame < maxRecWaitFrame && !showCredit)
            {
                frame++;
                yield return new WaitForSeconds(0f);
            }
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
        frame = 0;
        if (showCredit)
        {
            while (showCredit && currentRecStatusBottomCam == 0)
                yield return new WaitForSeconds(0f);
        }
        else
        {
            while (frame < maxRecWaitFrame && !showCredit)
            {
                frame++;
                yield return new WaitForSeconds(0f);
            }
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

    private IEnumerator UpperEggAnimation(GameObject Upper,float dist)
    {
       int frame = 30;
        float posChgsbyFrame = (dist - this.transform.localPosition.y) / frame;
        while (frame-- >0)
        {
                Upper.transform.localPosition = new Vector3(Upper.transform.localPosition.x,
                    Upper.transform.localPosition.y + posChgsbyFrame, Upper.transform.localPosition.z);
            yield return new WaitForSeconds(0f);
        }
    }

    private IEnumerator LowerEggAnimation(GameObject Lower, float dist)
    {
        int frame = 50;
        float posChgsbyFrame = (dist - this.transform.localPosition.y) / frame;
        while (frame-- > 0)
        {
            Lower.transform.localPosition = new Vector3(Lower.transform.localPosition.x,
                    Lower.transform.localPosition.y + posChgsbyFrame, Lower.transform.localPosition.z);
            yield return new WaitForSeconds(0f);
        }
    }
    private IEnumerator PreparationToMain()
    {
        int frame = 20;
        
        while (frame-- > 0)
        {
            foreach (GameObject i in TitleImg)
            {
                if (i.GetComponent<RawImage>())
                    i.GetComponent<RawImage>().color = new Color(1, 1, 1, frame / 20f);
                if (i.GetComponent<Text>())
                    i.GetComponent<Text>().color = new Color(1, 1, 1, frame / 20f);
            }

            yield return new WaitForSeconds(0f);
        }

        OpeningAnimation();
        isWaiting = false;
        if (CameraManager) CameraManager.isOpening = isOpening;
        if (CanvaManager) CanvaManager.isOpening = isOpening;
    }
    
}
