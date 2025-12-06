using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RandomTowerDefense.ProcedualAnimation
{
    /// <summary>
    /// プロシージュラル脚配置システム - IKベース脚ステップアニメーションシステム
    ///
    /// 主な機能:
    /// - 地形アダプティブ脚位置自動調整システム
    /// - SphereCast探査によるリアルタイム地面検出
    /// - ランダム化要素付き自然なステップ位置計算
    /// - アニメーションカーブベーススムーズステップ高さ制御
    /// - クールダウンタイマー付き自動ステップ管理システム
    /// - 感情的デバッグビジュアライゼーションシステム
    /// </summary>
    public class ProceduralLegPlacement : MonoBehaviour
    {
        #region Serialized Fields

        [Header("脚状態情報")]
        [SerializeField] [Tooltip("脚が地面に接地しているかどうか")]
        public bool legGrounded = false;

        [SerializeField] [Tooltip("ステップ点のワールド座標")]
        public Vector3 stepPoint;

        [SerializeField] [Tooltip("ステップ点の法線ベクトル")]
        public Vector3 stepNormal;

        [Header("位置設定")]
        [SerializeField] [Tooltip("最適安息位置（ローカル座標）")]
        public Vector3 optimalRestingPosition = Vector3.forward;

        [SerializeField] [Tooltip("世界速度ベクトル")]
        public Vector3 worldVelocity = Vector3.right;

        [SerializeField] [Tooltip("世界ターゲット位置")]
        public Vector3 worldTarget = Vector3.zero;

        [Header("IKターゲット")]
        [SerializeField] [Tooltip("IKターゲットトランスフォーム")]
        public Transform ikTarget;

        [SerializeField] [Tooltip("IKポールターゲットトランスフォーム")]
        public Transform ikPoleTarget;

        [Header("ステップパラメータ")]
        [SerializeField] [Range(0f, 1f)] [Tooltip("配置ランダム化強度")]
        public float placementRandomization = 0;

        [SerializeField] [Tooltip("自動ステップ機能有効化")]
        public bool autoStep = true;

        [SerializeField] [Tooltip("地面探査レイヤーマスク")]
        public LayerMask solidLayer;

        [SerializeField] [Range(0.1f, 2f)] [Tooltip("ステップ探査半径")]
        public float stepRadius = 0.25f;

        [SerializeField] [Tooltip("ステップ高さアニメーションカーブ")]
        public AnimationCurve stepHeightCurve;

        [SerializeField] [Range(0f, 2f)] [Tooltip("ステップ高さ倍率")]
        public float stepHeightMultiplier = 0.25f;

        [SerializeField] [Range(0.1f, 5f)] [Tooltip("ステップ間クールダウン時間")]
        public float stepCooldown = 1f;

        [SerializeField] [Range(0.1f, 2f)] [Tooltip("ステップアニメーション継続時間")]
        public float stepDuration = 0.5f;

        [SerializeField] [Range(0f, 1f)] [Tooltip("ステップタイミングオフセット")]
        public float stepOffset;

        [SerializeField] [Tooltip("最後のステップ時刻")]
        public float lastStep = 0;

        #endregion

#region Public Properties

        /// <summary>
        /// 安息位置 - ワールド座標での安息位置
        /// </summary>
        public Vector3 restingPosition
        {
            get
            {
                return transform.TransformPoint(optimalRestingPosition);
            }
        }

        /// <summary>
        /// 希望位置 - ランダム化要素を含む希望ステップ位置
        /// </summary>
        public Vector3 desiredPosition
        {
            get
            {
                return restingPosition + worldVelocity + (Random.insideUnitSphere * placementRandomization);
            }
        }

        /// <summary>
        /// ステップ進行率 - 現在のステップアニメーション進行率
        /// </summary>
        public float percent
        {
            get
            {
                return Mathf.Clamp01((Time.time - lastStep) / stepDuration);
            }
        }

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// 初期化処理 - システム初期設定と初回ステップ
        /// </summary>
        private void Start()
        {
            InitializeLegPlacement();
        }

        /// <summary>
        /// 毎フレーム更新 - IKターゲット更新と自動ステップ判定
        /// </summary>
        private void Update()
        {
            UpdateIkTarget();

            if (Time.time > lastStep + stepCooldown && autoStep)
            {
                Step();
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// ステップ実行 - 新しいステップ位置への移動開始
        /// </summary>
        public void Step()
        {
            stepPoint = worldTarget = AdjustPosition(desiredPosition);
            lastStep = Time.time;
        }

        /// <summary>
        /// 速度更新 - 新しい世界速度ベクトルの適用
        /// </summary>
        /// <param name="newVelocity">新しい速度ベクトル</param>
        public void MoveVelocity(Vector3 newVelocity)
        {
            worldVelocity = Vector3.Lerp(worldVelocity, newVelocity, 1f - percent);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 脚配置システム初期化 - 初期状態設定と初回ステップ
        /// </summary>
        private void InitializeLegPlacement()
        {
            worldVelocity = Vector3.zero;
            lastStep = Time.time + stepCooldown * stepOffset;
            ikTarget.position = restingPosition;
            Step();
        }

        /// <summary>
        /// IKターゲット更新 - ステップアニメーションと高さ調整
        /// </summary>
        public void UpdateIkTarget()
        {
            stepPoint = AdjustPosition(worldTarget + worldVelocity);
            Vector3 heightOffset = stepNormal * stepHeightCurve.Evaluate(percent) * stepHeightMultiplier;
            ikTarget.position = Vector3.Lerp(ikTarget.position, stepPoint, percent) + heightOffset;
        }

        /// <summary>
        /// 位置調整 - 地形検出と最適位置計算
        /// </summary>
        /// <param name="position">目標位置</param>
        /// <returns>調整後の位置</returns>
        public Vector3 AdjustPosition(Vector3 position)
        {
            Vector3 direction = position - ikPoleTarget.position;
            RaycastHit hit;

            if (Physics.SphereCast(ikPoleTarget.position, stepRadius, direction, out hit, direction.magnitude * 2f, solidLayer))
            {
                // 地面検出成功
                Debug.DrawLine(ikPoleTarget.position, hit.point, Color.green, 0f);
                position = hit.point;
                stepNormal = hit.normal;
                legGrounded = true;
            }
            else
            {
                // 地面検出失敗 - 安息位置にフォールバック
                Debug.DrawLine(ikPoleTarget.position, restingPosition, Color.red, 0f);
                position = restingPosition;
                stepNormal = Vector3.zero;
                legGrounded = false;
            }

            return position;
        }

        #endregion

        #region Debug and Utilities

        /// <summary>
        /// Gizmo描画 - 脚の位置と探査範囲のビジュアライゼーション
        /// </summary>
        public void OnDrawGizmos()
        {
            // 安息位置からターゲットへのライン（青）
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(restingPosition, worldTarget);

            // ターゲットからステップ点へのライン（緑）
            Gizmos.color = Color.green;
            Gizmos.DrawLine(worldTarget, stepPoint);

            // 安息位置マーカー（青）
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(restingPosition, 0.1f);

            // ターゲット位置マーカー（緑）
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(worldTarget, 0.1f);

            // ステップ点マーカー（緑）
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(stepPoint, 0.1f);

            // 探査範囲（黒）
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(ikPoleTarget.position, stepRadius);
        }

        #endregion
    }
}
