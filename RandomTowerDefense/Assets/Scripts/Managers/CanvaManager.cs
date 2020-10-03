using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvaManager : MonoBehaviour
{
    public List<Canvas> LandscapeCanva_Main;
    public List<Canvas> LandscapeCanva_Sub;
    public List<Canvas> LandscapeCanva_Open;

    public List<Canvas> PortraitCanva_Main;
    public List<Canvas> PortraitCanva_Sub;
    public List<Canvas> PortraitCanva_Open;
    [HideInInspector]
    public bool isOpening;
    [HideInInspector]
    public bool isOption;
    // Use this for initialization
    void Start()
    {
        isOpening = true;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Canvas i in LandscapeCanva_Main)
            i.enabled = isOpening && (Screen.width > Screen.height);
        foreach (Canvas i in LandscapeCanva_Sub)
            i.enabled = !isOpening && (Screen.width > Screen.height);

        foreach (Canvas i in PortraitCanva_Main)
            i.enabled = isOpening && (Screen.width <= Screen.height);
        foreach (Canvas i in PortraitCanva_Sub)
            i.enabled = !isOpening && (Screen.width <= Screen.height);

        if (isOption) {
            foreach (Canvas i in LandscapeCanva_Open)
                i.enabled = (Screen.width > Screen.height);
            foreach (Canvas i in PortraitCanva_Open)
                i.enabled = (Screen.width <= Screen.height);
        }
    }
}
