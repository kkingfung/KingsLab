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

    public GameObject RightCam;
    public Camera RightCamComponent;
    public List<Vector3> RightCamStartPt;
    public List<Vector3> RightCamStayPt;
    public List<Vector3> RightCamEndPt;
    public GameObject BottomCam;
    public Camera BottomCamComponent;
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
    public List<Material> PetrifyMat;

    [Header("Other Settings")]
    public List<GameObject> TitleImg;
    public List<RawImage> TitleImgRaw;
    public List<Text> TitleImgText;

    public List<GameObject> TitleEffectImg;
    private List<VisualEffect> TitleEffectImgVFX;
    public List<string> bundleUrl;
    public List<string> bundleName;

    public GameObject BoidSpawn;
    public GameObject LandscapeFadeImg;
    public GameObject PortraitFadeImg;

    private FadeEffect LandscapeFade;
    private FadeEffect PortraitFade;

    //Manager
    public AudioManager AudioManager;
    public CameraManager CameraManager;
    public CanvaManager CanvaManager;
    public InputManager InputManager;
    public GyroscopeManager GyroscopeManager;

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
    private const float maxRecTimer = 0.5f;
    private const float maxRecWaitTimer = 5;

    private void OnEnable()
    {
        //BoidSpawn.SetActive(true);
        Time.timeScale = 1;
    }
    private void Awake()
    {
        base.Awake();

        PlayerPrefs.SetInt("StageID", 0);

        PlayerPrefs.SetFloat("zoomRate",0f);

        PlayerPrefs.SetFloat("waveNum", 1);
        PlayerPrefs.SetFloat("stageSize", 1);
        PlayerPrefs.SetFloat("enmNum", 1);
        PlayerPrefs.SetFloat("enmSpeed", 1);
        PlayerPrefs.SetFloat("obstaclePercent", 1);
        PlayerPrefs.SetFloat("spawnSpeed", 1);
        PlayerPrefs.SetFloat("hpMax", 10);
        PlayerPrefs.SetFloat("resource", 1);
    }
    // Start is called before the first frame update
    private void Start()
    {
        isOpening = true;
        isOption = false;
        showCredit = false;

        rightCamState = RecordCameraState.Exit;
        currentRecStatusRightCam = 1;
        nextRecStatusRightCam = (currentRecStatusRightCam % maxRecStatus) + 1;
        bottomCamState = RecordCameraState.Exit;
        currentRecStatusBottomCam = 1;
        nextRecStatusBottomCam = (currentRecStatusBottomCam % maxRecStatus) + 1;

        //InputManager = FindObjectOfType<InputManager>();
        //AudioManager = FindObjectOfType<AudioManager>();
        //CameraManager = FindObjectOfType<CameraManager>();
        //CanvaManager = FindObjectOfType<CanvaManager>();
        //GyroscopeManager = FindObjectOfType<GyroscopeManager>();

        LandscapeFade = LandscapeFadeImg.GetComponent<FadeEffect>();
        PortraitFade = PortraitFadeImg.GetComponent<FadeEffect>();

        AudioManager.PlayAudio("bgm_Opening");
        PlayerPrefs.SetFloat("zoomRate", 0f);

        foreach (Material mat in PetrifyMat)
        {
            mat.SetFloat("_Progress", 0);
        }

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

    TitleImgRaw = new List<RawImage>();
            TitleImgText = new List<Text>();
        foreach (GameObject i in TitleImg)
        {
            TitleImgRaw.Add(i.GetComponent<RawImage>());
            TitleImgText.Add(i.GetComponent<Text>());
        }

        TitleEffectImgVFX = new List<VisualEffect>();
        foreach (GameObject i in TitleEffectImg)
        {
            TitleEffectImgVFX.Add(i.GetComponent<VisualEffect>());
            TitleEffectImgVFX[TitleEffectImgVFX.Count-1].Stop();
        }

        RightCamComponent = RightCam.GetComponent<Camera>();
        BottomCamComponent = BottomCam.GetComponent<Camera>();

        for (int i = 0; i < bundleUrl.Count; ++i)
            LoadBundle.LoadAssetBundle(bundleUrl[i], bundleName[i], "Assets/AssetBundles");
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
            foreach (VisualEffect i in TitleEffectImgVFX)
            {
                i.Play();
            }
            StartCoroutine(PreparationToMain());
        }

        if (showCredit && InputManager.GetAnyInput()) {
            EnableCredit();
        }

        if (!isOpening){
            if (rightCamState == RecordCameraState.Exit)
                StartCoroutine(RecRightCamOperation());
            if (bottomCamState == RecordCameraState.Exit)
                StartCoroutine(RecBottomCamOperation());
        }

        DarkenCam.SetActive(isOption|| showCredit);
    }

    private void LateUpdate()
    {
        if (isOption) {
            RightCamComponent.enabled = false;
            BottomCamComponent.enabled = false;
        }
    }
    #region CommonOperation
    public void MoveToStageSelection()
    {
        if (Time.time - TimeRecord < TimeWait) return;
        StartCoroutine(PetrifyAnimation());
        TimeRecord = Time.time;
        isWaiting = true;
        AudioManager.PlayAudio("se_Button");
    }

    public void OptionStatus() 
    {
        if (Time.time - TimeRecord < TimeWait) return;
        isOption = !isOption;
        CanvaManager.isOption = isOption;
        GyroscopeManager.isFunctioning = !isOption;
        TimeRecord = Time.time;
        AudioManager.PlayAudio("se_Button");
    }

    public void EnableCredit()
    {
        if (Time.time - TimeRecord < TimeWait) return;
        showCredit = !showCredit;
        if (showCredit)
        {
            nextRecStatusRightCam = 0;
            nextRecStatusBottomCam = 0;
        }
        else
        {
            nextRecStatusRightCam = 1;
            nextRecStatusBottomCam = 1;
        }
        TimeRecord = Time.time;
        if (showCredit) AudioManager.PlayAudio("se_Button");
    }
    private IEnumerator PetrifyAnimation()
    {
        float progress = 0f;
        int frame = 15;
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
        float timer = 0;

        //StarttoStay
        rightCamState = RecordCameraState.StarttoStay;
        RightCam.transform.position = RightCamStartPt[currentRecStatusRightCam];

        spd = RightCamStayPt[currentRecStatusRightCam] - RightCamStartPt[currentRecStatusRightCam];
        spd /= maxRecTimer;

        while (timer < maxRecTimer)
        {
            timer += Time.deltaTime;
            if (timer > maxRecTimer) timer = maxRecTimer;
            RightCam.transform.position = RightCamStartPt[currentRecStatusRightCam] + spd * timer;
            yield return new WaitForSeconds(0f);
        }

        //Stay
        rightCamState = RecordCameraState.Stay;
        timer = 0;
        if (showCredit)
        {
            while (showCredit && currentRecStatusRightCam == 0)
                yield return new WaitForSeconds(0f);
        }
        else
        {
            while (timer < maxRecWaitTimer && !showCredit)
            {
                timer += Time.deltaTime;
                yield return new WaitForSeconds(0f);
            }
        }

        //StaytoExit
        rightCamState = RecordCameraState.StaytoExit;
        spd = RightCamEndPt[currentRecStatusRightCam] - RightCamStayPt[currentRecStatusRightCam];
        spd /= maxRecTimer;
        timer = 0;

        while (timer < maxRecTimer)
        {
            timer += Time.deltaTime;
            if (timer > maxRecTimer) timer = maxRecTimer;
            RightCam.transform.position = RightCamStayPt[currentRecStatusRightCam] + spd * timer;
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
        float timer = 0;

        //StarttoStay
        bottomCamState = RecordCameraState.StarttoStay;
        BottomCam.transform.position = BottomCamStartPt[currentRecStatusBottomCam];

        spd = BottomCamStayPt[currentRecStatusBottomCam] - BottomCamStartPt[currentRecStatusBottomCam];
        spd /= maxRecTimer;

        while (timer < maxRecTimer)
        {
            timer += Time.deltaTime;
            if (timer > maxRecTimer) timer = maxRecTimer;
            BottomCam.transform.position = BottomCamStartPt[currentRecStatusBottomCam] + spd * timer;
            yield return new WaitForSeconds(0f);
        }

        //Stay
        bottomCamState = RecordCameraState.Stay;
        timer = 0;
        if (showCredit)
        {
            while (showCredit && currentRecStatusBottomCam == 0)
                yield return new WaitForSeconds(0f);
        }
        else
        {
            while (timer < maxRecWaitTimer && !showCredit)
            {
                timer += Time.deltaTime;
                yield return new WaitForSeconds(0f);
            }
        }

        //StaytoExit
        bottomCamState = RecordCameraState.StaytoExit;
        spd = BottomCamEndPt[currentRecStatusBottomCam] - BottomCamStayPt[currentRecStatusBottomCam];
        spd /= maxRecTimer;
        timer = 0;

        while (timer < maxRecTimer)
        {
            timer += Time.deltaTime;
            if (timer > maxRecTimer) timer = maxRecTimer;
            BottomCam.transform.position = BottomCamStayPt[currentRecStatusBottomCam] + spd * timer;
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

        AudioManager.PlayAudio("bgm_Title",true);
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
            for (int i = 0; i < TitleImg.Count; ++i)
            {
                if (TitleImgRaw[i])
                    TitleImgRaw[i].color = new Color(frame / 40f, frame / 40f, frame / 40f, frame / 40f);
                if (TitleImgText[i])
                    TitleImgText[i].color = new Color(1, 1, 1, frame / 20f);
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
