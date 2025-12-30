using UnityEditor;
using UnityEngine;

namespace RandomTowerDefense.ProcedualAnimation
{
    /// <summary>
    /// 高速IK FABRIKアルゴリズム - Forward And Backward Reaching Inverse Kinematicsシステム
    ///
    /// 主な機能:
    /// - FABRIK（Forward And Backward Reaching IK）アルゴリズムによる高性能IK計算
    /// - 前進・後退反復処理による自然な関節配置システム
    /// - ルート空間座標系変換による高精度位置制御
    /// - ポールベクトル制約による関節角度制限機能
    /// - スナップバック機能による初期姿勢復帰制御
    /// - 到達不能領域での自動ストレッチ機能
    /// </summary>
    public class FastIKFabric : MonoBehaviour
    {
        #region Serialized Fields

        [Header("IK設定")]
        [SerializeField] [Range(1, 10)] [Tooltip("IKチェーンの長さ（関節数）")]
        public int ChainLength = 2;

        [SerializeField] [Tooltip("IKターゲット位置オブジェクト")]
        public Transform Target;

        [SerializeField] [Tooltip("ポールベクトル制約用オブジェクト")]
        public Transform Pole;

        [Header("計算パラメータ")]
        [SerializeField] [Range(1, 50)] [Tooltip("反復計算回数")]
        public int Iterations = 10;

        [SerializeField] [Range(0.0001f, 0.1f)] [Tooltip("収束判定闾値")]
        public float Delta = 0.001f;

        [SerializeField] [Range(0f, 1f)] [Tooltip("初期姿勢復帰強度")]
        public float SnapBackStrength = 1f;

        #endregion

        #region Protected Fields

        protected float[] _bonesLength; // ボーン長配列
        protected float _completeLength; // 総チェーン長
        protected Transform[] _bones; // ボーン配列
        protected Vector3[] _positions; // 位置配列
        protected Vector3[] _startDirectionSucc; // 初期方向配列
        protected Quaternion[] _startRotationBone; // 初期回転配列
        protected Quaternion _startRotationTarget; // ターゲット初期回転
        protected Transform _root; // ルートトランスフォーム


        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// 初期化処理 - システム初期化実行
        /// </summary>
        private void Awake()
        {
            InitializeIKSystem();
        }

        /// <summary>
        /// フレーム後処理 - IK計算実行
        /// </summary>
        private void LateUpdate()
        {
            ResolveIK();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// IKシステム初期化 - ボーンチェーン構築と初期状態記録
        /// </summary>
        private void InitializeIKSystem()
        {
            // 配列初期化
            _bones = new Transform[ChainLength + 1];
            _positions = new Vector3[ChainLength + 1];
            _bonesLength = new float[ChainLength];
            _startDirectionSucc = new Vector3[ChainLength + 1];
            _startRotationBone = new Quaternion[ChainLength + 1];

            // ルート検索
            _root = transform;
            for (var i = 0; i <= ChainLength; ++i)
            {
                if (_root == null)
                    throw new UnityException("The chain value is longer than the ancestor chain!");
                _root = _root.parent;
            }

            // ターゲット初期化
            if (Target == null)
            {
                Target = new GameObject(gameObject.name + " Target").transform;
                SetPositionRootSpace(Target, GetPositionRootSpace(transform));
            }
            _startRotationTarget = GetRotationRootSpace(Target);

            // ボーンデータ初期化
            var current = transform;
            _completeLength = 0;
            for (var i = _bones.Length - 1; i >= 0; i--)
            {
                _bones[i] = current;
                _startRotationBone[i] = GetRotationRootSpace(current);

                if (i == _bones.Length - 1)
                {
                    // 末端ボーン
                    _startDirectionSucc[i] = GetPositionRootSpace(Target) - GetPositionRootSpace(current);
                }
                else
                {
                    // 中間ボーン
                    _startDirectionSucc[i] = GetPositionRootSpace(_bones[i + 1]) - GetPositionRootSpace(current);
                    _bonesLength[i] = _startDirectionSucc[i].magnitude;
                    _completeLength += _bonesLength[i];
                }

                current = current.parent;
            }
        }

        /// <summary>
        /// IK解決処理 - FABRIKアルゴリズム実行
        /// </summary>
        private void ResolveIK()
        {
            if (Target == null)
                return;

            if (_bonesLength.Length != ChainLength)
                InitializeIKSystem();

            // 現在位置取得
            for (int i = 0; i < _bones.Length; ++i)
                _positions[i] = GetPositionRootSpace(_bones[i]);

            var targetPosition = GetPositionRootSpace(Target);
            var targetRotation = GetRotationRootSpace(Target);

            // 到達可能性判定
            if ((targetPosition - GetPositionRootSpace(_bones[0])).sqrMagnitude >= _completeLength * _completeLength)
            {
                // 到達不能領域での直線ストレッチ処理
                ExecuteStretchToTarget(targetPosition);
            }
            else
            {
                // FABRIK反復処理実行
                ExecuteFABRIKIteration(targetPosition);
            }

            // ポールベクトル制約適用
            ApplyPoleConstraint();

            // 最終位置・回転設定
            ApplyFinalTransforms(targetRotation);
        }

        /// <summary>
        /// ターゲットへの直線ストレッチ処理 - 到達不能領域での処理
        /// </summary>
        /// <param name="targetPosition">ターゲット位置</param>
        private void ExecuteStretchToTarget(Vector3 targetPosition)
        {
            var direction = (targetPosition - _positions[0]).normalized;
            for (int i = 1; i < _positions.Length; ++i)
                _positions[i] = _positions[i - 1] + direction * _bonesLength[i - 1];
        }

        /// <summary>
        /// FABRIK反復処理実行 - 前進・後退アルゴリズム
        /// </summary>
        /// <param name="targetPosition">ターゲット位置</param>
        private void ExecuteFABRIKIteration(Vector3 targetPosition)
        {
            // スナップバック適用
            for (int i = 0; i < _positions.Length - 1; ++i)
                _positions[i + 1] = Vector3.Lerp(_positions[i + 1], _positions[i] + _startDirectionSucc[i], SnapBackStrength);

            // FABRIK反復計算
            for (int iteration = 0; iteration < Iterations; iteration++)
            {
                // 後退フェーズ（ターゲットから開始）
                for (int i = _positions.Length - 1; i > 0; i--)
                {
                    if (i == _positions.Length - 1)
                        _positions[i] = targetPosition;
                    else
                        _positions[i] = _positions[i + 1] + (_positions[i] - _positions[i + 1]).normalized * _bonesLength[i];
                }

                // 前進フェーズ（ルートから開始）
                for (int i = 1; i < _positions.Length; ++i)
                    _positions[i] = _positions[i - 1] + (_positions[i] - _positions[i - 1]).normalized * _bonesLength[i - 1];

                // 収束判定
                if ((_positions[_positions.Length - 1] - targetPosition).sqrMagnitude < Delta * Delta)
                    break;
            }
        }

        /// <summary>
        /// ポールベクトル制約適用 - 自然な関節角度制限処理
        /// </summary>
        private void ApplyPoleConstraint()
        {
            if (Pole != null)
            {
                var polePosition = GetPositionRootSpace(Pole);
                for (int i = 1; i < _positions.Length - 1; ++i)
                {
                    var plane = new Plane(_positions[i + 1] - _positions[i - 1], _positions[i - 1]);
                    var projectedPole = plane.ClosestPointOnPlane(polePosition);
                    var projectedBone = plane.ClosestPointOnPlane(_positions[i]);
                    var angle = Vector3.SignedAngle(projectedBone - _positions[i - 1], projectedPole - _positions[i - 1], plane.normal);
                    _positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (_positions[i] - _positions[i - 1]) + _positions[i - 1];
                }
            }
        }

        /// <summary>
        /// 最終変換適用 - 位置と回転の設定
        /// </summary>
        /// <param name="targetRotation">ターゲット回転</param>
        private void ApplyFinalTransforms(Quaternion targetRotation)
        {
            for (int i = 0; i < _positions.Length; ++i)
            {
                if (i == _positions.Length - 1)
                    SetRotationRootSpace(_bones[i], Quaternion.Inverse(targetRotation) * _startRotationTarget * Quaternion.Inverse(_startRotationBone[i]));
                else
                    SetRotationRootSpace(_bones[i], Quaternion.FromToRotation(_startDirectionSucc[i], _positions[i + 1] - _positions[i]) * Quaternion.Inverse(_startRotationBone[i]));
                SetPositionRootSpace(_bones[i], _positions[i]);
            }
        }

        /// <summary>
        /// ルート空間位置取得 - ワールド座標からルート相対座標への変換
        /// </summary>
        /// <param name="current">対象トランスフォーム</param>
        /// <returns>ルート空間座標</returns>
        private Vector3 GetPositionRootSpace(Transform current)
        {
            if (_root == null)
                return current.position;
            else
                return Quaternion.Inverse(_root.rotation) * (current.position - _root.position);
        }

        /// <summary>
        /// ルート空間位置設定 - ルート相対座標からワールド座標への変換
        /// </summary>
        /// <param name="current">対象トランスフォーム</param>
        /// <param name="position">設定位置</param>
        private void SetPositionRootSpace(Transform current, Vector3 position)
        {
            if (_root == null)
                current.position = position;
            else
                current.position = _root.rotation * position + _root.position;
        }

        /// <summary>
        /// ルート空間回転取得 - ワールド回転からルート相対回転への変換
        /// </summary>
        /// <param name="current">対象トランスフォーム</param>
        /// <returns>ルート空間回転</returns>
        private Quaternion GetRotationRootSpace(Transform current)
        {
            if (_root == null)
                return current.rotation;
            else
                return Quaternion.Inverse(current.rotation) * _root.rotation;
        }

        /// <summary>
        /// ルート空間回転設定 - ルート相対回転からワールド回転への変換
        /// </summary>
        /// <param name="current">対象トランスフォーム</param>
        /// <param name="rotation">設定回転</param>
        private void SetRotationRootSpace(Transform current, Quaternion rotation)
        {
            if (_root == null)
                current.rotation = rotation;
            else
                current.rotation = _root.rotation * rotation;
        }

        #endregion
    }
}