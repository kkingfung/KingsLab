using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.Mathematics;
using Unity.MLAgents.Sensors;

public class TestingAgentScript : Agent
{
    private WaveManager waveManager;
    private FilledMapGenerator filledMapGenerator;
    private TowerSpawner towerSpawner;

    private int2 AgentCoord;
    //private int2 MaxCoord;
    //private Vector3 CastlePos;

    private int counter;

    int[] TowerDistribution;
    public override void Initialize()
    {
        waveManager = FindObjectOfType<WaveManager>();
        waveManager.SpawnPointByAI = 1;
        counter = 0;

        filledMapGenerator = FindObjectOfType<FilledMapGenerator>();
     
        towerSpawner = FindObjectOfType<TowerSpawner>();
    }

    private void Reset()
    {
        waveManager.SpawnPointByAI = 1;
        counter = 0;
    }

    private void FixedUpdate()
    {
        if (waveManager && waveManager.isSpawning && towerSpawner)
        {
            if(counter<100)
            counter++;
            RequestDecision();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        TowerDistribution = CntTowerRankTotal();
        if (TowerDistribution.Length == 4) {
            sensor.AddObservation(TowerDistribution[0]);
            sensor.AddObservation(TowerDistribution[1]);
            sensor.AddObservation(TowerDistribution[2]);
            sensor.AddObservation(TowerDistribution[3]);
        }
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        int temp = waveManager.SpawnPointByAI;

        if (vectorAction[0] == -1)
            waveManager.SpawnPointByAI = 0;
        else if (vectorAction[0] == 0)
            waveManager.SpawnPointByAI = 1;
        else if (vectorAction[0] == 1)
            waveManager.SpawnPointByAI = 2;

        if (temp == waveManager.SpawnPointByAI)
            AddReward(-1 * counter/100.0f);
        else
            counter = 0;

    }

    public override void Heuristic(float[] actionsOut)
    {
        if (Input.GetKey(KeyCode.Keypad4))
            waveManager.SpawnPointByAI = 0;
        else if (Input.GetKey(KeyCode.Keypad5))
            waveManager.SpawnPointByAI = 1;
        else if (Input.GetKey(KeyCode.Keypad6))
            waveManager.SpawnPointByAI = 2;
    }

    public override void OnEpisodeBegin()
    {
        Reset();
    }

    public void EnemyDisappear(Vector3 EnemyOriPos,Vector3 EnemyDiePos) {
        //Adding Reward
        AddReward((EnemyOriPos- EnemyDiePos).sqrMagnitude/1000f);
    }

    #region MakingDecision
    // 0 | 1
    //-------
    // 2 | 3

    int[] CntTowerRankTotal() {
        int[] TotalRankInQuarteredMap = new int[4];
        if (towerSpawner == null) return TotalRankInQuarteredMap;
        AgentCoord = filledMapGenerator.GetTileIDFromPosition(this.transform.position);
        //MaxCoord = filledMapGenerator.MapSize;

        int[] SubTotalRankInQuarteredMap;

        int rank = 1;
        SubTotalRankInQuarteredMap= CntTowerRankSubtotal(towerSpawner.TowerNightmareRank1);
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

    int[] CntTowerRankSubtotal(List<GameObject> towerList) {
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
}
