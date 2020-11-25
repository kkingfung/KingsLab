using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusChecker : MonoBehaviour
{
    private readonly int[] BonusForAllMonstersByRankChk = { 100, 250, 500, 800 };
    private readonly int BonusForAllRanksByTypeChk = 2000;

    public ResourceManager resourceManager;
    public TowerSpawner towerSpawner;
    bool[] AllMonstersByRankBonus= { false,false,false,false};
    bool[] AllRanksByMonsterBonus= { false,false,false,false};

    // Start is called before the first frame update
    void Start()
    {
        //towerSpawner = FindObjectOfType<TowerSpawner>();
        //resourceManager = FindObjectOfType<ResourceManager>();
    }

    // Update is called once per frame
    void Update()
    {

        if (!AllRanksByMonsterBonus[(int)TowerInfo.TowerInfoID.Enum_TowerNightmare])
            AllRanksByMonsterBonus[(int)TowerInfo.TowerInfoID.Enum_TowerNightmare] 
                = MonsterList_Type(TowerInfo.TowerInfoID.Enum_TowerNightmare);

        if (!AllRanksByMonsterBonus[(int)TowerInfo.TowerInfoID.Enum_TowerSoulEater])
            AllRanksByMonsterBonus[(int)TowerInfo.TowerInfoID.Enum_TowerSoulEater]
                = MonsterList_Type(TowerInfo.TowerInfoID.Enum_TowerSoulEater);

        if (!AllRanksByMonsterBonus[(int)TowerInfo.TowerInfoID.Enum_TowerTerrorBringer])
            AllRanksByMonsterBonus[(int)TowerInfo.TowerInfoID.Enum_TowerTerrorBringer]
                = MonsterList_Type(TowerInfo.TowerInfoID.Enum_TowerTerrorBringer);

        if (!AllRanksByMonsterBonus[(int)TowerInfo.TowerInfoID.Enum_TowerUsurper])
            AllRanksByMonsterBonus[(int)TowerInfo.TowerInfoID.Enum_TowerUsurper]
               = MonsterList_Type(TowerInfo.TowerInfoID.Enum_TowerUsurper);

        for (int i = 0; i < AllMonstersByRankBonus.Length; ++i)
        {
            if (!AllMonstersByRankBonus[i])
                AllMonstersByRankBonus[i] = MonseterList_Rank(i + 1);
        }
    }

    bool MonsterList_Type(TowerInfo.TowerInfoID towerID) {
        int result = 0x00000;

        switch (towerID) 
        {
            case TowerInfo.TowerInfoID.Enum_TowerNightmare:
                for (int i = TowerSpawner.MonsterMaxRank; i > 0; --i) {
                    switch (i) {
                        case 1:
                            if (CheckActivenessInList(towerSpawner.TowerNightmareRank1))
                                result |= (0x1 << (i));
                            break;
                        case 2:
                            if (CheckActivenessInList(towerSpawner.TowerNightmareRank2))
                                result |= (0x1 << (i));
                            break;
                        case 3:
                            if (CheckActivenessInList(towerSpawner.TowerNightmareRank3))
                                result |= (0x1 << (i));
                            break;
                        case 4:
                            if (CheckActivenessInList(towerSpawner.TowerNightmareRank4))
                                result |= (0x1 << (i));
                            break;
                    }
                    if (result == 0x00000)
                        return false;
                }
                break;
            case TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                for (int i = TowerSpawner.MonsterMaxRank; i > 0; --i)
                {
                    switch (i)
                    {
                        case 1:
                            if (CheckActivenessInList(towerSpawner.TowerSoulEaterRank1))
                                result |= (0x1 << (i));
                            break;
                        case 2:
                            if (CheckActivenessInList(towerSpawner.TowerSoulEaterRank2))
                                result |= (0x1 << (i));
                            break;
                        case 3:
                            if (CheckActivenessInList(towerSpawner.TowerSoulEaterRank3))
                                result |= (0x1 << (i));
                            break;
                        case 4:
                            if (CheckActivenessInList(towerSpawner.TowerSoulEaterRank4))
                                result |= (0x1 << (i));
                            break;
                    }
                    if (result == 0x00000)
                        return false;
                }
                break;
            case TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
                for (int i = TowerSpawner.MonsterMaxRank; i > 0; --i)
                {
                    switch (i)
                    {
                        case 1:
                            if (CheckActivenessInList(towerSpawner.TowerTerrorBringerRank1))
                                result |= (0x1 << (i));
                            break;
                        case 2:
                            if (CheckActivenessInList(towerSpawner.TowerTerrorBringerRank2))
                                result |= (0x1 << (i));
                            break;
                        case 3:
                            if (CheckActivenessInList(towerSpawner.TowerTerrorBringerRank3))
                                result |= (0x1 << (i));
                            break;
                        case 4:
                            if (CheckActivenessInList(towerSpawner.TowerTerrorBringerRank4))
                                result |= (0x1 << (i));
                            break;
                    }
                    if (result == 0x00000)
                        return false;
                }
                break;
            case TowerInfo.TowerInfoID.Enum_TowerUsurper:
                for (int i = TowerSpawner.MonsterMaxRank; i > 0; --i)
                {
                    switch (i)
                    {
                        case 1:
                            if (CheckActivenessInList(towerSpawner.TowerUsurperRank1))
                                result |= (0x1 << (i));
                            break;
                        case 2:
                            if (CheckActivenessInList(towerSpawner.TowerUsurperRank2))
                                result |= (0x1 << (i));
                            break;
                        case 3:
                            if (CheckActivenessInList(towerSpawner.TowerUsurperRank3))
                                result |= (0x1 << (i));
                            break;
                        case 4:
                            if (CheckActivenessInList(towerSpawner.TowerUsurperRank4))
                                result |= (0x1 << (i));
                            break;
                    }
                    if (result == 0x00000)
                        return false;
                }
                break;
        }
        resourceManager.ChangeMaterial(BonusForAllRanksByTypeChk);
        return true;
    }

    bool CheckActivenessInList(List<GameObject> towerList)
    {
        foreach (GameObject i in towerList)
        {
            if (i.activeSelf == false)
                continue;
            return true;
        }
        return false;
    }

    bool MonseterList_Rank(int rank)
    {
        switch (rank)
        {
            case 1:
                if (CheckActivenessInList(towerSpawner.TowerNightmareRank1) == false)
                    return false;
                if (CheckActivenessInList(towerSpawner.TowerSoulEaterRank1) == false)
                    return false;
                if (CheckActivenessInList(towerSpawner.TowerTerrorBringerRank1) == false)
                    return false;
                if (CheckActivenessInList(towerSpawner.TowerUsurperRank1) == false)
                    return false;
                break;
            case 2:
                if (CheckActivenessInList(towerSpawner.TowerNightmareRank2) == false)
                    return false;
                if (CheckActivenessInList(towerSpawner.TowerSoulEaterRank2) == false)
                    return false;
                if (CheckActivenessInList(towerSpawner.TowerTerrorBringerRank2) == false)
                    return false;
                if (CheckActivenessInList(towerSpawner.TowerUsurperRank2) == false)
                    return false;
                break;
            case 3:
                if (CheckActivenessInList(towerSpawner.TowerNightmareRank3) == false)
                    return false;
                if (CheckActivenessInList(towerSpawner.TowerSoulEaterRank3) == false)
                    return false;
                if (CheckActivenessInList(towerSpawner.TowerTerrorBringerRank3) == false)
                    return false;
                if (CheckActivenessInList(towerSpawner.TowerUsurperRank3) == false)
                    return false;
                break;
            case 4:
                if (CheckActivenessInList(towerSpawner.TowerNightmareRank4) == false)
                    return false;
                if (CheckActivenessInList(towerSpawner.TowerSoulEaterRank4) == false)
                    return false;
                if (CheckActivenessInList(towerSpawner.TowerTerrorBringerRank4) == false)
                    return false;
                if (CheckActivenessInList(towerSpawner.TowerUsurperRank4) == false)
                    return false;
                break;
        }

        resourceManager.ChangeMaterial(BonusForAllMonstersByRankChk[rank-1]);
        return true;
    }
}
