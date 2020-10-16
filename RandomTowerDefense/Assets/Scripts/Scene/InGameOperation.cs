using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

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

    [Header("OtherCamera Settings")]
    public GameObject DarkenCam;

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
    public GameObject LandscapeFadeImg;
    public GameObject PortraitFadeImg;

    FadeEffect LandscapeFade;
    FadeEffect PortraitFade;

    public bool isDebugging;//For ingame Debugger
    public bool isTutorial;//For 1st Stage Only
    //public VolumeProfile volumeProfile; // For Spare

    int IslandNow = 0;//For changing colour of Sea/Sky
    int IslandEnabled = 0;//Check when win

    [HideInInspector]
    public int currScreenShown = 0;//0:Main, 1:Top-Left, 2:Top, 3:Top-Right

    //Manager
    AudioManager AudioManager;
    //CameraManager CameraManager;
    CanvaManager CanvaManager;
    //InputManager InputManager;
    GyroscopeManager GyroscopeManager;

    TouchScreenKeyboard keyboard;//For RecordBroad if top 5
    bool CancelKeybroad = false;

    //Prevent DoubleHit
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
        MainCam.transform.position = MainCamStayPt[0];

        LandscapeFade = LandscapeFadeImg.GetComponent<FadeEffect>();
        PortraitFade = PortraitFadeImg.GetComponent<FadeEffect>();

        //InputManager = FindObjectOfType<InputManager>();
        AudioManager = FindObjectOfType<AudioManager>();
        //CameraManager = FindObjectOfType<CameraManager>();
        CanvaManager = FindObjectOfType<CanvaManager>();
        GyroscopeManager = FindObjectOfType<GyroscopeManager>();

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

        ArrowOperation();
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

        SetNextScene(sceneID==0?"GameScene":"LoadingScene");

        PlayerPrefs.SetInt("IslandNow", IslandNow);
        SceneOut();
        isSceneFinished = true;
    }
    #endregion

    public void ChangeScreenShownByButton(int chgValue)
    {

    }

    private void ChangeScreenShownByGyro()
    {
        //Gyroscope Operation
        //if (GyroscopeManager.LeftShake)

        //if (GyroscopeManager.RightShake)

    }


}