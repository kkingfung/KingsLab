using System;
using System.Collections.Generic;
using UnityEngine;

public class MyMobileInput
{
    public readonly int maxTouch = 2;
    public Touch[] TouchInfo; //0:first finger 1:second fingers
    public bool[] isTouch; //0:one finger 1:two fingers

    public MyMobileInput() {
        TouchInfo = new Touch[2];
        isTouch = new bool[2];
    }
};

[System.Serializable]
public class InputManager : MonoBehaviour
{
    readonly float tapStayTime = 2f;

    //Input Test
    public LBM LBMTest;

    //Mobile Input
    bool isAndroid;
    bool isiOS;
    MyMobileInput mobileInput;
    bool useTouch;

    //Common Variables
    PlayerManager playerManager;
    CameraManager cameraManager;
    float timeRecord;
    [HideInInspector]
    public float zoomFactor;

    void Awake()
    {
        mobileInput = new MyMobileInput();

        //Checking Platform
        isAndroid = (Application.platform == RuntimePlatform.Android);
        isiOS = (Application.platform == RuntimePlatform.IPhonePlayer);
    }

    void Start()
    {
        zoomFactor = 1f;
        useTouch = isAndroid || isiOS;
        playerManager = FindObjectOfType<PlayerManager>();
        cameraManager = FindObjectOfType<CameraManager>();
    }

    void Update()
    {
        if (useTouch) UpdateTouchInfo();
        if (LBMTest) LBMTesting(); //Only for Loading Scene
    }

    void UpdateTouchInfo()
    {
        int TouchCount = Input.touchCount;
        if (TouchCount == 0) return;

        //reset Boolean
        mobileInput.isTouch[0] = false;
        mobileInput.isTouch[1] = false;

        for (int touchId = 0; touchId < Math.Min(TouchCount,mobileInput.maxTouch); ++touchId)
        {
            Touch touch = Input.GetTouch(touchId);
            mobileInput.TouchInfo[touchId] = touch;
            mobileInput.isTouch[touchId] = true;
            if (Time.time - timeRecord > tapStayTime)
                playerManager.isSkillActive = true;

            switch (touch.phase)
            {
                //TODO: Use Skills / Sell Tower
                case TouchPhase.Began:
                    if (touchId == 0) timeRecord = Time.time;
                    break;
                case TouchPhase.Moved:
                    if (TouchCount==2) HandleZoom();
                    break;
                case TouchPhase.Ended:
                    if (touchId == 0) {
                        if (playerManager && playerManager.isSkillActive)
                            playerManager.CheckSkill(touch.position); 
                    }
                    break;
                case TouchPhase.Stationary:
                    break;
            }
        }
    }

    void HandleZoom()
    {
        if (cameraManager == null) return;

        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

        float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
        float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

        float difference = currentMagnitude - prevMagnitude;
        zoomFactor -= difference * 0.01f;
        zoomFactor = Mathf.Clamp(zoomFactor, 0.5f, 1.5f);
        cameraManager.Zoom(zoomFactor);
    }

    void LBMTesting()
    {
        if (LBMTest == null) return;
        
        if (useTouch) {
            LBMTest.Interaction(mobileInput.isTouch[0], !mobileInput.isTouch[0], mobileInput.TouchInfo[0].position, true);
            LBMTest.Interaction(mobileInput.isTouch[1], !mobileInput.isTouch[1], mobileInput.TouchInfo[1].position, true);
        }
        else 
            LBMTest.Interaction(Input.GetMouseButton(0), !Input.GetMouseButton(0),Input.mousePosition,true);
    }

    public bool GetAnyInput() {
        return mobileInput.isTouch[0] || Input.GetMouseButton(0);
    }
}
