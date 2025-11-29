using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.UI;
using RandomTowerDefense.Scene;
using RandomTowerDefense.DOTS.Spawner;
using RandomTowerDefense.MapGenerator;
using RandomTowerDefense.Managers.System;
using RandomTowerDefense.Info;

namespace RandomTowerDefense.Managers.Macro
{
    /// <summary>
    /// ステージ管理システム - ゲーム進行とステージ状態の制御
    ///
    /// 主な機能:
    /// - ゲーム結果管理（勝利/敗北検出）
    /// - 敵スポーンポイント設定と管理
    /// - 城の体力追跡とダメージ視覚化
    /// - ゲーム終了状態遷移とUI制御
    /// - ステージ進行とアンロックシステム
    /// - ステージイベント用視覚エフェクト連携
    /// - マップ生成統合
    /// </summary>
    public class StageManager : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// ダメージエフェクト表示時間
        /// </summary>
        private readonly float timerForDmgVFX = 0.05f;

        /// <summary>
        /// ステージ表示位置
        /// </summary>
        private readonly Vector3 displayPos = new Vector3(57.5f, 31f, 3.5f);

        /// <summary>
        /// 敵スポーンポイント数
        /// </summary>
        private readonly int EnemySpawnPtNum = 3;

        /// <summary>
        /// フェードイン・アウト速度
        /// </summary>
        private readonly float FadeRate = 0.02f;

        #endregion

        #region Enums

        public enum GameResult
        {
            Lose = -1,
            NotEndedYet = 0,
            Won = 1
        }

        #endregion

        #region Serialized Fields

        [Header("🎯 Spawn Configuration")]
        [HideInInspector]
        public FilledMapCoord[] SpawnPoint;
        public GameObject EnemySpawnPortPrefab;
        public GameObject WaveDisplayMeshPrefab;

        [Header("🎮 Manager References")]
        public InGameOperation sceneManager;
        public AudioManager audioManager;
        public TimeManager timeManager;
        public WaveManager waveManager;
        public ScoreCalculation scoreCalculation;

        [Header("🎨 UI Elements")]
        public List<GameObject> GameClearCanva;
        public List<GameObject> GameOverCanva;
        public Material CoveringMateral;

        [Header("🗺️ Map Generation")]
        public FilledMapGenerator mapGenerator;
        #endregion

        private GameObject[] EnemySpawnPort;
        private MeshDestroy[] castleDestroy;

        [HideInInspector]
        public int CastleEntityID;
        public CastleSpawner castleSpawner;

        private int result = 0;
        private bool isReady = false;
        //private bool OrientationRecord = false;

        public VisualEffect dmgVFX;
        void Awake()
        {
            SpawnPoint = new Coord[EnemySpawnPtNum + 1];
            EnemySpawnPort = new GameObject[EnemySpawnPtNum];
            if (dmgVFX) dmgVFX.Stop();

            if (mapGenerator)
            {
                if (sceneManager.GetCurrIsland() != StageInfoDetail.IslandNum - 1)
                {
                    mapGenerator.OnNewStage(sceneManager.GetCurrIsland());
                }
                else
                {
                    float TotalSize = PlayerPrefs.GetFloat("stageSize", 64);
                    TotalSize = Mathf.Sqrt(TotalSize);
                    mapGenerator.CustomizeMapAndCreate((int)(TotalSize + 0.9f), (int)(TotalSize + 0.9f));
                }
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            //sceneManager = FindObjectOfType<InGameOperation>();
            //mapGenerator = FindObjectOfType<FilledMapGenerator>();
            //audioManager = FindObjectOfType<AudioManager>();
            //timeManager = FindObjectOfType<TimeManager>();
            //waveManager = FindObjectOfType<WaveManager>();
            //scoreCalculation = FindObjectOfType<ScoreCalculation>();
            //castleSpawner = FindObjectOfType<CastleSpawner>();

            result = (int)GameResult.NotEndedYet;
            foreach (GameObject i in GameClearCanva)
            {
                i.SetActive(false);
            }
            foreach (GameObject i in GameOverCanva)
                i.SetActive(false);

            if (mapGenerator)
            {
                //Fixed CastleMapPos
                int[] entityID = castleSpawner.Spawn(mapGenerator.CoordToPosition(SpawnPoint[0]) + mapGenerator.transform.position,
                    Quaternion.Euler(0f, 90f, 0f), (int)PlayerPrefs.GetFloat("hpMax", 5), 0f);
                CastleEntityID = entityID[0];
                castleDestroy = castleSpawner.GameObjects[CastleEntityID].GetComponentsInChildren<MeshDestroy>();

                for (int i = 0; i < EnemySpawnPtNum; ++i)
                {
                    Vector3 pos = mapGenerator.CoordToPosition(SpawnPoint[i + 1]) + mapGenerator.transform.position + Vector3.up * 0.01f;
                    if (i == 0)
                    {
                        GameObject WaveDisplayMesh = Instantiate(WaveDisplayMeshPrefab, displayPos, Quaternion.Euler(90f, 0, 0));
                        WaveDisplayMesh.transform.parent = this.transform;
                        waveManager.waveNumMesh = WaveDisplayMesh.GetComponent<TextMesh>();
                        waveManager.waveNumMesh.text = "WAVE 1";
                    }

                    if (sceneManager.CheckIfTutorial() && i != 1) continue;
                    EnemySpawnPort[i] = Instantiate(EnemySpawnPortPrefab, pos, Quaternion.identity);
                    EnemySpawnPort[i].transform.parent = this.transform;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (result != (int)GameResult.NotEndedYet && isReady)
            {
                if (sceneManager.DarkenCam.activeSelf == false)
                    sceneManager.DarkenCam.SetActive(true);
                //bool OrientationChk = OrientationRecord;
                //OrientationRecord = sceneManager.OrientationLand;
                //if (OrientationRecord != OrientationChk)
                GameEndCanvaUpdate();
            }
        }

        private void GameEndCanvaUpdate()
        {
            if (sceneManager.OrientationLand)
            {
                if (result > 0)
                {
                    if (GameOverCanva[0].activeSelf == true)
                        GameOverCanva[0].SetActive(false);
                    if (GameOverCanva[1].activeSelf == true)
                        GameOverCanva[1].SetActive(false);
                    if (GameClearCanva[0].activeSelf == true)
                        GameClearCanva[0].SetActive(false);
                    if (GameClearCanva[1].activeSelf == false)
                        GameClearCanva[1].SetActive(true);
                }
                else
                {
                    if (GameOverCanva[0].activeSelf == true)
                        GameOverCanva[0].SetActive(false);
                    if (GameOverCanva[1].activeSelf == false)
                        GameOverCanva[1].SetActive(true);
                    if (GameClearCanva[0].activeSelf == true)
                        GameClearCanva[0].SetActive(false);
                    if (GameClearCanva[1].activeSelf == true)
                        GameClearCanva[1].SetActive(false);
                }
            }
            else
            {
                if (result > 0)
                {
                    if (GameOverCanva[0].activeSelf == true)
                        GameOverCanva[0].SetActive(false);
                    if (GameOverCanva[1].activeSelf == true)
                        GameOverCanva[1].SetActive(false);
                    if (GameClearCanva[0].activeSelf == false)
                        GameClearCanva[0].SetActive(true);
                    if (GameClearCanva[1].activeSelf == true)
                        GameClearCanva[1].SetActive(false);
                }
                else
                {
                    if (GameOverCanva[0].activeSelf == false)
                        GameOverCanva[0].SetActive(true);
                    if (GameOverCanva[1].activeSelf == true)
                        GameOverCanva[1].SetActive(false);
                    if (GameClearCanva[0].activeSelf == true)
                        GameClearCanva[0].SetActive(false);
                    if (GameClearCanva[1].activeSelf == true)
                        GameClearCanva[1].SetActive(false);
                }
            }
        }

        public bool CheckLose()
        {
            if (GetCurrHP() <= 0 && result == (int)GameResult.NotEndedYet)
            {
                result = (int)GameResult.Lose;

                isReady = false;
                sceneManager.SetOptionStatus(false);
                audioManager.StopBGM();
                audioManager.PlayAudio("se_Lose");
                timeManager.SetTimeScale(0);
                //foreach(MeshDestroy i in castleDestroy)
                //    i.DestroyMesh();
                scoreCalculation.CalculationScore();
                StartCoroutine(FadeInRoutine());
                return true;
            }
            return false;
        }

        public bool SetWin()
        {
            if (result == (int)GameResult.Lose || result == (int)GameResult.Won)
                return false;

            result = (int)GameResult.Won;


            if (sceneManager.GetEnabledIsland() == sceneManager.GetCurrIsland() && sceneManager.GetEnabledIsland() < StageInfoDetail.IslandNum - 1)
                PlayerPrefs.SetInt("IslandEnabled", sceneManager.GetCurrIsland() + 1);

            isReady = false;
            sceneManager.SetOptionStatus(false);
            audioManager.StopBGM();
            audioManager.PlayAudio("se_Clear");
            timeManager.SetTimeScale(0);
            //foreach(MeshDestroy i in castleDestroy)
            //    i.DestroyMesh();
            scoreCalculation.CalculationScore();
            StartCoroutine(FadeInRoutine());
            return true;
        }
        public int GetMaxHP()
        {
            if (castleSpawner && castleSpawner.castle)
                return castleSpawner.castle.MaxCastleHP;
            return 1;
        }
        public int GetCurrHP() { return Mathf.Max(0, castleSpawner.castleHPArray[0]); }

        private IEnumerator FadeOutRoutine()
        {
            float Threshold = 0f;
            while (Threshold < 1f)
            {
                CoveringMateral.SetFloat("_FadeThreshold", Threshold);
                Threshold += FadeRate;
                yield return new WaitForSeconds(0f);
            }
            CoveringMateral.SetFloat("_FadeThreshold", Threshold);
        }

        private IEnumerator FadeInRoutine()
        {
            float Threshold = 1f;
            while (Threshold > 0f)
            {
                CoveringMateral.SetFloat("_FadeThreshold", Threshold);
                Threshold -= FadeRate;
                yield return new WaitForSeconds(0f);
            }
            CoveringMateral.SetFloat("_FadeThreshold", Threshold);
            isReady = true;

            StartCoroutine(FadeOutRoutine());
        }

        public int GetResult() { return result; }

        public Vector3 GetPortalPosition(int index)
        {
            if (index >= EnemySpawnPort.Length)
                return new Vector3();
            else if (EnemySpawnPort[index] == null)
                return EnemySpawnPort[0].transform.position;

            return EnemySpawnPort[index].transform.position;
        }

        public void AddedHealth(int Val = 1)
        {
            castleSpawner.castle.AddedHealth(Val);
        }

        public int GetHealth()
        {
            return castleSpawner.castle.CurrCastleHP;
        }

        public void PlayDmgAnim()
        {
            StartCoroutine(DmgAnimation());
        }


        private IEnumerator DmgAnimation()
        {
            if (dmgVFX)
                dmgVFX.Play();
            float timerCount = timerForDmgVFX;
            while (timerCount > 0)
            {
                timerCount -= Time.deltaTime;
                yield return new WaitForSeconds(0f);
            }
            if (dmgVFX)
                dmgVFX.Stop();
        }

        public int GetCurrIsland()
        {
            return sceneManager.GetCurrIsland();
        }
    }
}
