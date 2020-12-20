using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Unity.Simulation.Games;

public class DebugManager : MonoBehaviour
{
    private readonly float timescaleChg = 1f;

    private float myTimeScale;
    private int CurrSpawningPoint;
    public WaveManager waveManager;

    //Simulation Parameter
    public bool isSimulationTest;
    [HideInInspector]
    public bool isFetchDone;

    //public float towerrank_Damage = 0;
    //public float towerlvl_Damage = 0;
    //public float enemylvl_Health = 0;
    //public float enemylvl_Speed = 0;

    private void Awake()
    {
        isFetchDone = false;
        if (isSimulationTest)
        {
            //GameSimManager.Instance.FetchConfig(OnFetchConfigDone);
        }
        else
            isFetchDone = true;
    }
    // Start is called before the first frame update
    void Start()
    {
        init();
    }

    //private void OnFetchConfigDone(GameSimConfigResponse gameSimConfigResponse)
    //{
    //    towerrank_Damage = gameSimConfigResponse.GetFloat("towerrank_Damage");
    //    towerlvl_Damage = gameSimConfigResponse.GetFloat("towerlvl_Damage");
    //    enemylvl_Health = gameSimConfigResponse.GetFloat("enemylvl_Health");
    //    enemylvl_Speed = gameSimConfigResponse.GetFloat("enemylvl_Speed");
    //
    //    isFetchDone = true;
    //}

    public void MapResetted()
    {
        if (isSimulationTest == false) return;
    //    GameSimManager.Instance.SetCounter("WaveArrived", waveManager.GetCurrentWaveNum());
        //Debug.Log("QuitNow");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void init()
    {
        myTimeScale = 1.0f;
        CurrSpawningPoint = 0;
        Time.timeScale = myTimeScale;
        Time.fixedDeltaTime = Time.timeScale;
    }

    // Update is called once per frame
    void Update()
    {

        if (waveManager)
            CurrSpawningPoint = waveManager.SpawnPointByAI;

        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            init();
        }
        else
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            ChangeSpeed(-1);
        }
        else
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            ChangeSpeed(+1);
        }
    }

    void ChangeSpeed(int decision)
    {
        myTimeScale += decision * timescaleChg;
        if (myTimeScale < 0) myTimeScale = 0;
        Time.timeScale = myTimeScale;
        Time.fixedDeltaTime = Time.timeScale;
        //Debug.Log(myTimeScale);
    }
}
