﻿using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class WaveManager : MonoBehaviour
{
    //private readonly float WaveChgSpeedFactor = 2.0f;
    //private readonly int FireworkMax=120;
    private readonly float BGMSpawnThreshold = 0.1f;//0.02f;

     //SpawnReferringToBGM
    [HideInInspector]
    public float timeBGM;
    private float[] dataBGM;
    private float dataBGMPrev;
    [HideInInspector]
    public bool readyToSpawn;

    private int TotalWaveNum;
    private int CurrentWaveNum;
    private float WaveTimer;
    private StageAttr CurrAttr;
    private bool inTutorial;
    private bool allSpawned;
    public bool isSpawning;

    public int SpawnPointByAI;


    public List<VisualEffect> FireWork;
    public List<Text> waveNumUI;
    [HideInInspector]
    public TextMesh waveNumMesh;

    //private int fireworkcounter;
    public InGameOperation sceneManager;
    public TutorialManager tutorialManager;
    public StageManager stageManager;
    public AudioManager audioManager;
    public EnemyManager enemyManager;
    public bool agentCallWait;

    // Start is called before the first frame update
    private void Start()
    {
        CurrAttr = StageInfo.GetStageInfo();
        //sceneManager = FindObjectOfType<InGameOperation>();
        //stageManager = FindObjectOfType<StageManager>();
        //enemySpawner = FindObjectOfType<EnemySpawner>();
        //tutorialManager = FindObjectOfType<TutorialManager>();
        TotalWaveNum = CurrAttr.waveNum;
        CurrentWaveNum = 0;
        //fireworkcounter = 0;
        allSpawned = false;
        agentCallWait = false;

        foreach (Text i in waveNumUI)
        {
            i.text = "";
            i.color = new Color(i.color.r, i.color.g, i.color.b, 0.0f);
        }

        if (sceneManager.GetCurrIsland() != 0)
        {
            inTutorial = false;
            WaveChg();
        }
        else
        {
            inTutorial = true;
        }
        WaveTimer = Time.time;

        SpawnPointByAI = -1;

        audioManager.PlayAudio("bgm_Battle", true);
        dataBGM = audioManager.GetClipWaveform("bgm_Battle");
        timeBGM = Time.time;
        dataBGMPrev = dataBGM[((int)timeBGM * 44100) % dataBGM.Length];
        readyToSpawn = false;
    }

    public int GetTotalWaveNum() { return TotalWaveNum; }
    public int GetCurrentWaveNum() { return CurrentWaveNum; }

    public bool WaveChg()
    {
        CurrentWaveNum++;

        //fireworkcounter = FireworkMax;
        //foreach (VisualEffect i in FireWork)
        //    i.Play();

        if (CurrentWaveNum > TotalWaveNum)
        {
            allSpawned = true;
            return true;
        }
        foreach (Text i in waveNumUI)
        {
            i.text = "WAVE " + CurrentWaveNum;
            i.color = new Color(i.color.r, i.color.g, i.color.b, 1.0f);
        }

        if (waveNumMesh)
            waveNumMesh.text = "WAVE " + CurrentWaveNum;
        StartCoroutine(SpawnWave(CurrAttr.waveDetail[CurrentWaveNum - 1]));
        return false;
    }
    // Update is called once per frame
    private void Update()
    {
        if (readyToSpawn == false && Time.time - timeBGM >= 0.35f)
        {
            //if (dataBGM[(int)(timeBGM * 0.25f* 44100) % dataBGM.Length] - dataBGMPrev > BGMSpawnThreshold)
            if (dataBGM[(int)(timeBGM * 0.25f * 44100) % dataBGM.Length] > BGMSpawnThreshold)
                readyToSpawn = true;
            //dataBGMPrev = dataBGM[(int)(timeBGM * 0.25f* 44100) % dataBGM.Length];
            timeBGM += 0.25f;
        }

        //if (fireworkcounter > 0) {
        //    if (--fireworkcounter == 0) {
        //        foreach (VisualEffect i in FireWork)
        //            i.Stop();
        //    }
        //}
        if (allSpawned && enemyManager.AllAliveMonstersList().Count == 0)
        {
            stageManager.SetWin();
            return;
        }

        if (inTutorial)
        {
            if (tutorialManager && tutorialManager.GetTutorialStage() >= TutorialManager.TutorialStageID.TutorialProgress_FirstWave)
            {
                WaveChg();
                WaveTimer = Time.time;
                inTutorial = false;
            }
        }
        else if (stageManager.GetResult() == 0)
        {
            if (isSpawning==false && Time.time - WaveTimer > CurrAttr.waveWaitTime)
            {
                WaveChg();
                //WaveTimer = Time.time;
            }
        }
    }
    public DebugManager debugManager;
    private IEnumerator SpawnWave(WaveAttr wave)
    {
        float spawnTimer = wave.enmStartTime;
        bool CheckCustomData = sceneManager && (sceneManager.GetCurrIsland() == StageInfo.IslandNum - 1);
        isSpawning = true;
        while (stageManager.GetResult() == 0 && isSpawning)
        {
            if (tutorialManager && tutorialManager.WaitingResponds) { }
            else { spawnTimer -= Time.deltaTime; }
            if (spawnTimer < 0)
            {
                for (int i = 0; i < wave.enmDetail.Count; ++i)
                {
                    if (wave.enmDetail[i].waveID > CurrentWaveNum)
                    {
                        break;
                    }
                    else if (wave.enmDetail[i].waveID < CurrentWaveNum)
                    {
                        continue;
                    }
                    else if (wave.enmDetail[i].waveID == CurrentWaveNum)
                    {
                        for (int j = wave.enmDetail[i].enmNum; j > 0; --j)
                        {
                            while (true) 
                            {
                                if (agentCallWait == false && readyToSpawn)
                                {
                                    enemyManager.SpawnMonster(wave.enmDetail[i].enmType,
                                        stageManager.GetPortalPosition()[SpawnPointByAI >= 0 ? SpawnPointByAI : wave.enmDetail[i].enmPort], CheckCustomData);
                                    readyToSpawn = false;
                                    timeBGM = Time.time;
                                    break;
                                }
                                yield return new WaitForSeconds(0);
                            }
                            yield return new WaitForSeconds(wave.enmSpawnPeriod / Mathf.Max(StageInfo.spawnSpeedEx, 1));
                        }
                    }
    
                }
                isSpawning = false;
                WaveTimer = Time.time;
            }

            yield return new WaitForSeconds(0f);
        }
    }

    public void SetCurrWAveNum(int num) {
        CurrentWaveNum = num;
    }
}