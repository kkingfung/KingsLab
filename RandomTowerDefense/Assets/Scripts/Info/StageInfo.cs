using System.Collections;
using System.Collections.Generic;
using Unity.Entities.Editor;
using UnityEngine;


public class StageAttr
{
    public int waveNum;
    public float waveTime;

    public int[] enmNumPerWave;
    public string[] enmType;
    
    public int mazeSize;
    public int hpMax;

    public StageAttr(int waveNum,int mazeSize, int[] enmNumPerWave, string[] enmType, int hpMax = 30, float waveTime = 30f)
    {
        this.waveNum = waveNum;
        this.waveTime = waveTime;
        this.enmNumPerWave = enmNumPerWave;
        this.enmType = enmType;
        this.mazeSize = mazeSize;
        this.hpMax = hpMax;
    }
}

public class StageInfo: MonoBehaviour
{
    public static readonly int[] waveNumFactor = { 30, 35, 50, 999 };
    public static readonly int[] stageSizeFactor = { 50, 80, 120, 150 };
    public static readonly float[] enmSpeedFactor = { 0.5f, 1f,1.5f, 2f, 4f };
    public static readonly float[] spawnSpeedFactor = { 4f, 2f, 1.5f, 1f, 0.5f };
    public static readonly int[] hpMaxFactor = { 1, 5, 10, 30 };

    public int waveNumEx;
    public int stageSizeEx;
    public float enmSpeedEx;
    public float spawnSpeedEx;
    public int hpMaxEx;


    static Dictionary<string, StageAttr> stageInfo;
    static void Init()
    {
        stageInfo = new Dictionary<string, StageAttr>();
        //TODO: Add base Info

    }

    static void Release()
    {
        stageInfo.Clear();
    }

    public static UpdateCustomizedData(int waveNumCustom,int stageSizeCustom, float enmSpeedCustom, float spawnSpeedCustom,int hpMaxCustom) {

        stageInfo["Extra"].
    }

    public static StageAttr GetStageInfo(string enmName)
    {
        if (stageInfo.ContainsKey(enmName))
            return stageInfo[enmName];
        return null;
    }
}
