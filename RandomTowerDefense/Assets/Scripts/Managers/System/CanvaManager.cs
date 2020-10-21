using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvaManager : MonoBehaviour
{
    public List<GameObject> LandscapeCanva_Main;
    public List<GameObject> LandscapeCanva_Sub;
    public List<GameObject> LandscapeCanva_Opt;

    public List<GameObject> PortraitCanva_Main;
    public List<GameObject> PortraitCanva_Sub;
    public List<GameObject> PortraitCanva_Opt;
    [HideInInspector]
    public bool isOpening;
    [HideInInspector]
    public bool isOption;

    ISceneChange SceneManager;

    // Use this for initialization
    void Start()
    {
        isOpening = true;
        SceneManager = FindObjectOfType<ISceneChange>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (GameObject i in LandscapeCanva_Main)
            i.SetActive(isOpening && (SceneManager.OrientationLand));
        foreach (GameObject i in LandscapeCanva_Sub)
            i.SetActive(!isOpening && (SceneManager.OrientationLand));

        foreach (GameObject i in PortraitCanva_Main)
            i.SetActive(isOpening && (!SceneManager.OrientationLand));
        foreach (GameObject i in PortraitCanva_Sub)
            i.SetActive(!isOpening && (!SceneManager.OrientationLand));

        foreach (GameObject i in LandscapeCanva_Opt)
            i.SetActive(isOption && (SceneManager.OrientationLand));
        foreach (GameObject i in PortraitCanva_Opt)
            i.SetActive(isOption && (!SceneManager.OrientationLand));
    }
}
