using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RandomTowerDefense.Boids
{
    /// <summary>
    /// 従来型ボイドスポナー - MonoBehaviourベースのボイド生成システム
    ///
    /// 主な機能:
    /// - 球状範囲内でのランダムボイドプレファブ生成
    /// - 複数プレファブタイプからのランダム選択機能
    /// - ヒエラルキー整理のための親オブジェクト設定
    /// - Awake時の一括生成による初期化時間最適化
    /// - カスタマイズ可能な生成範囲と数量設定
    /// - ランダム方向初期化による自然な群れ形成
    /// </summary>
    public class BoidSpawner : MonoBehaviour
    {
        #region Serialized Fields

        [Header("スポーン設定")]
        [SerializeField] [Tooltip("生成するボイドのプレファブリスト")]
        public List<Boid> prefab;

        [SerializeField] [Tooltip("生成範囲の半径")]
        public float spawnRadius = 10f;

        [SerializeField] [Tooltip("生成するボイドの数")]
        public int spawnCount = 10;

        #endregion

        #region Private Fields

        private BoidSettings _settings;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// 起動時初期化 - ボイドの一括生成処理
        /// </summary>
        private void Awake()
        {
            SpawnBoids();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// ボイド生成処理 - 指定範囲内にランダムでボイドを配置
        /// </summary>
        private void SpawnBoids()
        {
            for (int i = 0; i < spawnCount; ++i)
            {
                // ランダム位置とプレファブ選択
                Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
                int rand = Random.Range(0, prefab.Count);

                // ボイド生成と初期設定
                Boid boid = Instantiate(prefab[rand]);
                boid.transform.position = pos;
                boid.transform.forward = Random.insideUnitSphere.normalized;
                boid.transform.parent = this.transform;
            }
        }

        #endregion
    }
}