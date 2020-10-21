using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    //Remark: Special Function for extra mode- infinity wave
   int TotalWaveNum;
   int CurrentWaveNum;
    int CurrIsland;
    StageAttr CurrAttr;

    // Start is called before the first frame update
    void Start()
    {
        CurrIsland = PlayerPrefs.GetInt("IslandNow");

        switch (CurrIsland) {
            case 0: CurrAttr = StageInfo.GetStageInfo("Easy"); break;
            case 1: CurrAttr = StageInfo.GetStageInfo("Normal"); break;
            case 2: CurrAttr = StageInfo.GetStageInfo("Hard"); break;
            case 3: CurrAttr = StageInfo.GetStageInfo("Extra"); break;
        }
        TotalWaveNum = CurrAttr.waveNum;
        CurrentWaveNum = 0;
    }

    public int GetTotalWaveNum() { return TotalWaveNum; }
    public int GetCurrentWaveNum() { return CurrentWaveNum; }

    // Update is called once per frame
    void Update()
    {
        
    }
}
