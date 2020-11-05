using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class WaveManager : MonoBehaviour
{
   
   private readonly int FireworkMax=120;
   private int TotalWaveNum;
   private int CurrentWaveNum;
   private float WaveTimer;

   private StageAttr CurrAttr;

    public List<VisualEffect> Firewall;
    public  List<Text> waveNumUI;

    private int fireworkcounter;
    private InGameOperation sceneManager; 
    private StageManager stageManager;
    private EnemySpawner enemySpawner;

    // Start is called before the first frame update
    void Start()
    {
        CurrAttr = StageInfo.GetStageInfo();
        sceneManager = FindObjectOfType<InGameOperation>();
        stageManager = FindObjectOfType<StageManager>();
        enemySpawner = FindObjectOfType<EnemySpawner>();
        TotalWaveNum = CurrAttr.waveNum;
        CurrentWaveNum = 0;
        fireworkcounter = 0;
        //WaveChg();
        WaveTimer = Time.time;
    }

    public int GetTotalWaveNum() { return TotalWaveNum; }
    public int GetCurrentWaveNum() { return CurrentWaveNum; }

    public bool WaveChg() {
        CurrentWaveNum++;
        fireworkcounter = FireworkMax;

        foreach (VisualEffect i in Firewall)
            i.Play();

        if (CurrentWaveNum > TotalWaveNum) {
            stageManager.SetWin();
            return true;
        }
        foreach (Text i in waveNumUI) {
            i.text = "WAVE " + CurrentWaveNum;
            i.color=new Color(i.color.r,i.color.g,i.color.b,1.0f);
        }

        StartCoroutine(SpawnWave(CurrAttr.waveDetail[CurrentWaveNum-1]));
        return false;
    }
    // Update is called once per frame
    void Update()
    {
        if (fireworkcounter > 0) {
            if (--fireworkcounter == 0) {
                foreach (VisualEffect i in Firewall)
                    i.Stop();
            }
        }
        //if (stageManager.GetResult()==0) {
        //    if (Time.time - WaveTimer> CurrAttr.waveWaitTime) {
        //        WaveChg();
        //        WaveTimer = Time.time;
        //    }
        //    if (enemySpawner.AllAliveMonstersList().Count <= 0)
        //        WaveTimer -= Time.deltaTime * 2.0f;
        //}
    }

    private IEnumerator SpawnWave(WaveAttr wave)
    {
        WaveAttr recordWave = new WaveAttr(wave);
        float spawnTimer = recordWave.enmStartTime;

        bool CheckCustomData = sceneManager && (sceneManager.GetCurrIsland() == StageInfo.IslandNum - 1);
        int startI = 0;

        while (true)
        {
            bool isSpawning = false;
            spawnTimer -= Time.deltaTime;
            if (spawnTimer < 0)
            {
                for (int i = startI; i < recordWave.enmDetail.Length; ++i)
                {
                    if (isSpawning == false && recordWave.enmDetail[i].waveID < i)
                        startI = i;
                    if (recordWave.enmDetail[i].waveID > CurrentWaveNum)
                        break;

                    if (recordWave.enmDetail[i].waveID == CurrentWaveNum) {
                        EnemyAttr attr = EnemyInfo.GetEnemyInfo(recordWave.enmDetail[i].enmType);

                        enemySpawner.Spawn(recordWave.enmDetail[i].enmType,
                            stageManager.GetPortalPosition()[recordWave.enmDetail[i].enmPort], new float3(),
                            (float)(attr.health*(1+0.005f*(CurrentWaveNum* CurrentWaveNum))),
                            attr.money * (CheckCustomData? (int)StageInfo.resourceEx :1),
                            attr.damage,attr.radius,
                             (float)(attr.speed * (1 + 0.005f * (CurrentWaveNum * CurrentWaveNum)* (CheckCustomData ? (int)StageInfo.enmSpeedEx : 1))),
                            attr.time
                            );

                        recordWave.enmDetail[i].enmNum--;
                    }
                }
            }
            else spawnTimer = wave.enmSpawnPeriod;

             yield return new WaitForSeconds(0f);
        }
    }
}