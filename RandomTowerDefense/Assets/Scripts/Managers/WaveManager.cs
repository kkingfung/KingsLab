using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveManager : MonoBehaviour
{
    //Remark: Special Function for extra mode- infinity wave
   private int TotalWaveNum;
   private int CurrentWaveNum;
   private int CurrIsland;
   private StageAttr CurrAttr;
    public  List<Text> waveNumUI;

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
        WaveChg();
    }

    public int GetTotalWaveNum() { return TotalWaveNum; }
    public int GetCurrentWaveNum() { return CurrentWaveNum; }

    public bool WaveChg() {
        CurrentWaveNum++;
        if (CurrentWaveNum > TotalWaveNum) return true;
        foreach (Text i in waveNumUI) {
            i.text = "WAVE " + CurrentWaveNum;
            i.color=new Color(i.color.r,i.color.g,i.color.b,1.0f);
        }
        return false;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
