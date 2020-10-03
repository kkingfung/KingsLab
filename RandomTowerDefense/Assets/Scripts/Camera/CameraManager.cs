using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class CameraManager : MonoBehaviour
{
    public List<Camera> LandscapeCam;
    public List<Camera> PortraitCam;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        foreach (Camera i in LandscapeCam)
            i.enabled = (Screen.width > Screen.height);
        foreach (Camera i in PortraitCam)
            i.enabled = (Screen.width <= Screen.height);
    }
}
