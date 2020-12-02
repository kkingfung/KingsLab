using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GyroscopeManager : MonoBehaviour
{
    readonly Vector2 shakeThreshold = new Vector2(5, 3); // horizontal
    readonly Vector2 shakeThresholdV = new Vector2(5, 5); // vertical
    readonly float timeInterval = 2f;
    readonly float sensitiveAdjustment = 0.5f;

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

    public InputManager inputManager;
    public CameraManager cameraManager;
    public ISceneChange sceneManager;
    private bool isActive;
    private void Awake()
    {
        Gyro = Input.gyro;
        Gyro.enabled = true;
        isActive = true;
        //For Locking Screen Orientation (Spare)
        //Screen.orientation = ScreenOrientation.LandscapeLeft;
        //Portrait//PortraitUpsideDown//LandscapeRight//AutoRotation
    }

    private void Start()
    {
        isFunctioning = inputManager.GetUseTouch();
        sensitivity = PlayerPrefs.GetFloat("Gyro",0);
        foreach (Slider i in senseSlider)
            i.value = sensitivity;

        timeRecord = 0;

        //roll = 0;
        yaw = 0;
        pitch = 0;

        cameraManager = FindObjectOfType<CameraManager>();
        sceneManager = FindObjectOfType<ISceneChange>();
        ResetReference();
    }

    private void Update()
    {
        if (isFunctioning && isActive)
        {
            UpdateGyroYPR();
            GyroModify();

            if (gyroDiffUI.Count > 0)
            {
                foreach (GameObject i in gyroDiffUI)
                {
                    i.transform.localEulerAngles = new Vector3(0, 0, yawRef);
                }
            }
        }

        if (gyroDiffUI.Count > 0)
        {
            foreach (GameObject i in gyroDiffUI)
            {
                i.transform.localEulerAngles = new Vector3(0, 0, yawRef);
            }
        }
    }

    public void setTempInactive() {
        isActive = false;
        StartCoroutine(WaitToResume());
    }

    private IEnumerator WaitToResume()
    {
        float timeRecord = 0;
        while (timeRecord < 0.2f)
        {
            timeRecord += Time.deltaTime;
            yaw = 0;
            pitch = 0;
            ResetReference();
            yield return new WaitForSeconds(0);
        }

        isActive = true;
    }
    public void SetGyro(float value) {
        sensitivity = value;
        foreach (Slider i in senseSlider)
            i.value = sensitivity;
        PlayerPrefs.SetFloat("Gyro", sensitivity);
    }

    public void SetYawChg(float value)
    {
        yawRef += value;
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
                if (rotRate.x > shakeThresholdV.x || rotRate.x < -shakeThresholdV.x)
                    VerticalShake = true;
                if (rotRate.y < -shakeThreshold.y)
                    LeftShake = true;
                if (rotRate.y > shakeThreshold.y)
                    RightShake = true;
                break;
            case DeviceOrientation.PortraitUpsideDown:
                if (rotRate.x > shakeThresholdV.x || rotRate.x < -shakeThresholdV.x)
                    VerticalShake = true;
                if (rotRate.y < -shakeThreshold.y)
                    RightShake = true;
                if (rotRate.y > shakeThreshold.y)
                    LeftShake = true;
                break;
            case DeviceOrientation.LandscapeLeft:
                if (rotRate.x > shakeThresholdV.y || rotRate.x < -shakeThresholdV.y)
                    VerticalShake = true;
                if (rotRate.y < -shakeThreshold.x)
                    LeftShake = true;
                if (rotRate.y > shakeThreshold.x)
                    RightShake = true;
                break;
            case DeviceOrientation.LandscapeRight:
                if (rotRate.x > shakeThresholdV.y || rotRate.x < -shakeThresholdV.y)
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
        if (sceneManager.GetOptionStatus()) return;
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
        pitch = Input.gyro.rotationRateUnbiased.x * Mathf.Rad2Deg * sensitiveAdjustment * sensitivity * Time.deltaTime;
        yaw = Input.gyro.rotationRateUnbiased.y * Mathf.Rad2Deg * sensitiveAdjustment * sensitivity * Time.deltaTime;
        //roll = Input.gyro.rotationRateUnbiased.z * Mathf.Rad2Deg*sensitiveAdjustment*sensitivity * Time.deltaTime;

        //rollRef += roll;
        yawRef += yaw;
        pitchRef += pitch;

    }
}
