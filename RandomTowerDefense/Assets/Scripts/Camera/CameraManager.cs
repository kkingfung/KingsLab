using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class CameraManager : MonoBehaviour
{
    public List<Camera> LandscapeCam_Main;
    public List<Camera> LandscapeCam_Sub;

    public List<Camera> PortraitCam_Main;
    public List<Camera> PortraitCam_Sub;

    [HideInInspector]
    public bool isOpening;
    // Use this for initialization
    void Start()
    {
        isOpening = true;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Camera i in LandscapeCam_Main)
            i.enabled = isOpening && (Screen.width > Screen.height);
        foreach (Camera i in LandscapeCam_Sub)
            i.enabled = !isOpening && (Screen.width > Screen.height);

        foreach (Camera i in PortraitCam_Main)
            i.enabled = isOpening && (Screen.width <= Screen.height);
        foreach (Camera i in PortraitCam_Sub)
            i.enabled = !isOpening && (Screen.width <= Screen.height);
    }
}
