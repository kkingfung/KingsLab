using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using RandomTowerDefense.Scene;

namespace RandomTowerDefense.ProcedualAnimation
{
    /// <summary>
    /// クリーチャーコントローラー - プロシージュラルアニメーションと歩行制御システム
    ///
    /// 主な機能:
    /// - プロシージュラル脚配置システムの統合制御
    /// - 動的歩容アルゴリズムとタイミング管理
    /// - 地形アラインメントと表面距離計算
    /// - ランダムターゲット移動と待機システム
    /// - 3D空間での回転と移動速度調整
    /// - 脚の速度ベクトル計算と回転予測
    /// - IKシステム連携とリアルタイムアニメーション
    /// </summary>
    public class CreatureController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("移動設定")]
        [SerializeField] public float moveInputFactor = 5f;
        [SerializeField] public Vector3 inputVelocity;
        [SerializeField] public Vector3 worldVelocity;
        [SerializeField] public float walkSpeed = 2f;
        [SerializeField] public float rotateInputFactor = 10f;
        [SerializeField] public float rotationSpeed = 10f;
        [SerializeField] public float averageRotationRadius = 3f;

        [Header("脚管理システム")]
        [SerializeField] public ProceduralLegPlacement[] legs;
        [SerializeField] public int index;
        [SerializeField] public bool dynamicGait = false;
        [SerializeField] public float timeBetweenSteps = 0.25f;
        [SerializeField] public float stepDurationRatio = 2f;
        [SerializeField]
        [Tooltip("動的歩容使用時のtimeBetweenSteps計算用")]
        public float maxTargetDistance = 1f;
        [SerializeField] public float lastStep = 0;

        [Header("地形アラインメント")]
        [SerializeField] public bool useAlignment = true;
        [SerializeField] public int[] nextLegTri;
        [SerializeField] public AnimationCurve sensitivityCurve;
        [SerializeField] public float desiredSurfaceDist = -1f;
        [SerializeField] public float dist;
        [SerializeField] public bool grounded = false;

        [Header("ターゲット位置")]
        [SerializeField] public Vector3 TargetLocation;

        #endregion

        #region Private Fields

        private float _mSpeed = 0;
        private float _rSpeed = 0;
        private Vector3 _targetPosition;
        private float _waitingTimer;
        private float _waitingRecord;
        private StageSelectOperation _sceneManager;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// 初期化処理 - システム参照取得とタイマー設定
        /// </summary>
        private void Start()
        {
            _sceneManager = FindObjectOfType<StageSelectOperation>();

            // 平均回転半径計算
            for (int i = 0; i < legs.Length; ++i)
            {
                averageRotationRadius += legs[i].restingPosition.z;
            }
            averageRotationRadius /= legs.Length;

            _waitingRecord = Time.time;
            _waitingTimer = 0;
        }

        /// <summary>
        /// 毎フレーム更新 - 脚の動作と位置管理
        /// </summary>
        private void Update()
        {
            // 地形アラインメント処理
            if (useAlignment)
            {
                CalculateOrientation();
            }

            Move();

            // 動的歩容計算
            if (dynamicGait)
            {
                if (grounded)
                {
                    timeBetweenSteps = maxTargetDistance / Mathf.Max(worldVelocity.magnitude,
                        Mathf.Abs(2 * Mathf.PI * _rSpeed * Mathf.Deg2Rad * averageRotationRadius));
                }
                else
                {
                    timeBetweenSteps = 0.25f;
                }
            }

            // ステップ処理
            if (Time.time > lastStep + (timeBetweenSteps / legs.Length) && legs != null)
            {
                index = (index + 1) % legs.Length;
                if (legs[index] == null) return;

                // 全脚の速度更新
                for (int i = 0; i < legs.Length; ++i)
                {
                    legs[i].MoveVelocity(CalculateLegVelocity(i));
                }

                // アクティブ脚のステップ処理
                legs[index].stepDuration = Mathf.Min(1f, (timeBetweenSteps / legs.Length) * stepDurationRatio);
                legs[index].worldVelocity = CalculateLegVelocity(index);
                if (legs[index].worldVelocity.sqrMagnitude > 1)
                {
                    legs[index].Step();
                }
                lastStep = Time.time;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// 脚速度計算 - 指定脚インデックスの移動速度ベクトル算出
        /// </summary>
        /// <param name="legIndex">脚インデックス</param>
        /// <returns>脚の世界速度ベクトル</returns>
        public Vector3 CalculateLegVelocity(int legIndex)
        {
            Vector3 legPoint = (legs[legIndex].restingPosition);
            Vector3 legDirection = legPoint - transform.position;
            Vector3 rotationalPoint = ((Quaternion.AngleAxis((_rSpeed * timeBetweenSteps) / 2f, transform.up) * legDirection) + transform.position) - legPoint;
            return rotationalPoint + (worldVelocity * timeBetweenSteps) / 2f;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 移動処理 - ターゲット位置への移動と回転制御
        /// </summary>
        private void Move()
        {
            // 待機時間チェック
            if (Time.time - _waitingRecord < _waitingTimer)
                return;

            // ターゲット到達チェック
            if ((_targetPosition.ToXZ() - transform.position.ToXZ()).sqrMagnitude < 1f)
            {
                UpdateTargetPosition();
            }

            // 移動速度設定
            _mSpeed = walkSpeed;
            Vector3 localInput = Vector3.ClampMagnitude(_targetPosition - this.transform.position, 1);
            inputVelocity = Vector3.MoveTowards(inputVelocity, localInput, Time.deltaTime * moveInputFactor);
            worldVelocity = inputVelocity * _mSpeed;

            // 回転計算
            Vector2 normDir = (_targetPosition.ToXZ() - transform.position.ToXZ());
            float angle = Mathf.Acos(Vector2.Dot(normDir.normalized, transform.right.ToXZ().normalized));

            _rSpeed = Mathf.MoveTowards(_rSpeed, ((Mathf.Rad2Deg * angle < 90f) ? 1 : -1) * rotationSpeed, rotateInputFactor * Time.deltaTime);
            transform.Rotate(0f, _rSpeed * Time.deltaTime, 0f);

            // 位置更新
            transform.position += (worldVelocity * Time.deltaTime);
        }

        /// <summary>
        /// 地形向き計算 - 脚の接地点から最適な姿勢を算出
        /// </summary>
        private void CalculateOrientation()
        {
            Vector3 up = Vector3.zero;
            float avgSurfaceDist = 0;
            grounded = false;

            Vector3 point, a, b, c;

            // 各脚の外積を計算して平均上方向を求める
            for (int i = 0; i < legs.Length; ++i)
            {
                point = legs[i].stepPoint;
                avgSurfaceDist += transform.InverseTransformPoint(point).y;
                a = (transform.position - point).normalized;
                b = ((legs[nextLegTri[i]].stepPoint) - point).normalized;
                c = Vector3.Cross(a, b);
                up += c * sensitivityCurve.Evaluate(c.magnitude) + (legs[i].stepNormal == Vector3.zero ? transform.forward : legs[i].stepNormal);
                grounded |= legs[i].legGrounded;

                // デバッグ描画
                Debug.DrawRay(point, a, Color.red, 0);
                Debug.DrawRay(point, b, Color.green, 0);
                Debug.DrawRay(point, c, Color.blue, 0);
            }

            up /= legs.Length;
            avgSurfaceDist /= legs.Length;
            dist = avgSurfaceDist;
            Debug.DrawRay(transform.position, up, Color.red, 0);

            // 前進方向を保持しながら上方向を適用
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, up), up), 22.5f * Time.deltaTime);

            if (grounded)
            {
                transform.Translate(0, -(-avgSurfaceDist + desiredSurfaceDist) * 0.5f, 0, Space.Self);
            }
            else
            {
                // 簡易重力
                transform.Translate(0, -20 * Time.deltaTime, 0, Space.World);
            }
        }

        #endregion

        #region Debug and Utilities

        /// <summary>
        /// Gizmo描画 - 平均回転半径の可視化
        /// </summary>
        public void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, averageRotationRadius);
        }

        /// <summary>
        /// 円弧描画 - デバッグ用円弧ライン描画
        /// </summary>
        /// <param name="point">開始点</param>
        /// <param name="dir">方向ベクトル</param>
        /// <param name="angle">角度</param>
        /// <param name="stepSize">ステップサイズ</param>
        /// <param name="color">色</param>
        /// <param name="duration">描画時間</param>
        public void DrawArc(Vector3 point, Vector3 dir, float angle, float stepSize, Color color, float duration)
        {
            if (angle < 0)
            {
                for (float i = 0; i > angle + 1; i -= stepSize)
                {
                    Debug.DrawLine(point + Quaternion.AngleAxis(i, transform.up) * dir,
                        point + Quaternion.AngleAxis(Mathf.Clamp(i - stepSize, angle, 0), transform.up) * dir,
                        color, duration);
                }
            }
            else
            {
                for (float i = 0; i < angle - 1; i += stepSize)
                {
                    Debug.DrawLine(point + Quaternion.AngleAxis(i, transform.up) * dir,
                        point + Quaternion.AngleAxis(Mathf.Clamp(i + stepSize, 0, angle), transform.up) * dir,
                        color, duration);
                }
            }
        }

        /// <summary>
        /// ターゲット位置更新 - ランダムな新しい目標位置設定
        /// </summary>
        private void UpdateTargetPosition()
        {
            _waitingTimer = Random.Range(2, 5);
            _waitingRecord = Time.time;
            Vector3 dir = new Vector3(Random.Range(-50f, 50f), 0, Random.Range(-50f, 50f));
            _targetPosition = TargetLocation + dir;
        }

        #endregion
    }
}
