using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvaManager : MonoBehaviour
{
    public List<Canvas> LandscapeCanva;
    public List<Canvas> PortraitCanva;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        foreach (Canvas i in LandscapeCanva)
            i.enabled = (Screen.width > Screen.height);
        foreach (Canvas i in PortraitCanva)
            i.enabled = (Screen.width <= Screen.height);
    }
}
