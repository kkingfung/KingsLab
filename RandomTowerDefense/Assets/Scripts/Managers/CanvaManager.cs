using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvaManager : MonoBehaviour
{
    public List<GameObject> LandscapeCanva_Main;
    public List<GameObject> LandscapeCanva_Sub;
    public List<GameObject> LandscapeCanva_Open;

    public List<GameObject> PortraitCanva_Main;
    public List<GameObject> PortraitCanva_Sub;
    public List<GameObject> PortraitCanva_Open;
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
        foreach (GameObject i in LandscapeCanva_Main)
            i.SetActive(isOpening && (Screen.width > Screen.height));
        foreach (GameObject i in LandscapeCanva_Sub)
            i.SetActive(!isOpening && (Screen.width > Screen.height));

        foreach (GameObject i in PortraitCanva_Main)
            i.SetActive(isOpening && (Screen.width <= Screen.height));
        foreach (GameObject i in PortraitCanva_Sub)
            i.SetActive(!isOpening && (Screen.width <= Screen.height));

        foreach (GameObject i in LandscapeCanva_Open)
            i.SetActive(isOption && (Screen.width > Screen.height));
        foreach (GameObject i in PortraitCanva_Open)
            i.SetActive(isOption && (Screen.width <= Screen.height));
    }
}
