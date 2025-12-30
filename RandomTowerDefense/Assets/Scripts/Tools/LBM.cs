using UnityEngine;
using System.Collections;

namespace RandomTowerDefense.Tools
{
	/// <summary>
	/// LBM (Lattice Boltzmann Method) - GPUベース流体シミュレーションシステム
	///
	/// 主な機能:
	/// - リアルタイム2D流体シミュレーション（格子ボルツマン法）
	/// - GPUコンピュートシェーダーによる並列計算
	/// - マウス/タッチインタラクション対応
	/// - 境界反射とマスキングシステム
	/// - マルチパスレンダリングパイプライン
	/// - ランドスケープ/ポートレートカメラ対応
	/// </summary>
	public class LBM : MonoBehaviour
	{
		#region Shader References

		/// <summary>初期化シェーダー</summary>
		[SerializeField] private Shader initShader = null;
		/// <summary>計算シェーダー</summary>
		[SerializeField] private Shader compShader = null;
		/// <summary>ステップシェーダー</summary>
		[SerializeField] private Shader stepShader = null;
		/// <summary>コピーシェーダー</summary>
		[SerializeField] private Shader copyShader = null;
		/// <summary>境界反射シェーダー</summary>
		[SerializeField] private Shader bounceBackShader = null;
		/// <summary>表示シェーダー</summary>
		[SerializeField] private Shader showShader = null;
		/// <summary>ペイントシェーダー</summary>
		[SerializeField] private Shader paintShader = null;

		#endregion

		#region Materials

		private Material initMat;
		private Material compMat;
		private Material stepMat;
		private Material copyMat;
		private Material bounceBackMat;
		private Material paintMat;
		private Material showMat;

		#endregion

		#region Render Textures

		private RenderTexture rt1;
		private RenderTexture rt2;
		private RenderTexture rt3;
		private RenderTexture rt1c;
		private RenderTexture rt2c;
		private RenderTexture rt3c;
		private RenderTexture rtMask;

		#endregion

		#region Public Fields

		/// <summary>
		/// ランドスケープカメラ参照
		/// </summary>
		public Camera LandscapeCam;

		/// <summary>
		/// ポートレートカメラ参照
		/// </summary>
		public Camera ProtraitCam;

		#endregion

		#region Private Fields

		private bool isDragging = false;
		private Vector2 interactPos;

		#endregion

		/// <summary>
		/// 初期化処理 - インタラクション位置の初期設定
		/// </summary>
		private void Awake()
		{
			interactPos = Input.mousePosition;
		}

		/// <summary>
		/// 開始時処理 - レンダーテクスチャとマテリアルのセットアップ
		/// </summary>
		private void Start()
		{
			SetupRTs();
			SetupMats();

			Graphics.Blit(null, rtMask, initMat);
			Graphics.Blit(null, rt1, initMat);
			Graphics.Blit(null, rt2, initMat);
			Graphics.Blit(null, rt3, initMat);
		}

		/// <summary>
		/// 毎フレーム更新 - LBMシミュレーションステップの実行
		/// </summary>
		private void Update()
		{
			if (isDragging)
			{
				Graphics.Blit(null, rtMask, initMat);
				paintMat.SetFloat("_X", interactPos.x);
				paintMat.SetFloat("_Y", interactPos.y);
				Graphics.Blit(null, rtMask, paintMat);
			}

			Graphics.Blit(null, rt1c, compMat, 0);
			Graphics.Blit(null, rt2c, compMat, 1);
			Graphics.Blit(null, rt3c, compMat, 2);

			Graphics.Blit(null, rt1, copyMat, 0);
			Graphics.Blit(null, rt2, copyMat, 1);
			Graphics.Blit(null, rt3, copyMat, 2);

			Graphics.Blit(null, rt1c, bounceBackMat, 0);
			Graphics.Blit(null, rt2c, bounceBackMat, 1);
			Graphics.Blit(null, rt3c, bounceBackMat, 2);

			Graphics.Blit(null, rt1, copyMat, 0);
			Graphics.Blit(null, rt2, copyMat, 1);
			Graphics.Blit(null, rt3, copyMat, 2);

			Graphics.Blit(null, rt1c, stepMat, 0);
			Graphics.Blit(null, rt1, copyMat, 0);

			Graphics.Blit(null, rt2c, stepMat, 1);
			Graphics.Blit(null, rt2, copyMat, 1);

			Graphics.Blit(null, rt3c, stepMat, 2);
			Graphics.Blit(null, rt3, copyMat, 2);

			compMat.SetFloat("_T", Time.timeSinceLevelLoad);
			stepMat.SetFloat("_T", Time.timeSinceLevelLoad);
		}

		/// <summary>
		/// GUI描画 - シミュレーション結果の画面表示
		/// </summary>
		private void OnGUI()
		{
			var tex = new Texture2D(1, 1);
			tex.SetPixel(0, 0, Color.white);
			Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), tex, showMat);
		}

		/// <summary>
		/// 破棄時処理 - レンダーテクスチャのリソース解放
		/// </summary>
		private void OnDestroy()
		{
			rt1.Release();
			rt2.Release();
			rt3.Release();
			rt1c.Release();
			rt2c.Release();
			rt3c.Release();
			rtMask.Release();
		}

		/// <summary>
		/// レンダーテクスチャセットアップ - シミュレーション用RTの作成
		/// </summary>
		private void SetupRTs()
		{
			rt1 = CreateRenderTexture(Screen.width, Screen.height);
			rt2 = CreateRenderTexture(Screen.width, Screen.height);
			rt3 = CreateRenderTexture(Screen.width, Screen.height);
			rt1c = CreateRenderTexture(Screen.width, Screen.height);
			rt2c = CreateRenderTexture(Screen.width, Screen.height);
			rt3c = CreateRenderTexture(Screen.width, Screen.height);
			rtMask = CreateRenderTexture(Screen.width, Screen.height);
		}

		/// <summary>
		/// マテリアルセットアップ - シェーダーからマテリアルを作成し、テクスチャを設定
		/// </summary>
		private void SetupMats()
		{
			initMat = new Material(initShader);
			copyMat = new Material(copyShader);
			compMat = new Material(compShader);
			stepMat = new Material(stepShader);
			bounceBackMat = new Material(bounceBackShader);
			showMat = new Material(showShader);
			paintMat = new Material(paintShader);

			copyMat.SetTexture("_Diffuse1", rt1c);
			copyMat.SetTexture("_Diffuse2", rt2c);
			copyMat.SetTexture("_Diffuse3", rt3c);

			compMat.SetTexture("_Diffuse1", rt1);
			compMat.SetTexture("_Diffuse2", rt2);
			compMat.SetTexture("_Diffuse3", rt3);
			compMat.SetFloat("_Dx", 1f / Screen.width);
			compMat.SetFloat("_Dy", 1f / Screen.height);

			bounceBackMat.SetTexture("_Diffuse1", rt1);
			bounceBackMat.SetTexture("_Diffuse2", rt2);
			bounceBackMat.SetTexture("_Diffuse3", rt3);
			bounceBackMat.SetTexture("_Mask", rtMask);

			stepMat.SetTexture("_Diffuse1", rt1);
			stepMat.SetTexture("_Diffuse2", rt2);
			stepMat.SetTexture("_Diffuse3", rt3);
			stepMat.SetFloat("_Dx", 1f / Screen.width);
			stepMat.SetFloat("_Dy", 1f / Screen.height);

			showMat.SetTexture("_Diffuse1", rt1c);
			showMat.SetTexture("_Diffuse2", rt2c);
			showMat.SetTexture("_Diffuse3", rt3c);
		}

		/// <summary>
		/// レンダーテクスチャ作成 - 指定サイズのレンダーテクスチャを作成
		/// </summary>
		/// <param name="width">幅</param>
		/// <param name="height">高さ</param>
		/// <returns>作成されたレンダーテクスチャ</returns>
		private RenderTexture CreateRenderTexture(int width, int height)
		{
			RenderTexture rt = new RenderTexture(width, height, 0);
			rt.format = RenderTextureFormat.ARGBFloat;
			rt.wrapMode = TextureWrapMode.Repeat;
			rt.filterMode = FilterMode.Point;

			rt.Create();
			return rt;
		}

		/// <summary>
		/// インタラクション処理 - マウス/タッチによる流体シミュレーションへの入力
		/// </summary>
		/// <param name="isButtonDown">ボタン押下フラグ</param>
		/// <param name="isButtonUp">ボタン解放フラグ</param>
		/// <param name="Pos">入力位置</param>
		/// <param name="isScreenPos">スクリーン座標フラグ（デフォルト: false）</param>
		public void Interaction(bool isButtonDown, bool isButtonUp, Vector3 Pos, bool isScreenPos = false)
		{
			if (isScreenPos)
			{
				interactPos.x = Pos.x / Screen.width;
				interactPos.y = Pos.y / Screen.height;
			}
			else
			{
				if (Screen.width > Screen.height)
				{
					interactPos.x = LandscapeCam.WorldToScreenPoint(Pos).x;
					interactPos.y = LandscapeCam.WorldToScreenPoint(Pos).y;
				}
				else
				{
					interactPos.x = ProtraitCam.WorldToScreenPoint(Pos).x;
					interactPos.y = ProtraitCam.WorldToScreenPoint(Pos).y;
				}
			}

			if (isButtonDown)
			{
				isDragging = true;
			}
			else if (isDragging && isButtonUp)
			{
				Graphics.Blit(null, rtMask, initMat);
				isDragging = false;
			}
		}
	}
}
