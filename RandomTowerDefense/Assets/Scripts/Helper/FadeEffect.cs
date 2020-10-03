using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeEffect : MonoBehaviour
{
    float Threshold;
    float FadeRate = 0.01f;
    public bool isReady { get; private set;}
    public void FadeIn() {
        Threshold = 0.0f;
        isReady = false;
        StartCoroutine(FadeInRoutine());
    }
    public void FadeOut()
    {
        Threshold = 1.0f;
        isReady = false;
        StartCoroutine(FadeOutRoutine());
    }

    public IEnumerator FadeOutRoutine()
    {
        while (Threshold > 0f) {
            GetComponent<MeshRenderer>().material.SetFloat("_FadeThreshold", Threshold);
            Debug.Log(Threshold);
            Threshold -= FadeRate;
            yield return new WaitForSeconds(0f);
        }
        Threshold = 0f;
        GetComponent<MeshRenderer>().material.SetFloat("_FadeThreshold", Threshold);
        isReady = true;
    }

    public IEnumerator FadeInRoutine()
    {
        while (Threshold < 1f)
        {
            GetComponent<MeshRenderer>().material.SetFloat("_FadeThreshold", Threshold);
            Threshold += FadeRate;
            yield return new WaitForSeconds(0f);
        }
        Threshold = 1f;
        GetComponent<MeshRenderer>().material.SetFloat("_FadeThreshold", Threshold);
        isReady = true;
    }
}
