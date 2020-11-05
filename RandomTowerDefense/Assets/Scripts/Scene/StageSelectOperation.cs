using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;
using UnityEngine.Rendering;

public class StageSelectOperation : ISceneChange
{
    private readonly float bloomInt_Default = 0.25f;
    private readonly float bloomInt_Max = 60.0f;

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

    [Header("Arrow Settings")]
    public List<GameObject> UILeftArrow;
    public List<GameObject> UIRightArrow;

    [Header("Petrify Settings")]
    public List<Image> PetrifyImgs;
    public List<RawImage> PetrifyRImgs;
    public List<SpriteRenderer> PetrifySpr;

    [Header("Other Settings")]
    public List<TextMesh> StageCustomText;
    public List<TextMesh> StageCustomTextDirect;
    public List<VisualEffect> SceneChgPS;

    public GameObject LandscapeFadeImg;
    public GameObject PortraitFadeImg;

    private FadeEffect LandscapeFade;
    private FadeEffect PortraitFade;

    public bool isDebugging;
    public VolumeProfile volumeProfile;

    private int IslandNow = 0;
    private int IslandNext = 0;
    private int IslandEnabled = 0;

    //Manager
    private AudioManager AudioManager;
    //private CameraManager CameraManager;
    private CanvaManager CanvaManager;
    //private InputManager InputManager;
    private GyroscopeManager GyroscopeManager;

    private TouchScreenKeyboard keyboard;
    private bool CancelKeybroad =false;

    //Prevent DoubleHit
    private float TimeRecord = 0;
    private const float TimeWait = 0.5f;

    //CameraOperation
    private readonly int maxRecFrame = 20;

    private void OnDestroy()
    {
        PlayerPrefs.Save();
    }

    // Start is called before the first frame update
    private void Start()
    {
        base.SceneIn();

        IslandNow = PlayerPrefs.GetInt("IslandNow");
        IslandNext = IslandNow;
        MainCam.transform.position = MainCamStayPt[IslandNow];
        RightCam.transform.position = RightCamStayPt[IslandNow];
        BottomCam.transform.position = BottomCamStayPt[IslandNow];

        if (isDebugging) IslandEnabled = StageInfo.IslandNum;
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

        if (PetrifyImgs.Count>0) {
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
    private void Update()
    {
        base.Update();

        foreach (Button i in OptionButton)
            i.interactable = !isOption && !isSceneFinished;

        //Change Scene
        if (isSceneFinished && ((LandscapeFade && LandscapeFade.isReady) || (PortraitFade && PortraitFade.isReady)))
        {
            SceneManager.LoadScene("GameScene");
            return;
        }

        ArrowOperation();

        if (isSceneFinished) return;

        if (!isOption) ChangeIsland();
        DarkenCam.SetActive(isOption);
    }

    #region CommonOperation
    public void ArrowOperation()
    {
        foreach (GameObject i in UILeftArrow)
            i.SetActive(!isSceneFinished && IslandNow > 0);
        foreach (GameObject i in UIRightArrow)
            i.SetActive(!isSceneFinished && IslandNow < IslandEnabled-1);
    }
    public void MoveToStage()
    {
        if (Time.time - TimeRecord < TimeWait) return;
        StartCoroutine(PetrifyAnimation());
        TimeRecord = Time.time;
        GameObject.FindObjectOfType<AudioManager>().PlayAudio("se_Button");
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
        SetNextScene("GameScene");
        PlayerPrefs.SetInt("IslandNow", IslandNow);
        SceneOut();
        isSceneFinished = true;
    }
    #endregion

    #region ChangeIsland
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
                IslandNext = Mathf.Clamp(IslandNow + 1, 0, IslandEnabled-1);
                break;
            case 2:
                IslandNext = Mathf.Clamp(IslandNow - 1, 0, IslandEnabled-1);
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
    #endregion

    #region CamOperation
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
    #endregion

    #region CustomizeStageOperation
    private void StageInfoOperation(int infoID, int chg)
    {
        CancelKeybroad = true;
        float result = StageInfo.SaveDataInPrefs(infoID, chg);
        StageCustomText[infoID].text = (result==999)? "∞" : result.ToString();
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
            CancelKeybroad = false;
        }
    }

    private IEnumerator TouchScreenInputUpdate(int infoID)
    {
        if (keyboard != null)
        {
            while (keyboard.status == TouchScreenKeyboard.Status.Visible && CancelKeybroad==false)
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
    }
    #endregion

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

    public int CurrentIslandNum() { return IslandNow; }
    public int EnabledtIslandNum() { return IslandEnabled; }
    public int NextIslandNum() { return IslandNext; }
}