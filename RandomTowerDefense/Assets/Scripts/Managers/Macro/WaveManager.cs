using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
using RandomTowerDefense.Scene;
using RandomTowerDefense.Managers.System;
using RandomTowerDefense.AI;
using RandomTowerDefense.Info;

namespace RandomTowerDefense.Managers.Macro
{
    /// <summary>
    /// ウェーブ管理システム - 敵ウェーブ生成と管理の制御
    ///
    /// 主な機能:
    /// - ステージ設定に基づく動的敵ウェーブスポーン
    /// - BGM同期敵スポーンタイミングシステム
    /// - ウェーブ遷移用視覚エフェクト連携
    /// - インテリジェントスポーンポイント選択用AIエージェント統合
    /// - チュートリアルモードウェーブ管理
    /// - ウェーブ進行追跡とUI更新
    /// - リズムベーススポーン用オーディオ波形解析
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        #region Constants
        private readonly float WaveChgSpeedFactor = 2.0f;
        private readonly int FireworkMax = 120;
        private readonly float BGMSpawnThreshold = 0.1f; // BGM amplitude threshold for spawning

        // オーディオ解析定数
        private const int AUDIO_SAMPLE_RATE = 44100;
        private const float BGM_TIME_OFFSET = 0.35f;
        private const float BGM_TIME_SCALE = 0.25f;

        // 初期化定数
        private const int INITIAL_WAVE_NUMBER = 0;
        private const int INITIAL_FIREWORK_COUNTER = 0;
        private const int INVALID_SPAWN_POINT = -1;
        private const int EMPTY_COUNT = 0;

        // UI透明度定数
        private const float UI_TRANSPARENT_ALPHA = 0.0f;
        private const float UI_OPAQUE_ALPHA = 1.0f;

        // ウェーブ進行定数
        private const int WAVE_ARRAY_INDEX_OFFSET = 1;
        private const int TUTORIAL_ISLAND_ID = 0;
        private const int GAME_RESULT_ONGOING = 0;
        private const int MIN_SPAWN_SPEED = 1;

        // コルーチン待機時間定数
        private const float COROUTINE_WAIT_TIME = 0.0f;
        #endregion

        #region Public Properties
        // BGM-synchronized spawning
        [HideInInspector]
        public float timeBGM;
        [HideInInspector]
        public bool readyToSpawn;
        [HideInInspector]
        public bool doneDownloading = false;
        public bool isSpawning;
        public int SpawnPointByAI;
        #endregion

        #region Private Fields
        // Audio analysis data
        private float[] __dataBGM;
        private float ___dataBGMPrev;

        // Wave management
        private int _totalWaveNum;
        private int _currentWaveNum;
        private float _waveTimer;
        private StageAttr _currAttr;
        private bool __inTutorial;
        private bool __allSpawned;
        private int _fireworkCounter;
        #endregion


        #region Serialized Fields
        [Header("✨ Visual Effects")]
        public List<VisualEffect> FireWork;

        [Header("📝 UI Elements")]
        public List<Text> waveNumUI;
        [HideInInspector]
        public TextMesh waveNumMesh;

        [Header("🎮 Manager References")]
        public InGameOperation sceneManager;
        public TutorialManager tutorialManager;
        public StageManager stageManager;
        public AudioManager audioManager;
        public EnemyManager enemyManager;

        [Header("🤖 AI Integration")]
        public bool agentCallWait;
        public AgentScript agent;
        #endregion
        // Start is called before the first frame update
        private void Start()
        {
            _currAttr = StageInfo.GetStageInfo();
            sceneManager = FindObjectOfType<InGameOperation>();
            stageManager = FindObjectOfType<StageManager>();
            enemySpawner = FindObjectOfType<EnemySpawner>();
            tutorialManager = FindObjectOfType<TutorialManager>();
            _totalWaveNum = _currAttr.waveNum;
            _currentWaveNum = INITIAL_WAVE_NUMBER;
            _fireworkCounter = INITIAL_FIREWORK_COUNTER;
            _allSpawned = false;
            agentCallWait = false;

            foreach (Text i in waveNumUI)
            {
                i.text = "";
                i.color = new Color(i.color.r, i.color.g, i.color.b, UI_TRANSPARENT_ALPHA);
            }

            if (sceneManager.GetCurrIsland() != TUTORIAL_ISLAND_ID)
            {
                _inTutorial = false;
                WaveChg();
            }
            else
            {
                _inTutorial = true;
            }
            _waveTimer = Time.time;

            SpawnPointByAI = INVALID_SPAWN_POINT;

            audioManager.PlayAudio("bgm_Battle", true);
            _dataBGM = audioManager.GetClipWaveform("bgm_Battle");
            timeBGM = Time.time;
            __dataBGMPrev = _dataBGM[((int)timeBGM * AUDIO_SAMPLE_RATE) % _dataBGM.Length];
            readyToSpawn = false;
        }

        public int Get_totalWaveNum() { return _totalWaveNum; }
        public int Get_currentWaveNum() { return _currentWaveNum; }

        public bool WaveChg()
        {
            if (doneDownloading == false) return false;
            _currentWaveNum++;

            _fireworkCounter = FireworkMax;
            foreach (VisualEffect i in FireWork)
                i.Play();

            if (_currentWaveNum > _totalWaveNum)
            {
                _allSpawned = true;
                return true;
            }
            foreach (Text i in waveNumUI)
            {
                i.text = "WAVE " + _currentWaveNum;
                i.color = new Color(i.color.r, i.color.g, i.color.b, UI_OPAQUE_ALPHA);
            }

            if (waveNumMesh)
                waveNumMesh.text = "WAVE " + _currentWaveNum;
            StartCoroutine(SpawnWave(_currAttr.waveDetail[_currentWaveNum - WAVE_ARRAY_INDEX_OFFSET]));
            return false;
        }

        // Update is called once per frame
        private void Update()
        {
            if (readyToSpawn == false && Time.time - timeBGM >= BGM_TIME_OFFSET)
            {
                if (_dataBGM[(int)(timeBGM * BGM_TIME_SCALE * AUDIO_SAMPLE_RATE) % _dataBGM.Length] > BGMSpawnThreshold)
                    readyToSpawn = true;
                __dataBGMPrev = _dataBGM[(int)(timeBGM * BGM_TIME_SCALE * AUDIO_SAMPLE_RATE) % _dataBGM.Length];
                timeBGM += BGM_TIME_SCALE;
            }

            if (_fireworkCounter > 0)
            {
                if (--_fireworkCounter == INITIAL_FIREWORK_COUNTER)
                {
                    foreach (VisualEffect i in FireWork)
                        i.Stop();
                }
            }

            if (_allSpawned && enemyManager.AllAliveMonstersList().Count == EMPTY_COUNT)
            {
                stageManager.SetWin();
                return;
            }

            if (_inTutorial)
            {
                if (tutorialManager && tutorialManager.GetTutorialStage() >= TutorialManager.TutorialStageID.TutorialProgress_FirstWave)
                {
                    WaveChg();
                    _waveTimer = Time.time;
                    _inTutorial = false;
                }
            }
            else if (stageManager.GetResult() == GAME_RESULT_ONGOING)
            {
                if (isSpawning == false && Time.time - _waveTimer > _currAttr.waveWaitTime)
                {
                    WaveChg();
                    _waveTimer = Time.time;
                }
            }
        }
        public DebugManager debugManager;
        private IEnumerator SpawnWave(WaveAttr wave)
        {
            float spawnTimer = wave.enmStartTime;
            bool CheckCustomData = sceneManager && (sceneManager.GetCurrIsland() == StageInfo.IslandNum - 1);
            isSpawning = true;
            while (stageManager.GetResult() == GAME_RESULT_ONGOING && isSpawning)
            {
                if (tutorialManager && tutorialManager.WaitingResponds) { }
                else { spawnTimer -= Time.deltaTime; }
                if (spawnTimer < 0)
                {
                    for (int i = 0; i < wave.enmDetail.Count; ++i)
                    {
                        if (wave.enmDetail[i].waveID > _currentWaveNum)
                        {
                            break;
                        }
                        else if (wave.enmDetail[i].waveID < _currentWaveNum)
                        {
                            continue;
                        }
                        else if (wave.enmDetail[i].waveID == _currentWaveNum)
                        {
                            for (int j = wave.enmDetail[i].enmNum; j > 0; --j)
                            {
                                while (true)
                                {
                                    if ((agent == null || agentCallWait == false) && readyToSpawn)
                                    {
                                        enemyManager.SpawnMonster(wave.enmDetail[i].enmType,
                                            stageManager.GetPortalPosition(SpawnPointByAI >= 0 ? SpawnPointByAI : wave.enmDetail[i].enmPort), CheckCustomData);
                                        readyToSpawn = false;
                                        timeBGM = Time.time;
                                        break;
                                    }
                                    yield return new WaitForSeconds(COROUTINE_WAIT_TIME);
                                }
                                yield return new WaitForSeconds(wave.enmSpawnPeriod / Mathf.Max(StageInfo.spawnSpeedEx, MIN_SPAWN_SPEED));
                            }
                        }

                    }
                    isSpawning = false;
                    _waveTimer = Time.time;
                }

                yield return new WaitForSeconds(COROUTINE_WAIT_TIME);
            }
        }

        public void SetCurrWAveNum(int num)
        {
            _currentWaveNum = num;
        }
    }
}