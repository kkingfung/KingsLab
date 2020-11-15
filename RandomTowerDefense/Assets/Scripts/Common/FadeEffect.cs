﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeEffect : MonoBehaviour
{
    //private enum EnumRenderType
    //{
    //    NotChecked = 0,
    //    FoundMeshRenderer,
    //    FoundRawImg,
    //    FoundSprRenderer,
    //    FoundImage,
    //}
    //private EnumRenderType rendertype;

    private float Threshold = 0.0f;
    private readonly float FadeRate = 0.02f;
    private float ThresholdRecord;
    Material FadeMat;
    public bool isReady { get; private set;}

    private void Awake()
    {
        if (FadeMat == null)
            GetFadeMaterial();
        FadeMat.SetFloat("_FadeThreshold", 1f);
        PlayerPrefs.SetFloat("_FadeThreshold", 1f);
        ThresholdRecord = Threshold;
    }
    private void Update() {
        Threshold = PlayerPrefs.GetFloat("_FadeThreshold");
        if (ThresholdRecord != Threshold)
        {
            FadeMat.SetFloat("_FadeThreshold", Threshold);
            ThresholdRecord = Threshold;
        }
      
    }
    public void GetFadeMaterial()
    {
        MeshRenderer chkMeshRender = GetComponent<MeshRenderer>();
        if (chkMeshRender)
        {
            FadeMat = chkMeshRender.material;
            //rendertype = EnumRenderType.FoundMeshRenderer;
        }
        else
        {
            RawImage chkRawImg = GetComponent<RawImage>();
            if (chkRawImg)
            {
                FadeMat = chkRawImg.material;
                //rendertype = EnumRenderType.FoundRawImg;
            }
            else
            {
                SpriteRenderer chkSprRender = GetComponent<SpriteRenderer>();
                if (chkSprRender)
                {
                    FadeMat = chkSprRender.material;
                    //rendertype = EnumRenderType.FoundSprRenderer;
                }
                else
                {
                    Image chkImage = GetComponent<Image>();
                    if (chkImage)
                    {
                        FadeMat = chkImage.material;
                        //rendertype = EnumRenderType.FoundImage;
                    }
                }
            }
        }
    }

    public void FadeIn() {
        Threshold = 0.0f;
        PlayerPrefs.SetFloat("_FadeThreshold", Threshold);
        isReady = false;
        if (this.gameObject.activeInHierarchy)
            StartCoroutine(FadeInRoutine());
    }
    public void FadeOut()
    {
        Threshold = 1.0f;
        PlayerPrefs.SetFloat("_FadeThreshold", Threshold);
        isReady = false;
        if (this.gameObject.activeInHierarchy)
            StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        if (FadeMat == null)
            GetFadeMaterial();

        while (Threshold > 0f) {
            Threshold -= FadeRate;
            PlayerPrefs.SetFloat("_FadeThreshold", Threshold);
            yield return new WaitForSeconds(0f);
        }
        isReady = true;
    }

    private IEnumerator FadeInRoutine()
    {
        if (FadeMat == null)
            GetFadeMaterial();

        while (Threshold < 1f)
        {
            Threshold += FadeRate;
            PlayerPrefs.SetFloat("_FadeThreshold", Threshold);
            yield return new WaitForSeconds(0f);
        }
        isReady = true;
    }
}
