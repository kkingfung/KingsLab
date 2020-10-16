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

    public List<Camera> GyroCamGp;

    public List<Slider> zoomSlider;

    [HideInInspector]
    public bool isOpening;
    [HideInInspector]
    public bool isGyroEnabled;

    GyroscopeManager GyroscopeManager;
    Vector3[] baseRotation;
    ISceneChange SceneManager;

    private void OnEnable()
    {
        isOpening = true;
        isGyroEnabled = false;
        baseRotation = new Vector3[GyroCamGp.Count];
        SceneManager = FindObjectOfType<ISceneChange>();

        for (int i = 0; i < GyroCamGp.Count; ++i) 
        {
            baseRotation[i] = GyroCamGp[i].transform.eulerAngles;
        }

        foreach (Slider i in zoomSlider)
            i.value = PlayerPrefs.GetFloat("zoomRate");

        Zoom(PlayerPrefs.GetFloat("zoomRate"));
    }

    // Use this for initialization
    void Start()
    {
        GyroscopeManager = FindObjectOfType<GyroscopeManager>();
        //GyroOperation
        if (GyroCamGp.Count > 0)
        {
            if (GyroscopeManager && GyroscopeManager.isFunctioning)
            {
                for (int i = 0; i < GyroCamGp.Count; ++i)
                {
                    GyroCamGp[i].fieldOfView = defaultFOV;
                }
            }
        }
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
                    GyroCamGp[i].transform.eulerAngles = baseRotation[i] + GyroscopeManager.CurrGyroRotation();
                }
            }
        }
    }

    public void Zoom(float zoomRate) {
        if (GyroCamGp.Count > 0)
        {
            foreach (Camera i in GyroCamGp)
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
