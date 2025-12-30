using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RandomTowerDefense.ProcedualAnimation
{
    /// <summary>
    /// グーグリーアイ - ランダムな視線移動アニメーションシステム
    ///
    /// 主な機能:
    /// - ランダムな視線方向生成とスムーズな補間
    /// - 確率ベースの視線変更タイミング制御
    /// - 3D球面上の自然な視線分布アルゴリズム
    /// - Quaternion.Slerpによるスムーズな回転アニメーション
    /// - 前方向バイアス付きランダム視線生成
    /// - リアルタイム視線追従システム
    /// </summary>
    public class GooglyEye : MonoBehaviour
    {
        #region Serialized Fields

        [Header("視線設定")]
        [SerializeField] [Range(0f, 1f)] [Tooltip("フレームあたりの視線変更確率")]
        public float changeChance = 0.1f;

        [SerializeField] [Tooltip("現在の視線方向ベクトル")]
        public Vector3 gaze;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// 初期化処理 - 初期視線方向設定
        /// </summary>
        private void Start()
        {
            UpdateGaze();
        }

        /// <summary>
        /// 毎フレーム更新 - 視線変更と回転補間
        /// </summary>
        private void Update()
        {
            // 確率ベース視線変更判定
            float p = Random.Range(0f, 1f);
            if (p < changeChance)
            {
                UpdateGaze();
            }

            // スムーズな回転補間
            transform.localRotation = Quaternion.Slerp(transform.localRotation,
                Quaternion.LookRotation(gaze), Time.deltaTime);
        }

        #endregion

        #region Public API

        /// <summary>
        /// 視線更新 - 新しいランダム視線方向生成
        /// </summary>
        public void UpdateGaze()
        {
            gaze = Random.onUnitSphere;
            gaze.z = Mathf.Abs(gaze.z * 2f); // 前方向バイアス
        }

        #endregion
    }
}
