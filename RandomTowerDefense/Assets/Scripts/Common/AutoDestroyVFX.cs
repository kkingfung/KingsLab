using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace RandomTowerDefense.Common
{
    /// <summary>
    /// VFX自動破棄ユーティリティ - ビジュアルエフェクトの自動ライフサイクル管理
    ///
    /// 主な機能:
    /// - タイマーベースVFX自動停止とオブジェクト非アクティブ化
    /// - VisualEffect Graph統合とパーティクルシステム制御
    /// - スキル連携VFXの特別処理（3秒遅延破棄）
    /// - ゲームオブジェクトプール最適化とメモリ管理
    /// - エフェクト再生状態追跡と適切なクリーンアップ
    /// </summary>
    public class AutoDestroyVFX : MonoBehaviour
    {
        #region Public Properties

        /// <summary>
        /// VFX破棄タイマー
        /// </summary>
        public float Timer;

        /// <summary>
        /// 関連スキルコンポーネント
        /// </summary>
        public Skill skill;

        #endregion

        #region Private Fields

        private VisualEffect _ps;
        private bool _tobeDestroy;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// 有効化時処理 - VFX初期化と再生開始
        /// </summary>
        private void OnEnable()
        {
            _ps = GetComponent<VisualEffect>();
            _ps.Play();
            skill = gameObject.GetComponent<Skill>();
            _tobeDestroy = false;
        }

        /// <summary>
        /// 毎フレーム更新 - VFX停止と破棄管理
        /// </summary>
        private void Update()
        {
            Timer = Timer + (_tobeDestroy ? Time.deltaTime : -1 * Time.deltaTime);
            if (_tobeDestroy == false && Timer < 0)
            {
                _ps.Stop();
                // VFXにはps.isPlayingがないため手動管理
                _tobeDestroy = true;
                if (skill != null)
                {
                    Destroy(gameObject, 3f);
                }
            }

            if (skill == null &&
                _tobeDestroy == true && Timer > 2)
            {
                gameObject.SetActive(false);
            }
        }

        #endregion
    }
}