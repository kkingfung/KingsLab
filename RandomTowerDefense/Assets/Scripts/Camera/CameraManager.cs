using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    readonly float defaultFOV = 60f;
    readonly float minFOV = 30f;
    readonly int rotateFrame = 60;

    public List<Camera> LandscapeCam_Main;
    public List<Camera> LandscapeCam_Sub;

    public List<Camera> PortraitCam_Main;
    public List<Camera> PortraitCam_Sub;

    public List<GameObject> GyroCamGp;
    public List<Camera> ZoomCamGp;

    public List<Slider> zoomSlider;

    [HideInInspector]
    public bool isOpening;
    [HideInInspector]
    public bool isGyroEnabled;

    GyroscopeManager GyroscopeManager;
    ISceneChange SceneManager;

    private void OnEnable()
    {
        isGyroEnabled = false;
        SceneManager = FindObjectOfType<ISceneChange>();

        foreach (Slider i in zoomSlider)
            i.value = PlayerPrefs.GetFloat("zoomRate");

    }

    // Use this for initialization
    void Start()
    {
        isOpening = true;

        GyroscopeManager = FindObjectOfType<GyroscopeManager>();
        //ZoomOperation
        if (GyroCamGp.Count > 0)
        {
            if (GyroscopeManager && GyroscopeManager.isFunctioning)
            {
                for (int i = 0; i < ZoomCamGp.Count; ++i)
                {
                    ZoomCamGp[i].fieldOfView = defaultFOV;
                }
            }
        }

        Zoom(PlayerPrefs.GetFloat("zoomRate"));
    }

    // Update is called once per frame
    void Update()
    {

        //Landscape
        foreach (Camera i in LandscapeCam_Main)
            i.enabled = isOpening && (SceneManager.OrientationLand);
        foreach (Camera i in LandscapeCam_Sub)
            i.enabled = !isOpening && (SceneManager.OrientationLand);

        //Portrait
        foreach (Camera i in PortraitCam_Main)
            i.enabled = isOpening && (!SceneManager.OrientationLand);
        foreach (Camera i in PortraitCam_Sub)
            i.enabled = !isOpening && (!SceneManager.OrientationLand);

        //GyroOperation
        foreach (Slider i in zoomSlider)
        {
            i.interactable = !SceneManager.GetOptionStatus();
        }

        if (GyroCamGp.Count > 0)
        {
            if (GyroscopeManager && GyroscopeManager.isFunctioning)
            {
                for (int i = 0; i < GyroCamGp.Count; ++i)
                {
                    GyroCamGp[i].transform.localEulerAngles = GyroscopeManager.CurrGyroRotation();
                }
            }
        }
    }

    public void Zoom(float zoomRate) {
        if (ZoomCamGp.Count > 0)
        {
            foreach (Camera i in ZoomCamGp)
            {
                i.fieldOfView = Mathf.Clamp(defaultFOV - zoomRate * (defaultFOV - minFOV), minFOV, defaultFOV);
            }
            foreach (Slider i in zoomSlider)
                i.value = zoomRate;
        }

        PlayerPrefs.SetFloat("zoomRate", zoomRate);
    }

    public void RotateCam(float targetAngle)
    {
        StartCoroutine(RotateMainCamera(targetAngle));

    }

    private IEnumerator RotateMainCamera(float targetAngle) 
    {
        int frame = 0;
        float angleChgsbyFrame = (targetAngle - this.transform.localEulerAngles.x) / rotateFrame;

        while (frame < rotateFrame)
        {
            this.transform.localEulerAngles = new Vector3(this.transform.localEulerAngles.x+ angleChgsbyFrame,
                this.transform.localEulerAngles.y, this.transform.localEulerAngles.z);
            frame ++;
            yield return new WaitForSeconds(0f);
        }
    }

}
