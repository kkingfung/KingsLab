using System;
using System.Collections.Generic;
using Unity.Entities;
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
    readonly float tapDoubleTime = 0.5f;
    readonly float dragDiff = 5.0f;//cooperate with Scene Script(toDrag)

    public GameObject ClickPrefab;
    //TouchButton
    public List<GameObject> Buttons;

    //Input Test
    public LBM LBMTest;
    public bool ApplyClickEfc;

    //Mobile Input
    private bool isAndroid;
    private bool isiOS;
    private MyMobileInput mobileInput;
    private bool useTouch;
    private bool isDragging;
    private Vector2 posDragging;

    //Common Variables
    private PlayerManager playerManager;
    private CameraManager cameraManager;
    private InGameOperation sceneManager;

    private float DragTimeRecord;
    private float TapTimeRecord;

    public Camera refCamL;
    public Camera refCamP;
    public Camera refCamLSub;
    public Camera refCamPSub;

    private void Awake()
    {
        mobileInput = new MyMobileInput();

        //Checking Platform
        isAndroid = (Application.platform == RuntimePlatform.Android);
        isiOS = (Application.platform == RuntimePlatform.IPhonePlayer);
    }

    private void Start()
    {
        isDragging = false;
        useTouch = isAndroid || isiOS;
        playerManager = FindObjectOfType<PlayerManager>();
        cameraManager = FindObjectOfType<CameraManager>();
        sceneManager = FindObjectOfType<InGameOperation>();
    }

    private void Update()
    {
        if (useTouch) UpdateTouchInfo();
        else UpdateMouseInfo();
        if (LBMTest) LBMTesting(); //Only for Loading Scene
        if (Buttons.Count > 0) RaycastTest();

        //ClickEffect
        if (ApplyClickEfc && ClickPrefab != null)
        {
            if (refCamL != null)
                ClickEffect((Screen.width > Screen.height) ? refCamL : refCamP);
            if (refCamLSub != null)
                ClickEffect((Screen.width > Screen.height) ? refCamLSub : refCamPSub);
        }
    }

    private void UpdateMouseInfo()
    {
        if (isDragging)
        {
            if (Input.mousePosition.x - posDragging.x > dragDiff)
            {
                FindObjectOfType<ISceneChange>().toDrag = 2;
                isDragging = false;
            }
            if (Input.mousePosition.x - posDragging.x < -dragDiff)
            {
                FindObjectOfType<ISceneChange>().toDrag = 1;
                isDragging = false;
            }
        }
    }

    private void ArenaActions(int TouchCount) {
        //For Arena Scene/Screen Only
        if (sceneManager.currScreenShown != 0) return;
            for (int touchId = 0; touchId < Math.Min(TouchCount, mobileInput.maxTouch); ++touchId)
            {
                Touch touch = Input.GetTouch(touchId);
                mobileInput.TouchInfo[touchId] = touch;
                mobileInput.isTouch[touchId] = true;
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        if (touchId == 0)
                        {
                            DragTimeRecord = Time.time;
                            if (playerManager && playerManager.isSkillActive == false)
                            {
                                BeginDrag();
                                playerManager.CheckStock(posDragging);
                            }
                        }
                        break;
                    case TouchPhase.Moved:
                        if (touchId == 0 && Time.time - DragTimeRecord > tapStayTime)
                            playerManager.CheckStock(posDragging);
                        break;
                    case TouchPhase.Ended:
                        if (touchId == 0)
                        {
                            if (playerManager && playerManager.isSkillActive == false && isDragging)
                                playerManager.UseStock(posDragging);
                            if (Time.time - DragTimeRecord < tapStayTime)
                            {
                                playerManager.RaycastTest(LayerMask.NameToLayer("Arena"), Time.time - TapTimeRecord < tapDoubleTime);
                                TapTimeRecord = Time.time;
                            }

                        }
                        if (isDragging && touchId == 0)
                        {
                            if (touch.position.x - posDragging.x > dragDiff)
                            {
                                FindObjectOfType<ISceneChange>().toDrag = 2;
                                isDragging = false;
                            }
                            if (touch.position.x - posDragging.x < -dragDiff)
                            {
                                FindObjectOfType<ISceneChange>().toDrag = 1;
                                isDragging = false;
                            }
                        }
                        break;
                    case TouchPhase.Stationary:
                        if (touchId == 0 && Time.time - DragTimeRecord > tapStayTime)
                        {
                            playerManager.CheckStock(posDragging);
                        }
                        break;
                }
            }
    }

    private void UpdateTouchInfo()
    {
        int TouchCount = Input.touchCount;
        if (TouchCount == 0)
        {
            isDragging = false;
            TapTimeRecord = 0f;
            return;
        }

        //reset Boolean
        mobileInput.isTouch[0] = false;
        mobileInput.isTouch[1] = false;

        if (TouchCount == 2 && 
            (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Moved)) 
            HandleZoom();

        if (sceneManager) ArenaActions(TouchCount);
    }

    public void BeginDrag() { //For Specified Screen Area (Button)
        isDragging = true;
        if (Input.touchCount>0)
        posDragging = Input.touches[0].position;
        else 
        posDragging = Input.mousePosition;
    }

    public void EndDrag() //For Spare
    {
        isDragging = false;
        /*
             if (touch.position.x - posDragging.x > dragDiff)
             {
                 FindObjectOfType<ISceneChange>().toDrag = 2;
                 isDragging = false;
             }
             if (touch.position.x - posDragging.x < -dragDiff)
             {
                 FindObjectOfType<ISceneChange>().toDrag = 1;
                 isDragging = false;
             }
        */
    }

    private void HandleZoom()
    {
        if (cameraManager == null) return;

        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

        float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
        float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

        float difference = currentMagnitude - prevMagnitude;
        float zoomFactor = difference * 0.01f + PlayerPrefs.GetFloat("zoomRate");
        zoomFactor = Mathf.Clamp(zoomFactor, 0.0f, 1.0f);
        cameraManager.Zoom(zoomFactor);
    }

    private void LBMTesting()
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

    private RaycastHit RaycastTest() {
        Ray ray = new Ray();
        RaycastHit hit = new RaycastHit();

        if (Input.touchCount > 0)
        {
            foreach (Touch t in Input.touches)
            {
                if (Screen.width > Screen.height)
                {
                    ray = refCamL.ScreenPointToRay(Input.GetTouch(t.fingerId).position);
                }
                else
                {
                    ray = refCamP.ScreenPointToRay(Input.GetTouch(t.fingerId).position);
                }

                if (Input.GetTouch(t.fingerId).phase == TouchPhase.Began)
                {
                    if (Physics.Raycast(ray, out hit, 1000, LayerMask.NameToLayer("ButtonLayer"))
                    &&  Buttons.Contains(hit.transform.gameObject))
                    {
                        if(hit.transform.GetComponent<RaycastFunction>())
                            hit.transform.GetComponent<RaycastFunction>().ActionFunc();
                        break;
                    }
                }
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (Screen.width > Screen.height)
            {
                ray = refCamL.ScreenPointToRay(Input.mousePosition);
            }
            else
            {
                ray = refCamP.ScreenPointToRay(Input.mousePosition);
            }

            if (Physics.Raycast(ray, out hit) && Buttons.Contains(hit.transform.gameObject))
            {
                if (hit.transform.GetComponent<RaycastFunction>())
                {
                    hit.transform.GetComponent<RaycastFunction>().ActionFunc();
                }
            }
        }

            return hit;
    }

    private void ClickEffect(Camera TargetCam)
    {
        if (TargetCam == null) return;
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            GameObject tmpObj = Instantiate(ClickPrefab, TargetCam.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, TargetCam.transform.forward.z)),
                TargetCam.transform.rotation);

            DestroyObject(tmpObj, 0.5f);
        }

        if (Input.touchCount > 0)
        {
            foreach (Touch t in Input.touches)
            {
                if (t.phase == TouchPhase.Began)
                {
                    GameObject tmpObj = Instantiate(ClickPrefab, TargetCam.ScreenToWorldPoint(
                        new Vector3(t.position.x, t.position.y, TargetCam.transform.forward.z)), TargetCam.transform.rotation);
                    DestroyObject(tmpObj, 0.5f);
                }
            }
        }
    }

    public bool GetUseTouch() { return useTouch; }
    public bool GetDraggingStatus() { return isDragging; }
    public Vector2 GetDragPos() { return posDragging; }
}
