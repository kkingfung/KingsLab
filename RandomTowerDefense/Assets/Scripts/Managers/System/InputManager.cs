using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using RandomTowerDefense.Scene;
using RandomTowerDefense.Managers.Macro;
using RandomTowerDefense.Tools;

namespace RandomTowerDefense.Managers.System
{
    /// <summary>
    /// 入力管理システム - クロスプラットフォームマウス・タッチ入力の統合管理
    ///
    /// 主な機能:
    /// - クロスプラットフォーム入力処理（PC用マウス、モバイル用タッチ）
    /// - ジェスチャー認識（タップ、ドラッグ、ピンチズーム）
    /// - レイキャストベースUIインタラクションシステム
    /// - ジョスチャー別マルチタッチサポート
    /// - タワー配置用アリーナ専用入力処理
    /// - クリックエフェクト視覚フィードバックシステム
    /// - シーン対応入力ルーティング
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        #region Enums
        public enum DragDirectionSel
        {
            DragToLeft = 1,
            DragToRight = 2,
        }

        public enum DragDirection
        {
            DragToDown = 0,
            DragToLeft = 1,
            DragToUp = 2,
            DragToRight = 3,
        }
        #endregion

        #region Constants
        public readonly int maxTouch = 1;
        private readonly float tapStayTime = 0.15f;
        private readonly float tapDoubleTime = 0.4f;
        private readonly float dragDiff = 40.0f; // Minimum drag distance to register gesture
        private readonly float touchTapDiff = 5.0f; // Maximum tap distance for UI interaction
        #endregion

        #region Serialized Fields
        [Header("🖱️ Click Effects")]
        public GameObject ClickPrefab;
        public bool ApplyClickEfc;

        [Header("🔘 UI Buttons")]
        public List<GameObject> ButtonsOtr; // General UI buttons
        public List<GameObject> ButtonsInGame; // In-game specific buttons

        [Header("🧪 Testing")]
        public LBM LBMTest;

        [Header("🎮 Manager References")]
        public PlayerManager playerManager;
        public CameraManager cameraManager;
        public StageSelectOperation sceneManagerSel;
        public InGameOperation sceneManager;

        [Header("📷 Camera References")]
        public Camera refCamL; // Landscape main camera
        public Camera refCamP; // Portrait main camera
        public Camera refCamLSub; // Landscape sub camera
        public Camera refCamPSub; // Portrait sub camera
        #endregion

        #region Private Fields
        // Platform detection
        private bool _isAndroid;
        private bool _isiOS;
        private bool _useTouch;

        // Input state tracking
        private Vector2 _posDragging;
        private Vector2 _posTap;
        private float _dragTimeRecord;
        #endregion

        #region Public Properties
        [HideInInspector]
        public bool isDragging;
        [HideInInspector]
        public float TapTimeRecord;
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Initialize platform detection and input method selection
        /// </summary>
        private void Awake()
        {
            // Platform detection for input method selection
            _isAndroid = (Application.platform == RuntimePlatform.Android);
            _isiOS = (Application.platform == RuntimePlatform.IPhonePlayer);
            _useTouch = _isAndroid || _isiOS;
        }

        /// <summary>
        /// Initialize input state and manager references
        /// </summary>
        private void Start()
        {
            isDragging = false;
        }

        /// <summary>
        /// Process input events and handle UI interactions
        /// </summary>
        private void Update()
        {
            // Handle UI raycasting if buttons are available
            if (sceneManager)
            {
                if (sceneManager.currScreenShown != 0 &&
                    ButtonsOtr.Count + ButtonsInGame.Count > 0)
                {
                    RaycastTest();
                }
            }
            else if (ButtonsOtr.Count + ButtonsInGame.Count > 0)
            {
                RaycastTest();
            }

            // Update input based on platform
            if (_useTouch)
            {
                UpdateTouchInfo();
            }
            else
            {
                UpdateMouseInfo();
            }

            // Handle loading scene testing
            if (LBMTest)
            {
                LBMTesting();
            }

            // Apply click visual effects
            if (ApplyClickEfc && ClickPrefab != null)
            {
                if (refCamL != null)
                {
                    ClickEffect((Screen.width > Screen.height) ? refCamL : refCamP);
                }
                if (refCamLSub != null)
                {
                    ClickEffect((Screen.width > Screen.height) ? refCamLSub : refCamPSub);
                }
            }
        }
        #endregion

        #region Private Methods - Arena Input
        /// <summary>
        /// Handle mouse input specifically for arena gameplay
        /// </summary>
        private void ArenaActionsByMouse()
        {
            //For Arena Scene/Screen Only
            if (sceneManager && sceneManager.currScreenShown != (int)InGameOperation.ScreenShownID.SSIDArena) return;

            if (Input.GetMouseButtonDown(0))
            {
                _dragTimeRecord = Time.time;
            }
            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                if (playerManager.isChecking == false)
                {
                    playerManager.RaycastTest(Time.time - TapTimeRecord < tapDoubleTime * Time.timeScale);
                }
                if (playerManager && playerManager.isSkillActive == false && playerManager.isChecking)
                    playerManager.UseStock(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
                TapTimeRecord = Time.time;
            }

            if (Input.GetMouseButton(0))
            {
                if (Time.time - _dragTimeRecord > tapStayTime * Time.timeScale)
                {
                    if (isDragging)
                    {
                        playerManager.CheckStock(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
                    }
                }

            }
        }
        /// <summary>
        /// Process mouse input for drag gestures and screen navigation
        /// </summary>
        private void UpdateMouseInfo()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _posTap = Input.mousePosition;
            }
            if (isDragging && Input.GetMouseButtonUp(0))
            {
                #region ScreenOperation
                if (sceneManagerSel != null)
                {
                    if (Input.mousePosition.x - _posDragging.x > dragDiff)
                    {
                        sceneManagerSel.toDrag = (int)DragDirectionSel.DragToRight;
                        isDragging = false;
                    }
                    if (Input.mousePosition.x - _posDragging.x < -dragDiff)
                    {
                        sceneManagerSel.toDrag = (int)DragDirectionSel.DragToLeft;
                        isDragging = false;
                    }
                }
                if (sceneManager != null)
                {
                    if (playerManager && !playerManager.StockCheckExist() && playerManager.isChecking == false)
                    {
                        float tempDiffX = Input.mousePosition.x - _posDragging.x;
                        float tempDiffY = Input.mousePosition.y - _posDragging.y;
                        if (tempDiffX * tempDiffX > tempDiffY * tempDiffY)
                        {
                            if (tempDiffX > dragDiff)
                            {
                                sceneManager.toDrag = (int)DragDirection.DragToLeft;
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

        /// <summary>
        /// Handle touch input specifically for arena gameplay
        /// </summary>
        /// <param name="TouchCount">Number of active touches</param>
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
                        _dragTimeRecord = Time.time;
                        break;
                    case TouchPhase.Moved:
                        if (Time.time - _dragTimeRecord > tapStayTime * Time.timeScale && isDragging && sceneManager)
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
                                if (touch.position.x - _posDragging.x > dragDiff)
                                {
                                    sceneManagerSel.toDrag = (int)DragDirectionSel.DragToRight;
                                }
                                if (touch.position.x - _posDragging.x < -dragDiff)
                                {
                                    sceneManagerSel.toDrag = (int)DragDirectionSel.DragToLeft;
                                }
                            }

                            if (sceneManager != null)
                            {

                                if (playerManager && !playerManager.StockCheckExist() && playerManager.isChecking == false)
                                {
                                    float tempDiffX = touch.position.x - _posDragging.x;
                                    float tempDiffY = touch.position.y - _posDragging.y;
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

                        _dragTimeRecord = float.MaxValue;
                        TapTimeRecord = Time.time;
                        break;
                    case TouchPhase.Stationary:
                        if (Time.time - _dragTimeRecord > tapStayTime * Time.timeScale && isDragging)
                        {
                            playerManager.CheckStock(touch.position);
                        }
                        break;
                }
            }
        }


        /// <summary>
        /// Process touch input for gestures and screen navigation
        /// </summary>
        private void UpdateTouchInfo()
        {
            int TouchCount = Input.touchCount;

            //reset Boolean
            //mobileInput.isTouch[0] = false;
            //mobileInput.isTouch[1] = false;

            if (TouchCount > 0)
            {
                _posTap = Input.touches[0].position;
            }

            if (TouchCount == 2 &&
                (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved))
            {
                HandleZoom();
            }

            ArenaActionsByTouch(TouchCount);
        }

        #endregion

        #region Public API
        /// <summary>
        /// Begin drag operation when user starts dragging gesture
        /// </summary>
        public void BeginDrag()
        { //For Specified Screen Area (Button)
            float previousRecord = _dragTimeRecord;
            if ((Input.touchCount > 0 && (Input.touches[0].phase == TouchPhase.Moved
                || Input.touches[0].phase == TouchPhase.Stationary))
                || Input.GetMouseButton(0))
            {
                _dragTimeRecord = Time.time;
                if (Input.touchCount > 0)
                {
                    _posDragging = Input.touches[0].position;
                }
                else
                {
                    _posDragging = Input.mousePosition;
                }
            }

            //if (DragTimeRecord - PreviousRecord < tapStayTime*Time.timeScale) {
            //    return;
            //}

            isDragging = true;
        }

        /// <summary>
        /// End drag operation (currently unused - kept for future use)
        /// </summary>
        public void EndDrag()
        {
            // isDragging = false;
        }

        #endregion

        #region Private Methods - Gestures
        /// <summary>
        /// Handle pinch-to-zoom gesture for camera control
        /// </summary>
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
            float zoomFactor = difference * 0.01f + PlayerPrefs.GetFloat("zoomRate", 0);
            zoomFactor = Mathf.Clamp(zoomFactor, 0.0f, 1.0f);
            cameraManager.Zoom(zoomFactor);
        }

        #endregion

        #region Private Methods - Testing
        /// <summary>
        /// Handle loading screen button testing (development feature)
        /// </summary>
        private void LBMTesting()
        {
            if (LBMTest == null) return;

            if (_useTouch)
            {
                // LBMTest.Interaction(mobileInput.isTouch[0], !mobileInput.isTouch[0], mobileInput.TouchInfo[0].position, true);
                // LBMTest.Interaction(mobileInput.isTouch[1], !mobileInput.isTouch[1], mobileInput.TouchInfo[1].position, true);
            }
            else
                LBMTest.Interaction(Input.GetMouseButton(0), !Input.GetMouseButton(0), Input.mousePosition, true);
        }

        /// <summary>
        /// Get current input activity status
        /// </summary>
        /// <returns>True if any input is currently active</returns>
        public bool GetAnyInput()
        {
            return Input.touchCount > 0 || Input.GetMouseButton(0);
        }

        /// <summary>
        /// Get whether touch input is being used
        /// </summary>
        /// <returns>True if using touch input</returns>
        public bool GetUseTouch()
        {
            return _useTouch;
        }

        /// <summary>
        /// Get current dragging status
        /// </summary>
        /// <returns>True if currently dragging</returns>
        public bool GetDraggingStatus()
        {
            return isDragging;
        }

        /// <summary>
        /// Get current drag position
        /// </summary>
        /// <returns>Current drag position in screen coordinates</returns>
        public Vector2 GetDragPos()
        {
            return _posDragging;
        }
        #endregion

        #region Private Methods - UI Interaction
        /// <summary>
        /// Perform raycast testing for UI button interaction
        /// </summary>
        /// <returns>RaycastHit result from the button interaction test</returns>
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
                ray2 = refCamL.ScreenPointToRay(_posTap);
                ray3 = refCamL.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
            }
            else
            {
                ray2 = refCamP.ScreenPointToRay(_posTap);
                ray3 = refCamP.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
            }

            if (_useTouch && Input.touchCount > 0)
            {
                foreach (Touch t in Input.touches)
                {
                    if (Input.GetTouch(t.fingerId).phase == TouchPhase.Ended)
                    {
                        ray = (Screen.width > Screen.height) ? refCamL.ScreenPointToRay(Input.GetTouch(t.fingerId).position) :
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
                                if (Physics.Raycast(ray, out hit, 200, LayerMask.GetMask("ButtonLayer")) &&
                                    ButtonsOtr.Contains(hit.transform.gameObject))
                                {
                                    RaycastFunction raycastFunction = hit.transform.GetComponent<RaycastFunction>();
                                    if (raycastFunction)
                                        raycastFunction.ActionFunc();
                                    break;
                                }
                            }
                        }
                        if (ButtonsInGame.Count > 0 && sceneManager.currScreenShown != 0)
                        {
                            if ((Input.GetTouch(t.fingerId).position - _posTap).sqrMagnitude < touchTapDiff &&
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
            else if (!_useTouch && Input.GetMouseButtonUp(0))
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

                    if ((Physics.Raycast(ray2, out hit2, 200, LayerMask.GetMask("StoreLayer")) &&
                        Physics.Raycast(ray, out hit, 200, LayerMask.GetMask("StoreLayer"))) &&
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

        #endregion

        #region Private Methods - Visual Effects
        /// <summary>
        /// Generate visual click effects at input positions
        /// </summary>
        /// <param name="TargetCam">Camera to use for screen-to-world conversion</param>
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

        #endregion
    }
}
