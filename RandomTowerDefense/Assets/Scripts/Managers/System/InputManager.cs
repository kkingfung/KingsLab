using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class MyMobileInput
{
    public readonly int maxTouch = 2;
    public Touch[] TouchInfo; //0:first finger 1:second fingers
    public bool[] isTouch; //0:one finger 1:two fingers

    public MyMobileInput()
    {
        TouchInfo = new Touch[2];
        isTouch = new bool[2];
    }
};

[System.Serializable]
public class InputManager : MonoBehaviour
{
    readonly float tapStayTime = 0.4f;
    readonly float tapDoubleTime = 0.25f;
    readonly float dragDiff = 40.0f;//cooperate with Scene Script(toDrag)

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
    [HideInInspector]
    public bool isDragging;
    private Vector2 posDragging;

    //Common Variables
    private PlayerManager playerManager;
    private CameraManager cameraManager;
    private InGameOperation sceneManager;

    private float DragTimeRecord;
    [HideInInspector]
    public float TapTimeRecord;

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

    private void ArenaActionsByMouse()
    {
        //For Arena Scene/Screen Only
        if (sceneManager.currScreenShown != 0) return;

        if (Input.GetMouseButtonDown(0))
        {
            DragTimeRecord = Time.time;
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (playerManager.isChecking==false)
                playerManager.RaycastTest(Time.time - TapTimeRecord < tapDoubleTime);
            if (playerManager && playerManager.isSkillActive == false && playerManager.isChecking)
                playerManager.UseStock(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
            DragTimeRecord = float.MaxValue;
            TapTimeRecord = Time.time;
        }

        if (Input.GetMouseButton(0))
        {
            if (Time.time - DragTimeRecord > tapStayTime/ Time.timeScale)
            {
                if (isDragging)
                {
                    playerManager.CheckStock(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
                }
            }

        }
    }
    private void UpdateMouseInfo()
    {
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            #region ScreenOperation
            if (FindObjectOfType<StageSelectOperation>() != null)
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
            if (FindObjectOfType<InGameOperation>() != null)
            {
                if (playerManager && !playerManager.StockCheckExist() && playerManager.isChecking == false)
                {
                    if (Input.mousePosition.x - posDragging.x > dragDiff)
                    {
                        FindObjectOfType<ISceneChange>().toDrag = 1;
                        isDragging = false;
                    }
                    if (Input.mousePosition.x - posDragging.x < -dragDiff)
                    {
                        FindObjectOfType<ISceneChange>().toDrag = 3;
                        isDragging = false; 
                    }
                    if (Input.mousePosition.y - posDragging.y > dragDiff)
                    {
                        FindObjectOfType<ISceneChange>().toDrag = 0;
                        isDragging = false; 
                    }
                    if (Input.mousePosition.y - posDragging.y < -dragDiff)
                    {
                        FindObjectOfType<ISceneChange>().toDrag = 2;
                        isDragging = false; 
                    }
                }
            }

            #endregion
        }
        if (sceneManager)
            ArenaActionsByMouse();

    }

    private void ArenaActionsByTouch(int TouchCount)
    {
        //For Arena Scene/Screen Only
        if (sceneManager.currScreenShown != 0) return;
        for (int touchId = 0; touchId < Math.Min(TouchCount, mobileInput.maxTouch); ++touchId)
        {
            Touch touch = Input.GetTouch(touchId);
            mobileInput.TouchInfo[touchId] = touch;
            mobileInput.isTouch[touchId] = true;
            if (touchId != 0) continue;
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    DragTimeRecord = Time.time;
                    break;
                case TouchPhase.Moved:
                    if (Time.time - DragTimeRecord > tapStayTime / Time.timeScale && isDragging)
                        playerManager.CheckStock(touch.position);
                    break;
                case TouchPhase.Ended:
                    if (playerManager.isChecking == false)
                        playerManager.RaycastTest(Time.time - TapTimeRecord < tapDoubleTime);
                    if (playerManager && playerManager.isSkillActive == false && playerManager.isChecking)
                        playerManager.UseStock(touch.position);

                    DragTimeRecord = float.MaxValue;
                    TapTimeRecord = Time.time;

                    if (isDragging)
                    {
                        #region ScreenOperation
                        if (FindObjectOfType<StageSelectOperation>() != null)
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
                        if (FindObjectOfType<InGameOperation>() != null)
                        {
                            if (playerManager && !playerManager.StockCheckExist() && playerManager.isChecking==false)
                            {

                                if (touch.position.x - posDragging.x > dragDiff)
                                {
                                    FindObjectOfType<ISceneChange>().toDrag = 1;
                                    isDragging = false;
                                }
                                if (touch.position.x - posDragging.x < -dragDiff)
                                {
                                    FindObjectOfType<ISceneChange>().toDrag = 3;
                                    isDragging = false;
                                }
                                if (touch.position.y - posDragging.y > dragDiff)
                                {
                                    FindObjectOfType<ISceneChange>().toDrag = 0;
                                    isDragging = false;
                                }
                                if (touch.position.y - posDragging.y < -dragDiff)
                                {
                                    FindObjectOfType<ISceneChange>().toDrag = 2;
                                    isDragging = false;
                                }
                            }
                            else isDragging = false;
                        }

                        #endregion
                    }
                    break;
                case TouchPhase.Stationary:
                    if (Time.time - DragTimeRecord > tapStayTime / Time.timeScale && isDragging)
                    {
                        playerManager.CheckStock(touch.position);
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
            return;
        }

        //reset Boolean
        mobileInput.isTouch[0] = false;
        mobileInput.isTouch[1] = false;

        if (TouchCount == 2 &&
            (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Moved))
            HandleZoom();

        if (sceneManager)
            ArenaActionsByTouch(TouchCount);
    }

    public void BeginDrag()
    { //For Specified Screen Area (Button)
        isDragging = true;
        DragTimeRecord = Time.time;
        if (Input.touchCount > 0)
            posDragging = Input.touches[0].position;
        else
            posDragging = Input.mousePosition;
    }

    public void EndDrag() //For Spare
    {
        if(playerManager.isChecking==false && sceneManager.currScreenShown==0 )
            if((useTouch && Input.touchCount>0 && (Input.touches[0].position - posDragging).magnitude < dragDiff)
                ||(!useTouch&& (new Vector2(Input.mousePosition.x, Input.mousePosition.y) - posDragging).magnitude < dragDiff))
        {

        isDragging = false;
        }
        
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

        if (useTouch)
        {
            LBMTest.Interaction(mobileInput.isTouch[0], !mobileInput.isTouch[0], mobileInput.TouchInfo[0].position, true);
            LBMTest.Interaction(mobileInput.isTouch[1], !mobileInput.isTouch[1], mobileInput.TouchInfo[1].position, true);
        }
        else
            LBMTest.Interaction(Input.GetMouseButton(0), !Input.GetMouseButton(0), Input.mousePosition, true);
    }

    public bool GetAnyInput()
    {
        return mobileInput.isTouch[0] || Input.GetMouseButton(0);
    }

    private RaycastHit RaycastTest()
    {
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
                    if (Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("ButtonLayer"))
                    && Buttons.Contains(hit.transform.gameObject))
                    {
                        if (hit.transform.GetComponent<RaycastFunction>())
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
