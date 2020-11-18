using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    //[SerializeField] private List<AnimationCurve> SpawnRateCurves;

    private EnemySpawner enemySpawner;
    // Start is called before the first frame update
    void Start()
    {
        enemySpawner = FindObjectOfType<EnemySpawner>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnMonster(string monsterName,Vector3 SpawnPoint)
    {
        if (enemySpawner.allMonsterList.ContainsKey(monsterName))
        {
            EnemyAttr attr = EnemyInfo.GetEnemyInfo(monsterName);
            int[] entityIDList=enemySpawner.Spawn(monsterName, SpawnPoint,new float3(), attr.health,attr.money,
                attr.damage, attr.radius, attr.speed, attr.time);
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
