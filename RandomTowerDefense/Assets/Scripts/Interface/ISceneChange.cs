using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ISceneChange : MonoBehaviour
{
    [Header("Gyro Settings")]
    public List<GameObject> LandscapeObjs;
    public List<GameObject> PortraitObjs;

    protected  FadeEffect fadeQuad;
    protected  FadeEffectUI[] fadeQuadUI;

    delegate void FadeAction();
    FadeAction FadeInDelegate;
    FadeAction FadeOutDelegate;

    protected bool isSceneFinished;

    protected void Start()
    {
        isSceneFinished = false;

        fadeQuadUI = FindObjectsOfType<FadeEffectUI>();
        if (fadeQuadUI.Length>0) {
            foreach (FadeEffectUI i in fadeQuadUI) {
                FadeInDelegate += i.FadeIn;
                FadeOutDelegate += i.FadeOut;
            }
        }

        fadeQuad = FindObjectOfType<FadeEffect>();
        if (fadeQuad) {
            FadeInDelegate += fadeQuad.FadeIn;
            FadeOutDelegate += fadeQuad.FadeOut;
        }
    }
    protected void OnDisable()
    {
        if (fadeQuadUI.Length > 0)
        {
            foreach (FadeEffectUI i in fadeQuadUI)
            {
                FadeInDelegate -= i.FadeIn;
                FadeOutDelegate -= i.FadeOut;
            }
        }

        if (fadeQuad)
        {
            FadeInDelegate -= fadeQuad.FadeIn;
            FadeOutDelegate -= fadeQuad.FadeOut;
        }
    }
    protected void SceneIn() {
        if (FadeInDelegate != null) {
            FadeInDelegate();
        }
    }
    protected void SceneOut() {
        if (FadeOutDelegate != null)
        {
            FadeOutDelegate();
        }
    }

    protected void Update()
    {
        foreach (GameObject i in LandscapeObjs)
            i.SetActive(Screen.width > Screen.height);
        foreach (GameObject i in PortraitObjs)
            i.SetActive(Screen.width <= Screen.height);
    }

    protected void SetNextScene(string sceneName) {
        PlayerPrefs.SetString("nextScene", sceneName);
    }
}
