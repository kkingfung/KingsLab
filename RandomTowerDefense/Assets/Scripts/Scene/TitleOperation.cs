using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleOperation: ISceneChange
{
    public Scene nextScene;
    public GameObject BoidSpawn;

    AudioManager AudioManager;
    InputManager inputMgt;
    bool isOpening;
   
    private void OnEnable()
    {
        BoidSpawn.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        isOpening = true;
        inputMgt = FindObjectOfType<InputManager>();
        AudioManager = FindObjectOfType<AudioManager>();

        AudioManager.PlayAudio("bgm_Opening");
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();

        if (isOpening && inputMgt.GetAnyInput()) BoidSpawnEffect();
    }

    void BoidSpawnEffect()
    {
        BoidSpawn.transform.position = Camera.main.transform.position- Camera.main.transform.forward*2.0f;
        GameObject.FindGameObjectWithTag("BoidWall").transform.position = BoidSpawn.transform.position;
        BoidSpawn.SetActive(true);
        isOpening = false;
        AudioManager.PlayAudio("bgm_Title");
    }
}
