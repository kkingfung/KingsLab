using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    public readonly float daytimeFactor = 30f;

    public List<Material> skyboxMat;
    public List<GameObject> terrainObj;
    private int stageID;
    private float shaderInput;
    private int maxSkyboxCubemap = 3;
    public bool showTerrain = false;
    //public TimeManager timeManager;
    public InGameOperation sceneManager;

    // Start is called before the first frame update
    private void Start()
    {
        //sceneManager = FindObjectOfType<InGameOperation>();
        stageID = sceneManager?sceneManager.GetCurrIsland():0;
        shaderInput = Time.time;
        //timeManager = FindObjectOfType<TimeManager>();
        RenderSettings.skybox = skyboxMat[stageID];

        if (showTerrain)
        {
            for (int i = 0; i < terrainObj.Count; ++i)
            {
                if (i == stageID) terrainObj[i].SetActive(true);
                else terrainObj[i].SetActive(false);
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (stageID == maxSkyboxCubemap) {
            shaderInput += Time.deltaTime / daytimeFactor;
            while (shaderInput > maxSkyboxCubemap) shaderInput -= maxSkyboxCubemap;
             skyboxMat[stageID].SetFloat("SkyboxFactor", shaderInput);
        } 
    }
}
