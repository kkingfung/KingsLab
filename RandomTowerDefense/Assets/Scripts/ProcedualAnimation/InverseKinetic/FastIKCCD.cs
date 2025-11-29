using UnityEngine;

namespace RandomTowerDefense.ProcedualAnimation
{
    /// <summary>
    /// 高速IK CCDアルゴリズム - サイクリック座標降下法による逆運動学システム
    ///
    /// 主な機能:
    /// - CCD（Cyclic Coordinate Descent）アルゴリズムによる高速IK計算
    /// - ボーンチェーン階層構造の自動検出と初期化システム
    /// - ポールベクトル制約による自然な関節角度制限
    /// - ターゲット位置への効率的収束計算とジッター回避
    /// - リアルタイム反復処理による滑らかなアニメーション
    /// - 直線姿勢問題の自動解決と安定性保証システム
    /// </summary>
    public class IKCCD : MonoBehaviour
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

        [SerializeField] [Range(0.0001f, 0.1f)] [Tooltip("収束判定閾値")]
        public float Delta = 0.001f;

        #endregion

        #region Protected Fields

        protected Quaternion _targetInitialRotation;
        protected Quaternion _endInitialRotation;
        protected float _completeLength;
        protected Transform[] _bones;
        protected Quaternion[] _initialRotation;


        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// 初期化処理 - ボーンチェーン構築と初期状態記録
        /// </summary>
        private void Awake()
        {
            // ボーンチェーン配列初期化
            _bones = new Transform[ChainLength + 1];
            _initialRotation = new Quaternion[ChainLength + 1];
            _targetInitialRotation = Target.rotation;
            _endInitialRotation = transform.rotation;

            // ボーンチェーン構築と長さ計算
            var current = transform;
            _completeLength = 0;
            for (int i = ChainLength - 1; i >= 0; i--)
            {
                _completeLength += (current.position - current.parent.position).magnitude;
                _bones[i + 1] = current;
                _bones[i] = current.parent;
                _initialRotation[i + 1] = current.rotation;
                _initialRotation[i] = current.parent.rotation;
                current = current.parent;
            }

            // チェーン有効性検証
            if (_bones[0] == null)
                throw new UnityException("The chain value is longer than the ancestor chain!");
        }

        /// <summary>
        /// フレーム後処理 - CCD反復アルゴリズム実行
        /// </summary>
        private void LateUpdate()
        {
            ExecuteCCDAlgorithm();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// CCD反復アルゴリズム実行 - サイクリック座標降下法によるIK計算
        /// </summary>
        private void ExecuteCCDAlgorithm()
        {
            var lastBone = _bones[_bones.Length - 1];

            // 初期回転状態にリセット
            for (var i = 0; i < _bones.Length; ++i)
                _bones[i].rotation = _initialRotation[i];

            // CCD反復処理
            for (int iteration = 0; iteration < Iterations; iteration++)
            {
                for (var i = _bones.Length - 1; i >= 0; i--)
                {
                    if (i == _bones.Length - 1)
                    {
                        // エンドエフェクターの回転調整
                        _bones[i].rotation = Target.rotation * Quaternion.Inverse(_targetInitialRotation) * _endInitialRotation;
                    }
                    else
                    {
                        // 各関節の回転計算
                        _bones[i].rotation = Quaternion.FromToRotation(lastBone.position - _bones[i].position, Target.position - _bones[i].position) * _bones[i].rotation;

                        // 直線姿勢ジッター回避処理
                        ApplyAntiStraightLineJitter(iteration, i, lastBone.position);

                        // ポールベクトル制約適用
                        ApplyPoleConstraint(i);
                    }

                    // 収束判定
                    if ((lastBone.position - Target.position).sqrMagnitude < Delta * Delta)
                        break;
                }
            }
        }

        /// <summary>
        /// 直線姿勢ジッター回避処理 - 特定条件下でのジッター適用
        /// </summary>
        /// <param name="iteration">現在の反復回数</param>
        /// <param name="boneIndex">ボーンインデックス</param>
        /// <param name="lastBonePosition">最終ボーンの位置</param>
        private void ApplyAntiStraightLineJitter(int iteration, int boneIndex, Vector3 lastBonePosition)
        {
            if (iteration == 5 && boneIndex == 0 &&
                (Target.position - lastBonePosition).sqrMagnitude > 0.01f &&
                (Target.position - _bones[boneIndex].position).sqrMagnitude < _completeLength * _completeLength)
            {
                _bones[boneIndex].rotation = Quaternion.AngleAxis(10, Vector3.up) * _bones[boneIndex].rotation;
            }
        }

        /// <summary>
        /// ポールベクトル制約適用 - 自然な関節角度制限処理
        /// </summary>
        /// <param name="boneIndex">ボーンインデックス</param>
        private void ApplyPoleConstraint(int boneIndex)
        {
            if (Pole != null && boneIndex + 2 <= _bones.Length - 1)
            {
                var plane = new Plane(_bones[boneIndex + 2].position - _bones[boneIndex].position, _bones[boneIndex].position);
                var projectedPole = plane.ClosestPointOnPlane(Pole.position);
                var projectedBone = plane.ClosestPointOnPlane(_bones[boneIndex + 1].position);

                if ((projectedBone - _bones[boneIndex].position).sqrMagnitude > 0.01f)
                {
                    var angle = Vector3.SignedAngle(projectedBone - _bones[boneIndex].position,
                        projectedPole - _bones[boneIndex].position, plane.normal);
                    _bones[boneIndex].rotation = Quaternion.AngleAxis(angle, plane.normal) * _bones[boneIndex].rotation;
                }
            }
        }

        #endregion
    }
}
