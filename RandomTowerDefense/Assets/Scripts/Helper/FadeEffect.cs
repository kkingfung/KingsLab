using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeEffect : MonoBehaviour
{
    float Threshold = 0.0f;
    float FadeRate = 0.01f;

    Material FadeMat;
    public bool isReady { get; private set;}

    private void Start()
    {
        if (FadeMat == null && GetComponent<MeshRenderer>()) FadeMat = GetComponent<MeshRenderer>().material;
        if (FadeMat == null && GetComponent<RawImage>()) FadeMat = GetComponent<RawImage>().material;
        if (FadeMat == null && GetComponent<SpriteRenderer>()) FadeMat = GetComponent<SpriteRenderer>().material;
        FadeMat.SetFloat("_FadeThreshold", 1f);
        PlayerPrefs.SetFloat("_FadeThreshold", 1f);
    }
    private void Update() {
        FadeMat.SetFloat("_FadeThreshold",PlayerPrefs.GetFloat("_FadeThreshold"));
    }

    public void FadeIn() {
        Threshold = 0.0f;
        isReady = false;
        if (this.gameObject.activeInHierarchy)
            StartCoroutine(FadeInRoutine());
    }
    public void FadeOut()
    {
        Threshold = 1.0f;
        isReady = false;
        if (this.gameObject.activeInHierarchy)
            StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        if (FadeMat == null && GetComponent<MeshRenderer>()) FadeMat = GetComponent<MeshRenderer>().material;
        if (FadeMat == null && GetComponent<RawImage>()) FadeMat = GetComponent<RawImage>().material;
        if (FadeMat == null && GetComponent<SpriteRenderer>()) FadeMat = GetComponent<SpriteRenderer>().material;

        while (Threshold > 0f) {
            FadeMat.SetFloat("_FadeThreshold", Threshold);
            Threshold -= FadeRate;
            yield return new WaitForSeconds(0f);
        }
        FadeMat.SetFloat("_FadeThreshold", Threshold);
        PlayerPrefs.SetFloat("_FadeThreshold",Threshold);
        isReady = true;
    }

    private IEnumerator FadeInRoutine()
    {
        if (FadeMat == null && GetComponent<MeshRenderer>()) FadeMat = GetComponent<MeshRenderer>().material;
        if (FadeMat == null && GetComponent<RawImage>()) FadeMat = GetComponent<RawImage>().material;
        if (FadeMat == null && GetComponent<SpriteRenderer>()) FadeMat = GetComponent<SpriteRenderer>().material;

        while (Threshold < 1f)
        {
            FadeMat.SetFloat("_FadeThreshold", Threshold);
            Threshold += FadeRate;
            yield return new WaitForSeconds(0f);
        }
        FadeMat.SetFloat("_FadeThreshold", Threshold);
        PlayerPrefs.SetFloat("_FadeThreshold", Threshold);
        isReady = true;
    }
}
