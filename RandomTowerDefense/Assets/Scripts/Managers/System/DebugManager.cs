using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    private readonly float timescaleChg = 1f;
    private readonly float BGMSpawnThreshold = 0.1f;//0.02f;

    private float myTimeScale;
    private int CurrSpawningPoint;
    public WaveManager waveManager;
    public AudioManager audioManager;
    private float[] data;
    public bool ready;
    private float prev;
    public float time;
    // Start is called before the first frame update
    void Start()
    {
        init();
        data = audioManager.GetClipWaveform("bgm_Battle");
        audioManager.PlayAudio("bgm_Battle", true);
        prev = data[((int)time * 44100) % data.Length];
    }

    private void init()
    {
        myTimeScale = 1.0f;
        CurrSpawningPoint = 0;
        time = Time.time;
        Time.timeScale = myTimeScale;
        Time.fixedDeltaTime = Time.timeScale;
        ready = false;
    }
    // Update is called once per frame
    void Update()
    {
        if (ready == false && Time.time - time >= 0.25f)
        {
            //if (data[(int)(time* 0.25f * 44100) % data.Length] - prev > BGMSpawnThreshold)
            if (data[(int)(time * 0.25f * 44100) % data.Length] > BGMSpawnThreshold)
                ready = true;
            //prev = data[(int)(time* 0.25f * 44100) % data.Length];
            time += 0.25f;
        }
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
