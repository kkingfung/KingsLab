using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    //[SerializeField] private List<AnimationCurve> SpawnRateCurves;
    public InGameOperation sceneManager;
    public EnemySpawner enemySpawner;
    public WaveManager waveManager;
    public DebugManager debugManager;

    // Start is called before the first frame update
    void Start()
    {
        //enemySpawner = FindObjectOfType<EnemySpawner>();
        //waveManager = FindObjectOfType<WaveManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnMonster(string monsterName,Vector3 SpawnPoint, bool CustomData)
    {
        if (enemySpawner.allMonsterList.ContainsKey(monsterName))
        {
            int waveNum = waveManager.GetCurrentWaveNum();
            EnemyAttr attr = EnemyInfo.GetEnemyInfo(monsterName);
            int[] entityIDList = enemySpawner.Spawn(monsterName, SpawnPoint, new float3(),
                attr.health * (1 + waveNum * 0.75f * (CustomData ? (int)StageInfo.enmAttributeEx : 1)
                + ((debugManager != null) ? debugManager.enemylvl_Health * waveNum : 0))
                , attr.money * (CustomData ? (int)StageInfo.resourceEx : 1), attr.damage, attr.radius, 
                attr.speed * (1 + 0.02f * waveNum * (CustomData ? (int)StageInfo.enmAttributeEx : 1)
                + ((debugManager != null) ? debugManager.enemylvl_Speed * waveNum : 0)), attr.time);
        }
    }


    public List<GameObject> AllAliveMonstersList()
    {
        return enemySpawner.AllAliveMonstersList();
    }

    public void SpawnBonusBoss(int bossID, Vector3 SpawnPoint) {
        bool CheckCustomData = sceneManager && (sceneManager.GetCurrIsland() == StageInfo.IslandNum - 1);
        switch (bossID) {
            case 0:
                SpawnMonster("MetalonGreen", SpawnPoint, CheckCustomData);
                break;
            case 1:
                SpawnMonster("MetalonPurple", SpawnPoint, CheckCustomData);
                break;
            case 2:
                SpawnMonster("MetalonRed", SpawnPoint, CheckCustomData);
                break;
        }
    }
}
