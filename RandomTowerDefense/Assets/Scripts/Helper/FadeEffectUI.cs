using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FadeEffectUI : MonoBehaviour
{
    float Threshold;
    float FadeRate = 0.05f;

    Material FadeMat;
    public bool isReady { get; private set;}

    private void Start()
    {
        FadeMat = GetComponent<RawImage>().material;
        FadeMat.SetFloat("_FadeThreshold", 1f);
    }

    public void FadeIn() {
        Threshold = 0.0f;
        isReady = false;
        if (this.transform.parent.gameObject.activeSelf)
            StartCoroutine(FadeInRoutine());
    }
    public void FadeOut()
    {
        Threshold = 1.0f;
        isReady = false;
        if (this.transform.parent.gameObject.activeSelf)
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        while (Threshold > 0f) {
            FadeMat.SetFloat("_FadeThreshold", Threshold);
            Threshold -= FadeRate;
            yield return new WaitForSeconds(0f);
        }
        Threshold = 0f;
        FadeMat.SetFloat("_FadeThreshold", Threshold);
        isReady = true;
    }

    private IEnumerator FadeInRoutine()
    {
        while (Threshold < 1f)
        {
            FadeMat.SetFloat("_FadeThreshold", Threshold);
            Threshold += FadeRate;
            yield return new WaitForSeconds(0f);
        }
        Threshold = 1f;
        FadeMat.SetFloat("_FadeThreshold", Threshold);
        isReady = true;
    }
}
