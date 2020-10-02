using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingOperation : MonoBehaviour
{
    public Scene nextScene;
    public List<GameObject> LandscapeObjs;
    public List<GameObject> PortraitObjs;
    public List<GameObject> RandomObjs;

    public List<GameObject> LoadingIcon;

    bool isFinished;
    float Progress;

    // Start is called before the first frame update
    void Start()
    {
        Progress = 0;
        isFinished = false;
        if (RandomObjs.Count == 0) return;
        int RndNum = Random.Range(0,RandomObjs.Count-1);
        foreach (GameObject i in RandomObjs) {
            i.SetActive(false);
        }
        RandomObjs[RndNum].SetActive(true); 
    }

    // Update is called once per frame
    void Update()
    {
        foreach (GameObject i in LandscapeObjs)
            i.SetActive(Screen.width > Screen.height);
            foreach (GameObject i in PortraitObjs)
                i.SetActive(Screen.width <= Screen.height);

        if (isFinished && nextScene.IsValid())
            SceneManager.LoadScene(nextScene.name, LoadSceneMode.Additive);

        if (Progress < 1) Progress += 0.01f;
        else isFinished = true;

        if (LoadingIcon.Count == 0) return;
        foreach (GameObject i in LoadingIcon)
        {
            i.GetComponent<MeshRenderer>().material.SetFloat("_DissolveAmount", 2.0f-Progress*2.0f);
        }
    }
}
