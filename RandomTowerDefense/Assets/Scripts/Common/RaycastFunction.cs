using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Experimental.TerrainAPI;
using RandomTowerDefense.Scene;
using RandomTowerDefense.Managers.Macro;
using RandomTowerDefense.Managers.System;
using RandomTowerDefense.Units;

namespace RandomTowerDefense.Common
{
    /// <summary>
    /// レイキャスト機能ユーティリティ - UI要素とのインタラクション処理
    ///
    /// 主な機能:
    /// - マルチレンダラー対応クリック検知（MeshRenderer、RawImage、SpriteRenderer）
    /// - ステージ選択とゲーム内ストアインタラクション統合
    /// - 視覚フィードバック用カラーアニメーション効果
    /// - 30種類のアクション処理（矢印ナビゲーション、ストア購入、ステージ選択等）
    /// - オーディオ統合とクリック音再生システム
    /// - ダイナミックレンダラー検出と適応的カラー制御
    /// </summary>
    public class RaycastFunction : MonoBehaviour
    {
        #region Enums

        /// <summary>
        /// レンダラータイプ識別用列挙型
        /// </summary>
        private enum EnumRenderType
        {
            NotChecked = 0,
            FoundMeshRenderer,
            FoundRawImg,
            FoundSprRenderer,
        }

        /// <summary>
        /// アクションタイプID定義
        /// </summary>
        private enum ActionTypeID
        {
            // ステージ選択シーンアクション
            StageSelection_Keybroad = 0,
            StageSelection_PreviousStage,
            StageSelection_NextStage,

            // ゲームシーン矢印ナビゲーション
            GameScene_UpArrow = 10,
            GameScene_DownArrow,
            GameScene_LeftArrow,
            GameScene_RightArrow,

            // ゲームシーンストア（軍隊購入）
            GameScene_StoreArmy1 = 20, // Nightmare
            GameScene_StoreArmy2,      // SoulEater
            GameScene_StoreArmy3,      // TerrorBringer
            GameScene_StoreArmy4,      // Usurper

            // ゲームシーンストア（城とボーナスボス）
            GameScene_StoreCastleHP = 30,
            GameScene_StoreBonusBoss1, // Green Metalon
            GameScene_StoreBonusBoss2, // Purple Metalon
            GameScene_StoreBonusBoss3, // Red Metalon

            // ゲームシーンストア（魔法スキル）
            GameScene_StoreMagicMeteor = 40,      // Fire
            GameScene_StoreMagicBlizzard,         // Ice
            GameScene_StoreMagicPetrification,    // Mind
            GameScene_StoreMagicMinions,          // Metal
        }

        #endregion

        #region Serialized Fields

        [SerializeField] public int ActionID;
        [SerializeField] public int InfoID;

        #endregion

        #region Private Fields

        private EnumRenderType _rendertype;
        private StageSelectOperation _selectionSceneManager;
        private InGameOperation _sceneManager;
        private PlayerManager _playerManager;
        private StoreManager _storeManager;
        private AudioManager _audioManager;

        private MeshRenderer _chkMeshRender;
        private RawImage _chkRawImg;
        private SpriteRenderer _chkSprRender;

        private Color _oriColor;
        private bool _colorRoutineRunning;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Awake()
        {
            _rendertype = EnumRenderType.NotChecked;
            _oriColor = Color.white;
            _colorRoutineRunning = false;
        }

        /// <summary>
        /// ゲームオブジェクト参照取得とカラー設定
        /// </summary>
        private void Start()
        {
            _selectionSceneManager = FindObjectOfType<StageSelectOperation>();
            _sceneManager = FindObjectOfType<InGameOperation>();
            _playerManager = FindObjectOfType<PlayerManager>();
            _storeManager = FindObjectOfType<StoreManager>();
            _audioManager = FindObjectOfType<AudioManager>();

            if (_rendertype == EnumRenderType.NotChecked)
            {
                GetColor();
            }
        }

        /// <summary>
        /// 毎フレーム更新 - ストアアイテムの視覚効果管理
        /// </summary>
        private void LateUpdate()
        {
            switch ((ActionTypeID)ActionID)
            {
                // ゲームシーン（左側ストア）
                case ActionTypeID.GameScene_StoreArmy1:
                case ActionTypeID.GameScene_StoreArmy2:
                case ActionTypeID.GameScene_StoreArmy3:
                case ActionTypeID.GameScene_StoreArmy4:
                    DarkenStoreSpr();
                    break;

                // ゲームシーン（上側ストア）
                case ActionTypeID.GameScene_StoreCastleHP:
                case ActionTypeID.GameScene_StoreBonusBoss1:
                case ActionTypeID.GameScene_StoreBonusBoss2:
                case ActionTypeID.GameScene_StoreBonusBoss3:
                    DarkenStoreSpr();
                    break;

                // ゲームシーン（右側ストア）
                case ActionTypeID.GameScene_StoreMagicMeteor:
                case ActionTypeID.GameScene_StoreMagicBlizzard:
                case ActionTypeID.GameScene_StoreMagicMinions:
                case ActionTypeID.GameScene_StoreMagicPetrification:
                    DarkenStoreSpr();
                    break;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// レンダラーコンポーネント検出と原色取得
        /// </summary>
        /// <returns>検出されたレンダラーの原色</returns>
        public Color GetColor()
        {
            _chkMeshRender = GetComponent<MeshRenderer>();
            if (_chkMeshRender)
            {
                _oriColor = _chkMeshRender.material.color;
                _rendertype = EnumRenderType.FoundMeshRenderer;
            }
            else
            {
                _chkRawImg = GetComponent<RawImage>();
                if (_chkRawImg)
                {
                    _oriColor = _chkRawImg.color;
                    _rendertype = EnumRenderType.FoundRawImg;
                }
                else
                {
                    _chkSprRender = GetComponent<SpriteRenderer>();
                    if (_chkSprRender)
                    {
                        _oriColor = _chkSprRender.color;
                        _rendertype = EnumRenderType.FoundSprRenderer;
                    }
                }
            }
            return _oriColor;
        }

        /// <summary>
        /// アクション実行処理 - ActionIDに基づく機能実行とオーディオ再生
        /// </summary>
        public void ActionFunc()
        {
            if (_selectionSceneManager == null && _sceneManager == null) return;

            if (_sceneManager && _sceneManager.GetOptionStatus()) return;
            if (_selectionSceneManager && _selectionSceneManager.GetOptionStatus()) return;

            switch ((ActionTypeID)ActionID)
            {
                // ステージ選択シーンアクション
                case ActionTypeID.StageSelection_Keybroad:
                    _selectionSceneManager.TouchKeybroad(InfoID);
                    break;
                case ActionTypeID.StageSelection_PreviousStage:
                    _selectionSceneManager.StageInfoChgSubtract(InfoID);
                    _audioManager.PlayAudio("se_Button");
                    break;
                case ActionTypeID.StageSelection_NextStage:
                    _selectionSceneManager.StageInfoChgAdd(InfoID);
                    _audioManager.PlayAudio("se_Button");
                    break;

                // ゲームシーン（スクリーン切り替え）
                case ActionTypeID.GameScene_UpArrow:
                    _sceneManager.nextScreenShown = (int)InGameOperation.ScreenShownID.SSIDTop;
                    _audioManager.PlayAudio("se_Button");
                    break;
                case ActionTypeID.GameScene_DownArrow:
                    _sceneManager.nextScreenShown = (int)InGameOperation.ScreenShownID.SSIDArena;
                    _audioManager.PlayAudio("se_Button");
                    break;
                case ActionTypeID.GameScene_LeftArrow:
                    _sceneManager.nextScreenShown =
                        (_sceneManager.currScreenShown == (int)InGameOperation.ScreenShownID.SSIDTopRight) ?
                        (int)InGameOperation.ScreenShownID.SSIDTop : (int)InGameOperation.ScreenShownID.SSIDTopLeft;
                    _audioManager.PlayAudio("se_Button");
                    break;
                case ActionTypeID.GameScene_RightArrow:
                    _sceneManager.nextScreenShown =
                    (_sceneManager.currScreenShown == (int)InGameOperation.ScreenShownID.SSIDTopLeft) ?
                    (int)InGameOperation.ScreenShownID.SSIDTop : (int)InGameOperation.ScreenShownID.SSIDTopRight;
                    _audioManager.PlayAudio("se_Button");
                    break;

                // ゲームシーンストア（軍隊購入）
                case ActionTypeID.GameScene_StoreArmy1:
                    _storeManager.RaycastAction(Upgrades.StoreItems.Army1, InfoID);
                    _audioManager.PlayAudio("se_Button");
                    break;
                case ActionTypeID.GameScene_StoreArmy2:
                    _storeManager.RaycastAction(Upgrades.StoreItems.Army2, InfoID);
                    _audioManager.PlayAudio("se_Button");
                    break;
                case ActionTypeID.GameScene_StoreArmy3:
                    _storeManager.RaycastAction(Upgrades.StoreItems.Army3, InfoID);
                    _audioManager.PlayAudio("se_Button");
                    break;
                case ActionTypeID.GameScene_StoreArmy4:
                    _storeManager.RaycastAction(Upgrades.StoreItems.Army4, InfoID);
                    _audioManager.PlayAudio("se_Button");
                    break;

                // ゲームシーンストア（城とボーナスボス）
                case ActionTypeID.GameScene_StoreCastleHP:
                    _storeManager.RaycastAction(Upgrades.StoreItems.CastleHP, InfoID);
                    _audioManager.PlayAudio("se_Button");
                    break;
                case ActionTypeID.GameScene_StoreBonusBoss1:
                    _storeManager.RaycastAction(Upgrades.StoreItems.BonusBoss1, InfoID);
                    _audioManager.PlayAudio("se_Button");
                    break;
                case ActionTypeID.GameScene_StoreBonusBoss2:
                    _storeManager.RaycastAction(Upgrades.StoreItems.BonusBoss2, InfoID);
                    _audioManager.PlayAudio("se_Button");
                    break;
                case ActionTypeID.GameScene_StoreBonusBoss3:
                    _storeManager.RaycastAction(Upgrades.StoreItems.BonusBoss3, InfoID);
                    _audioManager.PlayAudio("se_Button");
                    break;

                // ゲームシーンストア（魔法スキル）
                case ActionTypeID.GameScene_StoreMagicMeteor:
                    _storeManager.RaycastAction(Upgrades.StoreItems.MagicMeteor, InfoID);
                    _audioManager.PlayAudio("se_Button");
                    break;
                case ActionTypeID.GameScene_StoreMagicBlizzard:
                    _storeManager.RaycastAction(Upgrades.StoreItems.MagicBlizzard, InfoID);
                    _audioManager.PlayAudio("se_Button");
                    break;
                case ActionTypeID.GameScene_StoreMagicMinions:
                    _storeManager.RaycastAction(Upgrades.StoreItems.MagicMinions, InfoID);
                    _audioManager.PlayAudio("se_Button");
                    break;
                case ActionTypeID.GameScene_StoreMagicPetrification:
                    _storeManager.RaycastAction(Upgrades.StoreItems.MagicPetrification, InfoID);
                    _audioManager.PlayAudio("se_Button");
                    break;
            }
            StartCoroutine(ColorRoutine());
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// ストアスプライトのダークニング効果処理
        /// </summary>
        private void DarkenStoreSpr()
        {
            if (_playerManager.hitStore == null || this.transform != _playerManager.hitStore)
            {
                switch (_rendertype)
                {
                    case EnumRenderType.FoundMeshRenderer:
                        _chkMeshRender.material.color = new Color(Mathf.Clamp(_chkMeshRender.material.color.r, 0, 0.3f),
                            Mathf.Clamp(_chkMeshRender.material.color.g, 0, 0.3f), Mathf.Clamp(_chkMeshRender.material.color.b, 0, 0.3f));
                        break;
                    case EnumRenderType.FoundRawImg:
                        _chkRawImg.color = new Color(Mathf.Clamp(_chkRawImg.color.r, 0, 0.3f),
                            Mathf.Clamp(_chkRawImg.color.g, 0, 0.3f), Mathf.Clamp(_chkRawImg.color.b, 0, 0.3f));
                        break;
                    case EnumRenderType.FoundSprRenderer:
                        _chkSprRender.color = new Color(Mathf.Clamp(_chkSprRender.color.r, 0, 0.3f),
                            Mathf.Clamp(_chkSprRender.color.g, 0, 0.3f), Mathf.Clamp(_chkSprRender.color.b, 0, 0.3f));
                        break;
                }
            }
            else
            {
                if (_colorRoutineRunning == false)
                {
                    switch (_rendertype)
                    {
                        case EnumRenderType.FoundMeshRenderer:
                            _chkMeshRender.material.color = _oriColor;
                            break;
                        case EnumRenderType.FoundRawImg:
                            _chkRawImg.color = _oriColor;
                            break;
                        case EnumRenderType.FoundSprRenderer:
                            _chkSprRender.color = _oriColor;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// カラーアニメーション処理コルーチン - クリック時のフィードバック効果
        /// </summary>
        /// <returns>コルーチンの進行状況</returns>
        private IEnumerator ColorRoutine()
        {
            _colorRoutineRunning = true;
            Color color = new Color();

            switch (_rendertype)
            {
                case EnumRenderType.NotChecked:
                    color = GetColor();
                    break;
                case EnumRenderType.FoundMeshRenderer:
                    color = _chkMeshRender.material.color;
                    break;
                case EnumRenderType.FoundRawImg:
                    color = _chkRawImg.color;
                    break;
                case EnumRenderType.FoundSprRenderer:
                    color = _chkSprRender.color;
                    break;
            }

            color.r = 0;
            color.g = 0;
            color.b = 0;
            float reqTime = 0;

            while (reqTime < 1f)
            {
                reqTime += Time.deltaTime * 3f;
                color.r = reqTime * _oriColor.r;
                color.g = reqTime * _oriColor.g;
                color.b = reqTime * _oriColor.b;

                switch (_rendertype)
                {
                    case EnumRenderType.FoundMeshRenderer:
                        _chkMeshRender.material.color = color;
                        break;
                    case EnumRenderType.FoundRawImg:
                        _chkRawImg.color = color;
                        break;
                    case EnumRenderType.FoundSprRenderer:
                        _chkSprRender.color = color;
                        break;
                }
                yield return new WaitForSeconds(0f);
            }

            switch (_rendertype)
            {
                case EnumRenderType.FoundMeshRenderer:
                    _chkMeshRender.material.color = _oriColor;
                    break;
                case EnumRenderType.FoundRawImg:
                    _chkRawImg.color = _oriColor;
                    break;
                case EnumRenderType.FoundSprRenderer:
                    _chkSprRender.color = _oriColor;
                    break;
            }
            _colorRoutineRunning = false;
        }

        #endregion
    }
}
