using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;
using UnityEngine.Rendering;
using RandomTowerDefense.Managers.Macro;
using RandomTowerDefense.Managers.System;
using RandomTowerDefense.Common;
using RandomTowerDefense.Info;

namespace RandomTowerDefense.Scene
{
    /// <summary>
    /// ステージ選択オペレーションクラス - ステージ選択シーンのUI制御と島管理
    ///
    /// 主な機能:
    /// - 4島ステージ選択システム（Easy、Normal、Hard、Ultra）
    /// - 動的ポストプロセッシング効果（ブルーム、視覚エフェクト）
    /// - マルチカメラ制御とUI統合（メイン、右、下カメラ）
    /// - ステージ解放状態管理とプログレス追跡
    /// - VFXエフェクト統合とシーン演出制御
    /// </summary>
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
        public List<Material> PetrifyMat;
        [Header("Other Settings")]
        public List<TextMesh> StageCustomText;

        public List<GameObject> Terrains;

        public MeshRenderer LandscapeFadeImg;
        public MeshRenderer PortraitFadeImg;

        private FadeEffect LandscapeFade;
        private FadeEffect PortraitFade;

        public bool isDebugging;
        public VolumeProfile volumeProfile;

        private int IslandNow = 0;
        private int IslandNext = 0;
        private int IslandEnabled = 0;

        //Manager
        public AudioManager AudioManager;
        public CameraManager CameraManager;
        public CanvaManager CanvaManager;
        public InputManager InputManager;
        public GyroscopeManager GyroscopeManager;

        private TouchScreenKeyboard keyboard;
        private bool CancelKeybroad = false;

        //Prevent DoubleHit
        private float TimeRecord = 0;
        private const float TimeWait = 0.5f;

        private bool lateStart;
        //CameraOperation
        private readonly float maxIslandChgTime = 0.2f;

        private void OnDestroy()
        {
            PlayerPrefs.Save();
        }

        // Start is called before the first frame update
        private void Start()
        {
            lateStart = true;

            IslandNow = PlayerPrefs.GetInt("IslandNow", 0);
            IslandNext = IslandNow;
            MainCam.transform.position = MainCamStayPt[IslandNow];
            RightCam.transform.position = RightCamStayPt[IslandNow];
            BottomCam.transform.position = BottomCamStayPt[IslandNow];

            if (isDebugging) IslandEnabled = StageInfoDetail.IslandNum;
            else IslandEnabled = PlayerPrefs.GetInt("IslandEnabled", 1);

            GameObject[] ClearMarks = GameObject.FindGameObjectsWithTag("ClearMark");
            for (int i = 0; i < ClearMarks.Length; ++i)
                ClearMarks[i].SetActive(i < IslandEnabled);

            for (int i = 0; i < StageCustomText.Count; ++i)
                StageInfoOperation(i, 0);

            LandscapeFade = LandscapeFadeImg.gameObject.GetComponent<FadeEffect>();
            PortraitFade = PortraitFadeImg.gameObject.GetComponent<FadeEffect>();

            InputManager = FindObjectOfType<InputManager>();
            AudioManager = FindObjectOfType<AudioManager>();
            CameraManager = FindObjectOfType<CameraManager>();
            CanvaManager = FindObjectOfType<CanvaManager>();
            GyroscopeManager = FindObjectOfType<GyroscopeManager>();

            AudioManager.PlayAudio("bgm_Title", true);

            UnityEngine.Rendering.Universal.Bloom bloom;
            volumeProfile.TryGet<UnityEngine.Rendering.Universal.Bloom>(out bloom);
            bloom.intensity.value = bloomInt_Max;

            StartCoroutine("SceneChgAnimation");

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
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();

            if (lateStart)
            {
                base.SceneIn();
                lateStart = false;
            }

            foreach (Button i in OptionButton)
            {
                i.interactable = !isOption && !isSceneFinished;
            }

            OptionButton[0].interactable = !isSceneFinished;
            OptionButton[1].interactable = !isSceneFinished;

            //Change Scene
            if (isSceneFinished && ((LandscapeFade && LandscapeFade.isReady) || (PortraitFade && PortraitFade.isReady)))
            {
                SceneManager.LoadScene("LoadingScene");
                return;
            }

            for (int i = 0; i < Terrains.Count; ++i)
            {
                Terrains[i].SetActive(i == IslandNow || i == IslandNext);
            }

            ArrowOperation();

            if (isSceneFinished) return;

            if (!isOption) ChangeIsland();
            DarkenCam.SetActive(isOption);

            OrientationLock = false;
        }

        #region CommonOperation
        public void ArrowOperation()
        {
            foreach (GameObject i in UILeftArrow)
                i.SetActive(!isSceneFinished && IslandNow > 0);
            foreach (GameObject i in UIRightArrow)
                i.SetActive(!isSceneFinished && IslandNow < IslandEnabled - 1);
        }
        public void MoveToStage()
        {

            if (!((LandscapeFade && LandscapeFade.isReady) || (PortraitFade && PortraitFade.isReady))) return;
            if (Time.time - TimeRecord < TimeWait) return;
            StartCoroutine(PetrifyAnimation());
            TimeRecord = Time.time;
            AudioManager.PlayAudio("se_Button");

        }

        public void OptionStatus()
        {
            if (Time.time - TimeRecord < TimeWait) return;
            isOption = !isOption;
            CanvaManager.isOption = isOption;
            TimeRecord = Time.time;
            AudioManager.PlayAudio("se_Button");
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
                    i.material.EnableKeyword("_Progress");
                    i.material.SetFloat("_Progress", progress);
                }
                foreach (RawImage i in PetrifyRImgs)
                {
                    i.material.EnableKeyword("_Progress");
                    i.material.SetFloat("_Progress", progress);
                }
                foreach (SpriteRenderer i in PetrifySpr)
                {
                    i.material.EnableKeyword("_Progress");
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
            AudioManager.PlayAudio("se_Button");
            IslandNext = Mathf.Clamp(IslandNow + chgValue, 0, IslandEnabled);
            if (GyroscopeManager) GyroscopeManager.ResetReference();
        }

        private void ChangeIslandByGyro()
        {
            //Gyroscope Operation
            if (GyroscopeManager.LeftShake)
            {
                toDrag = (int)InputManager.DragDirectionSel.DragToRight;
            }
            if (GyroscopeManager.RightShake)
            {
                toDrag = (int)InputManager.DragDirectionSel.DragToLeft;
            }
        }

        public void ChangeIsland()
        {
            if (Time.time - TimeRecord < TimeWait) return;

            //Gyroscope Operation
            ChangeIslandByGyro();

            //Touch Operation
            switch (toDrag)
            {
                case 0: default: break;
                case 1:
                    IslandNext = Mathf.Clamp(IslandNow + 1, 0, IslandEnabled - 1);
                    break;
                case 2:
                    IslandNext = Mathf.Clamp(IslandNow - 1, 0, IslandEnabled - 1);
                    break;
            }

            toDrag = 0;

            if (IslandNext != IslandNow)
            {
                GyroscopeManager.ResetReference();
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
            float timer = 0;

            MainCam.transform.position = MainCamStayPt[IslandNow];

            spd = MainCamStayPt[IslandNext] - MainCamStayPt[IslandNow];
            spd /= maxIslandChgTime;

            while (timer < maxIslandChgTime &&
                ((MainCam.transform.position + spd * Time.deltaTime - MainCamStayPt[IslandNext]).sqrMagnitude <
                (MainCam.transform.position - MainCamStayPt[IslandNext]).sqrMagnitude))
            {
                timer += Time.deltaTime;
                if (timer > maxIslandChgTime) timer = maxIslandChgTime;
                MainCam.transform.position = MainCamStayPt[IslandNow] + spd * timer;
                yield return new WaitForSeconds(0f);
            }
            MainCam.transform.position = MainCamStayPt[IslandNext];
            IslandNow = IslandNext;
        }

        private IEnumerator RecRightCamOperation()
        {
            Vector3 spd;
            float timer = 0;

            RightCam.transform.position = RightCamStayPt[IslandNow];

            spd = RightCamStayPt[IslandNext] - RightCamStayPt[IslandNow];
            spd /= maxIslandChgTime;

            while (timer < maxIslandChgTime &&
                ((RightCam.transform.position + spd * Time.deltaTime - RightCamStayPt[IslandNext]).sqrMagnitude <
                (RightCam.transform.position - RightCamStayPt[IslandNext]).sqrMagnitude))
            {
                timer += Time.deltaTime;
                if (timer > maxIslandChgTime) timer = maxIslandChgTime;
                RightCam.transform.position = RightCamStayPt[IslandNow] + spd * timer;
                yield return new WaitForSeconds(0f);
            }
            RightCam.transform.position = RightCamStayPt[IslandNext];
        }
        private IEnumerator RecBottomCamOperation()
        {
            Vector3 spd;
            float timer = 0;

            BottomCam.transform.position = BottomCamStayPt[IslandNow];

            spd = BottomCamStayPt[IslandNext] - BottomCamStayPt[IslandNow];
            spd /= maxIslandChgTime;

            while (timer < maxIslandChgTime &&
                ((BottomCam.transform.position + spd * Time.deltaTime - BottomCamStayPt[IslandNext]).sqrMagnitude <
                (BottomCam.transform.position - BottomCamStayPt[IslandNext]).sqrMagnitude))
            {
                timer += Time.deltaTime;
                if (timer > maxIslandChgTime) timer = maxIslandChgTime;
                BottomCam.transform.position = BottomCamStayPt[IslandNow] + spd * timer;
                yield return new WaitForSeconds(0f);
            }
            BottomCam.transform.position = BottomCamStayPt[IslandNext];
        }
        #endregion

        #region CustomizeStageOperation
        private void StageInfoOperation(int infoID, int chg)
        {
            CancelKeybroad = true;
            float result = StageInfoDetail.SaveDataInPrefs(infoID, chg);
            StageCustomText[infoID].text = (result == 999) ? "∞" : result.ToString();
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
                switch (infoID)
                {
                    case 0:
                        keyboard.characterLimit = 3;
                        break;
                    case 1:
                        keyboard.characterLimit = 2;
                        break;
                    case 2:
                    case 3:
                        keyboard.characterLimit = 1;
                        break;
                    case 4:
                        keyboard.characterLimit = 2;
                        break;
                    case 5:
                    case 6:
                        keyboard.characterLimit = 3;
                        break;
                }

                StartCoroutine(TouchScreenInputUpdate(infoID));
                CancelKeybroad = false;
            }
        }

        private IEnumerator TouchScreenInputUpdate(int infoID)
        {
            if (keyboard != null)
            {
                while (keyboard.status == TouchScreenKeyboard.Status.Visible && CancelKeybroad == false)
                {
                    StageCustomText[infoID].text = keyboard.text;
                    yield return new WaitForSeconds(0f);
                }

                if (keyboard.status == TouchScreenKeyboard.Status.Done
                || keyboard.status == TouchScreenKeyboard.Status.Canceled)
                {
                    switch (infoID)
                    {
                        case 0:
                        case 1:
                        case 4:
                            int InputValue;
                            if (int.TryParse(keyboard.text, out InputValue))
                            {
                                if (infoID == 0)
                                {
                                    InputValue = Mathf.Clamp(InputValue,
                                        DefaultStageInfos.MinMapDepth * DefaultStageInfos.MinMapDepth,
                                        DefaultStageInfos.MaxMapDepth * DefaultStageInfos.MaxMapDepth);
                                    StageCustomText[infoID].text = InputValue.ToString();
                                }
                                StageInfoDetail.SaveDataInPrefsByDirectInput(infoID, InputValue);
                            }
                            else
                            {
                                StageCustomText[infoID].text = StageInfoDetail.SaveDataInPrefs(infoID, 0).ToString();
                            }
                            break;
                        case 2:
                        case 3:
                        case 5:
                        case 6:
                            if (float.TryParse(keyboard.text, out float OutputValue))
                            {
                                if (infoID == 5)
                                {
                                    OutputValue = Mathf.Clamp(OutputValue,
                                        DefaultStageInfos.MinObstaclePercent, DefaultStageInfos.MaxObstaclePercent);
                                }

                                StageCustomText[infoID].text = OutputValue.ToString();
                                StageInfoDetail.SaveDataInPrefsByDirectInput(infoID, OutputValue);
                            }
                            else
                            {
                                StageCustomText[infoID].text = StageInfoDetail.SaveDataInPrefs(infoID, 0).ToString();
                            }
                            break;
                    }
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
        }

        public int CurrentIslandNum() { return IslandNow; }
        public int EnabledtIslandNum() { return IslandEnabled; }
        public int NextIslandNum() { return IslandNext; }
    }
}