using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RandomTowerDefense.Common
{
    /// <summary>
    /// フェードエフェクトユーティリティ - シーン長移時のフェードイン・アウト処理
    ///
    /// 主な機能:
    /// - マルチレンダラー対応（MeshRenderer、RawImage、SpriteRenderer、Image）
    /// - シェーダーベースフェード効果とPlayerPrefs連携
    /// - コルーチンベーススムーズアニメーション
    /// - フェード完了状態追跡とコールバック連携
    /// - 動的マテリアル検出とシェーダーパラメーター制御
    /// </summary>
    public class FadeEffect : MonoBehaviour
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
            FoundImage,
        }

        #endregion

        #region Constants

        /// <summary>
        /// フェード処理レート（フレーム毎のアルファ変化量）
        /// </summary>
        private const float FADE_RATE = 0.02f;

        /// <summary>
        /// フェード最小しきい値（完全透明）
        /// </summary>
        private const float FADE_MIN_THRESHOLD = 0.0f;

        /// <summary>
        /// フェード最大しきい値（完全不透明）
        /// </summary>
        private const float FADE_MAX_THRESHOLD = 1.0f;

        #endregion

        #region Public Properties

        /// <summary>
        /// フェード処理完了フラグ
        /// </summary>
        public bool isReady { get; private set; }

        #endregion

        #region Private Fields

        private EnumRenderType _renderType;
        private float _threshold = FADE_MIN_THRESHOLD;
        private float _thresholdRecord;
        private Material _fadeMat;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// 初期化処理 - フェードマテリアル設定とPlayerPrefs初期化
        /// </summary>
        private void Start()
        {
            if (_fadeMat == null)
                GetFadeMaterial();

            _fadeMat.SetFloat("_FadeThreshold", FADE_MAX_THRESHOLD);
            PlayerPrefs.SetFloat("_FadeThreshold", FADE_MAX_THRESHOLD);
            _thresholdRecord = _threshold;
        }

        /// <summary>
        /// ゲームオブジェクト有効化時処理
        /// </summary>
        private void OnEnable()
        {
            _threshold = PlayerPrefs.GetFloat("_FadeThreshold");
        }

        /// <summary>
        /// 毎フレーム更新 - しきい値変更検知とシェーダーパラメーター更新
        /// </summary>
        private void Update()
        {
            if (_thresholdRecord != _threshold)
            {
                _fadeMat.SetFloat("_FadeThreshold", _threshold);
                _thresholdRecord = _threshold;
            }
        }

        #endregion
        #region Public Methods

        /// <summary>
        /// フェードイン実行 - 画面を暗転から通常表示へ
        /// </summary>
        public void FadeIn()
        {
            _threshold = FADE_MIN_THRESHOLD;
            PlayerPrefs.SetFloat("_FadeThreshold", _threshold);
            isReady = false;

            if (gameObject.activeInHierarchy)
                StartCoroutine(FadeInRoutine());
        }

        /// <summary>
        /// フェードアウト実行 - 画面を通常表示から暗転へ
        /// </summary>
        public void FadeOut()
        {
            _threshold = 1.0f;
            PlayerPrefs.SetFloat("_FadeThreshold", _threshold);
            isReady = false;

            if (gameObject.activeInHierarchy)
                StartCoroutine(FadeOutRoutine());
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// フェードマテリアル取得 - レンダラーコンポーネントからマテリアル検出
        /// </summary>
        public void GetFadeMaterial()
        {
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer)
            {
                _fadeMat = meshRenderer.material;
                _renderType = EnumRenderType.FoundMeshRenderer;
                return;
            }

            RawImage rawImage = GetComponent<RawImage>();
            if (rawImage)
            {
                _fadeMat = rawImage.material;
                _renderType = EnumRenderType.FoundRawImg;
                return;
            }

            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer)
            {
                _fadeMat = spriteRenderer.material;
                _renderType = EnumRenderType.FoundSprRenderer;
                return;
            }

            Image image = GetComponent<Image>();
            if (image)
            {
                _fadeMat = image.material;
                _renderType = EnumRenderType.FoundImage;
            }
        }

        /// <summary>
        /// フェードアウトコルーチン - しきい値を段階的に減少
        /// </summary>
        /// <returns>コルーチンの進行状況</returns>
        private IEnumerator FadeOutRoutine()
        {
            if (_fadeMat == null)
                GetFadeMaterial();

            while (_threshold > 0f)
            {
                _threshold -= FADE_RATE;
                PlayerPrefs.SetFloat("_FadeThreshold", _threshold);
                yield return null;
            }

            isReady = true;
        }

        /// <summary>
        /// フェードインコルーチン - しきい値を段階的に増加
        /// </summary>
        /// <returns>コルーチンの進行状況</returns>
        private IEnumerator FadeInRoutine()
        {
            if (_fadeMat == null)
                GetFadeMaterial();

            while (_threshold < FADE_MAX_THRESHOLD)
            {
                _threshold += FADE_RATE;
                PlayerPrefs.SetFloat("_FadeThreshold", _threshold);
                yield return null;
            }

            isReady = true;
        }

        #endregion
    }
}
