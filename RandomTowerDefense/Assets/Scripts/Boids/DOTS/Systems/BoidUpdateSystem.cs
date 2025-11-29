using Unity;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using RandomTowerDefense.Boids.DOTS;
using RandomTowerDefense.DOTS.Components;

namespace RandomTowerDefense.Boids.DOTS.Systems
{
    /// <summary>
    /// ボイド更新システム - DOTSアーキテクチャ対応フロッキング動作と衡突回避システム
    ///
    /// 主な機能:
    /// - JobComponentSystemベースの高性能エンティティ更新システム
    /// - 群れモデル（Boids）アルゴリズムによる自然な動作実現
    /// - 球面境界との衝突検知と回避処理システム
    /// - BoidHelper統合による最適化された方向検索
    /// - リアルタイムデバッグ描画とビジュアルフィードバック
    /// - カスタム四元数計算による高速回転処理
    /// </summary>
    public class BoidUpdateSystem : JobComponentSystem
    {
        #region System Lifecycle

        /// <summary>
        /// システム更新処理 - ボイドエンティティの行動と移動統合更新
        /// </summary>
        /// <param name="inputDeps">入力依存関係</param>
        /// <returns>ジョブハンドル</returns>
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float deltaTime = Time.DeltaTime;

            return Entities.WithAll<BoidTag>().ForEach((Entity entity, ref OriPos oripos,
                ref BoidData dataType, ref BoidDataAvg dataAvgType, ref Velocity vecType, ref BoidRotation rotType,
                ref BoidSettingDots settingType) =>
            {
                // フロッキングデータ同期
                dataAvgType.avgFlockHeading = dataType.flockHeading;
                dataAvgType.avgAvoidanceHeading = dataType.flockCentre;
                dataAvgType.centreOfFlockmates = dataType.avoidanceHeading;
                dataAvgType.numPerceivedFlockmates = dataType.numFlockmates;

                Vector3 acceleration = Vector3.zero;

                // 衡突回避処理
                if (IsHeadingForCollision(dataType.position, settingType, oripos.Value, oripos.BoundingRadius))
                {
                    Vector3 collisionAvoidDir = ObstacleRays(dataType.position, dataType.direction,
                        settingType, rotType.rotation, oripos.Value, oripos.BoundingRadius);
                    Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir,
                        new Vector3(vecType.Value.x, vecType.Value.y, vecType.Value.z),
                        settingType) * settingType.avoidCollisionWeight;
                    acceleration += collisionAvoidForce;
                }

                // 速度と位置更新
                vecType.Value += new float3(acceleration.x, acceleration.y, acceleration.z) * deltaTime;
                float speed = new Vector3(vecType.Value.x, vecType.Value.y, vecType.Value.z).magnitude;
                Vector3 dir = new Vector3(vecType.Value.x, vecType.Value.y, vecType.Value.z).normalized;
                speed = Mathf.Clamp(speed, settingType.minSpeed, settingType.maxSpeed);
                vecType.Value = dir * speed;

                dataType.position += vecType.Value * deltaTime;
                dataType.direction = dir;

                // デバッグ描画
                Debug.DrawLine(dataType.position, dataType.position + dataType.direction, Color.blue);
                Debug.DrawLine(dataType.position, dataType.position + vecType.Value, Color.red);
            }).WithoutBurst().Schedule(inputDeps);
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// ステアリング力計算 - 目標ベクトルに向かう操舵力を計算
        /// </summary>
        /// <param name="vector">目標ベクトル</param>
        /// <param name="velocity">現在の速度</param>
        /// <param name="settings">ボイド設定</param>
        /// <returns>クランプされたステアリング力</returns>
        private static Vector3 SteerTowards(Vector3 vector, Vector3 velocity, BoidSettingDots settings)
        {
            Vector3 v = vector.normalized * settings.maxSpeed - velocity;
            return Vector3.ClampMagnitude(v, settings.maxSteerForce);
        }

        /// <summary>
        /// 境界衝突予測判定 - エンティティが球面境界と衝突するかチェック
        /// </summary>
        /// <param name="currPos">現在位置</param>
        /// <param name="setting">ボイド設定</param>
        /// <param name="oriPos">原点位置</param>
        /// <param name="boundRadius">境界半径</param>
        /// <returns>衝突予測がある場合はtrue</returns>
        private static bool IsHeadingForCollision(Vector3 currPos, BoidSettingDots setting,
            Vector3 oriPos, float boundRadius)
        {
            float dist = (currPos - oriPos).magnitude;
            return (dist + setting.boundsRadius + setting.collisionAvoidDst > boundRadius);
        }

        /// <summary>
        /// カスタム四元数ベクトル乗算 - LinearAlgebra統合による最適化
        /// </summary>
        /// <param name="rotation">回転四元数</param>
        /// <param name="vec">変換するベクトル</param>
        /// <returns>回転適用後のベクトル</returns>
        private static Vector3 QuaternionMultiplyVector(Quaternion rotation, Vector3 vec)
        {
            LinearAlgebra.Quaternion3d quaternion = new LinearAlgebra.Quaternion3d(
                rotation.x, rotation.y, rotation.z, rotation.w);

            return quaternion * vec;
        }

        /// <summary>
        /// 障害物回避レイキャスティング - BoidHelper方向配列を使用した最適方向検索
        /// </summary>
        /// <param name="currPos">現在位置</param>
        /// <param name="forward">前方向</param>
        /// <param name="setting">ボイド設定</param>
        /// <param name="rotation">回転四元数</param>
        /// <param name="oriPos">原点位置</param>
        /// <param name="boundRadius">境界半径</param>
        /// <returns>最適な回避方向ベクトル</returns>
        private static Vector3 ObstacleRays(Vector3 currPos, Vector3 forward, BoidSettingDots setting,
            Quaternion rotation, Vector3 oriPos, float boundRadius)
        {
            Vector3[] rayDirections = BoidHelper.directions;
            float finalDist = (currPos + forward - oriPos).sqrMagnitude;

            for (int i = 0; i < rayDirections.Length; ++i)
            {
                Vector3 dir = QuaternionMultiplyVector(rotation, rayDirections[i]);
                float dist = (currPos + dir - oriPos).sqrMagnitude;
                if (dist < finalDist)
                {
                    return dir;
                }
            }

            return forward;
        }

        #endregion
    }
}
