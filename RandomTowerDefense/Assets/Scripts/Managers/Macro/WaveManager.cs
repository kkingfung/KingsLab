using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class WaveManager : MonoBehaviour
{

    //private readonly int FireworkMax=120;
    private int TotalWaveNum;
   private int CurrentWaveNum;
   private float WaveTimer;

   private StageAttr CurrAttr;
    private bool inTutorial;

    public List<VisualEffect> FireWork;
    public  List<Text> waveNumUI;
    [HideInInspector]
    public TextMesh waveNumMesh;

    //private int fireworkcounter;
    private InGameOperation sceneManager;
    private TutorialManager tutorialManager;
    private StageManager stageManager;
    private EnemySpawner enemySpawner;

    // Start is called before the first frame update
    void Start()
    {
        CurrAttr = StageInfo.GetStageInfo();
        sceneManager = FindObjectOfType<InGameOperation>();
        stageManager = FindObjectOfType<StageManager>();
        enemySpawner = FindObjectOfType<EnemySpawner>();
        tutorialManager = FindObjectOfType<TutorialManager>();
        TotalWaveNum = CurrAttr.waveNum;
        CurrentWaveNum = 0;
        //fireworkcounter = 0;

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
        else {
            inTutorial = true;
        }
        WaveTimer = Time.time;
    }

    public int GetTotalWaveNum() { return TotalWaveNum; }
    public int GetCurrentWaveNum() { return CurrentWaveNum; }

    public bool WaveChg() {
        CurrentWaveNum++;

        //fireworkcounter = FireworkMax;
        //foreach (VisualEffect i in FireWork)
        //    i.Play();

        if (CurrentWaveNum > TotalWaveNum)
        {
            stageManager.SetWin();
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
    void Update()
    {
        //if (fireworkcounter > 0) {
        //    if (--fireworkcounter == 0) {
        //        foreach (VisualEffect i in FireWork)
        //            i.Stop();
        //    }
        //}

        if (inTutorial) {
            if (tutorialManager && tutorialManager.GetTutorialStage()>= TutorialManager.TutorialStageID.TutorialProgress_FirstWave) 
            {
                WaveChg();
                WaveTimer = Time.time;
                inTutorial = false;
            }
        }
        else if (stageManager.GetResult() == 0)
        {
            if (Time.time - WaveTimer > CurrAttr.waveWaitTime)
            {
                WaveChg();
                WaveTimer = Time.time;
            }
            if (enemySpawner.AllAliveMonstersList().Count <= 0)
                WaveTimer -= Time.deltaTime * 2.0f;
        }
    }

    private IEnumerator SpawnWave(WaveAttr wave)
    {
        float spawnTimer = wave.enmStartTime;
        bool CheckCustomData = sceneManager && (sceneManager.GetCurrIsland() == StageInfo.IslandNum - 1);
        bool isSpawning = true;
        while (stageManager.GetResult()==0 && isSpawning)
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
                    if (wave.enmDetail[i].waveID < CurrentWaveNum)
                    {
                        continue;
                    }
                    if (wave.enmDetail[i].waveID == CurrentWaveNum)
                    {
                        for (int j = wave.enmDetail[i].enmNum; j > 0; --j)
                        {
                            EnemyAttr attr = EnemyInfo.GetEnemyInfo(wave.enmDetail[i].enmType);
                            enemySpawner.Spawn(wave.enmDetail[i].enmType,
                                stageManager.GetPortalPosition()[wave.enmDetail[i].enmPort], new float3(),
                                (float)(attr.health * (1 + 0.005f * (CurrentWaveNum * CurrentWaveNum))),
                                attr.money * (CheckCustomData ? (int)StageInfo.resourceEx : 1),
                                attr.damage, attr.radius,
                                 (float)(attr.speed * (1 + 0.005f * (CurrentWaveNum * CurrentWaveNum) * (CheckCustomData ? (int)StageInfo.enmSpeedEx : 1))),
                                attr.time
                                );

                            yield return new WaitForSeconds(wave.enmSpawnPeriod);
                        }
                    }
                }
                isSpawning = false;
            }

            yield return new WaitForSeconds(0f);
        }
    }
}