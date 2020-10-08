using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GyroscopeManager : MonoBehaviour
{
    readonly Vector2 shakeThreshold = new Vector2(20f,10f); // horizontal, vertical
    readonly float timeInterval = 1f;

    public bool isFunctioning = false;

    //For Slightly Changes
    [Range(0,1)]
    float sensitivity=1f;
    public List<Slider> senseSlider;

    //For SuddenChange
    [HideInInspector]
    public bool LeftShake = false;
    [HideInInspector]
    public bool RightShake = false;
    [HideInInspector]
    public bool VerticalShake = false;

    Vector3 ReferenceCenter;
    Vector3 lastRotation;
    Vector3 currRotation;
    float timeRecord;

    Gyroscope Gyro;

    private void OnEnable()
    {
        sensitivity = PlayerPrefs.GetFloat("Gyro");
        Gyro = Input.gyro;
        Gyro.enabled = true;
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat("Gyro", sensitivity);
        Gyro.enabled = false;
    }

    private void Start()
    {
        ResetReference();
        foreach (Slider i in senseSlider)
            i.value = sensitivity;
        timeRecord = Time.time;
    }

    private void Update()
    {
        if (isFunctioning)
        {
            GyroModify();
        }
    }

    public void SetGyro(float value) {
        sensitivity = value;
        foreach (Slider i in senseSlider)
            sensitivity = i.value;
    }

    void GyroModify()
    {
        currRotation = GyroToUnity(Input.gyro.attitude).eulerAngles;
        //Reset Shake
        LeftShake = false;
        RightShake = false;
        VerticalShake = false;

        if (Time.time - timeRecord > timeInterval)
        {
            if (Input.deviceOrientation == DeviceOrientation.Portrait || Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown)
            {
                LeftShake = (currRotation.y - lastRotation.y > shakeThreshold.x * timeInterval);
                RightShake = (currRotation.y - lastRotation.y < -1f*shakeThreshold.x * timeInterval);
                VerticalShake = (Mathf.Abs(currRotation.x - lastRotation.x) > shakeThreshold.y * timeInterval);
            }
            else
            {
                LeftShake = (currRotation.x - lastRotation.x > shakeThreshold.x * timeInterval);
                RightShake = (currRotation.x - lastRotation.x < -1f * shakeThreshold.x * timeInterval);
                VerticalShake = (Mathf.Abs(currRotation.y - lastRotation.y) > shakeThreshold.y * timeInterval);
            }
            timeRecord = Time.time;
            lastRotation = new Vector3(currRotation.x, currRotation.y, currRotation.z);
        }
    }

    private static Quaternion GyroToUnity(Quaternion q)
    {
        return new Quaternion(-q.x, -q.z, -q.y, q.w) * Quaternion.Euler(90, 0, 0);
        //return new Quaternion(q.x, q.y, -q.z, -q.w);
    }

    public void ResetReference()
    {
        ReferenceCenter= currRotation;
    }

    public Vector3 CurrGyroRotation() 
    {
        Vector3 rot = (currRotation - ReferenceCenter)*(sensitivity+0.5f);
        Mathf.Clamp(rot.x, -45.0f, 45.0f);
        Mathf.Clamp(rot.y, -45.0f, 45.0f);

        rot.z = 0;
        return rot;
    }
}
