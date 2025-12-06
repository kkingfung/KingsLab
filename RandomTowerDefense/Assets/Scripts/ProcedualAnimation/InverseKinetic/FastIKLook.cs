using UnityEngine;

namespace RandomTowerDefense.ProcedualAnimation
{
    /// <summary>
    /// 高速IK Look-atシステム - ターゲット追跡アルゴリズム
    ///
    /// 主な機能:
    /// - ターゲットオブジェクトへの高速ルックアット回転機能
    /// - 初期方向ベクトルと回転状態の記録システム
    /// - Quaternion.FromToRotationによる最短回転パス計算
    /// - リアルタイムターゲット追跡とスムーズな向き変更
    /// - 簡素な設定での直観的なターゲット追跡動作
    /// - ヘッドボーンや眼球制御に適した軽量IKソリューション
    /// </summary>
    public class FastIKLook : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Look-at設定")]
        [SerializeField] [Tooltip("追跡対象ターゲット")]
        public Transform Target;

        #endregion

        #region Protected Fields

        protected Vector3 _startDirection; // 初期方向ベクトル
        protected Quaternion _startRotation; // 初期回転状態

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// 初期化処理 - 初期方向と回転状態記録
        /// </summary>
        private void Awake()
        {
            InitializeLookAtSystem();
        }

        /// <summary>
        /// 毎フレーム更新 - ターゲット追跡処理
        /// </summary>
        private void Update()
        {
            UpdateLookAtRotation();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Look-atシステム初期化 - 初期状態記録処理
        /// </summary>
        private void InitializeLookAtSystem()
        {
            if (Target == null)
                return;

            _startDirection = Target.position - transform.position;
            _startRotation = transform.rotation;
        }

        /// <summary>
        /// Look-at回転更新 - ターゲット方向への回転計算
        /// </summary>
        private void UpdateLookAtRotation()
        {
            if (Target == null)
                return;

            // 現在のターゲット方向計算
            Vector3 currentDirection = Target.position - transform.position;

            // 初期方向から現在方向への回転適用
            transform.rotation = Quaternion.FromToRotation(_startDirection, currentDirection) * _startRotation;
        }

        #endregion
    }
}
