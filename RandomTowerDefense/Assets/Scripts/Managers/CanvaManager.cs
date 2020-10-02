using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvaManager : MonoBehaviour
{
    public Canvas PortraitCanva;
    public Canvas LandscapeCanva;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        PortraitCanva.enabled = Screen.width <= Screen.height;
        LandscapeCanva.enabled = Screen.width > Screen.height;
    }
}
