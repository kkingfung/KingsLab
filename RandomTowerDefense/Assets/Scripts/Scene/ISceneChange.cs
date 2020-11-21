using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;

public class ISceneChange : MonoBehaviour
{
    protected struct LatiosSceneChangeDummyTag : IComponentData { }

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

    protected void Awake()
    {
        isSceneFinished = false;
        toDrag = 0;

        OrientationLock = false;
        fadeQuad = FindObjectsOfType<FadeEffect>();
        if (fadeQuad.Length > 0)
        {
            foreach (FadeEffect i in fadeQuad)
            {
                FadeInDelegate += i.FadeIn;
                FadeOutDelegate += i.FadeOut;
            }
        }

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entityManager.DestroyEntity(entityManager.GetAllEntities(Allocator.Temp));
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
    protected void SceneIn()
    {
        FadeInDelegate?.Invoke();
        //if (FadeInDelegate != null)
        //{
        //    FadeInDelegate();
        //}
    }
    protected void SceneOut()
    {
        OrientationLock = true;
        FadeOutDelegate?.Invoke();
        //if (FadeOutDelegate != null)
        //{
        //    FadeOutDelegate();
        //}
    }

    protected void Update()
    {
        if (!OrientationLock)
        {
            OrientationLand = Screen.width > Screen.height;
        }
        //else 
        //{
        //    Screen.orientation = OrientationLand? ScreenOrientation.Landscape: ScreenOrientation.Portrait;
        //}
        foreach (GameObject i in LandscapeObjs)
        {
            SpriteRenderer spr = i.GetComponent<SpriteRenderer>();
            if (spr)
            {
                spr.enabled = OrientationLand;
            }
            else
            {
                MeshRenderer mesh = i.GetComponent<MeshRenderer>();
                if (mesh)
                    mesh.enabled = OrientationLand;
                else
                {
                    i.SetActive(OrientationLand);
                }
            }
        }
        foreach (GameObject i in PortraitObjs)
        {
            SpriteRenderer spr = i.GetComponent<SpriteRenderer>();
            if (spr)
            {
                spr.enabled = !OrientationLand;
            }
            else
            {
                MeshRenderer mesh = i.GetComponent<MeshRenderer>();
                if (mesh)
                    mesh.enabled = !OrientationLand;
                else
                {
                    i.SetActive(!OrientationLand);
                }
            }
        }
    }

    protected void SetNextScene(string sceneName)
    {
        PlayerPrefs.SetString("nextScene", sceneName);
    }

    public bool GetOptionStatus() { return isOption; }
    public void SetOptionStatus(bool enabled) { isOption = enabled; }

}
