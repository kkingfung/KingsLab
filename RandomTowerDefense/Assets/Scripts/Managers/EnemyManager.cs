using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using RandomTowerDefense.Scene;
using RandomTowerDefense.Managers.Macro;
using RandomTowerDefense.DOTS.Spawner;
using RandomTowerDefense.Info;

namespace RandomTowerDefense.Managers.System
{
    /// <summary>
    /// エネミーのスポーンとライフサイクル管理を行うマネージャークラス
    /// </summary>
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

        public void SpawnMonster(string monsterName, Vector3 SpawnPoint, bool CustomData)
        {
            if (enemySpawner.allMonsterList.ContainsKey(monsterName))
            {
                int waveNum = waveManager.GetCurrentWaveNum();
                EnemyAttr attr = EnemyInfo.GetEnemyInfo(monsterName);
                int[] entityIDList;
                if (monsterName == "MetalonGreen" || monsterName == "MetalonPurple" || monsterName == "MetalonRed")
                {
                    entityIDList = enemySpawner.Spawn(monsterName, SpawnPoint, new float3(),
                    attr.Health * (0.5f * (CustomData ? (int)StageInfoDetail.customStageInfo.EnemyAttributeFactor : 1)
                    //+ ((debugManager != null) ? debugManager.enemylvl_Health * waveNum : 0)
                    ), attr.Money * (CustomData ? (int)StageInfoDetail.customStageInfo.ResourceFactor : 1), attr.Damage, attr.Radius,
                    attr.Speed * (1 + 0.1f * (CustomData ? (int)StageInfoDetail.customStageInfo.EnemyAttributeFactor : 1)
                    //+ ((debugManager != null) ? debugManager.enemylvl_Speed * waveNum : 0)
                    ), attr.Time);
                }
                else
                {
                    entityIDList = enemySpawner.Spawn(monsterName, SpawnPoint, new float3(),
                        attr.Health * waveNum * 0.5f * (CustomData ? (int)StageInfoDetail.customStageInfo.EnemyAttributeFactor : 1),
                        attr.Money * (CustomData ? (int)StageInfoDetail.customStageInfo.ResourceFactor : 1),
                        attr.Damage,
                        attr.Radius,
                        attr.Speed * (1 + 0.1f * waveNum * (CustomData ? (int)StageInfoDetail.customStageInfo.EnemyAttributeFactor : 1)),
                        attr.Time);
                }
            }
        }


        public List<GameObject> AllAliveMonstersList()
        {
            return enemySpawner.AllAliveMonstersList();
        }

        public void SpawnBonusBoss(int bossID, Vector3 SpawnPoint)
        {
            bool CheckCustomData = sceneManager && (sceneManager.GetCurrIsland() == StageInfoDetail.IslandNum - 1);
            switch (bossID)
            {
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
}
