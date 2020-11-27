using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;

public class AgentScript : Agent
{
    public AITrainingOperation trainingSceneManager;
    public FilledMapGenerator filledMapGenerator;

    public EnemySpawner enemySpawner;//GetEnemyInfo For TowerBuildDecision
    public StageManager stageManager;
    public TowerSpawner towerSpawner;//GetAllTowerInfo For EnemySpawningDecision
    public WaveManager waveManager;

    //For Reset Episode
    public TowerManager towerManager;
    public ResourceManager resourceManager;

    private bool isTower;

    private int2 AgentCoord;
    //private int2 MaxCoord;
    //private Vector3 CastlePos;

    private int counter;
    private int HpRecord;

    int[] TowerDistribution;
    int[] EnemyDistribution;

    public override void Initialize()
    {
        waveManager.SpawnPointByAI = 1;
        counter = 0;
        HpRecord = 1;
        isTower = GetComponent<BehaviorParameters>().TeamId == 0 ? false : true;
    }

    private void Reset()
    {
        waveManager.SpawnPointByAI = 1;
        waveManager.SetCurrWAveNum(0);
        counter = 0;

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
            towerManager.removeTowerFromList(tower);
        }

        filledMapGenerator.GenerateMap();
        resourceManager.ResetMaterial();
        if (PathfindingGridSetup.Instance != null)
            PathfindingGridSetup.Instance.isActived = false;
    }

    private void FixedUpdate()
    {
        if (stageManager)
            CheckCastleHP();

        if (isTower == false)
        {
            if (waveManager && waveManager.isSpawning && towerSpawner)
            {
                if (counter < 100)
                    counter++;
                RequestDecision();
            }
        }
        else
        {
            if (trainingSceneManager && trainingSceneManager.pillar == null)
            {
                RequestDecision();
            }
        }
        if (waveManager.GetCurrentWaveNum() >= 50 || Input.GetKey(KeyCode.Keypad2))
        {
            if (PathfindingGridSetup.Instance != null && PathfindingGridSetup.Instance.isActived == true)
                EndEpisode();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(isTower ? 1f : -1f);

        TowerDistribution = CntTowerRankTotal();
        if (TowerDistribution.Length == 4)
        {
            sensor.AddObservation((float)TowerDistribution[0]);
            sensor.AddObservation((float)TowerDistribution[1]);
            sensor.AddObservation((float)TowerDistribution[2]);
            sensor.AddObservation((float)TowerDistribution[3]);
        }

        EnemyDistribution = CntEnemyTotal();
        if (EnemyDistribution.Length == 4)
        {
            sensor.AddObservation((float)EnemyDistribution[0]);
            sensor.AddObservation((float)EnemyDistribution[1]);
            sensor.AddObservation((float)EnemyDistribution[2]);
            sensor.AddObservation((float)EnemyDistribution[3]);
        }
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        if (isTower == false)
        {
            int temp = waveManager.SpawnPointByAI;

            if (vectorAction[0] < -0.3)
                waveManager.SpawnPointByAI = 0;
            else if (vectorAction[0] > 0.3)
                waveManager.SpawnPointByAI = 2;
            else
                waveManager.SpawnPointByAI = 1;

            if (temp == waveManager.SpawnPointByAI)
                AddReward(-1 * counter / 100.0f);
            else
                counter = 0;

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
        if (Input.GetKey(KeyCode.Keypad4))
            waveManager.SpawnPointByAI = 0;
        else if (Input.GetKey(KeyCode.Keypad5))
            waveManager.SpawnPointByAI = 1;
        else if (Input.GetKey(KeyCode.Keypad6))
            waveManager.SpawnPointByAI = 2;

        if (Input.GetKey(KeyCode.Keypad0))
            trainingSceneManager.pillar = GetRandomFreePillar(true);
    }

    public override void OnEpisodeBegin()
    {
        Reset();
        Application.Quit();
    }

    public void EnemyDisappear(Vector3 EnemyOriPos, Vector3 EnemyDiePos)
    {
        if (isTower == false)
            //Adding Reward
            AddReward((EnemyOriPos - EnemyDiePos).sqrMagnitude / 1000f);
        else
            AddReward(1);
    }

    public void CheckCastleHP()
    {
        int CurrHp = stageManager.GetHealth();
        if (CurrHp != HpRecord)
        {
            if (isTower == true)
            {
                if (HpRecord > CurrHp)
                    AddReward(CurrHp - HpRecord);
            }
            else
                AddReward(100);
            HpRecord = CurrHp;
        }
    }

    #region MakingDecisionForEnemy
    // 0 | 1
    //-------
    // 2 | 3

    int[] CntTowerRankTotal()
    {
        int[] TotalRankInQuarteredMap = new int[4];
        if (towerSpawner == null) return TotalRankInQuarteredMap;
        AgentCoord = filledMapGenerator.GetTileIDFromPosition(this.transform.position);
        //MaxCoord = filledMapGenerator.MapSize;

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
            if (coordTarget.x < AgentCoord.x)
            {
                if (coordTarget.y < AgentCoord.y)
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
                if (coordTarget.y < AgentCoord.y)
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

        AgentCoord = filledMapGenerator.GetTileIDFromPosition(this.transform.position);
        //MaxCoord = filledMapGenerator.MapSize;

        foreach (GameObject i in allMonsters)
        {
            if (i.activeSelf == false) continue;
            int2 EnmCoord = filledMapGenerator.GetTileIDFromPosition(i.transform.position);
            if (EnmCoord.x <= AgentCoord.x && EnmCoord.y >= AgentCoord.y)
            {
                TotalRankInQuarteredMap[0]++;
            }
            else if (EnmCoord.x <= AgentCoord.x && EnmCoord.y >= AgentCoord.y)
            {
                TotalRankInQuarteredMap[1]++;
            }
            else if (EnmCoord.x <= AgentCoord.x && EnmCoord.y <= AgentCoord.y)
            {
                TotalRankInQuarteredMap[2]++;
            }
            else if (EnmCoord.x >= AgentCoord.x && EnmCoord.y >= AgentCoord.y)
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

        int cnt = 0;
        while (cnt < filledMapGenerator.PillarList.Count)
        {
            int id = UnityEngine.Random.Range(0, filledMapGenerator.PillarList.Count);
            if (filledMapGenerator.PillarList[id].state == (isFree ? 0 : 1))
            {
                if (filledMapGenerator.PillarList[id].surroundSpace > 0)
                    return filledMapGenerator.PillarList[id].obj;
                if (targetPillar == null)
                    targetPillar = filledMapGenerator.PillarList[id].obj;
            }

            cnt++;
        }
        return targetPillar;
    }
    private GameObject GetRandomFreePillar(int areaID)
    {
        Pillar targetPillar = null;
        int cnt = 0;
        while (cnt < filledMapGenerator.PillarList.Count)
        {
            int id = UnityEngine.Random.Range(0, filledMapGenerator.PillarList.Count);
            switch (areaID)
            {
                case 0:
                    if (filledMapGenerator.PillarList[id].mapSize.x <= AgentCoord.x
                        && filledMapGenerator.PillarList[id].mapSize.y >= AgentCoord.y)
                    {
                        if (filledMapGenerator.PillarList[id].state == 0)
                        {
                            if (filledMapGenerator.PillarList[id].surroundSpace > 0)
                            {
                                //filledMapGenerator.PillarList[id].state = 1;
                                return filledMapGenerator.PillarList[id].obj;
                            }
                            if (targetPillar == null)
                                targetPillar = filledMapGenerator.PillarList[id];
                        }
                    }
                    break;
                case 1:
                    if (filledMapGenerator.PillarList[id].mapSize.x >= AgentCoord.x
                        && filledMapGenerator.PillarList[id].mapSize.y <= AgentCoord.y)
                    {
                        if (filledMapGenerator.PillarList[id].state == 0)
                        {
                            if (filledMapGenerator.PillarList[id].surroundSpace > 0)
                            {
                                //filledMapGenerator.PillarList[id].state = 1;
                                return filledMapGenerator.PillarList[id].obj;
                            }
                            if (targetPillar == null)
                                targetPillar = filledMapGenerator.PillarList[id];
                        }
                    }
                    break;
                case 2:
                    if (filledMapGenerator.PillarList[id].mapSize.x <= AgentCoord.x
                        && filledMapGenerator.PillarList[id].mapSize.y <= AgentCoord.y)
                    {
                        if (filledMapGenerator.PillarList[id].state == 0)
                        {
                            if (filledMapGenerator.PillarList[id].surroundSpace > 0)
                            {
                                //filledMapGenerator.PillarList[id].state = 1;
                                return filledMapGenerator.PillarList[id].obj;
                            }
                            if (targetPillar == null)
                                targetPillar = filledMapGenerator.PillarList[id];
                        }
                    }
                    break;
                case 3:
                    if (filledMapGenerator.PillarList[id].mapSize.x >= AgentCoord.x
                        && filledMapGenerator.PillarList[id].mapSize.y >= AgentCoord.y)
                    {
                        if (filledMapGenerator.PillarList[id].state == 0)
                        {
                            if (filledMapGenerator.PillarList[id].surroundSpace > 0)
                            {
                                //filledMapGenerator.PillarList[id].state = 1;
                                return filledMapGenerator.PillarList[id].obj;
                            }
                            if (targetPillar == null)
                                targetPillar = filledMapGenerator.PillarList[id];
                        }
                    }
                    break;
            }
            cnt++;
        }
        if (targetPillar != null)
        {
            //    targetPillar.state = 1;
            return targetPillar.obj;
        }
        return null;
    }
    #endregion
}
