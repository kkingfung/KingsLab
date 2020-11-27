using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    //[SerializeField] private List<AnimationCurve> SpawnRateCurves;

    public EnemySpawner enemySpawner;
    public WaveManager waveManager;
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

    public void SpawnMonster(string monsterName,Vector3 SpawnPoint)
    {
        if (enemySpawner.allMonsterList.ContainsKey(monsterName))
        {
            int waveNum = waveManager.GetCurrentWaveNum();
            EnemyAttr attr = EnemyInfo.GetEnemyInfo(monsterName);
            int[] entityIDList = enemySpawner.Spawn(monsterName, SpawnPoint, new float3(),
                attr.health * (1+ 0.5f * waveNum),attr.money,
                attr.damage, attr.radius, attr.speed * (1 + 0.02f * waveNum ), attr.time);
        }
    }

    public void SpawnBonusBoss(int bossID, Vector3 SpawnPoint) {
        switch (bossID) {
            case 0:
                SpawnMonster("MetalonGreen", SpawnPoint);
                break;
            case 1:
                SpawnMonster("MetalonPurple", SpawnPoint);
                break;
            case 2:
                SpawnMonster("MetalonRed", SpawnPoint);
                break;
        }
    }
}
