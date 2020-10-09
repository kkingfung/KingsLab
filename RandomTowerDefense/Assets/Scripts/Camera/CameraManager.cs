using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class CameraManager : MonoBehaviour
{
    readonly float defaultFOV = 60f;
    readonly int rotateFrame = 60;

    public List<Camera> LandscapeCam_Main;
    public List<Camera> LandscapeCam_Sub;

    public List<Camera> PortraitCam_Main;
    public List<Camera> PortraitCam_Sub;

    public List<GameObject> GyroCamHolder;

    [HideInInspector]
    public bool isOpening;
    [HideInInspector]
    public bool isGyroEnabled;

    GyroscopeManager GyroscopeManager;
    Vector3[] baseRotation;

    private void OnEnable()
    {
        isOpening = true;
        isGyroEnabled = false;
        baseRotation = new Vector3[GyroCamHolder.Count];

        for (int i = 0; i < GyroCamHolder.Count; ++i) 
        {
            baseRotation[i] = GyroCamHolder[i].transform.eulerAngles;
        } 
    }

    // Use this for initialization
    void Start()
    {
        GyroscopeManager = FindObjectOfType<GyroscopeManager>();
    }

    // Update is called once per frame
    void Update()
    {
        //Landscape
        foreach (Camera i in LandscapeCam_Main)
            i.enabled = isOpening && (Screen.width > Screen.height);
        foreach (Camera i in LandscapeCam_Sub)
            i.enabled = !isOpening && (Screen.width > Screen.height);

        //Portrait
        foreach (Camera i in PortraitCam_Main)
            i.enabled = isOpening && (Screen.width <= Screen.height);
        foreach (Camera i in PortraitCam_Sub)
            i.enabled = !isOpening && (Screen.width <= Screen.height);

        //GyroOperation
        if (GyroCamHolder.Count > 0)
        {
            if (GyroscopeManager && GyroscopeManager.isFunctioning)
            {
                for (int i = 0; i < GyroCamHolder.Count; ++i)
                {
                    GyroCamHolder[i].transform.eulerAngles = baseRotation[i] + GyroscopeManager.CurrGyroRotation();
                }
            }
        }
    }

    public void Zoom(float zoomRate) {
        if (GyroCamHolder.Count > 0)
        {
            foreach (GameObject i in GyroCamHolder)
            {
                Camera[] cam = i.GetComponentsInChildren<Camera>();
                foreach (Camera j in cam)
                {
                    j.fieldOfView = Mathf.Clamp(zoomRate * defaultFOV, 30f, 90f);
                }
            }
        }
    }

    public void RotateCam(float targetAngle)
    {
        StartCoroutine(RotateMainCamera(targetAngle));


    }

    private IEnumerator RotateMainCamera(float targetAngle) 
    {
        int frame = 0;
        float angleChgsbyFrame = (targetAngle - this.transform.eulerAngles.x) / rotateFrame;

        while (frame < rotateFrame)
        {
            this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x+ angleChgsbyFrame,
                this.transform.eulerAngles.y, this.transform.eulerAngles.z);
            frame ++;
            yield return new WaitForSeconds(0f);
        }
    }
}
