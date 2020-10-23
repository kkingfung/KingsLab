using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;

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

    [Header("Petrify Settings")]
    public List<Image> PetrifyImgs;
    public List<RawImage> PetrifyRImgs;
    public List<SpriteRenderer> PetrifySpr;

    [Header("Other Settings")]
    public List<GameObject> TitleImg;
    public List<GameObject> TitleEffectImg;
    public GameObject BoidSpawn;
    public GameObject LandscapeFadeImg;
    public GameObject PortraitFadeImg;

    private FadeEffect LandscapeFade;
    private FadeEffect PortraitFade;

    //Manager
    private AudioManager AudioManager;
    private CameraManager CameraManager;
    private CanvaManager CanvaManager;
    private InputManager InputManager;
    private GyroscopeManager GyroscopeManager;

    private bool isOpening;
    private bool isWaiting;
    private bool showCredit;
    private float TimeRecord = 0;
    private const float TimeWait = 0.2f;

    //CameraAnimation
    private const float targetCamAngle = -45f;

    enum RecordCameraState
    {
        StarttoStay,
        Stay,
        StaytoExit,
        Exit
    };

    private int currentRecStatusRightCam;
    private int nextRecStatusRightCam;
    private RecordCameraState rightCamState;

    private int currentRecStatusBottomCam;
    private int nextRecStatusBottomCam;
    private RecordCameraState bottomCamState;

    private const int maxRecStatus = 4;
    private const int maxRecFrame = 20;
    private const int maxRecWaitFrame = 120;

    private void OnEnable()
    {
        BoidSpawn.SetActive(true);
    }
    private void Awake()
    {
        PlayerPrefs.SetInt("StageID", 0);

        PlayerPrefs.SetFloat("zoomRate",0f);

        PlayerPrefs.SetFloat("waveNum", 1);
        PlayerPrefs.SetFloat("stageSize", 400);
        PlayerPrefs.SetFloat("enmNum", 1);
        PlayerPrefs.SetFloat("enmSpeed", 1);
        PlayerPrefs.SetFloat("spawnSpeed", 1);
        PlayerPrefs.SetFloat("hpMax", 5);
        PlayerPrefs.SetFloat("resource", 1);
    }
    // Start is called before the first frame update
    private void Start()
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
        PlayerPrefs.SetFloat("zoomRate", 0f);


        if (PetrifyImgs.Count > 0)
        {
            foreach (Image i in PetrifyImgs)
                i.material.SetFloat("_Progress", 0);
        }
        if (PetrifyRImgs.Count > 0)
        {
            foreach (RawImage i in PetrifyRImgs)
                i.material.SetFloat("_Progress", 0);
        }
        if (PetrifySpr.Count > 0)
        {
            foreach (SpriteRenderer i in PetrifySpr)
                i.material.SetFloat("_Progress", 0);
        }

        foreach (GameObject i in TitleEffectImg)
        {
            i.GetComponent<VisualEffect>().Stop();
        }
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();

        foreach (Button i in StartButton)
            i.interactable = !showCredit && !isOption && !isWaiting;
        foreach (Button i in OptionButton)
            i.interactable = !showCredit && !isWaiting;
        foreach (Button i in CreditButton)
            i.interactable = !isOption && !isWaiting;

        //Change Scene
        if (isSceneFinished && ((LandscapeFade && LandscapeFade.isReady) || (PortraitFade && PortraitFade.isReady)))
        {
            SceneManager.LoadScene("LoadingScene");
            return;
        }

        if (isSceneFinished || isWaiting) return;

        if (isOpening && InputManager.GetAnyInput()) {
            isWaiting = true;
            foreach (GameObject i in TitleEffectImg)
            {
                i.GetComponent<VisualEffect>().Play();
            }
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
    }

    private void LateUpdate()
    {
        if (isOption) {
            RightCam.GetComponent<Camera>().enabled = false;
            BottomCam.GetComponent<Camera>().enabled = false;
        }
    }
    #region CommonOperation
    public void MoveToStageSelection()
    {
        if (Time.time - TimeRecord < TimeWait) return;
        StartCoroutine(PetrifyAnimation());
        TimeRecord = Time.time;
        isWaiting = true;
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
    private IEnumerator PetrifyAnimation()
    {
        float progress = 0f;
        int frame = 60;
        float rate = 1 / (float)frame;
        while (frame-- > 0)
        {
            progress += rate;
            foreach (Image i in PetrifyImgs)
            {
                //i.material.EnableKeyword("_Progress");
                i.material.SetFloat("_Progress", progress);
            }
            foreach (RawImage i in PetrifyRImgs)
            {
                //i.material.EnableKeyword("_Progress");
                i.material.SetFloat("_Progress", progress);
            }
            foreach (SpriteRenderer i in PetrifySpr)
            {
                //i.material.EnableKeyword("_Progress");
                i.material.SetFloat("_Progress", progress);
            }
            yield return new WaitForSeconds(0f);
        }
        SetNextScene("StageSelection");
        SceneOut();
        isSceneFinished = true;
    }
    #endregion

    #region CamOperation
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
    #endregion

    #region OpeningOperation
    void OpeningAnimation()
    {
        CameraManager.RotateCam(targetCamAngle);

        GameObject[] Upper = GameObject.FindGameObjectsWithTag("UpperEgg");
        StartCoroutine(UpperEggAnimation(Upper[0], 0.4f));
        StartCoroutine(UpperEggAnimation(Upper[1], 0.4f));

        GameObject[] Lower = GameObject.FindGameObjectsWithTag("LowerEgg");
        StartCoroutine(LowerEggAnimation(Lower[0], -0.5f));
        StartCoroutine(LowerEggAnimation(Lower[1], -0.2f));

        isOpening = false;

        AudioManager.PlayAudio("bgm_Title");
        AudioManager.PlayAudio("se_Button");
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
                    i.GetComponent<RawImage>().color = new Color(frame / 40f, frame / 40f, frame / 40f, frame / 40f);
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
    #endregion
}
