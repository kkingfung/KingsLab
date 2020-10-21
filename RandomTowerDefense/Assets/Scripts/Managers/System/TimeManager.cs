using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class TimeManager : MonoBehaviour
{
	public readonly float daytimeFactor = 120f;

	public float timeFactor = 0.05f;
	public float timeLength = 2f;

	bool isControl = false;
	void Update()
	{
		if (isControl) return;
		Time.timeScale += (1f / timeLength) * Time.unscaledDeltaTime;
		Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 3f);
	}

	public void AdjustTime()
	{
		Time.timeScale = timeFactor;
		Time.fixedDeltaTime = Time.timeScale * .02f;
	}

	public void TimeControl() 
	{
		isControl = !isControl;
	}

}
