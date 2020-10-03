using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingOperation : ISceneChange
{
    public string nextScene;
    public List<GameObject> RandomObjs;
    public List<GameObject> LoadingIcon;

    bool isFinished;
    float Progress;

    // Start is called before the first frame update
    private void Start()
    {
        base.Start();
        base.SceneIn();

        Progress = 0;
        isFinished = false;
        if (RandomObjs.Count == 0) return;
        Random.seed = Time.frameCount;
        int RndNum = Random.Range(0,RandomObjs.Count-1);
        foreach (GameObject i in RandomObjs) {
            i.SetActive(false);
        }
        RandomObjs[RndNum].SetActive(true); 
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();

        //Change Scene
        if (isFinished && nextScene != null)
        {
            if (fadeQuad.isReady)
                SceneManager.LoadScene(nextScene);
        }

        //LoadingScene Animation
        if (isFinished||LoadingIcon.Count == 0 || fadeQuad.isReady==false) return;
        //Amend if necessary
        if (Progress < 1) Progress += 0.01f;
        else
        {
            isFinished = true;
            if (fadeQuad) SceneOut();
        }
        foreach (GameObject i in LoadingIcon)
        {
            i.GetComponent<MeshRenderer>().material.SetFloat("_DissolveAmount", 2.0f-Progress*2.0f);
        }
    }
}
