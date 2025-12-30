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
    /// エネミー管理システム - 敵スポーンとライフサイクルの制御
    ///
    /// 主な機能:
    /// - ウェーブベース敵スポーン管理
    /// - ボーナスボス召喚システム
    /// - カスタムステージ属性との統合
    /// - 動的敵属性スケーリング（HP、スピード、報酬）
    /// - ECSスポーナーシステム連携
    /// - アクティブ敵追跡システム
    /// </summary>
    public class EnemyManager : MonoBehaviour
    {
        //[SerializeField] private List<AnimationCurve> SpawnRateCurves;
        /// <summary>シーン管理クラスの参照</summary>
        public InGameOperation sceneManager;
        /// <summary>敵生成管理クラスの参照</summary>
        public EnemySpawner enemySpawner;
        /// <summary>ウェーブ管理クラスの参照</summary>
        public WaveManager waveManager;
        /// <summary>デバッグ管理クラスの参照</summary>
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

        /// <summary>
        /// 指定名の敵を指定位置にスポーン
        /// </summary>
        /// <param name="monsterName">敵の名前（辞書キー）</param>
        /// <param name="SpawnPoint">スポーン位置</param>
        /// <param name="CustomData">カスタムステージデータを使用するか</param>
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

        /// <summary>
        /// 現在アクティブな全敵のリストを取得
        /// </summary>
        /// <returns>アクティブな敵GameObjectのリスト</returns>
        public List<GameObject> AllAliveMonstersList()
        {
            return enemySpawner.AllAliveMonstersList();
        }

        /// <summary>
        /// ボーナスボスを指定位置にスポーン
        /// </summary>
        /// <param name="bossID">ボスID（0:緑、1:紫、2:赤）</param>
        /// <param name="SpawnPoint">スポーン位置</param>
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
