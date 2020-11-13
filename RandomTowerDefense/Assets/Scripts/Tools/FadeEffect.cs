using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeEffect : MonoBehaviour
{
     private float Threshold = 0.0f;
    private readonly float FadeRate = 0.02f;
    private float ThresholdRecord;
    Material FadeMat;
    public bool isReady { get; private set;}

    private void Awake()
    {
        if (FadeMat == null && GetComponent<MeshRenderer>()) FadeMat = GetComponent<MeshRenderer>().material;
        if (FadeMat == null && GetComponent<RawImage>()) FadeMat = GetComponent<RawImage>().material;
        if (FadeMat == null && GetComponent<SpriteRenderer>()) FadeMat = GetComponent<SpriteRenderer>().material;
        if (FadeMat == null && GetComponent<Image>()) FadeMat = GetComponent<Image>().material;
        FadeMat.SetFloat("_FadeThreshold", 1f);
        PlayerPrefs.SetFloat("_FadeThreshold", 1f);
        ThresholdRecord = Threshold;
    }
    private void Update() {
        if (ThresholdRecord != Threshold)
        {
            FadeMat.SetFloat("_FadeThreshold", Threshold);
            ThresholdRecord = Threshold;
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
        if (FadeMat == null && GetComponent<MeshRenderer>()) FadeMat = GetComponent<MeshRenderer>().material;
        if (FadeMat == null && GetComponent<RawImage>()) FadeMat = GetComponent<RawImage>().material;
        if (FadeMat == null && GetComponent<SpriteRenderer>()) FadeMat = GetComponent<SpriteRenderer>().material;
        if (FadeMat == null && GetComponent<Image>()) FadeMat = GetComponent<Image>().material;

        while (Threshold > 0f) {
            Threshold -= FadeRate;
            PlayerPrefs.SetFloat("_FadeThreshold", Threshold);
            yield return new WaitForSeconds(0f);
        }
        isReady = true;
    }

    private IEnumerator FadeInRoutine()
    {
        if (FadeMat == null && GetComponent<MeshRenderer>()) FadeMat = GetComponent<MeshRenderer>().material;
        if (FadeMat == null && GetComponent<RawImage>()) FadeMat = GetComponent<RawImage>().material;
        if (FadeMat == null && GetComponent<SpriteRenderer>()) FadeMat = GetComponent<SpriteRenderer>().material;
        if (FadeMat == null && GetComponent<Image>()) FadeMat = GetComponent<Image>().material;

        while (Threshold < 1f)
        {
            Threshold += FadeRate;
            PlayerPrefs.SetFloat("_FadeThreshold", Threshold);
            yield return new WaitForSeconds(0f);
        }
        isReady = true;
    }
}
