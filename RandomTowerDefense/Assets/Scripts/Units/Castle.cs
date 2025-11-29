using UnityEngine;
using Unity.Entities;
using System.Collections;
using RandomTowerDefense.DOTS.Spawner;
using RandomTowerDefense.Managers.Macro;
using RandomTowerDefense.Managers.System;

namespace RandomTowerDefense.Units
{
    /// <summary>
    /// 城ユニットシステム - プレイヤー基地の体力管理と状態制御
    ///
    /// 主な機能:
    /// - 城のHP管理と表示更新
    /// - ダメージ受け取りと回復処理
    /// - シールド視覚エフェクト制御
    /// - ECSエンティティとのデータ同期
    /// - ゲーム終了条件処理
    /// - オーディオフィードバック連携
    /// </summary>
    public class Castle : MonoBehaviour
    {
        #region Public Properties
        public int MaxCastleHP;
        public int CurrCastleHP;
        #endregion

        #region Serialized Fields
        [Header("🔰 Visual Components")]
        public GameObject Shield;
        public TextMesh HPText;
        #endregion

        #region Private Fields
        private AudioManager _audioManager;
        private AudioSource _audioSource;
        private StageManager _stageManager;
        private CastleSpawner _castleSpawner;
        private TutorialManager _tutorialManager;
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// 初期化処理 - コンポーネント参照と初期HP設定
        /// </summary>
        private void Start()
        {
            _audioManager = FindObjectOfType<AudioManager>();
            _castleSpawner = FindObjectOfType<CastleSpawner>();
            _audioSource = GetComponent<AudioSource>();
            _stageManager = FindObjectOfType<StageManager>();
            _tutorialManager = FindObjectOfType<TutorialManager>();
            MaxCastleHP = (int)PlayerPrefs.GetFloat("hpMax", 10);
            CurrCastleHP = MaxCastleHP;
        }

        /// <summary>
        /// 毎フレーム更新 - HP状態監視とUI更新
        /// </summary>
        private void Update()
        {
            int previousCastleHP = CurrCastleHP;
            CurrCastleHP = GetCastleHpFromEntity();

            HPText.text = CurrCastleHP < 0 ? "0" : CurrCastleHP.ToString();
            if (previousCastleHP > CurrCastleHP)
            {
                AddedHealth(0);
                _stageManager.PlayDmgAnim();
            }
            Shield.SetActive(CurrCastleHP > 1);
        }
        #endregion

        #region Public API
        /// <summary>
        /// 城のHPを変更し、ゲーム終了条件をチェック
        /// </summary>
        /// <param name="Val">変更するHP量（デフォルト：1）</param>
        /// <returns>城がHP0以下になったか</returns>
        public bool AddedHealth(int Val = 1)
        {
            _castleSpawner.castleHPArray[0] = CurrCastleHP + Val;
            if (CurrCastleHP + Val <= 0)
            {
                if (_stageManager.CheckLose())
                {
                    _tutorialManager.DestroyAllRelated();
                }
            }
            if (CurrCastleHP > 0)
            {
                SetCastleHpToEntity(CurrCastleHP + Val);
            }

            return CurrCastleHP <= 0;
        }

        /// <summary>
        /// ECSエンティティにHP値を設定
        /// </summary>
        /// <param name="hp">設定するHP値</param>
        public void SetCastleHpToEntity(int hp)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            entityManager.SetComponentData(_castleSpawner.Entities[0], new Health
            {
                Value = hp
            });

            if (MaxCastleHP < hp)
            {
                MaxCastleHP = hp;
            }
        }
        /// <summary>
        /// ECSエンティティから現在のHPを取得
        /// </summary>
        /// <returns>現在のHP値</returns>
        public int GetCastleHpFromEntity()
        {
            if (_castleSpawner.castleHPArray.Length > 0)
            {
                return _castleSpawner.castleHPArray[0];
            }
            return -1;
        }
        #endregion
    }
}