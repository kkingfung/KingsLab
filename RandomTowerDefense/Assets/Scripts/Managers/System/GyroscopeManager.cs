using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GyroscopeManager : MonoBehaviour
{
    readonly Vector2 shakeThreshold = new Vector2(5,8); // horizontal,vertical
    readonly float timeInterval = 2f;

    public bool isFunctioning;
    public List<GameObject> gyroDiffUI;

    //For Slightly Changes
    [Range(0,1)]
    float sensitivity=0f;
    public List<Slider> senseSlider;

    //For SuddenChange
    [HideInInspector]
    public bool LeftShake = false;
    [HideInInspector]
    public bool RightShake = false;
    [HideInInspector]
    public bool VerticalShake = false;

    private Gyroscope Gyro;

   // private float roll;//z axis
    private float pitch;//x axis
    private float yaw;//y axis

    //private float rollRef;//z axis
    private float pitchRef;//x axis
    private float yawRef;//y axis

    private float timeRecord;

    private CameraManager cameraManager;
    private void Awake()
    {
        Gyro = Input.gyro;
        Gyro.enabled = true;

        //For Locking Screen Orientation (Spare)
        //Screen.orientation = ScreenOrientation.LandscapeLeft;
        //Portrait//PortraitUpsideDown//LandscapeRight//AutoRotation
    }

    private void Start()
    {
        isFunctioning = FindObjectOfType<InputManager>().GetUseTouch();
        sensitivity = PlayerPrefs.GetFloat("Gyro");
        foreach (Slider i in senseSlider)
            i.value = sensitivity;

        timeRecord = 0;

        //roll = 0;
        yaw = 0;
        pitch = 0;

        cameraManager = FindObjectOfType<CameraManager>();
        ResetReference();
    }

    private void Update()
    {
        if (isFunctioning)
        {
            UpdateGyroYPR();
            GyroModify();
    
            if (gyroDiffUI.Count > 0)
            {
                foreach (GameObject i in gyroDiffUI)
                {
                    switch (Input.deviceOrientation)
                    {
                        case DeviceOrientation.Portrait:
                            i.transform.localEulerAngles = new Vector3(0,0, yawRef);
                            break;
                        case DeviceOrientation.PortraitUpsideDown:
                            i.transform.localEulerAngles = new Vector3(0, 0, -yawRef);
                            break;
                        case DeviceOrientation.LandscapeLeft:
                            i.transform.localEulerAngles = new Vector3(0, 0, pitchRef);
                            break;
                        case DeviceOrientation.LandscapeRight:
                            i.transform.localEulerAngles = new Vector3(0, 0, -pitchRef);
                            break;
                    }
                }
            }
        }
    }

    public void SetGyro(float value) {
        sensitivity = value;
        foreach (Slider i in senseSlider)
            i.value = sensitivity;
        PlayerPrefs.SetFloat("Gyro", sensitivity);
    }

    public void GyroModify()
    {
        //Reset Shake
        LeftShake = false;
        RightShake = false;
        VerticalShake = false;

        Vector3 rotRate = Input.gyro.rotationRate;

        switch (Input.deviceOrientation)
        {
            case DeviceOrientation.Portrait:
                if (rotRate.x > shakeThreshold.x || rotRate.x < shakeThreshold.x)
                    VerticalShake = true;
                if (rotRate.y < -shakeThreshold.y)
                    LeftShake = true;
                if (rotRate.y > shakeThreshold.y)
                    RightShake = true;
                break;
            case DeviceOrientation.PortraitUpsideDown:
                if (rotRate.x > shakeThreshold.x || rotRate.x < shakeThreshold.x)
                    VerticalShake = true;
                if (rotRate.y < -shakeThreshold.y)
                    RightShake = true;
                if (rotRate.y > shakeThreshold.y)
                    LeftShake = true;
                break;
            case DeviceOrientation.LandscapeLeft:
                if (rotRate.x > shakeThreshold.y || rotRate.x < shakeThreshold.y)
                    VerticalShake = true;
                if (rotRate.y < -shakeThreshold.x)
                    LeftShake = true;
                if (rotRate.y > shakeThreshold.x)
                    RightShake = true;
                break;
            case DeviceOrientation.LandscapeRight:
                if (rotRate.x > shakeThreshold.y || rotRate.x < shakeThreshold.y)
                    VerticalShake = true;
                if (rotRate.y < -shakeThreshold.x)
                    LeftShake = true;
                if (rotRate.y > shakeThreshold.x)
                    RightShake = true;
                break;
        }

        if (VerticalShake && Time.time - timeRecord > timeInterval)
        {
            timeRecord = Time.time;
            LeftShake = false;
            RightShake = false;
        }

        if (LeftShake && Time.time - timeRecord > timeInterval)
        {
            timeRecord = Time.time;
            VerticalShake = false;
            RightShake = false;
        }


        if (RightShake && Time.time - timeRecord > timeInterval)
        {
            timeRecord = Time.time;
            VerticalShake = false;
            LeftShake = false;
        }
    }

    private static Quaternion GyroToUnity(Quaternion q)
    {
        return new Quaternion(-q.x, -q.z, -q.y, q.w) * Quaternion.Euler(90, 0, 0);
        //return new Quaternion(q.x, q.y, -q.z, -q.w);
    }

    public void ResetReference()
    {
        if (FindObjectOfType<ISceneChange>().GetOptionStatus()) return;
        cameraManager.ResetGyroCam();

        //rollRef = 0;
        yawRef = 0;
        pitchRef = 0;
    }

    public float GetWorldYaw() {
        return -yaw;
    }

    public float GetLocalPitch()
    {
        return -pitch;
    }
    public void UpdateGyroYPR() 
    {
        pitch = Input.gyro.rotationRateUnbiased.x * Mathf.Rad2Deg*0.05f*sensitivity;
        yaw = Input.gyro.rotationRateUnbiased.y * Mathf.Rad2Deg * 0.05f * sensitivity;
        //roll = Input.gyro.rotationRateUnbiased.z * Mathf.Rad2De*0.05f*sensitivity;

        //rollRef += roll;
        yawRef += yaw;
        pitchRef += pitch;

    }
}
