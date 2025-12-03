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

        /// <summary>
        /// 初期化処理 - ボーナスチェッカーの初期状態設定
        /// </summary>
        void Start()
        {
            //towerSpawner = FindObjectOfType<TowerSpawner>();
            //resourceManager = FindObjectOfType<ResourceManager>();

            TowerNewlyBuilt = -1;
            TowerLevelChg = false;
        }

        /// <summary>
        /// 毎フレーム更新 - タワー状態変化の検知とボーナス判定
        /// </summary>
        void Update()
        {
            if (TowerNewlyBuilt >= 0)
            {
                if (TowerNewlyBuilt == 0 && !_allRanksByMonsterBonus[(int)TowerInfo.TowerInfoID.EnumTowerNightmare])
                    _allRanksByMonsterBonus[(int)TowerInfo.TowerInfoID.EnumTowerNightmare]
                        = MonsterList_Type(TowerInfo.TowerInfoID.EnumTowerNightmare);

                else if (TowerNewlyBuilt == 1 && !_allRanksByMonsterBonus[(int)TowerInfo.TowerInfoID.EnumTowerSoulEater])
                    _allRanksByMonsterBonus[(int)TowerInfo.TowerInfoID.EnumTowerSoulEater]
                        = MonsterList_Type(TowerInfo.TowerInfoID.EnumTowerSoulEater);

                else if (TowerNewlyBuilt == 2 && !_allRanksByMonsterBonus[(int)TowerInfo.TowerInfoID.EnumTowerTerrorBringer])
                    _allRanksByMonsterBonus[(int)TowerInfo.TowerInfoID.EnumTowerTerrorBringer]
                        = MonsterList_Type(TowerInfo.TowerInfoID.EnumTowerTerrorBringer);

                else if (TowerNewlyBuilt == 3 && !_allRanksByMonsterBonus[(int)TowerInfo.TowerInfoID.EnumTowerUsurper])
                    _allRanksByMonsterBonus[(int)TowerInfo.TowerInfoID.EnumTowerUsurper]
                       = MonsterList_Type(TowerInfo.TowerInfoID.EnumTowerUsurper);

                TowerNewlyBuilt = -1;
            }
            if (TowerLevelChg)
            {
                for (int i = 0; i < _allMonstersByRankBonus.Length; ++i)
                {
                    if (!_allMonstersByRankBonus[i])
                        _allMonstersByRankBonus[i] = CheckAllMonstersByRank(i + 1);
                }
                TowerLevelChg = false;
            }
        }

        bool MonsterList_Type(TowerInfo.TowerInfoID towerID)
        {
            int result = 0x00000;

            switch (towerID)
            {
                case TowerInfo.TowerInfoID.EnumTowerNightmare:
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
                case TowerInfo.TowerInfoID.EnumTowerSoulEater:
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
                case TowerInfo.TowerInfoID.EnumTowerTerrorBringer:
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
                case TowerInfo.TowerInfoID.EnumTowerUsurper:
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

        /// <summary>
        /// 指定ランクで全タワータイプが揃っているか確認しボーナス支給
        /// </summary>
        /// <param name="rank">確認するランク（1-4）</param>
        /// <returns>全タワータイプが揃っている場合true</returns>
        private bool CheckAllMonstersByRank(int rank)
        {
            if (rank < 1 || rank > 4) return false;

            bool hasAllTypes = CheckAllTowerTypesForRank(rank);

            if (hasAllTypes)
            {
                resourceManager.ChangeMaterial(BonusForAllMonstersByRankChk[rank - 1]);
            }

            return hasAllTypes;
        }

        /// <summary>
        /// 指定ランクで全てのタワータイプが存在するかチェック
        /// </summary>
        /// <param name="rank">確認するランク</param>
        /// <returns>全タワータイプが存在する場合true</returns>
        private bool CheckAllTowerTypesForRank(int rank)
        {
            var towerTypes = new[]
            {
                TowerInfo.TowerInfoID.EnumTowerNightmare,
                TowerInfo.TowerInfoID.EnumTowerSoulEater,
                TowerInfo.TowerInfoID.EnumTowerTerrorBringer,
                TowerInfo.TowerInfoID.EnumTowerUsurper
            };

            foreach (var towerType in towerTypes)
            {
                if (!HasActiveTowerOfTypeAndRank(towerType, rank))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 指定タイプとランクのタワーがアクティブに存在するかチェック
        /// </summary>
        /// <param name="towerType">タワータイプ</param>
        /// <param name="rank">タワーランク（1-4）</param>
        /// <returns>該当タワーが存在する場合true</returns>
        private bool HasActiveTowerOfTypeAndRank(TowerInfo.TowerInfoID towerType, int rank)
        {
            List<GameObject> towerList = null;

            switch (towerType)
            {
                case TowerInfo.TowerInfoID.EnumTowerNightmare:
                    towerList = rank switch
                    {
                        1 => towerSpawner.TowerNightmareRank1,
                        2 => towerSpawner.TowerNightmareRank2,
                        3 => towerSpawner.TowerNightmareRank3,
                        4 => towerSpawner.TowerNightmareRank4,
                        _ => null
                    };
                    break;
                case TowerInfo.TowerInfoID.EnumTowerSoulEater:
                    towerList = rank switch
                    {
                        1 => towerSpawner.TowerSoulEaterRank1,
                        2 => towerSpawner.TowerSoulEaterRank2,
                        3 => towerSpawner.TowerSoulEaterRank3,
                        4 => towerSpawner.TowerSoulEaterRank4,
                        _ => null
                    };
                    break;
                case TowerInfo.TowerInfoID.EnumTowerTerrorBringer:
                    towerList = rank switch
                    {
                        1 => towerSpawner.TowerTerrorBringerRank1,
                        2 => towerSpawner.TowerTerrorBringerRank2,
                        3 => towerSpawner.TowerTerrorBringerRank3,
                        4 => towerSpawner.TowerTerrorBringerRank4,
                        _ => null
                    };
                    break;
                case TowerInfo.TowerInfoID.EnumTowerUsurper:
                    towerList = rank switch
                    {
                        1 => towerSpawner.TowerUsurperRank1,
                        2 => towerSpawner.TowerUsurperRank2,
                        3 => towerSpawner.TowerUsurperRank3,
                        4 => towerSpawner.TowerUsurperRank4,
                        _ => null
                    };
                    break;
            }

            return towerList != null && CheckActivenessInList(towerList);
        }
    }
}