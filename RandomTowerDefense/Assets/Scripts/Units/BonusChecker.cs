using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RandomTowerDefense.DOTS.Spawner;
using RandomTowerDefense.Info;
using RandomTowerDefense.Managers.Macro;

namespace RandomTowerDefense.Units
{
    /// <summary>
    /// ボーナスチェッカーシステム - タワー組み合わせボーナスの検出と支給
    ///
    /// 主な機能:
    /// - 同ランク全タワータイプボーナス検出
    /// - 同タイプ全ランクタワーボーナス検出
    /// - リソース報酬自動支給システム
    /// - リアルタイムボーナス状態監視
    /// - パフォーマンスフレンドリーボーナス計算
    /// </summary>
    public class BonusChecker : MonoBehaviour
    {
        #region Constants
        private readonly int[] BonusForAllMonstersByRankChk = { 100, 250, 500, 800 };
        private readonly int BonusForAllRanksByTypeChk = 2000;
        #endregion

        #region Serialized Fields
        [Header("🎮 Manager References")]
        public ResourceManager resourceManager;
        public TowerSpawner towerSpawner;
        #endregion

        #region Public Properties
        public int TowerNewlyBuilt;
        public bool TowerLevelChg;
        #endregion

        #region Private Fields
        private readonly bool[] _allMonstersByRankBonus = { false, false, false, false };
        private readonly bool[] _allRanksByMonsterBonus = { false, false, false, false };
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            //towerSpawner = FindObjectOfType<TowerSpawner>();
            //resourceManager = FindObjectOfType<ResourceManager>();

            TowerNewlyBuilt = -1;
            TowerLevelChg = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (TowerNewlyBuilt >= 0)
            {
                if (TowerNewlyBuilt == 0 && !_allRanksByMonsterBonus[(int)TowerInfo.TowerInfoID.Enum_TowerNightmare])
                    _allRanksByMonsterBonus[(int)TowerInfo.TowerInfoID.Enum_TowerNightmare]
                        = MonsterList_Type(TowerInfo.TowerInfoID.Enum_TowerNightmare);

                else if (TowerNewlyBuilt == 1 && !_allRanksByMonsterBonus[(int)TowerInfo.TowerInfoID.Enum_TowerSoulEater])
                    _allRanksByMonsterBonus[(int)TowerInfo.TowerInfoID.Enum_TowerSoulEater]
                        = MonsterList_Type(TowerInfo.TowerInfoID.Enum_TowerSoulEater);

                else if (TowerNewlyBuilt == 2 && !_allRanksByMonsterBonus[(int)TowerInfo.TowerInfoID.Enum_TowerTerrorBringer])
                    _allRanksByMonsterBonus[(int)TowerInfo.TowerInfoID.Enum_TowerTerrorBringer]
                        = MonsterList_Type(TowerInfo.TowerInfoID.Enum_TowerTerrorBringer);

                else if (TowerNewlyBuilt == 3 && !_allRanksByMonsterBonus[(int)TowerInfo.TowerInfoID.Enum_TowerUsurper])
                    _allRanksByMonsterBonus[(int)TowerInfo.TowerInfoID.Enum_TowerUsurper]
                       = MonsterList_Type(TowerInfo.TowerInfoID.Enum_TowerUsurper);

                TowerNewlyBuilt = -1;
            }
            if (TowerLevelChg)
            {
                for (int i = 0; i < _allMonstersByRankBonus.Length; ++i)
                {
                    if (!_allMonstersByRankBonus[i])
                        _allMonstersByRankBonus[i] = MonseterList_Rank(i + 1);
                }
                TowerLevelChg = false;
            }
        }

        bool MonsterList_Type(TowerInfo.TowerInfoID towerID)
        {
            int result = 0x00000;

            switch (towerID)
            {
                case TowerInfo.TowerInfoID.Enum_TowerNightmare:
                    for (int i = TowerSpawner.MonsterMaxRank; i > 0; --i)
                    {
                        switch (i)
                        {
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

            resourceManager.ChangeMaterial(BonusForAllMonstersByRankChk[rank - 1]);
            return true;
        }
    }
}