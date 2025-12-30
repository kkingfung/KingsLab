using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RandomTowerDefense.Boids
{
    /// <summary>
    /// 個別ボイドエンティティ - フロッキング動作を行う個体オブジェクト
    ///
    /// 主な機能:
    /// - 3つの基本フロッキング動作実装（整列、結束、分離）
    /// - 障害物回避のためのレイキャスティングシステム
    /// - ターゲット追従とマルチレイ衝突検知
    /// - カスタム四元数計算による回転処理最適化
    /// - 速度と操舵力のクランプによる自然な動作制御
    /// - 動的マテリアルカラー変更システム
    /// </summary>
    public class Boid : MonoBehaviour
    {
        #region Serialized Fields

        [HideInInspector] public Vector3 avgFlockHeading;
        [HideInInspector] public Vector3 avgAvoidanceHeading;
        [HideInInspector] public Vector3 centreOfFlockmates;
        [HideInInspector] public int numPerceivedFlockmates;

        #endregion

        #region Private Fields

        private BoidSettings _settings;
        private Vector3 _velocity;
        private Material _material;
        private Transform _cachedTransform;
        private Transform _target;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// 初期化処理 - コンポーネント参照取得
        /// </summary>
        private void Awake()
        {
            _material = transform.GetComponentInChildren<SkinnedMeshRenderer>().material;
            _cachedTransform = transform;
        }

        #endregion

        #region Public API

        /// <summary>
        /// ボイド初期化 - 設定とターゲット設定
        /// </summary>
        /// <param name="settings">ボイド動作設定</param>
        /// <param name="target">追従ターゲット</param>
        public void Initialize(BoidSettings settings, Transform target)
        {
            _target = target;
            _settings = settings;

            float startSpeed = (_settings.minSpeed + _settings.maxSpeed) / 2f;
            _velocity = transform.forward * startSpeed;
        }

        /// <summary>
        /// ボイドの色設定
        /// </summary>
        /// <param name="col">設定する色</param>
        public void SetColour(Color col)
        {
            if (_material != null)
            {
                _material.color = col;
            }
        }

        /// <summary>
        /// ボイド動作更新 - フロッキング動作と衝突回避の統合処理
        /// </summary>
        public void UpdateBoid()
        {
            Vector3 acceleration = Vector3.zero;

            // ターゲット追従力計算
            if (_target != null)
            {
                Vector3 offsetToTarget = (_target.position - this.transform.position);
                acceleration = SteerTowards(offsetToTarget) * _settings.targetWeight;
            }

            // フロッキング動作計算
            if (numPerceivedFlockmates != 0)
            {
                centreOfFlockmates /= numPerceivedFlockmates;
                Vector3 offsetToFlockmatesCentre = (centreOfFlockmates - this.transform.position);

                var alignmentForce = SteerTowards(avgFlockHeading) * _settings.alignWeight;
                var cohesionForce = SteerTowards(offsetToFlockmatesCentre) * _settings.cohesionWeight;
                var seperationForce = SteerTowards(avgAvoidanceHeading) * _settings.seperateWeight;

                acceleration += alignmentForce;
                acceleration += cohesionForce;
                acceleration += seperationForce;
            }

            // 衝突回避処理
            if (IsHeadingForCollision())
            {
                Vector3 collisionAvoidDir = ObstacleRays();
                Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * _settings.avoidCollisionWeight;
                acceleration += collisionAvoidForce;
            }

            // 速度と位置更新
            _velocity += acceleration * Time.deltaTime;
            float speed = _velocity.magnitude;
            Vector3 dir = _velocity / speed;
            speed = Mathf.Clamp(speed, _settings.minSpeed, _settings.maxSpeed);
            _velocity = dir * speed;

            this.transform.position += _velocity * Time.deltaTime;
            this.transform.forward = dir;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 衝突予測判定 - 前方に障害物があるかチェック
        /// </summary>
        /// <returns>衝突予測がある場合はtrue</returns>
        private bool IsHeadingForCollision()
        {
            RaycastHit hit;
            if (Physics.SphereCast(this.transform.position, _settings.boundsRadius,
                this.transform.forward, out hit, _settings.collisionAvoidDst, _settings.obstacleMask))
            {
                return true;
            }
            return false;
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
        /// 障害物回避レイキャスティング - 複数方向をテストして安全な方向を検出
        /// </summary>
        /// <returns>安全な移動方向ベクトル</returns>
        private Vector3 ObstacleRays()
        {
            Vector3[] rayDirections = BoidHelper.directions;

            for (int i = 0; i < rayDirections.Length; ++i)
            {
                Vector3 dir = QuaternionMultiplyVector(this.transform.rotation, rayDirections[i]);
                Ray ray = new Ray(this.transform.position, dir);
                if (!Physics.SphereCast(ray, _settings.boundsRadius, _settings.collisionAvoidDst, _settings.obstacleMask))
                {
                    return dir;
                }
            }

            return this.transform.forward;
        }

        /// <summary>
        /// ステアリング力計算 - 目標方向への操舵力を計算
        /// </summary>
        /// <param name="vector">目標方向ベクトル</param>
        /// <returns>クランプされたステアリング力</returns>
        private Vector3 SteerTowards(Vector3 vector)
        {
            Vector3 v = vector.normalized * _settings.maxSpeed - _velocity;
            return Vector3.ClampMagnitude(v, _settings.maxSteerForce);
        }

        #endregion
    }
}