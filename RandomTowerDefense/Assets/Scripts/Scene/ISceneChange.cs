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
    private SpriteRenderer[] LandscapeSpr;
    private MeshRenderer[] LandscapeMesh;
    public List<GameObject> PortraitObjs;
    private SpriteRenderer[] PortraitSpr;
    private MeshRenderer[] PortraitMesh;

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

    protected virtual void Awake()
    {
        isSceneFinished = false;
        toDrag = 0;

        OrientationLock = false;
        fadeQuad = FindObjectsOfType<FadeEffect>();
        if (fadeQuad != null && fadeQuad.Length > 0)
        {
            foreach (FadeEffect i in fadeQuad)
            {
                FadeInDelegate += i.FadeIn;
                FadeOutDelegate += i.FadeOut;
            }
        }

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entityManager.DestroyEntity(entityManager.GetAllEntities(Allocator.Temp));

        LandscapeSpr = new SpriteRenderer[LandscapeObjs.Count];
        LandscapeMesh = new MeshRenderer[LandscapeObjs.Count];

        PortraitSpr = new SpriteRenderer[PortraitObjs.Count];
        PortraitMesh = new MeshRenderer[PortraitObjs.Count];

        for (int i = 0; i < LandscapeObjs.Count; ++i)
        {
            LandscapeSpr[i] = LandscapeObjs[i].GetComponent<SpriteRenderer>();
            LandscapeMesh[i] = LandscapeObjs[i].GetComponent<MeshRenderer>();
        }
        for (int i = 0; i < PortraitObjs.Count; ++i)
        {
            PortraitSpr[i] = PortraitObjs[i].GetComponent<SpriteRenderer>();
            PortraitMesh[i] = PortraitObjs[i].GetComponent<MeshRenderer>();
        }
    }

    protected void OnDisable()
    {
        if (fadeQuad!=null && fadeQuad.Length > 0)
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

    protected virtual void Update()
    {
        if (!OrientationLock)
        {
            OrientationLand = Screen.width > Screen.height;
        }
        //else 
        //{
        //    Screen.orientation = OrientationLand? ScreenOrientation.Landscape: ScreenOrientation.Portrait;
        //}


        for (int i = 0; i < LandscapeObjs.Count; ++i)
        {
            if(LandscapeSpr[i])
                LandscapeSpr[i].enabled = OrientationLand;
            else if(LandscapeMesh[i])
                LandscapeMesh[i].enabled = OrientationLand;
            else
                LandscapeObjs[i].SetActive(OrientationLand);
        }

        for (int i = 0; i < PortraitObjs.Count; ++i)
        {
            if (PortraitSpr[i])
                PortraitSpr[i].enabled = !OrientationLand;
            else if (PortraitMesh[i])
                PortraitMesh[i].enabled = !OrientationLand;
            else
                PortraitObjs[i].SetActive(!OrientationLand);
        }
    }

    protected void SetNextScene(string sceneName)
    {
        PlayerPrefs.SetString("nextScene", sceneName);
    }

    public bool GetOptionStatus() { return isOption; }
    public void SetOptionStatus(bool enabled) { isOption = enabled; }

}
