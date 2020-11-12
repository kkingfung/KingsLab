using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[SerializeField]
public class TimeManager : MonoBehaviour
{
	public float timeFactor = 0.05f;
	public float timeLength = 0.02f;

	public List<Text> text;

	private float OriTimeScale;
	private float OriFixedTimeScale;

	private int[] timeScaleFactor = { 1, 2, 3 };
	private int timeScaleId;

	private bool isControl = false;

	private InGameOperation sceneManager;
	private InputManager inputManager;
	void Start()
	{
		timeScaleId = 0;
		Time.timeScale = 1.0f;
		OriTimeScale = Time.timeScale;
		OriFixedTimeScale = Time.fixedDeltaTime;

		foreach (Text i in text)
			i.text = "X" + (int)Time.timeScale;

		sceneManager = FindObjectOfType<InGameOperation>();
		inputManager = FindObjectOfType<InputManager>();
	}
	void Update()
	{
		if (isControl) return;
		Time.timeScale = OriTimeScale;
		Time.fixedDeltaTime = OriFixedTimeScale;
	}

	public void AdjustTime()
	{
		Time.timeScale = timeFactor;
		Time.fixedDeltaTime = Time.timeScale * .02f;
	}

	public void TimeControl()
	{
		isControl = !isControl;
		if (isControl)
		{
			AdjustTime();
		}
	}

	public void ChgTimeScale(int chg = 1)
	{
		if (sceneManager && sceneManager.GetOptionStatus()) return;
		SetTimeScale(timeScaleId + chg);
		inputManager.TapTimeRecord = 0;
	}
	public void SetTimeScale(int target)
	{
		timeScaleId = target;
		timeScaleId %= timeScaleFactor.Length;
		Time.timeScale = timeScaleFactor[timeScaleId];
		OriTimeScale = Time.timeScale;
		foreach (Text i in text)
			i.text = "X" + (int)Time.timeScale;
	}
	public bool GetControl() { return isControl; }
}
