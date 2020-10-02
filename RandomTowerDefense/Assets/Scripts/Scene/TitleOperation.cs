using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleOperation: MonoBehaviour
{
    public Scene nextScene;
    public List<GameObject> LandscapeObjs;
    public List<GameObject> PortraitObjs;
    public GameObject BoidSpawn;

    InputManager inputMgt;
    bool isOpening;
    private void OnEnable()
    {
        BoidSpawn.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {
        isOpening = true;
        inputMgt = FindObjectOfType<InputManager>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (GameObject i in LandscapeObjs)
            i.SetActive(Screen.width > Screen.height);
        foreach (GameObject i in PortraitObjs)
            i.SetActive(Screen.width <= Screen.height);

        if (!isOpening)
        {
            BoidSpawn.SetActive(true);
        }
        else {
            isOpening=!inputMgt.GetAnyInput();
        }
    }
}
