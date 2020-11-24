using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    private readonly float timescaleChg = 0.5f;

    private float myTimeScale;
    private int CurrSpawningPoint;
    public WaveManager waveManager;

    // Start is called before the first frame update
    void Start()
    {
        init();
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
        Debug.Log(myTimeScale);
    }
}
