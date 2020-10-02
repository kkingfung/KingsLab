using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingOperation : MonoBehaviour
{
    public Scene nextScene;
    bool isFinished;
    // Start is called before the first frame update
    void Start()
    {
        isFinished = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(isFinished && nextScene!=null)
            SceneManager.LoadScene(nextScene.name, LoadSceneMode.Additive);
    }
}
