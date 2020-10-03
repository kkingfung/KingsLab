using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ISceneChange : MonoBehaviour
{
    public List<GameObject> LandscapeObjs;
    public List<GameObject> PortraitObjs;
    protected  FadeEffect fadeQuad;

    delegate void FadeAction();
    FadeAction FadeInDelegate;
    FadeAction FadeOutDelegate;

    protected void Start()
    {
        fadeQuad = FindObjectOfType<FadeEffect>();
        if (fadeQuad) {
            FadeInDelegate += fadeQuad.FadeIn;
            FadeOutDelegate += fadeQuad.FadeOut;
        }
    }
    protected void OnDisable()
    {
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
}
