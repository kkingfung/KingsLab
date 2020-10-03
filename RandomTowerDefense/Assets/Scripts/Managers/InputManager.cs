using System;
using System.Collections.Generic;
using UnityEngine;

public class MyMobileInput
{
    public int maxTouch = 2;
    public Vector2[] TouchPos; //0:first finger 1:second fingers
    public bool[] isTouch; //0:one finger 1:two fingers
    public bool certifyTouch;
};

[System.Serializable]
public class InputManager : MonoBehaviour
{
    private const int waitTime = 500;

    //Input Type
    public bool useTouch;

    bool isAndroid;
    bool isiOS;

    //Mobile Input
    MyMobileInput mobileInput;

    //Input Setting
    public float mouseSensitivity = 10;
    public Vector2 pitchMinMax = new Vector2(-40, 85);
    public float rotationSmoothTime = 0.1f;

    [HideInInspector]
    public float yaw;
    [HideInInspector]
    public float pitch;
    float smoothYaw;
    float smoothPitch;

    float yawSmoothV;
    float pitchSmoothV;

    Vector3 LookVertical;
    Vector3 LookHorizontal;

    //Non-input
    private bool isWaiting;
    private int waitCount = -1;
    public float distThreshold = 10;

    //Gyroscope
    private float RotateSpeed = 50.0f;
    private bool GyroEnabled;
    private Gyroscope Gyroscope;
    private GameObject CameraGyro;
    [HideInInspector]
    public Quaternion GyroRotation;
    [HideInInspector]
    public Quaternion OriGyroRotation;

    public LBM LBMTest = null;
    void Awake()
    {
        mobileInput = new MyMobileInput();

        isAndroid = (Application.platform == RuntimePlatform.Android);
        isiOS = (Application.platform == RuntimePlatform.IPhonePlayer);

        //Gyroscope Update
        GyroEnabled = (useTouch) && (isAndroid || isiOS) && SystemInfo.supportsGyroscope;
        if (GyroEnabled)
        {
            Gyroscope = Input.gyro;
            Gyroscope.enabled = true;
            GyroRotation = GyroToUnity(Input.gyro.attitude);
            ResetGyro();
        }
        CameraGyro = new GameObject("CameraGyro");
        CameraGyro.transform.position = transform.position;
        transform.SetParent(CameraGyro.transform);
    }

    void Start()
    {
        if (!(isAndroid || isiOS))
             mobileInput.certifyTouch = false;

        mobileInput.isTouch = new bool[2];
        waitCount = 0;
    }

    void Update()
    {
        //Gyroscope Update
        if (GyroEnabled)
        {
            //Seperating Rotate Axis
            CameraGyro.transform.Rotate(0.0f, -Input.gyro.rotationRateUnbiased.y * RotateSpeed * Time.deltaTime, 0.0f);
            transform.Rotate(-Input.gyro.rotationRateUnbiased.x * RotateSpeed * Time.deltaTime, 0.0f, 0.0f);
            GyroRotation = GyroToUnity(Input.gyro.attitude);
            OriGyroRotation = GyroToUnity(Input.gyro.attitude);
        }

        if (useTouch) UpdateTouchInfo();
        else UpdateMouseInfo();

        LBMTesting(); //Only for Loading Scene
    }

    void LateUpdate()
    {
        if (isWaiting) waitCount = Mathf.Min(waitTime, waitCount + 1);
        else waitCount = 0;
    }

    #region UpdateMobileInput
    void UpdateTouchInfo()
    {
        if (!(isAndroid || isiOS)) return;
        int TouchCount = Input.touchCount;
        if (TouchCount > 0)
        {
            mobileInput.certifyTouch = false;
            return;
        }
        else { mobileInput.certifyTouch = true; }

        for (int touchId = 0; touchId < TouchCount && touchId<mobileInput.maxTouch; ++touchId)
        {
            Touch touch = Input.GetTouch(touchId);
            switch (touch.phase)
            {
                //Customize if necessary
                //TODO: Use Skills / Sell Tower
                case TouchPhase.Began:
                case TouchPhase.Moved:
                case TouchPhase.Ended:
                case TouchPhase.Stationary:
                    // Record touch position.
                    mobileInput.TouchPos[touchId] = touch.position;
                    mobileInput.isTouch[touchId] = true;
                    break;
            }
        }
    }
    static Quaternion GyroToUnity(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }
    #endregion

    #region UpdatePCInput
    void UpdateMouseInfo()
    {
        // Look input
        yaw += Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch - Input.GetAxisRaw("Mouse Y") * mouseSensitivity, pitchMinMax.x, pitchMinMax.y);
        smoothPitch = Mathf.SmoothDampAngle(smoothPitch, pitch, ref pitchSmoothV, rotationSmoothTime);
        float smoothYawOld = smoothYaw;
        smoothYaw = Mathf.SmoothDampAngle(smoothYaw, yaw, ref yawSmoothV, rotationSmoothTime);
        LookVertical = Vector3.right * smoothPitch;
        LookHorizontal = Vector3.up * Mathf.DeltaAngle(smoothYawOld, smoothYaw);

    }
    void UpdateKeyInfo()
    {
        //TODO: for simulating Gyroscope
    }
    #endregion

    public void ResetGyro()
    {
        OriGyroRotation = GyroRotation;
    }
    public bool RequestPause() { return waitCount >= waitTime; }

    void LBMTesting()
    {
        if (LBMTest == null) return;
        
        if (useTouch) {
            LBMTest.Interaction(mobileInput.isTouch[0], !mobileInput.isTouch[0], mobileInput.TouchPos[0], true);
            LBMTest.Interaction(mobileInput.isTouch[1], !mobileInput.isTouch[1], mobileInput.TouchPos[1], true);
        }
        else 
            LBMTest.Interaction(Input.GetMouseButton(0), !Input.GetMouseButton(0),Input.mousePosition,true);
    }

    public bool GetAnyInput() {
        return mobileInput.isTouch[0] || Input.GetMouseButton(0);
    }
}
