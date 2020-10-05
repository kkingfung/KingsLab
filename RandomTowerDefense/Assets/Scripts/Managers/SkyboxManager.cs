using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxManager : MonoBehaviour
{
    public List<Material> skyboxMat;
    int stageID;
    const float timeFactor=120f;
    float shaderInput;
    int maxSkyboxCubemap = 3;
    //RenderSettings.skybox
    // Start is called before the first frame update
    void Start()
    {
        stageID = 3;//= PlayerPrefs.GetInt("StageID");
        RenderSettings.skybox = skyboxMat[stageID];
    }

    // Update is called once per frame
    void Update()
    {
        if (stageID == 3) {
            shaderInput = (Time.time / timeFactor);
            while (shaderInput > maxSkyboxCubemap) shaderInput -= maxSkyboxCubemap;
             skyboxMat[stageID].SetFloat("SkyboxFactor", shaderInput);
        } 
    }
}
