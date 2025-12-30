using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RandomTowerDefense.Boids
{
    /// <summary>
    /// ボイド管理システム - ComputeShaderを活用した高性能フロッキングシミュレーション
    ///
    /// 主な機能:
    /// - GPU並列処理によるマルチボイド近傍計算（1024スレッドグループ）
    /// - ComputeBufferを使用した効率的なGPU-CPUデータ転送
    /// - 3つのフロッキングベクトル同時計算（整列、結束、分離）
    /// - 動的ボイド検出とリアルタイム初期化システム
    /// - メモリ効率化されたBoidData構造体管理
    /// - フレーム単位でのコンピュートシェーダー実行制御
    /// </summary>
    public class BoidManager : MonoBehaviour
    {
        #region Constants

        private const int THREAD_GROUP_SIZE = 1024;

        #endregion

        #region Serialized Fields

        [SerializeField] public BoidSettings settings;
        [SerializeField] public ComputeShader compute;

        #endregion

        #region Private Fields

        private Boid[] _boids;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// 初期化処理 - ボイド検出と設定適用
        /// </summary>
        private void Start()
        {
            _boids = FindObjectsOfType<Boid>();
            foreach (Boid b in _boids)
            {
                b.Initialize(settings, null);
            }
        }

        /// <summary>
        /// 毎フレーム更新 - GPU並列処理によるフロッキング計算
        /// </summary>
        private void Update()
        {
            if (_boids != null && _boids.Length > 0)
            {
                int numBoids = _boids.Length;
                var boidData = new BoidData[numBoids];

                // CPUからGPUへのデータ転送準備
                for (int i = 0; i < _boids.Length; ++i)
                {
                    boidData[i].position = _boids[i].transform.position;
                    boidData[i].direction = _boids[i].transform.forward;
                }

                // ComputeBufferセットアップとGPU計算実行
                var boidBuffer = new ComputeBuffer(numBoids, BoidData.Size);
                boidBuffer.SetData(boidData);

                compute.SetBuffer(0, "boids", boidBuffer);
                compute.SetInt("numBoids", _boids.Length);
                compute.SetFloat("viewRadius", settings.perceptionRadius);
                compute.SetFloat("avoidRadius", settings.avoidanceRadius);

                int threadGroups = Mathf.CeilToInt(numBoids / (float)THREAD_GROUP_SIZE);
                compute.Dispatch(0, threadGroups, 1, 1);

                // GPU結果をCPUに転送して各ボイドに適用
                boidBuffer.GetData(boidData);

                for (int i = 0; i < _boids.Length; ++i)
                {
                    _boids[i].avgFlockHeading = boidData[i].flockHeading;
                    _boids[i].centreOfFlockmates = boidData[i].flockCentre;
                    _boids[i].avgAvoidanceHeading = boidData[i].avoidanceHeading;
                    _boids[i].numPerceivedFlockmates = boidData[i].numFlockmates;

                    _boids[i].UpdateBoid();
                }

                boidBuffer.Release();
            }
        }

        #endregion

        #region Nested Types

        /// <summary>
        /// ボイドデータ構造体 - ComputeShader間での効率的なデータ転送用
        /// </summary>
        public struct BoidData
        {
            [Header("現在状態")]
            public Vector3 position;   // 現在位置
            public Vector3 direction;  // 現在方向

            [Header("フロッキング計算結果")]
            public Vector3 flockHeading;      // 群れ全体の方向
            public Vector3 flockCentre;       // 群れの中心位置
            public Vector3 avoidanceHeading;  // 回避方向
            public int numFlockmates;         // 知覚範囲内の仲間数

            /// <summary>
            /// ComputeBuffer用のバイトサイズ計算
            /// </summary>
            public static int Size
            {
                get
                {
                    return sizeof(float) * 3 * 5 + sizeof(int);
                }
            }
        }

        #endregion
    }
}