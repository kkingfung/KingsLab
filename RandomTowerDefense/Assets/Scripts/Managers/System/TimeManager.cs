using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class TimeManager : MonoBehaviour
{
	public readonly float daytimeFactor = 120f;

	public float timeFactor = 0.05f;
	public float timeLength = 0.02f;

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
		if (isControl) {
			AdjustTime();
		}
	}

}
