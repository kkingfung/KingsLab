using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.UI;
using Unity.Entities;
using Unity.MLAgents;
using RandomTowerDefense.DOTS.Spawner;
using RandomTowerDefense.Managers.System;
using RandomTowerDefense.Managers.Macro;
using RandomTowerDefense.AI;

namespace RandomTowerDefense.Units
{
    /// <summary>
    /// 敵ユニットシステム - 敵キャラクターの行動と状態管理
    ///
    /// 主な機能:
    /// - 体力とダメージ管理システム
    /// - 移動とパスファインディング
    /// - 状態効果処理（スロー、石化等）
    /// - 視覚的フィードバックとエフェクト連携
    /// - リソース報酬システム
    /// - ECS統合とパフォーマンス最適化
    /// - ML-Agents学習データ提供
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        #region Constants

        // ダメージ表示関連
        private readonly int DmgCntIncrement = 2;
        private readonly int ResizeFactor = 5;
        private readonly float ResizeScale = 0.0f;

        // アニメーション・タイミング定数
        private readonly float EnemyDestroyTime = 0.2f;
        private const float DESTROY_EFFECT_DELAY_MULTIPLIER = 0.2f;
        private const float POST_DESTROY_WAIT_TIME = 0.8f;

        // 初期値・初期化定数
        private const float INITIAL_HEALTH_RECORD = 1f;
        private const int INITIAL_DAMAGED_COUNT = 30;
        private const float INITIAL_HP_BAR_VALUE = 1f;

        // エフェクト・視覚効果定数
        private const int RESIZE_ANIMATION_CYCLE = 3;
        private const int EFFECT_SPAWN_INTERVAL = 5;
        private const float SLOW_THRESHOLD = 0.1f;
        private const float EFFECT_POSITION_Y_OFFSET = 0.5f;
        private const int VFX_SPAWN_COUNT_DIVISOR = 10;
        private const int MIN_VFX_SPAWN_COUNT = 1;

        // その他の定数
        private const int INVALID_ENTITY_ID = -1;
        private const int HEALTH_SYNC_THRESHOLD = 1;

        // エフェクト・スポーン定数
        private const int ENEMY_DAMAGE_EFFECT_ID = 4;
        private const int DEATH_EFFECT_ID = 1;
        private const int DESTROY_VFX_ID = 2;

        #endregion

        #region Serialized Fields
        /// <summary>
        /// 敵生成管理クラスの参照
        /// </summary>
        [Header("🎮 Manager References")]
        public EnemySpawner enemySpawner;
        /// <summary>
        /// リソース管理クラスの参照
        /// </summary>
        public ResourceManager resourceManager;
        /// <summary>
        /// エフェクト生成管理クラスの参照
        /// </summary>
        public EffectSpawner effectManager;
        #endregion

        #region Private Fields
        private Vector3 _oriScale;
        private Vector3 _prevPos;
        private int _damagedCount = 0;
        private int _money;
        private float _healthRecord;
        private float _slowRecord;
        private float _petrifyRecord;
        private int _entityID = INVALID_ENTITY_ID;
        #endregion

        private AgentScript _agent;
        private Vector3 oriPos;

        private MeshRenderer[] meshes;
        private SkinnedMeshRenderer[] meshesSkinned;
        private List<Material> matsPetrify;
        private List<Material> matsSlow;

        /// <summary>
        /// アニメーションコントローラー
        /// </summary>
        public Animator animator;
        /// <summary>
        /// HP表示スライダーUI
        /// </summary>
        public Slider HpBar;
        private RectTransform HpBarRot;

        private bool isSync;
        private bool isDead;
        private bool isReady;
        private bool useScaleChg;

        // Start is called before the first frame update
        private void Awake()
        {
            if (enemySpawner == null) enemySpawner = FindObjectOfType<EnemySpawner>();
            if (effectManager == null) effectManager = FindObjectOfType<EffectSpawner>();
            if (resourceManager == null) resourceManager = FindObjectOfType<ResourceManager>();
            if (animator == null)
                animator = GetComponent<Animator>();

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (HpBar == null)
                HpBar = GetComponentInChildren<Slider>();
            if (HpBar != null)
                HpBarRot = HpBar.gameObject.GetComponent<RectTransform>();

            meshes = GetComponentsInChildren<MeshRenderer>();
            meshesSkinned = GetComponentsInChildren<SkinnedMeshRenderer>();

            matsPetrify = new List<Material>();
            matsSlow = new List<Material>();

            for (int i = 0; i < meshes.Length; ++i)
            {
                List<Material> mats = new List<Material>();
                meshes[i].GetMaterials(mats);
                foreach (Material j in mats)
                {
                    if (j.name == "FreezingMat (Instance)")
                    {
                        matsSlow.Add(j);
                    }
                    else if (j.name == "Desertification (Instance)")
                    {
                        matsPetrify.Add(j);
                    }
                }
            }

            for (int i = 0; i < meshesSkinned.Length; ++i)
            {
                List<Material> mats = new List<Material>();
                meshesSkinned[i].GetMaterials(mats);
                foreach (Material j in mats)
                {
                    if (j.name == "FreezingMat (Instance)")
                    {
                        matsSlow.Add(j);
                    }
                    else if (j.name == "Desertification (Instance)")
                    {
                        matsPetrify.Add(j);
                    }
                }
            }

            useScaleChg = false;
        }


        private void MyStart()
        {
            _healthRecord = INITIAL_HEALTH_RECORD;
            _prevPos = transform.position;
            oriPos = _prevPos;
            isDead = false;
            isReady = false;
            isSync = false;
            _damagedCount = INITIAL_DAMAGED_COUNT;
            _oriScale = transform.localScale;
            transform.localScale = new Vector3();
            StartCoroutine(StartAnim());
        }

        // Update is called once per frame
        private void LateUpdate()
        {
            if (isDead) return;
            if (_entityID >= 0 && _healthRecord > 0)
            {
                float currHP = enemySpawner.healthArray[_entityID];
                if (currHP > HpBar.maxValue)
                    HpBar.maxValue = currHP;
                if (currHP != _healthRecord && _healthRecord > 0)
                {
                    Damaged(currHP);
                    _healthRecord = currHP;
                    HpBar.value = Mathf.Max(currHP, 0);
                }
                HpBarRot.eulerAngles = Vector3.zero;
                //CheckingStatus
                Petrified();
                Slowed();
                if (isSync == false && currHP > HEALTH_SYNC_THRESHOLD) isSync = true;
            }

            if (_damagedCount > 0 && isReady && useScaleChg)
            {
                _damagedCount--;
                if ((_damagedCount / ResizeFactor) % RESIZE_ANIMATION_CYCLE == 0 && _damagedCount != 0)
                    transform.localScale = _oriScale * ResizeScale;
                else
                    transform.localScale = _oriScale;
            }

            if (transform.position != _prevPos)
            {
                Vector3 direction = transform.position - _prevPos;
                transform.forward = direction;
                _prevPos = transform.position;
            }
        }
        /// <summary>
        /// 敵エンティティに必要な管理クラスの参照を設定
        /// </summary>
        /// <param name="enemySpawner">敵スポーナーの参照</param>
        /// <param name="effectManager">エフェクト管理の参照</param>
        /// <param name="resourceManager">リソース管理の参照</param>
        public void LinkingManagers(EnemySpawner enemySpawner, EffectSpawner effectManager,
        ResourceManager resourceManager)
        {
            this.enemySpawner = enemySpawner;
            this.effectManager = effectManager;
            this.resourceManager = resourceManager;
        }


        /// <summary>
        /// 敵エンティティを指定したパラメータで初期化
        /// </summary>
        /// <param name="_entityID">エンティティID</param>
        /// <param name="money">撃破時の報酬金額</param>
        /// <param name="agent">ML-Agentsスクリプト（オプション）</param>
        public void Init(int _entityID, int money, AgentScript agent = null)
        {
            MyStart();

            this._entityID = _entityID;
            this._money = money;
            this._agent = agent;
            HpBar.maxValue = INITIAL_HP_BAR_VALUE;
            HpBar.value = INITIAL_HP_BAR_VALUE;

            if (animator == null)
                animator = GetComponent<Animator>();

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
            if (animator)
                animator.SetTrigger("Reused");
        }

        /// <summary>
        /// ダメージ処理 - HP減少時のビジュアルフィードバックと死亡判定
        /// </summary>
        /// <param name="currHP">現在のHP値</param>
        public void Damaged(float currHP)
        {
            if (isSync == false) return;
            _damagedCount += DmgCntIncrement;
            if (_damagedCount % EFFECT_SPAWN_INTERVAL == 0)
            {
                effectManager.Spawn(ENEMY_DAMAGE_EFFECT_ID, this.transform.position);
            }
            if (currHP <= 0)
            {
                isDead = true;
                StartCoroutine(DieAnimation());
            }
        }

        /// <summary>
        /// 死亡処理 - オブジェクトプールへの返却とクリーンアップ
        /// </summary>
        public void Die()
        {
            transform.localScale = _oriScale;
            isReady = false;
            if (_agent) _agent.EnemyDisappear(oriPos, this.transform.position);
            StartCoroutine(EndAnim());
        }

        /// <summary>
        /// 石化状態エフェクト更新処理
        /// </summary>
        public void Petrified()
        {
            float petrifyAmt = enemySpawner.petrifyArray[_entityID];
            if (_petrifyRecord == petrifyAmt) return;
            else _petrifyRecord = petrifyAmt;

            foreach (Material j in matsPetrify)
            {
                j.SetFloat("_Progress", petrifyAmt);
            }
        }
        /// <summary>
        /// スロー状態エフェクト更新処理
        /// </summary>
        public void Slowed()
        {
            float slow = enemySpawner.slowArray[_entityID];
            if (Mathf.Abs(_slowRecord - slow) < SLOW_THRESHOLD) return;
            else _slowRecord = slow;

            foreach (Material j in matsSlow)
            {
                j.SetFloat("_Progress", slow);
            }
        }

        /// <summary>
        /// 死亡アニメーション処理 - エフェクト再生とリソース報酬支給
        /// </summary>
        /// <returns>コルーチンイテレータ</returns>
        private IEnumerator DieAnimation()
        {
            if (animator)
                animator.SetTrigger("Dead");

            effectManager.Spawn(DEATH_EFFECT_ID, this.transform.position + Vector3.up * EFFECT_POSITION_Y_OFFSET);
            yield return new WaitForSeconds(EnemyDestroyTime * DESTROY_EFFECT_DELAY_MULTIPLIER);

            GameObject vfx = effectManager.Spawn(DESTROY_VFX_ID, this.transform.position);
            if (vfx)
                vfx.GetComponent<VisualEffect>().SetInt("SpawnCount", Mathf.Max(_money / VFX_SPAWN_COUNT_DIVISOR, MIN_VFX_SPAWN_COUNT));
            resourceManager.ChangeMaterial(_money);
            Die();
        }

        /// <summary>
        /// 終了アニメーション処理 - スケール縮小とオブジェクト破棄
        /// </summary>
        /// <returns>コルーチンイテレータ</returns>
        private IEnumerator EndAnim()
        {
            float timeCounter = 0;
            yield return new WaitForSeconds(POST_DESTROY_WAIT_TIME);
            float spd = transform.localScale.x / (EnemyDestroyTime);
            while (timeCounter < EnemyDestroyTime)
            {
                float delta = Time.deltaTime;
                timeCounter += delta;
                transform.localScale = new Vector3(transform.localScale.x - spd * delta,
                    transform.localScale.y - spd * delta, transform.localScale.z - spd * delta);
                yield return new WaitForSeconds(0);
            }

            transform.localScale = new Vector3();

            this.gameObject.SetActive(false);
            Destroy(this.gameObject);
        }

        /// <summary>
        /// 出現アニメーション処理 - スケール拡大と初期化
        /// </summary>
        /// <returns>コルーチンイテレータ</returns>
        private IEnumerator StartAnim()
        {
            float timeCounter = 0;
            float spd = _oriScale.x / (EnemyDestroyTime);
            while (timeCounter < EnemyDestroyTime)
            {
                float delta = Time.deltaTime;
                timeCounter += delta;
                transform.localScale = new Vector3(transform.localScale.x + spd * delta,
                    transform.localScale.y + spd * delta, transform.localScale.z + spd * delta);
                yield return new WaitForSeconds(0);
            }

            transform.localScale = _oriScale;
            isReady = true;
        }
    }
}
