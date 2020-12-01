using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Entities;

public class LoadingOperation : ISceneChange
{
    private readonly float LoadingSpd = 0.02f;
    [HideInInspector]
    public string nextScene;
    public List<GameObject> RandomObjs;
    public List<GameObject> LoadingIcon;
    private List<MeshRenderer> LoadingIconRenderer;

    AsyncOperation loadingOperation;
    private bool isLoading;
    // Start is called before the first frame update
    private void Start()
    {
        base.SceneIn();
        LoadingIconRenderer = new List<MeshRenderer>();
        for (int i=0;i< LoadingIcon.Count;++i)
        {
            LoadingIconRenderer.Add(LoadingIcon[i].GetComponent<MeshRenderer>());
            LoadingIconRenderer[i].material.SetFloat("_DissolveAmount", 2);
        }

        nextScene = PlayerPrefs.GetString("nextScene","TitleScene");
        isLoading = false; 
        if (RandomObjs.Count == 0) return;
        Random.InitState(Time.frameCount);
        int RndNum = Random.Range(0,RandomObjs.Count-1);
        foreach (GameObject i in RandomObjs) {
            i.SetActive(false);
        }
        RandomObjs[RndNum].SetActive(true);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        bool chkFadeReady = false;
        foreach (FadeEffect i in fadeQuad) {
            if (i.isReady) {
                chkFadeReady = true;
                break;
            }
        }

        //LoadingScene Animation
        if (isSceneFinished || LoadingIcon.Count == 0 || !chkFadeReady) {
            return; 
        }

        if (!isLoading && chkFadeReady)
        {
            StartCoroutine("FadeLoadingScreen");
            isLoading = true;
        }
    }

    IEnumerator FadeLoadingScreen()
    {
        loadingOperation = SceneManager.LoadSceneAsync(nextScene);
        loadingOperation.allowSceneActivation = false;
        float duration=0.0f;
        float progress;
        while (duration < 1)
        {
            progress = Mathf.Clamp01(loadingOperation.progress / 0.9f);
            if (duration < progress) duration += LoadingSpd;
            foreach (MeshRenderer i in LoadingIconRenderer)
            {
                i.material.SetFloat("_DissolveAmount", 2.0f - duration * 2.0f);
            }
            yield return null;
        }

        isSceneFinished = true;
        SceneOut();

        bool chkFadeReady = false;
        while (!chkFadeReady) {
            foreach (FadeEffect i in fadeQuad)
            {
                if (i.isReady)
                {
                    chkFadeReady = true;
                    break;
                }
            }
            yield return null;
        }

        loadingOperation.allowSceneActivation = true;
    }
}
