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
        private VisualEffect ps;
        public float Timer;
        private bool tobeDestroy;
        public Skill skill;
        private void OnEnable()
        {
            ps = GetComponent<VisualEffect>();
            ps.Play();
            skill = gameObject.GetComponent<Skill>();
            tobeDestroy = false;
        }
        private void Update()
        {
            Timer = Timer + (tobeDestroy ? Time.deltaTime : -1 * Time.deltaTime);
            if (tobeDestroy == false && Timer < 0)
            {
                ps.Stop();
                //No ps.isPlaying in VFX
                tobeDestroy = true;
                if (skill != null)
                {
                    Destroy(gameObject, 3f);
                }
            }

            if (skill == null && 
                tobeDestroy == true && Timer > 2)
            {
                gameObject.SetActive(false);
            }
        }
    }
}