using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[SerializeField]
public class TimeManager : MonoBehaviour
{
	/// <summary>
	/// 時間調整時のタイムスケール係数
	/// </summary>
	public float timeFactor = 0.05f;
	/// <summary>
	/// 時間調整時の固定タイムステップ長
	/// </summary>
	public float timeLength = 0.02f;

	/// <summary>
	/// タイムスケール表示用のテキストUIリスト
	/// </summary>
	public List<Text> text;

	private float OriTimeScale;
	private float OriFixedTimeScale;

	private float[] timeScaleFactor = { 1f, 2f, 4f };
	private int[] timeScaleShow = { 1, 2, 3 };
	private int timeScaleId;

	private bool isControl = false;

	/// <summary>
	/// ゲームシーン管理クラスの参照
	/// </summary>
	public InGameOperation sceneManager;
	/// <summary>
	/// 入力管理クラスの参照
	/// </summary>
	public InputManager inputManager;
	void Start()
	{
		timeScaleId = 0;
		Time.timeScale = 1.0f;
		OriTimeScale = Time.timeScale;
		OriFixedTimeScale = Time.fixedDeltaTime;

		foreach (Text i in text)
			i.text = "X" + (int)Time.timeScale;

		//sceneManager = FindObjectOfType<InGameOperation>();
		//inputManager = FindObjectOfType<InputManager>();
	}
	/// <summary>
	/// 毎フレーム呼び出され、タイムスケールを原状に戻す
	/// </summary>
	void Update()
	{
		if (isControl) return;
		Time.timeScale = OriTimeScale;
		Time.fixedDeltaTime = OriFixedTimeScale;
	}

	/// <summary>
	/// ゲームの時間スケールと固定タイムステップを調整
	/// </summary>
	public void AdjustTime()
	{
		Time.timeScale = timeFactor;
		Time.fixedDeltaTime = Time.timeScale * timeLength;
	}

	/// <summary>
	/// 時間制御のオンオフを切り替え
	/// </summary>
	public void TimeControl()
	{
		isControl = !isControl;
		if (isControl)
		{
			AdjustTime();
		}
	}

	/// <summary>
	/// タイムスケールIDを指定した値だけ変更
	/// </summary>
	/// <param name="chg">変更する値（デフォルト: 1）</param>
	public void ChgTimeScale(int chg = 1)
	{
		if (sceneManager && sceneManager.GetOptionStatus()) return;
		SetTimeScale(timeScaleId + chg);
		inputManager.TapTimeRecord = 0;
	}
	/// <summary>
	/// タイムスケールIDを指定した値に設定
	/// </summary>
	/// <param name="target">設定するタイムスケールID</param>
	public void SetTimeScale(int target)
	{
		timeScaleId = target;
		timeScaleId %= timeScaleFactor.Length;
		Time.timeScale = timeScaleFactor[timeScaleId];
		OriTimeScale = Time.timeScale;
		foreach (Text i in text)
			i.text = "X" + timeScaleShow[timeScaleId];
	}
	public bool GetControl() { return isControl; }
}
