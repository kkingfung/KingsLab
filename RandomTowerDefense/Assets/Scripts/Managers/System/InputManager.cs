using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

//public class MyMobileInput
//{
//    public Touch[] TouchInfo; //0:first finger 1:second fingers
//    public bool[] isTouch; //0:one finger 1:two fingers

//    public MyMobileInput()
//    {
//        TouchInfo = new Touch[2];
//        isTouch = new bool[2];
//    }
//};

[System.Serializable]
public class InputManager : MonoBehaviour
{
    public readonly int maxTouch = 1;
    public enum DragDirectionSel
    {
        DragToLeft=1,
        DragToRight = 2,
    }
    public enum DragDirection
    {
        DragToDown = 0,
        DragToLeft = 1,
        DragToUp = 2,
        DragToRight = 3,
    }
    readonly float tapStayTime = 0.15f;
    readonly float tapDoubleTime = 0.4f;
    readonly float dragDiff = 40.0f;//cooperate with Scene Script(toDrag)
    readonly float touchTapDiff = 5.0f;//Check Buy Store/Switch Focus

    public GameObject ClickPrefab;
    //TouchButton
    public List<GameObject> ButtonsOtr;
    public List<GameObject> ButtonsInGame;

    //Input Test
    public LBM LBMTest;
    public bool ApplyClickEfc;

    //Mobile Input
    private bool isAndroid;
    private bool isiOS;
    //private MyMobileInput mobileInput;
    private bool useTouch;
    [HideInInspector]
    public bool isDragging;
    private Vector2 posDragging;
    private Vector2 posTap;

    //Common Variables
    public PlayerManager playerManager;
    public CameraManager cameraManager;
    public StageSelectOperation sceneManagerSel;
    public InGameOperation sceneManager;
    //public TutorialManager tutorialManager;

    private float DragTimeRecord;
    [HideInInspector]
    public float TapTimeRecord;

    public Camera refCamL;
    public Camera refCamP;
    public Camera refCamLSub;
    public Camera refCamPSub;

    private void Awake()
    {
        //mobileInput = new MyMobileInput();

        //Checking Platform
        isAndroid = (Application.platform == RuntimePlatform.Android);
        isiOS = (Application.platform == RuntimePlatform.IPhonePlayer);
        useTouch = isAndroid || isiOS;
    }

    private void Start()
    {
        isDragging = false;

        //playerManager = FindObjectOfType<PlayerManager>();
        //cameraManager = FindObjectOfType<CameraManager>();
        //sceneManager = FindObjectOfType<InGameOperation>();
        //sceneManagerSel = FindObjectOfType<StageSelectOperation>();
        //tutorialManager = FindObjectOfType<TutorialManager>();
    }

    private void Update()
    {
        //if (tutorialManager && tutorialManager.WaitingResponds) return;
        if (sceneManager) {
            if (sceneManager.currScreenShown != 0 && 
                ButtonsOtr.Count + ButtonsInGame.Count > 0) 
            { 
                RaycastTest();
            }
        }
        else if (ButtonsOtr.Count+ButtonsInGame.Count > 0)
            RaycastTest(); 

        if (useTouch) 
            UpdateTouchInfo();
        else 
            UpdateMouseInfo();
        if (LBMTest) 
            LBMTesting(); //Only for Loading Scene


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
        if (sceneManager && sceneManager.currScreenShown != (int)InGameOperation.ScreenShownID.SSIDArena) return;

        if (Input.GetMouseButtonDown(0))
        {
            DragTimeRecord = Time.time;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            if (playerManager.isChecking == false)
            {
                playerManager.RaycastTest(Time.time - TapTimeRecord < tapDoubleTime*Time.timeScale);
            }
            if (playerManager && playerManager.isSkillActive == false && playerManager.isChecking)
                playerManager.UseStock(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
            TapTimeRecord = Time.time;
        }

        if (Input.GetMouseButton(0))
        {
            if (Time.time - DragTimeRecord > tapStayTime* Time.timeScale)
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
        if (Input.GetMouseButtonDown(0))
        {
            posTap = Input.mousePosition;
        }
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            #region ScreenOperation
            if (sceneManagerSel != null)
            {
                if (Input.mousePosition.x - posDragging.x > dragDiff)
                {
                    sceneManagerSel.toDrag = (int)DragDirectionSel.DragToRight;
                    isDragging = false;
                }
                if (Input.mousePosition.x - posDragging.x < -dragDiff)
                {
                    sceneManagerSel.toDrag = (int)DragDirectionSel.DragToLeft;
                    isDragging = false; 
                }
            }
            if (sceneManager != null)
            {
                if (playerManager && !playerManager.StockCheckExist() && playerManager.isChecking == false)
                {
                    float tempDiffX = Input.mousePosition.x - posDragging.x;
                    float tempDiffY = Input.mousePosition.y - posDragging.y;
                    if (tempDiffX * tempDiffX > tempDiffY * tempDiffY)
                    {
                        if (tempDiffX > dragDiff)
                        {
                            sceneManager.toDrag =(int)DragDirection.DragToLeft;
                            isDragging = false;
                        }
                        else if (tempDiffX < -dragDiff)
                        {
                            sceneManager.toDrag = (int)DragDirection.DragToRight;
                            isDragging = false;
                        }
                    }
                    else 
                    {
                        if (tempDiffY > dragDiff)
                        {
                            sceneManager.toDrag = (int)DragDirection.DragToDown;
                            isDragging = false;
                        }
                        else if (tempDiffY < -dragDiff)
                        {
                            sceneManager.toDrag = (int)DragDirection.DragToUp;
                            isDragging = false;
                        }
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
        if (sceneManager && sceneManager.currScreenShown != (int)InGameOperation.ScreenShownID.SSIDArena) return;

        for (int touchId = 0; touchId < Math.Min(TouchCount, maxTouch); ++touchId)
        {
            Touch touch = Input.GetTouch(touchId);
            //mobileInput.TouchInfo[touchId] = touch;
            //mobileInput.isTouch[touchId] = true;
            if (touchId != 0) continue;
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    DragTimeRecord = Time.time;
                    break;
                case TouchPhase.Moved:
                    if (Time.time - DragTimeRecord > tapStayTime * Time.timeScale && isDragging && sceneManager)
                        playerManager.CheckStock(touch.position);
                    break;
                case TouchPhase.Ended:
                    if (sceneManager != null)
                    {
                        if (playerManager.isChecking == false)
                        {
                            playerManager.RaycastTest(Time.time - TapTimeRecord < tapDoubleTime * Time.timeScale);
                        }
                        if (playerManager && playerManager.isSkillActive == false && playerManager.isChecking)
                            playerManager.UseStock(touch.position);
                    }
         
                    if (isDragging)
                    {
                        #region ScreenOperation
                        if (sceneManagerSel != null)
                        {
                            if (touch.position.x - posDragging.x > dragDiff)
                            {
                                sceneManagerSel.toDrag = (int)DragDirectionSel.DragToRight;
                            }
                            if (touch.position.x - posDragging.x < -dragDiff)
                            {
                                sceneManagerSel.toDrag = (int)DragDirectionSel.DragToLeft;
                            }
                        }

                        if (sceneManager != null)
                        {

                            if (playerManager && !playerManager.StockCheckExist() && playerManager.isChecking==false)
                            {
                                float tempDiffX = touch.position.x - posDragging.x;
                                float tempDiffY = touch.position.y - posDragging.y;
                                if (tempDiffX * tempDiffX > tempDiffY * tempDiffY)
                                {
                                    if (tempDiffX > dragDiff)
                                    {
                                        sceneManager.toDrag = (int)DragDirection.DragToLeft;
                                    }
                                    else if (tempDiffX < -dragDiff)
                                    {
                                        sceneManager.toDrag = (int)DragDirection.DragToRight;
                                    }
                                }
                                else
                                {
                                    if (tempDiffY > dragDiff)
                                    {
                                        sceneManager.toDrag = (int)DragDirection.DragToDown;
                                    }
                                    else if (tempDiffY < -dragDiff)
                                    {
                                        sceneManager.toDrag = (int)DragDirection.DragToUp;
                                    }
                                }
                            }
 
                        }

                        #endregion
                        isDragging = false;
                    }

                    DragTimeRecord = float.MaxValue;
                    TapTimeRecord = Time.time;
                    break;
                case TouchPhase.Stationary:
                    if (Time.time - DragTimeRecord > tapStayTime * Time.timeScale && isDragging)
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

        //reset Boolean
        //mobileInput.isTouch[0] = false;
        //mobileInput.isTouch[1] = false;

        if (TouchCount>0)
        {
            posTap = Input.touches[0].position;
        }

        if (TouchCount == 2 &&
            (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Moved))
            HandleZoom();

       ArenaActionsByTouch(TouchCount);
    }

    public void BeginDrag()
    { //For Specified Screen Area (Button)
        float PreviousRecord = DragTimeRecord;
        if ((Input.touchCount > 0 && (Input.touches[0].phase== TouchPhase.Moved
            || Input.touches[0].phase == TouchPhase.Stationary))
            ||Input.GetMouseButton(0))
        {
            DragTimeRecord = Time.time;
            if (Input.touchCount > 0)
                posDragging = Input.touches[0].position;
            else
                posDragging = Input.mousePosition;
        }

        //if (DragTimeRecord - PreviousRecord < tapStayTime*Time.timeScale) {
        //    return;
        //}
  
        isDragging = true;
    }

    public void EndDrag() //For Spare
    {
       // isDragging = false;
    }

    private void HandleZoom()
    {
        if (cameraManager == null) return;
        isDragging = false;
        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

        float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
        float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

        float difference = currentMagnitude - prevMagnitude;
        float zoomFactor = difference * 0.01f + PlayerPrefs.GetFloat("zoomRate",0);
        zoomFactor = Mathf.Clamp(zoomFactor, 0.0f, 1.0f);
        cameraManager.Zoom(zoomFactor);
    }

    private void LBMTesting()
    {
        if (LBMTest == null) return;

        if (useTouch)
        {
           // LBMTest.Interaction(mobileInput.isTouch[0], !mobileInput.isTouch[0], mobileInput.TouchInfo[0].position, true);
           // LBMTest.Interaction(mobileInput.isTouch[1], !mobileInput.isTouch[1], mobileInput.TouchInfo[1].position, true);
        }
        else
            LBMTest.Interaction(Input.GetMouseButton(0), !Input.GetMouseButton(0), Input.mousePosition, true);
    }

    public bool GetAnyInput()
    {
        return Input.touchCount>0 || Input.GetMouseButton(0);
    }

    private RaycastHit RaycastTest()
    {
        Ray ray = new Ray();
        Ray ray2 = new Ray();
        Ray ray3 = new Ray();
        RaycastHit hit = new RaycastHit();
        RaycastHit hit2 = new RaycastHit();
        RaycastHit hit3 = new RaycastHit();

        if (Screen.width > Screen.height)
        {
            ray2 = refCamL.ScreenPointToRay(posTap);
            ray3 = refCamL.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
        }
        else 
        {
            ray2 = refCamP.ScreenPointToRay(posTap);
            ray3 = refCamP.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
        }

        if (useTouch && Input.touchCount > 0)
        {
            foreach (Touch t in Input.touches)
            {
                if (Input.GetTouch(t.fingerId).phase == TouchPhase.Ended)
                {
                    ray = (Screen.width > Screen.height)? refCamL.ScreenPointToRay(Input.GetTouch(t.fingerId).position):
                        refCamP.ScreenPointToRay(Input.GetTouch(t.fingerId).position);

                    if (ButtonsOtr.Count > 0)
                    {
                        if (sceneManager)
                        {
                            if ((Physics.Raycast(ray2, out hit2, 200, LayerMask.GetMask("ButtonLayer")) &&
                            Physics.Raycast(ray, out hit, 200, LayerMask.GetMask("ButtonLayer")))
                                && ButtonsOtr.Contains(hit.transform.gameObject))
                            {
                                RaycastFunction raycastFunction = hit.transform.GetComponent<RaycastFunction>();
                                if (hit.transform == hit2.transform && raycastFunction)
                                    raycastFunction.ActionFunc();
                                break;
                            }
                        }
                        else
                        {
                            if (Physics.Raycast(ray, out hit, 200, LayerMask.GetMask("ButtonLayer"))&& 
                                ButtonsOtr.Contains(hit.transform.gameObject))
                            {
                                RaycastFunction raycastFunction = hit.transform.GetComponent<RaycastFunction>();
                                if (raycastFunction)
                                    raycastFunction.ActionFunc();
                                break;
                            }
                        }
                    }
                    if (ButtonsInGame.Count > 0 && sceneManager.currScreenShown!=0)
                    {
                        if ((Input.GetTouch(t.fingerId).position - posTap).sqrMagnitude < touchTapDiff &&
                         Physics.Raycast(ray3, out hit3, 200, LayerMask.GetMask("StoreLayer"))
                         && ButtonsInGame.Contains(hit3.transform.gameObject))
                        {
                            hit3.transform.GetComponent<RaycastFunction>().ActionFunc();
                            break;
                        }
                    }
                }
            }
        }
        else if (!useTouch && Input.GetMouseButtonUp(0))
        {
            ray = (Screen.width > Screen.height) ? refCamL.ScreenPointToRay(Input.mousePosition) :
                    refCamP.ScreenPointToRay(Input.mousePosition);

            if (ButtonsOtr.Count > 0)
            {
                if (sceneManager)
                {
                    if ((Physics.Raycast(ray2, out hit2, 200, LayerMask.GetMask("ButtonLayer")) &&
                    Physics.Raycast(ray, out hit, 200, LayerMask.GetMask("ButtonLayer")))
                && ButtonsOtr.Contains(hit.transform.gameObject))
                    {
                        RaycastFunction raycastFunction = hit.transform.GetComponent<RaycastFunction>();
                        if (hit.transform == hit2.transform && raycastFunction)
                            raycastFunction.ActionFunc();
                    }
                }
                else
                {
                    if (Physics.Raycast(ray, out hit, 200, LayerMask.GetMask("ButtonLayer")) && 
                        ButtonsOtr.Contains(hit.transform.gameObject))
                    {
                        RaycastFunction raycastFunction = hit.transform.GetComponent<RaycastFunction>();
                        if (raycastFunction)
                            raycastFunction.ActionFunc();
                    }
                }
            }

            if (ButtonsInGame.Count > 0 && sceneManager && sceneManager.currScreenShown != 0)
            {
    
                if ((Physics.Raycast(ray2, out hit2, 200) && Physics.Raycast(ray, out hit, 200)) &&
                    ButtonsInGame.Contains(hit.transform.gameObject))
                {
                    RaycastFunction raycastFunction = hit.transform.GetComponent<RaycastFunction>();
                    if (hit.transform == hit2.transform && raycastFunction)
                    {
                        raycastFunction.ActionFunc();
                    }
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

            Destroy(tmpObj, 0.5f);
        }

        if (Input.touchCount > 0)
        {
            foreach (Touch t in Input.touches)
            {
                if (t.phase == TouchPhase.Began)
                {
                    GameObject tmpObj = Instantiate(ClickPrefab, TargetCam.ScreenToWorldPoint(
                        new Vector3(t.position.x, t.position.y, TargetCam.transform.forward.z)), TargetCam.transform.rotation);
                    Destroy(tmpObj, 0.5f);
                }
            }
        }
    }

    public bool GetUseTouch() { return useTouch; }
    public bool GetDraggingStatus() { return isDragging; }
    public Vector2 GetDragPos() { return posDragging; }
}
