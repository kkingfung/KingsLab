using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using RandomTowerDefense.DOTS.Spawner;
using RandomTowerDefense.Info;

namespace RandomTowerDefense.Managers
{
    /// <summary>
    /// タワータイプ固有の操作を処理するユーティリティクラス
    /// タイプベースのロジックを一元化することでタワー管理のコード重複を排除
    /// </summary>
    public static class TowerTypeHandler
    {
        /// <summary>
        /// スポーナーから特定のタイプとランクのタワーリストを取得
        /// </summary>
        /// <param name="spawner">タワースポーナーの参照</param>
        /// <param name="type">取得するタワータイプ</param>
        /// <param name="rank">取得するタワーランク</param>
        /// <returns>タイプとランクが一致するタワーのリスト</returns>
        public static List<GameObject> GetTowerListByTypeAndRank(TowerSpawner spawner, TowerInfo.TowerInfoID type, int rank)
        {
            switch (type)
            {
                case TowerInfo.TowerInfoID.EnumTowerNightmare:
                    return GetNightmareListByRank(spawner, rank);
                case TowerInfo.TowerInfoID.EnumTowerSoulEater:
                    return GetSoulEaterListByRank(spawner, rank);
                case TowerInfo.TowerInfoID.EnumTowerTerrorBringer:
                    return GetTerrorBringerListByRank(spawner, rank);
                case TowerInfo.TowerInfoID.EnumTowerUsurper:
                    return GetUsurperListByRank(spawner, rank);
                default:
                    return new List<GameObject>();
            }
        }

        private static List<GameObject> GetNightmareListByRank(TowerSpawner spawner, int rank)
        {
            switch (rank)
            {
                case 1: return new List<GameObject>(spawner.TowerNightmareRank1);
                case 2: return new List<GameObject>(spawner.TowerNightmareRank2);
                case 3: return new List<GameObject>(spawner.TowerNightmareRank3);
                default: return new List<GameObject>();
            }
        }

        private static List<GameObject> GetSoulEaterListByRank(TowerSpawner spawner, int rank)
        {
            switch (rank)
            {
                case 1: return new List<GameObject>(spawner.TowerSoulEaterRank1);
                case 2: return new List<GameObject>(spawner.TowerSoulEaterRank2);
                case 3: return new List<GameObject>(spawner.TowerSoulEaterRank3);
                default: return new List<GameObject>();
            }
        }

        private static List<GameObject> GetTerrorBringerListByRank(TowerSpawner spawner, int rank)
        {
            switch (rank)
            {
                case 1: return new List<GameObject>(spawner.TowerTerrorBringerRank1);
                case 2: return new List<GameObject>(spawner.TowerTerrorBringerRank2);
                case 3: return new List<GameObject>(spawner.TowerTerrorBringerRank3);
                default: return new List<GameObject>();
            }
        }

        private static List<GameObject> GetUsurperListByRank(TowerSpawner spawner, int rank)
        {
            switch (rank)
            {
                case 1: return new List<GameObject>(spawner.TowerUsurperRank1);
                case 2: return new List<GameObject>(spawner.TowerUsurperRank2);
                case 3: return new List<GameObject>(spawner.TowerUsurperRank3);
                default: return new List<GameObject>();
            }
        }

        /// <summary>
        /// タワータイプのUIカラーを取得
        /// </summary>
        /// <param name="type">タワータイプ</param>
        /// <returns>UI表示用のカラー</returns>
        public static Color GetTowerColor(TowerInfo.TowerInfoID type)
        {
            switch (type)
            {
                case TowerInfo.TowerInfoID.EnumTowerNightmare:
                    return Color.yellow;
                case TowerInfo.TowerInfoID.EnumTowerSoulEater:
                    return Color.grey;
                case TowerInfo.TowerInfoID.EnumTowerTerrorBringer:
                    return Color.cyan;
                case TowerInfo.TowerInfoID.EnumTowerUsurper:
                    return Color.magenta;
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// タワータイプのビジュアルエフェクト用VFXカラーを取得
        /// </summary>
        /// <param name="type">タワータイプ</param>
        /// <returns>VFXシステム用のVector4カラー</returns>
        public static Vector4 GetTowerVFXColor(TowerInfo.TowerInfoID type)
        {
            switch (type)
            {
                case TowerInfo.TowerInfoID.EnumTowerNightmare:
                    return new Vector4(1, 1, 0, 1);
                case TowerInfo.TowerInfoID.EnumTowerSoulEater:
                    return new Vector4(0, 1, 0, 1);
                case TowerInfo.TowerInfoID.EnumTowerTerrorBringer:
                    return new Vector4(0, 0, 1, 1);
                case TowerInfo.TowerInfoID.EnumTowerUsurper:
                    return new Vector4(1, 0, 0, 1);
                default:
                    return new Vector4(1, 1, 1, 1);
            }
        }

        /// <summary>
        /// タワータイプに適切なオーラGameObjectを取得
        /// </summary>
        /// <param name="type">タワータイプ</param>
        /// <param name="nightmareAura">Nightmareタワーオーラプリファブ</param>
        /// <param name="soulEaterAura">Soul Eaterタワーオーラプリファブ</param>
        /// <param name="terrorBringerAura">Terror Bringerタワーオーラプリファブ</param>
        /// <param name="usurperAura">Usurperタワーオーラプリファブ</param>
        /// <returns>タワータイプに適切なオーラGameObject</returns>
        public static GameObject GetTowerAura(TowerInfo.TowerInfoID type, GameObject nightmareAura, GameObject soulEaterAura, GameObject terrorBringerAura, GameObject usurperAura)
        {
            switch (type)
            {
                case TowerInfo.TowerInfoID.EnumTowerNightmare:
                    return nightmareAura;
                case TowerInfo.TowerInfoID.EnumTowerSoulEater:
                    return soulEaterAura;
                case TowerInfo.TowerInfoID.EnumTowerTerrorBringer:
                    return terrorBringerAura;
                case TowerInfo.TowerInfoID.EnumTowerUsurper:
                    return usurperAura;
                default:
                    return null;
            }
        }

        /// <summary>
        /// タワータイプとランクのスポーナーインデックスを計算
        /// </summary>
        /// <param name="type">タワータイプ</param>
        /// <param name="rank">タワーランク</param>
        /// <param name="colorNumber">タイプごとのカラーバリエーション数</param>
        /// <returns>タワースポーナー用のスポーンインデックス</returns>
        public static int GetTowerSpawnIndex(TowerInfo.TowerInfoID type, int rank, int colorNumber)
        {
            return rank - 1 + colorNumber * (int)type;
        }
    }
}