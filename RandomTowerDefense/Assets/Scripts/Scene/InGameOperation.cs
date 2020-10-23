using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEditor.ShaderGraph.Internal;

public class InGameOperation : ISceneChange
{
    public enum ScreenShownID {
        SSIDArena = 0,
        SSIDTopLeft = 1,
        SSIDTop = 2,
        SSIDTopRight = 3,
    }

    //Camera Start/Stay/End Point
    [Header("MainCamera Settings")]
    public GameObject MainCam;
    public List<Vector3> MainCamStayPt;
    public List<Vector3> MainCamRotationAngle;
    public GameObject StoreGp;//MainWith MainCam

    [Header("OtherCamera Settings")]
    public GameObject DarkenCam;

    [Header("UI Settings")]
    public List<Text> UIIslandName;
    public List<Text> UICurrentGold;

    [Header("Button Settings")]
    public List<Button> OptionButton;

    [Header("Arrow Settings")]
    public List<GameObject> UIUpArrow;
    public List<GameObject> UIDownArrow;
    public List<GameObject> UILeftArrow;
    public List<GameObject> UIRightArrow;

    [Header("Petrify Settings")]
    public List<Image> PetrifyImgs;
    public List<RawImage> PetrifyRImgs;
    public List<SpriteRenderer> PetrifySpr;

    [Header("Other Settings")]
    public Material FloorMat;
    public GameObject LandscapeFadeImg;
    public GameObject PortraitFadeImg;

    private FadeEffect LandscapeFade;
    private FadeEffect PortraitFade;

    public bool isDebugging;//For ingame Debugger
    public bool isTutorial;//For 1st Stage Only
    //public VolumeProfile volumeProfile; // For Spare

    private int IslandNow = 0;//For changing colour of Sea/Sky
    private int IslandEnabled = 0;//Check when win

    [HideInInspector]
    public int currScreenShown = 0;//0:Main, 1:Top-Left, 2:Top, 3:Top-Right
    private int nextScreenShown = 0;
    private bool isScreenChanging = false;

    //Manager
    private AudioManager AudioManager;
    //private CameraManager CameraManager;
    private CanvaManager CanvaManager;
    //private InputManager InputManager;
    private GyroscopeManager GyroscopeManager;
    private ResourceManager resourceManager;
    private WaveManager waveManager;
    private TimeManager timeManager;

    private TouchScreenKeyboard keyboard;//For RecordBroad if top 5
    private bool CancelKeybroad = false;

    //Prevent DoubleHit
    private float TimeRecord = 0;
    private const float TimeWait = 0.01f;

    //CameraOperation
    private const int maxRecFrame = 20;

    private void Awake()
    {
        StageInfo.Init();
        TowerInfo.Init();
        EnemyInfo.Init();
        SkillInfo.Init();
        SkillStack.init();
        Upgrades.init();
    }
    // Start is called before the first frame update
    private void Start()
    {
        base.Start();
        base.SceneIn();

        IslandNow = PlayerPrefs.GetInt("IslandNow");
        FloorMat.SetFloat("ShapesSides",3+ IslandNow);
        MainCam.transform.position = MainCamStayPt[0];
        MainCam.transform.rotation = Quaternion.Euler(MainCamRotationAngle[0]);

        DarkenCam.SetActive(false);

        LandscapeFade = LandscapeFadeImg.GetComponent<FadeEffect>();
        PortraitFade = PortraitFadeImg.GetComponent<FadeEffect>();

        //InputManager = FindObjectOfType<InputManager>();
        AudioManager = FindObjectOfType<AudioManager>();
        //CameraManager = FindObjectOfType<CameraManager>();
        CanvaManager = FindObjectOfType<CanvaManager>();
        GyroscopeManager = FindObjectOfType<GyroscopeManager>();
        waveManager = FindObjectOfType<WaveManager>();
        resourceManager = FindObjectOfType<ResourceManager>();
        timeManager = FindObjectOfType<TimeManager>();

        AudioManager.PlayAudio("bgm_Battle",true);

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

        foreach (Text i in UIIslandName)
        {
            switch (IslandNow) {
                case 0: i.text = "ヒジリカ島";
                    FloorMat.SetColor("_Color",new Color(0.34f,1f,0f));
                    break;
                case 1: i.text = "テンシュキ島";
                    FloorMat.SetColor("_Color", new Color(0.82f, 0.47f, 1f));
                    break;
                case 2: i.text = "ニモハサ島";
                    FloorMat.SetColor("_Color", new Color(1f, 0.2f, 0f));
                    break;
                case 3: i.text = "ギイシカ島";
                    FloorMat.SetColor("_Color", new Color(1f, 0.7f, 0f));
                    break;
            }
        }

        toDrag = -1;
        isOption = false;
 
    }

    // Update is called once per frame
    private void Update()
    {
        base.Update();

        foreach (Button i in OptionButton)
            i.interactable = !isOption && !isSceneFinished;

        //Change Scene
        if (isSceneFinished && ((LandscapeFade && LandscapeFade.isReady) || (PortraitFade && PortraitFade.isReady)))
        {
            SceneManager.LoadScene("LoadingScene");
            return;
        }

        ArrowOperation();
        ChangeScreenShownByGyro();
        ChangeScreenShownByDrag();

        foreach (Text i in UICurrentGold)
        {
            i.text = resourceManager.GetCurrMaterial().ToString()+"G";
        }
    }

    #region CommonOperation
    public void ArrowOperation()
    {
        //0:Main, 1:Top-Left, 2:Top, 3:Top-Right
        switch ((ScreenShownID)currScreenShown)
        {
            case ScreenShownID.SSIDArena:
                foreach (GameObject i in UIUpArrow) i.SetActive(true);
                foreach (GameObject i in UIDownArrow) i.SetActive(false);
                foreach (GameObject i in UILeftArrow) i.SetActive(false);
                foreach (GameObject i in UIRightArrow) i.SetActive(false);
                break;
            case ScreenShownID.SSIDTopLeft:
                foreach (GameObject i in UIUpArrow) i.SetActive(false);
                foreach (GameObject i in UIDownArrow) i.SetActive(true);
                foreach (GameObject i in UILeftArrow) i.SetActive(false);
                foreach (GameObject i in UIRightArrow) i.SetActive(true);
                break;
            case ScreenShownID.SSIDTop:
                foreach (GameObject i in UIUpArrow) i.SetActive(false);
                foreach (GameObject i in UIDownArrow) i.SetActive(true);
                foreach (GameObject i in UILeftArrow) i.SetActive(true);
                foreach (GameObject i in UIRightArrow) i.SetActive(true);
                break;
            case ScreenShownID.SSIDTopRight:
                foreach (GameObject i in UIUpArrow) i.SetActive(false);
                foreach (GameObject i in UIDownArrow) i.SetActive(true);
                foreach (GameObject i in UILeftArrow) i.SetActive(true);
                foreach (GameObject i in UIRightArrow) i.SetActive(false);
                break;
        }
    }

    public void MoveToStage(int SceneID)
    {
        if (Time.time - TimeRecord < TimeWait) return;
        StartCoroutine(PetrifyAnimation(SceneID));
        TimeRecord = Time.time;
    }

    public void OptionStatus()
    {
        if (Time.time - TimeRecord < TimeWait) return;
        isOption = !isOption;
        CanvaManager.isOption = isOption;
        GyroscopeManager.isFunctioning = !isOption;
        TimeRecord = Time.time;
        timeManager.TimeControl();
        DarkenCam.SetActive(isOption);
    }

    private IEnumerator PetrifyAnimation(int sceneID)
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

        SetNextScene(sceneID==0?"GameScene": "StageSelection");

        PlayerPrefs.SetInt("IslandNow", IslandNow);
        SceneOut();
        isSceneFinished = true;
    }
    #endregion

    #region CameraOperation
    public void ChangeScreenShownByButton(int chgValue)
    {
        //DownArrow:0,LeftArrow:1,UpArrow:2 ,RightArrow:3
        switch (chgValue) {
            case 0:
                if (currScreenShown == 0) return;
                nextScreenShown = 0;
                break;
            case 1:
                if (currScreenShown == 2 || currScreenShown == 3) nextScreenShown = currScreenShown - 1;
                else return;
                break;
            case 2:
                if (currScreenShown==0) nextScreenShown =2;
                else return;
                break;
            case 3:
                if (currScreenShown == 1 || currScreenShown == 2) nextScreenShown = currScreenShown + 1;
                else return;
                break;
        }
        if (nextScreenShown != currScreenShown)
        {
            isScreenChanging = true;
            StartCoroutine(ChangeScreenShown());
        }
    }

    private void ChangeScreenShownByGyro()
    {
        if (isScreenChanging) return;
        //Gyroscope Operation
        if (GyroscopeManager.LeftShake) 
            ChangeScreenShownByButton(1);
        if (GyroscopeManager.RightShake) 
            ChangeScreenShownByButton(3);
        if (GyroscopeManager.VerticalShake)
            ChangeScreenShownByButton(currScreenShown==0 ?2:0);
    }

    private void ChangeScreenShownByDrag()
    {
        if (isScreenChanging) return;

        //Touch Operation
        switch (toDrag)
        {
            default: break;
            case 0:
            case 1:
            case 2:
            case 3:
                ChangeScreenShownByButton(toDrag);break;
        }

        toDrag = -1;
    }

    private IEnumerator ChangeScreenShown() {
            Vector3 spd;
            int frame = maxRecFrame;
        spd = MainCamRotationAngle[nextScreenShown] - MainCam.transform.localEulerAngles;

        while (spd.x > 180f) spd.x -= 360f;
        while (spd.y > 180f) spd.y -= 360f;
        while (spd.z > 180f) spd.z -= 360f;

        while (spd.x < -180f) spd.x += 360f;
        while (spd.y < -180f) spd.y += 360f;
        while (spd.z < -180f) spd.z += 360f;

        spd /= maxRecFrame;

            while (frame-->0 )
            {
                MainCam.transform.localEulerAngles += spd;
                yield return new WaitForSeconds(0f);
            }
        currScreenShown = nextScreenShown;
        isScreenChanging = false;
    }

    #endregion

    #region GameProcessing
    public int GetCurrIsland() { return IslandNow; }

    #endregion
}