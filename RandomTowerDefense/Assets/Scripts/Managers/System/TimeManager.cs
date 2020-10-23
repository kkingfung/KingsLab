using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class TimeManager : MonoBehaviour
{
	public readonly float daytimeFactor = 120f;

	public float timeFactor = 0.05f;
	public float timeLength = 2f;

	private float OriTimeScale;
	private float OriFixedTimeScale;
	private bool isControl = false;
	void Start() {
		Time.timeScale = 3.0f;
		OriTimeScale = Time.timeScale;
		OriFixedTimeScale = Time.fixedDeltaTime;
	}
	void Update()
	{
		if (isControl) return;
		if (Time.timeScale < OriTimeScale)
		{
			Time.timeScale += (1f / timeLength) * Time.unscaledDeltaTime;
			Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, OriTimeScale);
		}
		if (Time.fixedDeltaTime < OriFixedTimeScale)
		{
			Time.fixedDeltaTime += (1f / timeLength) * Time.unscaledDeltaTime;
			Time.fixedDeltaTime = Mathf.Clamp(Time.fixedDeltaTime, 0f, OriFixedTimeScale);
		}
	}

	public void AdjustTime()
	{
		Time.timeScale = timeFactor;
		Time.fixedDeltaTime = Time.timeScale * .02f;
	}

	public void TimeControl() 
	{
		isControl = !isControl;
		if (isControl) {
			AdjustTime();
		}
	}

}
