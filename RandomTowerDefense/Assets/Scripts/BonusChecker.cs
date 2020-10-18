using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusChecker : MonoBehaviour
{
    private readonly int[] BonusForAllMonstersByRankChk = { 100, 250, 500, 800, 1200 };
    private readonly int BonusForAllRanksByTypeChk = 2000;

    PlayerManager playerManager;
    ResourceManager resourceManager;

    bool[] AllMonstersByRankBonus= { false,false,false,false,false};
    bool[] AllRanksByMonsterBonus= { false,false,false,false};

    // Start is called before the first frame update
    void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        resourceManager = FindObjectOfType<ResourceManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!AllMonstersByRankBonus[0]) AllMonstersByRankBonus[0] = MonseterList_Type(playerManager.TowerNightmareList);
        if (!AllMonstersByRankBonus[1]) AllMonstersByRankBonus[1] = MonseterList_Type(playerManager.TowerSoulEaterList);
        if (!AllMonstersByRankBonus[2]) AllMonstersByRankBonus[2] = MonseterList_Type(playerManager.TowerTerrorBringerList);
        if (!AllMonstersByRankBonus[3]) AllMonstersByRankBonus[3] = MonseterList_Type(playerManager.TowerUsurperList);

        for (int i = 0; i < AllMonstersByRankBonus.Length; ++i)
            if (!AllRanksByMonsterBonus[i]) AllMonstersByRankBonus[i] = MonseterList_Rank(i+1);
    }

    bool MonseterList_Type(List<GameObject> targetList) {
        int result = 0x00000;
        foreach (GameObject i in targetList)
        {
            if ((result & (0x00001 << (i.GetComponent<Tower>().rank - 1))) == 0x00000)
            {
                result &= (0x00001 << (i.GetComponent<Tower>().rank - 1));
            }
            if (result == 0x11111) {
                resourceManager.ChangeMaterial(BonusForAllRanksByTypeChk);
                return true;
            }
        }

        return false;
    }

    bool MonseterList_Rank(int rank)
    {
        bool result = false;
        foreach (GameObject i in playerManager.TowerNightmareList) {
            if (i.GetComponent<Tower>().rank == rank) {
                result = true;break;
            }
        }
        if (result == false) return result;
        result = false;
        foreach (GameObject i in playerManager.TowerSoulEaterList)
        {
            if (i.GetComponent<Tower>().rank == rank)
            {
                result = true; break;
            }
        }
        if (result == false) return result;
        foreach (GameObject i in playerManager.TowerTerrorBringerList)
        {
            if (i.GetComponent<Tower>().rank == rank)
            {
                result = true; break;
            }
        }
        if (result == false) return result;
        foreach (GameObject i in playerManager.TowerUsurperList)
        {
            if (i.GetComponent<Tower>().rank == rank)
            {
                result = true; break;
            }
        }
        if (result == false) return result;

        resourceManager.ChangeMaterial(BonusForAllMonstersByRankChk[rank-1]);
        return true;
    }
}
