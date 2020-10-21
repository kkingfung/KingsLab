using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ISceneChange : MonoBehaviour
{
    protected readonly int IslandNum = 4;
    
    [Header("Gyro Settings")]
    public List<GameObject> LandscapeObjs;
    public List<GameObject> PortraitObjs;

    protected FadeEffect[] fadeQuad;

    delegate void FadeAction();
    FadeAction FadeInDelegate;
    FadeAction FadeOutDelegate;

    protected bool isSceneFinished;
    protected bool isOption;

    [HideInInspector]
    public int toDrag;// 0: No Action, 1: Left Dir, 2: Right Dir

    [HideInInspector]
    public bool OrientationLand;
    public bool OrientationLock;

    protected void Start()
    {
        isSceneFinished = false;
        toDrag = 0;

        OrientationLock = false;
        fadeQuad = FindObjectsOfType<FadeEffect>();
        if (fadeQuad.Length > 0) {
            foreach (FadeEffect i in fadeQuad)
            {
                FadeInDelegate += i.FadeIn;
                FadeOutDelegate += i.FadeOut;
            }
        }
    }
    protected void OnDisable()
    {
        if (fadeQuad.Length > 0)
        {
            foreach (FadeEffect i in fadeQuad)
            {
                FadeInDelegate -= i.FadeIn;
                FadeOutDelegate -= i.FadeOut;
            }
        }
    }
    protected void SceneIn() {
        if (FadeInDelegate != null) {
            FadeInDelegate();
        }
    }
    protected void SceneOut() {
        OrientationLock = true;
        if (FadeOutDelegate != null)
        {
            FadeOutDelegate();
        }
    }

    protected void Update()
    {
        if(!OrientationLock)
        OrientationLand = Screen.width > Screen.height;

        foreach (GameObject i in LandscapeObjs)
            i.SetActive(OrientationLand);
        foreach (GameObject i in PortraitObjs)
            i.SetActive(!OrientationLand);
    }

    protected void SetNextScene(string sceneName) {
        PlayerPrefs.SetString("nextScene", sceneName);
    }

    public bool GetOptionStatus() { return isOption; }

    public int GetTotalIslandNum() { return IslandNum; }
}
