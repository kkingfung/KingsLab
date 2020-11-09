using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    public List<Material> skyboxMat;
    public List<GameObject> terrainObj;
    int stageID;
    float shaderInput;
    int maxSkyboxCubemap = 3;

    TimeManager timeManager;
    //RenderSettings.skybox
    // Start is called before the first frame update
    void Start()
    {
        stageID =  PlayerPrefs.GetInt("IslandNow",0);
        timeManager = FindObjectOfType<TimeManager>();
        RenderSettings.skybox = skyboxMat[stageID];

        for(int i=0;i< terrainObj.Count;++i) {
            if (i == stageID) terrainObj[i].SetActive(true);
            else terrainObj[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (stageID == 3) {
            shaderInput = (Time.time / timeManager.daytimeFactor);
            while (shaderInput > maxSkyboxCubemap) shaderInput -= maxSkyboxCubemap;
             skyboxMat[stageID].SetFloat("SkyboxFactor", shaderInput);
        } 
    }
}
