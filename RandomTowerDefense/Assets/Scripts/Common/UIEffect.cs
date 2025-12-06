using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RandomTowerDefense.Managers.System;
using RandomTowerDefense.Scene;

namespace RandomTowerDefense.Common
{
    /// <summary>
    /// UIエフェクトユーティリティ - 汎用UIアニメーションとインタラクション効果
    ///
    /// 主な機能:
    /// - 12種類UIエフェクト（タイトル、矢印、アニメーション、フェード等）
    /// - マルチレンダラー対応（Text、Image、TextMesh、SpriteRenderer等）
    /// - クロスプラットフォームタッチ・マウス入力対応
    /// - シーン固有UIアニメーション（タイトル、ステージ選択、ゲーム）
    /// - レイキャストベースインタラクション検知
    /// - 動的テキストアニメーションとタイプライター効果
    /// </summary>
    public class UIEffect : MonoBehaviour
    {
        /// <summary>
        /// エフェクトIDタイプ列挙型
        /// </summary>
        public enum EffectIDType
        {
            TitleRecord = -1,
            TitleInstruction = 0,
            SelectionArrowVertical = 1,
            SelectionArrowHorizontal = 2,
            OptionCanvasGyro = 3,
            CustomIslandInfo = 4,
            IslandInfo = 5,
            BossSprite = 6,
            ClearMark = 7,
            BossFrame = 8,
            SellingMark = 9,
            WaveNumber = 10,
            ScoreInformation = 11
        }

        #region Constants

        /// <summary>
        /// UI拡大ベクトル値
        /// </summary>
        private static readonly Vector3 ENLARGE_VECTOR = new Vector3(0.03f, 0.03f, 0);

        /// <summary>
        /// レイキャスト最大距離
        /// </summary>
        private const float RAYCAST_MAX_DISTANCE = 200f;

        /// <summary>
        /// アルファ減少レート
        /// </summary>
        private const float ALPHA_DECREASE_RATE = 0.005f;

        /// <summary>
        /// アルファ増加レート
        /// </summary>
        private const float ALPHA_INCREASE_RATE = 0.1f;

        /// <summary>
        /// テキスト表示速度
        /// </summary>
        private const float TEXT_DISPLAY_SPEED = 0.5f;

        /// <summary>
        /// 最小アルファ値
        /// </summary>
        private const float MIN_ALPHA_VALUE = 0.0f;

        /// <summary>
        /// 最大アルファ値
        /// </summary>
        private const float MAX_ALPHA_VALUE = 1.0f;

        #endregion

        #region Public Properties

        // int から enum に変更
        [SerializeField] public EffectIDType EffectID = EffectIDType.TitleInstruction;
        /// <summary>
        /// UI識別子（汎用目的）
        /// </summary>
        [SerializeField] public int uiID = 0;
        [SerializeField] public float magnitude = 0;
        [SerializeField] public Camera targetCam = null;
        [SerializeField] public Camera subCam = null;
        [SerializeField] public List<GameObject> relatedObjs;

        #endregion

        #region Private Fields

        private Text _text;
        private Slider _slider;
        private Image _image;
        private TextMesh _textMesh;
        private SpriteRenderer _spr;

        private Vector3 _oriPosRect;
        private Vector3 _oriRot;
        private Vector3 _oriScale;
        private Color _oriColour;

        private float _alpha;
        private string _fullText;
        private float _textCnt;

        private List<TextMesh> _relatedObjsTextMesh;
        private List<SpriteRenderer> _relatedObjsSpriteRenderer;
        private RectTransform _rectTrans;

        #endregion

        #region Manager References

        private StageSelectOperation _selectionSceneManager;
        private InGameOperation _gameSceneManager;
        private PlayerManager _playerManager;
        private InputManager _inputManager;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// 初期化処理 - コンポーネント参照取得とUIエフェクトセットアップ
        /// </summary>
        private void Start()
        {
            InitializeComponents();
            InitializeManagerReferences();
            InitializeTransformProperties();
            InitializeTextProperties();
            InitializeRelatedObjects();
        }

        /// <summary>
        /// 毎フレーム更新 - エフェクトIDに基づくUI効果処理
        /// </summary>
        private void Update()
        {
            switch (EffectID)
            {
                case EffectIDType.TitleRecord: // -1
                    HandleTitleSceneRecord();
                    break;
                case EffectIDType.TitleInstruction: // 0
                    HandleTitleSceneInstruction();
                    break;
                case EffectIDType.SelectionArrowVertical: // 1
                    HandleSelectionArrowVertical();
                    break;
                case EffectIDType.SelectionArrowHorizontal: // 2
                    HandleSelectionArrowHorizontal();
                    break;
                case EffectIDType.OptionCanvasGyro: // 3
                    HandleOptionCanvasGyro();
                    break;
                case EffectIDType.CustomIslandInfo: // 4
                    HandleCustomIslandInfo();
                    break;
                case EffectIDType.IslandInfo: // 5
                    HandleIslandInfo();
                    break;
                case EffectIDType.BossSprite: // 6
                    HandleBossSprite();
                    break;
                case EffectIDType.ClearMark: // 7
                    HandleClearMark();
                    break;
                case EffectIDType.BossFrame: // 8
                    HandleBossFrame();
                    break;
                case EffectIDType.SellingMark: // 9
                    HandleSellingMark();
                    break;
                case EffectIDType.WaveNumber: // 10
                    HandleWaveNumber();
                    break;
                case EffectIDType.ScoreInformation: // 11
                    HandleScoreInfo();
                    break;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// スコア表示用リセット処理
        /// </summary>
        public void ResetForScoreShow()
        {
            _textCnt = 0;
            if (_text) _fullText = _text.text;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// コンポーネント参照初期化
        /// </summary>
        private void InitializeComponents()
        {
            _text = GetComponent<Text>();
            _slider = GetComponentInParent<Slider>();
            _image = GetComponentInParent<Image>();
            if (_image) _oriColour = _image.color;

            _textMesh = GetComponentInParent<TextMesh>();
            _spr = GetComponentInParent<SpriteRenderer>();
            if (_image == null && _spr) _oriColour = _spr.color;
        }

        /// <summary>
        /// マネージャー参照初期化
        /// </summary>
        private void InitializeManagerReferences()
        {
            _selectionSceneManager = FindObjectOfType<StageSelectOperation>();
            _gameSceneManager = FindObjectOfType<InGameOperation>();
            _playerManager = FindObjectOfType<PlayerManager>();
            _inputManager = FindObjectOfType<InputManager>();
        }

        /// <summary>
        /// トランスフォームプロパティ初期化
        /// </summary>
        private void InitializeTransformProperties()
        {
            _oriRot = transform.localEulerAngles;
            _oriScale = transform.localScale;
            _alpha = 0f;

            _rectTrans = GetComponent<RectTransform>();
            if (_rectTrans)
            {
                _oriPosRect = new Vector3(_rectTrans.localPosition.x, _rectTrans.localPosition.y, _rectTrans.localPosition.z);
            }
        }

        /// <summary>
        /// テキストプロパティ初期化
        /// </summary>
        private void InitializeTextProperties()
        {
            _textCnt = 0;
            if (_textMesh) _fullText = _textMesh.text;
            else if (_text) _fullText = _text.text;
        }

        /// <summary>
        /// 関連オブジェクト初期化
        /// </summary>
        private void InitializeRelatedObjects()
        {
            _relatedObjsTextMesh = new List<TextMesh>();
            _relatedObjsSpriteRenderer = new List<SpriteRenderer>();
            foreach (GameObject obj in relatedObjs)
            {
                _relatedObjsTextMesh.Add(obj.GetComponent<TextMesh>());
                _relatedObjsSpriteRenderer.Add(obj.GetComponent<SpriteRenderer>());
            }
        }

        /// <summary>
        /// タイトルシーンレコード処理
        /// </summary>
        private void HandleTitleSceneRecord()
        {
            RaycastHit hit;
            Ray ray = GetInputRay();

            if (Physics.Raycast(ray, out hit, RAYCAST_MAX_DISTANCE, LayerMask.GetMask("RecordBroad")))
            {
                ShowRelatedObjects();
            }
            else
            {
                HideRelatedObjects();
            }
        }

        /// <summary>
        /// 入力レイを取得
        /// </summary>
        /// <returns>入力に基づくRayオブジェクト</returns>
        private Ray GetInputRay()
        {
            Ray ray = new Ray();
            Camera cam = (Screen.width > Screen.height) ? targetCam : subCam;

            if (_inputManager.GetUseTouch())
            {
                if (Input.touchCount > 0)
                    ray = cam.ScreenPointToRay(Input.GetTouch(0).position);
            }
            else
            {
                ray = cam.ScreenPointToRay(Input.mousePosition);
            }

            return ray;
        }

        /// <summary>
        /// 関連オブジェクト表示
        /// </summary>
        private void ShowRelatedObjects()
        {
            for (int i = 0; i < relatedObjs.Count; ++i)
            {
                if (_relatedObjsTextMesh[i])
                {
                    _relatedObjsTextMesh[i].color = new Color(_relatedObjsTextMesh[i].color.r,
                        _relatedObjsTextMesh[i].color.g, _relatedObjsTextMesh[i].color.b, 1f);
                }
                else if (_relatedObjsSpriteRenderer[i])
                {
                    _relatedObjsSpriteRenderer[i].color = new Color(_relatedObjsSpriteRenderer[i].color.r,
                        _relatedObjsSpriteRenderer[i].color.g, _relatedObjsSpriteRenderer[i].color.b, 1f);
                }
            }
        }

        /// <summary>
        /// 関連オブジェクト非表示
        /// </summary>
        private void HideRelatedObjects()
        {
            for (int i = 0; i < relatedObjs.Count; ++i)
            {
                if (_relatedObjsTextMesh[i])
                {
                    float alpha = Mathf.Max(MIN_ALPHA_VALUE, _relatedObjsTextMesh[i].color.a - ALPHA_DECREASE_RATE);
                    _relatedObjsTextMesh[i].color = new Color(_relatedObjsTextMesh[i].color.r,
                        _relatedObjsTextMesh[i].color.g, _relatedObjsTextMesh[i].color.b, alpha);
                }
                else if (_relatedObjsSpriteRenderer[i])
                {
                    float alpha = Mathf.Max(MIN_ALPHA_VALUE, _relatedObjsSpriteRenderer[i].color.a - ALPHA_DECREASE_RATE);
                    _relatedObjsSpriteRenderer[i].color = new Color(_relatedObjsSpriteRenderer[i].color.r,
                        _relatedObjsSpriteRenderer[i].color.g, _relatedObjsSpriteRenderer[i].color.b, alpha);
                }
            }
        }

        /// <summary>
        /// タイトルシーン指示処理
        /// </summary>
        private void HandleTitleSceneInstruction()
        {
            if (_text) _text.color = new Color(_text.color.r, _text.color.g, _text.color.b,
                Mathf.Abs(Mathf.Sin(Time.time * magnitude)));
        }

        /// <summary>
        /// 選択シーン垂直矢印処理
        /// </summary>
        private void HandleSelectionArrowVertical()
        {
            _rectTrans.localPosition = _oriPosRect + Mathf.Sin(Time.time) * magnitude * targetCam.transform.forward;
        }

        /// <summary>
        /// 選択シーン水平矢印処理
        /// </summary>
        private void HandleSelectionArrowHorizontal()
        {
            _rectTrans.localPosition = _oriPosRect + Mathf.Sin(Time.time) * magnitude * targetCam.transform.right;
        }

        /// <summary>
        /// オプションキャンバスジャイロ処理
        /// </summary>
        private void HandleOptionCanvasGyro()
        {
            if (_slider)
            {
                transform.localEulerAngles = new Vector3(_oriRot.x, _oriRot.y, _oriRot.z - 360f * _slider.value);
            }
        }

        /// <summary>
        /// カスタム島情報処理
        /// </summary>
        private void HandleCustomIslandInfo()
        {
            if (_spr == null) return;

            _alpha = (_selectionSceneManager.CurrentIslandNum() == _selectionSceneManager.NextIslandNum() &&
                     _selectionSceneManager.CurrentIslandNum() == uiID) ?
                     (_alpha < MAX_ALPHA_VALUE ? _alpha + ALPHA_INCREASE_RATE : MAX_ALPHA_VALUE) : 0;

            _spr.color = new Color(_oriColour.r, _oriColour.g, _oriColour.b, _alpha);
        }

        /// <summary>
        /// 島情報処理
        /// </summary>
        private void HandleIslandInfo()
        {
            if (_selectionSceneManager == null || _textMesh == null) return;

            _textCnt = (_selectionSceneManager.CurrentIslandNum() == uiID) ?
                      Mathf.Min(_textCnt + 1, _fullText.Length) : 0;

            _textMesh.text = _fullText.Substring(0, (int)_textCnt);
        }

        /// <summary>
        /// ボススプライト処理
        /// </summary>
        private void HandleBossSprite()
        {
            if (_selectionSceneManager == null || _spr == null) return;
            _spr.color = Color.black;
        }

        /// <summary>
        /// クリアマーク処理
        /// </summary>
        private void HandleClearMark()
        {
            if (_selectionSceneManager == null || _image == null) return;

            _image.color = (_selectionSceneManager.EnabledtIslandNum() - 1 > uiID) ?
                          _oriColour : Color.clear;
        }

        /// <summary>
        /// ボスフレーム処理
        /// </summary>
        private void HandleBossFrame()
        {
            /// <summary>
            /// 現在は空実装 - 必要に応じて実装可能
            /// </summary>
        }

        /// <summary>
        /// 販売マーク処理
        /// </summary>
        private void HandleSellingMark()
        {
            if (_playerManager.isSelling &&
                !_gameSceneManager.GetOptionStatus() &&
                _gameSceneManager.currScreenShown == (int)InGameOperation.ScreenShownID.SSIDArena)
            {
                _image.color = new Color(_image.color.r, _image.color.g, _image.color.b,
                    Mathf.Abs(Mathf.Sin(Time.time * magnitude)));
            }
            else
            {
                _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 0);
            }
        }

        /// <summary>
        /// ウェーブ番号処理
        /// </summary>
        private void HandleWaveNumber()
        {
            if (_text && _text.color.a > 0)
            {
                _text.color = new Color(_text.color.r, _text.color.g, _text.color.b,
                    _text.color.a - magnitude);
            }
        }

        /// <summary>
        /// スコア情報処理
        /// </summary>
        private void HandleScoreInfo()
        {
            if (_text == null) return;

            _textCnt = Mathf.Min(_textCnt + TEXT_DISPLAY_SPEED, _fullText.Length);
            _text.text = _fullText.Substring(0, (int)_textCnt);
        }

        #endregion
    }
}
