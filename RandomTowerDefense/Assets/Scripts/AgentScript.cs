using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using RandomTowerDefense.Scene;
using RandomTowerDefense.MapGenerator;
using RandomTowerDefense.DOTS.Spawner;
using RandomTowerDefense.Managers.Macro;
using RandomTowerDefense.Managers.System;
using RandomTowerDefense.Managers;
using RandomTowerDefense.Info;
using RandomTowerDefense.Units;

namespace RandomTowerDefense.AI
{
    /// <summary>
    /// ML-Agents AI コントローラー ― タワー構築と敵スポーンの両 AI エージェントを統合管理するモジュール
    ///
    /// 主要機能:
    /// - デュアルエージェント構成：タワービルダーAI／エネミースポーナーAI
    /// - 自己対戦型トレーニング環境による競合学習
    /// - クアドラントベースの観測システムによる空間認識
    /// - タワー配置と敵スポーンをリアルタイムで意思決定
    /// - パフォーマンス指標および戦略的成果に基づく報酬システム
    /// </summary>
    public class AgentScript : Agent
    {
        public AITrainingOperation trainingSceneManager;
        public FilledMapGenerator filledMapGenerator;

        /// <summary>タワー構築決定用の敵スポーナー</summary>
        public EnemySpawner enemySpawner;
        public StageManager stageManager;
        /// <summary>敵スポーン決定用のタワースポーナー</summary>
        public TowerSpawner towerSpawner;
        public WaveManager waveManager;
        public DebugManager debugManager;

        /// <summary>エピソードリセット用マネージャー</summary>
        public TowerManager towerManager;
        public ResourceManager resourceManager;

        private bool _isTower;
        public bool isLearning;

        private int2 _agentCoord;
        private int2 _maxCoord;

        private int _counter;
        private int _hpRecord;
        private int _waveRecord;

        private int _loseCounter;

        private int[] _towerDistribution;
        private int[] _enemyDistribution;
        private int[] _pillarDistribution;

        private System.Random _prng;

        public override void Initialize()
        {
            waveManager.SpawnPointByAI = 1;
            _counter = 0;
            _hpRecord = 1;
            _waveRecord = 0;
            _isTower = GetComponent<BehaviorParameters>().TeamId == 0 ? false : true;
            _prng = new System.Random((int)Time.time);
        }

        private void Reset()
        {
            waveManager.SetCurrWAveNum(0);
            _counter = 0;
            _loseCounter = 0;
            if (trainingSceneManager)
                trainingSceneManager.pillar = null;

            foreach (GameObject enemy in enemySpawner.GameObjects)
            {
                if (enemy == null) break;
                if (enemy.activeSelf == false) continue;
                enemy.GetComponent<Enemy>().Damaged(0);
            }

            foreach (GameObject tower in towerSpawner.GameObjects)
            {
                if (tower == null) break;
                if (tower.activeSelf == false) continue;
                towerManager.RemoveTowerFromList(tower);
            }

            _pillarDistribution = null;

            filledMapGenerator.GenerateMap();
            resourceManager.ResetMaterial();
            if (PathfindingGridSetup.Instance != null)
                PathfindingGridSetup.Instance.isActivated = false;
        }

        private void FixedUpdate()
        {
            if (debugManager && debugManager.isSimulationTest && debugManager.isFetchDone == false)
                return;

            if (stageManager)
                CheckCastleHP();
            if (waveManager)
                CheckWave();

            if (_isTower == false)
            {
                RequestDecision();
                if (waveManager && waveManager.isSpawning && towerSpawner)
                {
                    if (_counter < 100)
                        _counter++;
                }
            }
            else
            {
                if (trainingSceneManager)
                {
                    if (trainingSceneManager.pillar == null)
                    {
                        RequestDecision();
                    }

                    if (isLearning && _loseCounter > 25)
                    {
                        int currWaveNum = waveManager.GetCurrentWaveNum();
                        AddReward(currWaveNum * currWaveNum);
                        if (PathfindingGridSetup.Instance != null && PathfindingGridSetup.Instance.isActivated == true)
                        {
                            AddReward(-1f * _loseCounter);
                            if (debugManager)
                                debugManager.MapResetted();
                            EndEpisode();
                        }
                    }
                }
            }
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(_isTower ? 1f : -1f);

            _towerDistribution = CntTowerRankTotal();
            if (_towerDistribution.Length == 4)
            {
                sensor.AddObservation((float)_towerDistribution[0]);
                sensor.AddObservation((float)_towerDistribution[1]);
                sensor.AddObservation((float)_towerDistribution[2]);
                sensor.AddObservation((float)_towerDistribution[3]);
            }

            _enemyDistribution = CntEnemyTotal();
            if (_enemyDistribution.Length == 4)
            {
                sensor.AddObservation((float)_enemyDistribution[0]);
                sensor.AddObservation((float)_enemyDistribution[1]);
                sensor.AddObservation((float)_enemyDistribution[2]);
                sensor.AddObservation((float)_enemyDistribution[3]);
            }

            if ((_pillarDistribution == null))
                _pillarDistribution = CntPillarTotal();
            if (_pillarDistribution.Length == 4)
            {
                sensor.AddObservation((float)_enemyDistribution[0]);
                sensor.AddObservation((float)_enemyDistribution[1]);
                sensor.AddObservation((float)_enemyDistribution[2]);
                sensor.AddObservation((float)_enemyDistribution[3]);
            }
        }

        public override void OnActionReceived(float[] vectorAction)
        {
            if (_isTower == false)
            {
                int temp = waveManager.SpawnPointByAI;
                if (vectorAction[0] < -0.3)
                    waveManager.SpawnPointByAI = 0;
                else if (vectorAction[0] > 0.3)
                    waveManager.SpawnPointByAI = 2;
                else
                    waveManager.SpawnPointByAI = 1;

                if (temp == waveManager.SpawnPointByAI)
                    AddReward(-1 * _counter / 100.0f);
                else
                    _counter = 0;

                if (vectorAction[1] > 0)
                {
                    waveManager.agentCallWait = true;
                    AddReward(-1);
                }
                else
                {
                    waveManager.agentCallWait = false;
                }
            }
            else
            {
                if (trainingSceneManager && trainingSceneManager.pillar == null)
                {
                    if (vectorAction[0] < -0.5f)
                        trainingSceneManager.pillar = GetRandomFreePillar(0);
                    else
                    if (vectorAction[0] < 0f)
                        trainingSceneManager.pillar = GetRandomFreePillar(1);
                    else
                    if (vectorAction[0] < 0.5f)
                        trainingSceneManager.pillar = GetRandomFreePillar(2);
                    else
                        trainingSceneManager.pillar = GetRandomFreePillar(3);
                }
                if (trainingSceneManager.tellMerge == false && vectorAction[1] > 0)
                    trainingSceneManager.tellMerge = true;
            }

        }

        public override void Heuristic(float[] actionsOut)
        {
            if (!_isTower)
            {
                if (Input.GetKey(KeyCode.Keypad4))
                {
                    waveManager.SpawnPointByAI = 0;
                }
                else if (Input.GetKey(KeyCode.Keypad5))
                {
                    waveManager.SpawnPointByAI = 1;
                }
                else if (Input.GetKey(KeyCode.Keypad6))
                {
                    waveManager.SpawnPointByAI = 2;
                }
            }
            if (_isTower && Input.GetKey(KeyCode.Keypad0))
                trainingSceneManager.pillar = GetRandomFreePillar(true);
        }

        public override void OnEpisodeBegin()
        {
            Reset();
        }

        public void EnemyDisappear(Vector3 EnemyOriPos, Vector3 EnemyDiePos)
        {
            if (_isTower == false)
            {
                // 報酬付与: 敵の移動距離に基づく
                AddReward((EnemyOriPos - EnemyDiePos).sqrMagnitude / 1000f);
            }
            else
            {
                AddReward(1);
            }
        }

        public void CheckCastleHP()
        {
            int CurrHp = stageManager.GetHealth();
            if (CurrHp != _hpRecord)
            {
                if (_isTower == true)
                {
                    if (_hpRecord > CurrHp)
                    {
                        _loseCounter += _hpRecord - CurrHp;
                        AddReward(CurrHp - _hpRecord);
                    }
                }
                else
                    AddReward(100);
                _hpRecord = CurrHp;
            }
        }
        public void CheckWave()
        {
            int CurrWave = waveManager.GetCurrentWaveNum();
            if (CurrWave != _waveRecord)
            {
                if (_isTower == true)
                {
                    AddReward(CurrWave * CurrWave);
                }
                _waveRecord = CurrWave;
            }
        }
        #region MakingDecisionForEnemy
        // 0 | 1
        //-------
        // 2 | 3

        int[] CntTowerRankTotal()
        {
            int[] TotalRankInQuarteredMap = new int[4];
            if (towerSpawner == null || filledMapGenerator == null) return TotalRankInQuarteredMap;
            _agentCoord = filledMapGenerator.GetTileIDFromPosition(this.transform.position);
            _agentCoord = new int2(StageInfoDetail.customStageInfo.StageSizeFactor / 2, StageInfoDetail.customStageInfo.StageSizeFactor / 2);
            _maxCoord = filledMapGenerator.MapSize;

            int[] SubTotalRankInQuarteredMap;

            int rank = 1;
            SubTotalRankInQuarteredMap = CntTowerRankSubtotal(towerSpawner.TowerNightmareRank1);
            for (int i = 0; i < 4; ++i)
                TotalRankInQuarteredMap[i] += SubTotalRankInQuarteredMap[i] * rank;
            SubTotalRankInQuarteredMap = CntTowerRankSubtotal(towerSpawner.TowerSoulEaterRank1);
            for (int i = 0; i < 4; ++i)
                TotalRankInQuarteredMap[i] += SubTotalRankInQuarteredMap[i] * rank;
            SubTotalRankInQuarteredMap = CntTowerRankSubtotal(towerSpawner.TowerTerrorBringerRank1);
            for (int i = 0; i < 4; ++i)
                TotalRankInQuarteredMap[i] += SubTotalRankInQuarteredMap[i] * rank;
            SubTotalRankInQuarteredMap = CntTowerRankSubtotal(towerSpawner.TowerUsurperRank1);
            for (int i = 0; i < 4; ++i)
                TotalRankInQuarteredMap[i] += SubTotalRankInQuarteredMap[i] * rank;

            rank++;
            SubTotalRankInQuarteredMap = CntTowerRankSubtotal(towerSpawner.TowerNightmareRank2);
            for (int i = 0; i < 4; ++i)
                TotalRankInQuarteredMap[i] += SubTotalRankInQuarteredMap[i] * rank;
            SubTotalRankInQuarteredMap = CntTowerRankSubtotal(towerSpawner.TowerSoulEaterRank2);
            for (int i = 0; i < 4; ++i)
                TotalRankInQuarteredMap[i] += SubTotalRankInQuarteredMap[i] * rank;
            SubTotalRankInQuarteredMap = CntTowerRankSubtotal(towerSpawner.TowerTerrorBringerRank2);
            for (int i = 0; i < 4; ++i)
                TotalRankInQuarteredMap[i] += SubTotalRankInQuarteredMap[i] * rank;
            SubTotalRankInQuarteredMap = CntTowerRankSubtotal(towerSpawner.TowerUsurperRank2);
            for (int i = 0; i < 4; ++i)
                TotalRankInQuarteredMap[i] += SubTotalRankInQuarteredMap[i] * rank;

            rank++;
            SubTotalRankInQuarteredMap = CntTowerRankSubtotal(towerSpawner.TowerNightmareRank3);
            for (int i = 0; i < 4; ++i)
                TotalRankInQuarteredMap[i] += SubTotalRankInQuarteredMap[i] * rank;
            SubTotalRankInQuarteredMap = CntTowerRankSubtotal(towerSpawner.TowerSoulEaterRank3);
            for (int i = 0; i < 4; ++i)
                TotalRankInQuarteredMap[i] += SubTotalRankInQuarteredMap[i] * rank;
            SubTotalRankInQuarteredMap = CntTowerRankSubtotal(towerSpawner.TowerTerrorBringerRank3);
            for (int i = 0; i < 4; ++i)
                TotalRankInQuarteredMap[i] += SubTotalRankInQuarteredMap[i] * rank;
            SubTotalRankInQuarteredMap = CntTowerRankSubtotal(towerSpawner.TowerUsurperRank3);
            for (int i = 0; i < 4; ++i)
                TotalRankInQuarteredMap[i] += SubTotalRankInQuarteredMap[i] * rank;

            rank++;
            SubTotalRankInQuarteredMap = CntTowerRankSubtotal(towerSpawner.TowerNightmareRank4);
            for (int i = 0; i < 4; ++i)
                TotalRankInQuarteredMap[i] += SubTotalRankInQuarteredMap[i] * rank;
            SubTotalRankInQuarteredMap = CntTowerRankSubtotal(towerSpawner.TowerSoulEaterRank4);
            for (int i = 0; i < 4; ++i)
                TotalRankInQuarteredMap[i] += SubTotalRankInQuarteredMap[i] * rank;
            SubTotalRankInQuarteredMap = CntTowerRankSubtotal(towerSpawner.TowerTerrorBringerRank4);
            for (int i = 0; i < 4; ++i)
                TotalRankInQuarteredMap[i] += SubTotalRankInQuarteredMap[i] * rank;
            SubTotalRankInQuarteredMap = CntTowerRankSubtotal(towerSpawner.TowerUsurperRank4);
            for (int i = 0; i < 4; ++i)
                TotalRankInQuarteredMap[i] += SubTotalRankInQuarteredMap[i] * rank;

            return TotalRankInQuarteredMap;
        }

        int[] CntTowerRankSubtotal(List<GameObject> towerList)
        {
            int[] SubTotalRankInQuarteredMap = new int[4];
            foreach (GameObject i in towerList)
            {
                if (i.activeSelf == false) continue;
                int2 coordTarget = filledMapGenerator.GetTileIDFromPosition(i.transform.position);
                if (coordTarget.x < _agentCoord.x)
                {
                    if (coordTarget.y < _agentCoord.y)
                    {
                        SubTotalRankInQuarteredMap[2]++;
                    }
                    else
                    {
                        SubTotalRankInQuarteredMap[0]++;
                    }
                }
                else
                {
                    if (coordTarget.y < _agentCoord.y)
                    {
                        SubTotalRankInQuarteredMap[3]++;
                    }
                    else
                    {
                        SubTotalRankInQuarteredMap[1]++;
                    }
                }
            }

            return SubTotalRankInQuarteredMap;
        }
        #endregion

        int[] CntPillarTotal()
        {
            int[] TotalRankInQuarteredMap = new int[4];
            if (filledMapGenerator == null) return TotalRankInQuarteredMap;
            List<Pillar> allPillar = filledMapGenerator.GetPillarList();
            if (allPillar == null || allPillar.Count == 0) return TotalRankInQuarteredMap;

            _agentCoord = filledMapGenerator.GetTileIDFromPosition(this.transform.position);
            _maxCoord = filledMapGenerator.MapSize;

            foreach (Pillar i in allPillar)
            {
                int2 PillarCoord = new int2(i.mapSize.x, i.mapSize.y);
                if (PillarCoord.x <= _agentCoord.x && PillarCoord.y >= _agentCoord.y)
                {
                    TotalRankInQuarteredMap[0]++;
                }
                else if (PillarCoord.x <= _agentCoord.x && PillarCoord.y >= _agentCoord.y)
                {
                    TotalRankInQuarteredMap[1]++;
                }
                else if (PillarCoord.x <= _agentCoord.x && PillarCoord.y <= _agentCoord.y)
                {
                    TotalRankInQuarteredMap[2]++;
                }
                else if (PillarCoord.x >= _agentCoord.x && PillarCoord.y >= _agentCoord.y)
                {
                    TotalRankInQuarteredMap[3]++;
                }
            }
            return TotalRankInQuarteredMap;
        }

        #region MakingDecisionForTower
        // 0 | 1
        //-------
        // 2 | 3

        int[] CntEnemyTotal()
        {
            int[] TotalRankInQuarteredMap = new int[4];
            if (enemySpawner == null) return TotalRankInQuarteredMap;
            List<GameObject> allMonsters = enemySpawner.AllAliveMonstersList();
            if (allMonsters.Count == 0) return TotalRankInQuarteredMap;

            _agentCoord = filledMapGenerator.GetTileIDFromPosition(this.transform.position);
            _maxCoord = filledMapGenerator.MapSize;

            foreach (GameObject i in allMonsters)
            {
                if (i.activeSelf == false) continue;
                int2 EnemyCoord = filledMapGenerator.GetTileIDFromPosition(i.transform.position);
                if (EnemyCoord.x <= _agentCoord.x && EnemyCoord.y >= _agentCoord.y)
                {
                    TotalRankInQuarteredMap[0]++;
                }
                else if (EnemyCoord.x <= _agentCoord.x && EnemyCoord.y >= _agentCoord.y)
                {
                    TotalRankInQuarteredMap[1]++;
                }
                else if (EnemyCoord.x <= _agentCoord.x && EnemyCoord.y <= _agentCoord.y)
                {
                    TotalRankInQuarteredMap[2]++;
                }
                else if (EnemyCoord.x >= _agentCoord.x && EnemyCoord.y >= _agentCoord.y)
                {
                    TotalRankInQuarteredMap[3]++;
                }
            }
            return TotalRankInQuarteredMap;
        }
        #endregion

        #region GetFreePillar
        private GameObject GetRandomFreePillar(bool isFree = true)
        {
            GameObject targetPillar = null;
            var pillarList = filledMapGenerator.GetPillarList();
            var totalPilliarCount = pillarList.Count;
            int cnt = 0;
            while (cnt < totalPilliarCount)
            {
                int id = _prng.Next(0, totalPilliarCount);
                var pillar = pillarList[id];
                if (pillar.state == (isFree ? 0 : 1))
                {
                    if (pillar.surroundSpace > 0)
                    {
                        return pillar.obj;
                    }

                    if (targetPillar == null)
                    {
                        targetPillar = pillar.obj;
                    }
                }

                cnt++;
            }
            return targetPillar;
        }

        /// <summary>
        /// 指定エリア内のランダムな空きピラーを取得
        /// </summary>
        /// <param name="areaID">エリアID（0～3）</param>
        /// <returns>ピラーのGameObject、見つからなければnull</returns>
        private GameObject GetRandomFreePillar(int areaID)
        {
            Pillar targetPillar = null;
            var pillarList = filledMapGenerator.GetPillarList();
            var totalPilliarCount = pillarList.Count;
            Func<Pillar, GameObject> getPillarCandidate = target =>
            {
                if (target.state == 0 && target.surroundSpace > 0)
                {
                    return target.obj;
                }

                return null;
            };

            int cnt = 0;
            while (cnt < totalPilliarCount)
            {
                int id = _prng.Next(0, totalPilliarCount);
                var pillar = pillarList[id];
                var result = getPillarCandidate(pillar);
                if (result != null)
                {
                    pillar.state = 1;
                    return result;
                }

                targetPillar = pillar;
                cnt++;
            }

            if (targetPillar != null)
            {
                targetPillar.state = 1;
                return targetPillar.obj;
            }

            return null;
        }

        #endregion
    }
}
