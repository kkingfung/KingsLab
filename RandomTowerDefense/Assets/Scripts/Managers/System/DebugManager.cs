using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    private readonly float timescaleChg = 0.5f;

    private float myTimeScale;
    private int CurrSpawningPoint;
    public WaveManager waveManager;
    public AudioManager audioManager;
    private float[] data;
    public bool ready;
    private float prev;
    private int frame;
    // Start is called before the first frame update
    void Start()
    {
        init();
        data = audioManager.GetClipWaveform("bgm_Battle");
        audioManager.PlayAudio("bgm_Battle", true);
        prev = data[frame % data.Length];
        Debug.Log(-1);
    }

    private void init()
    {
        myTimeScale = 1.0f;
        CurrSpawningPoint = 0;
        frame = 0;
        Time.timeScale = myTimeScale;
        Time.fixedDeltaTime = Time.timeScale;
        ready = false;
    }
    // Update is called once per frame
    void Update()
    {
        frame++;
        if (ready == false)
        {
            if (Mathf.Abs(prev - data[frame % data.Length]) > 0.02f)
            {
                ready = true;
            }
            prev = data[frame % data.Length];
        }
        return;

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
