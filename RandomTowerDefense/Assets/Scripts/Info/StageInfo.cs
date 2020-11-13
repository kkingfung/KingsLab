﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Physics;
using UnityEditor;
using UnityEngine;

public class EnmDetail
{
    public int waveID;
    public int enmNum;
    public int enmPort;
    public string enmType;

    public EnmDetail(int waveID, int enmNum, int enmPort, string enmType)
    {
        this.waveID = waveID;
        this.enmNum = enmNum;
        this.enmPort = enmPort;
        this.enmType = enmType;
    }
}

public class WaveAttr
{
    public float enmStartTime;
    public float enmSpawnPeriod;
    public List<EnmDetail> enmDetail;
    public WaveAttr(float enmStartTime, float enmSpawnPeriod, List<EnmDetail> enmDetail)
    {
        this.enmStartTime = enmStartTime;
        this.enmSpawnPeriod = enmSpawnPeriod;
        this.enmDetail = enmDetail;
    }
    public WaveAttr(WaveAttr wave)
    {
        this.enmStartTime = wave.enmStartTime;
        this.enmSpawnPeriod = wave.enmSpawnPeriod;
        this.enmDetail = wave.enmDetail;
    }
}

public class StageAttr
{
    public int waveNum;
    public float waveWaitTime;
    public WaveAttr[] waveDetail;

    public StageAttr(int waveNum, WaveAttr[] waveAttr, float waveTime = 60f)
    {
        this.waveNum = waveNum;
        this.waveWaitTime = waveTime;
        this.waveDetail = waveAttr;
    }
}

public static class StageInfo
{
    public static readonly int IslandNum = 4;
    public static readonly int MaxMapDepth = 20;
    public static readonly int MinMapDepth = 6;

    private static StageAttr stageInfo;
    public static readonly string[] monsterCat0 = {
        "Slime","Mushroom","TurtleShell"
    };
    public static readonly string[] monsterCat1 = {
        "Footman","Grunt","TurtleShell",
        "FootmanS","GruntS","Skeleton",
        "StoneMonster"
    };
    public static readonly string[] monsterCat2 = {
        "FootmanS","GruntS","SpiderGhost",
        "SkeletonArmed","Golem","GolemS",
        "FreeLich",  "FreeLichS",  "Bull",
         "StoneMonster"
    };
    public static readonly string[] monsterCat3 = {
        "FootmanS","GruntS","SpiderGhost",
        "SkeletonArmed","GolemS", "FreeLichS",  
        "Bull", "Dragon"
    };
    //Extra Stage Customization
    public enum StageInfoID{
        Enum_stageSize = 0,
        Enum_waveNum,
        Enum_enmNum,
        Enum_enmSpeed,
        Enum_hpMax,
        Enum_spawnSpeed,
        Enum_resource
    }

    public static readonly int[] stageSizeFactor = { 36, 72, 108, 144 };//6*6 + 9*8 + 12*9 + 16*9
    public static readonly int[] waveNumFactor = { 30, 35, 50, 999 };
    public static readonly float[] enmNumFactor = { 0.5f, 1f, 2f, 4f, 8f };
    public static readonly float[] enmSpeedFactor = { 0.5f, 1f,1.5f, 2f, 4f };
    public static readonly int[] hpMaxFactor = { 1, 5, 10, 30 };

    public static readonly float[] spawnSpeedFactor = { 0.5f, 1f, 1.5f, 2f, 4f };
    public static readonly float[] resourceFactor = { 0.5f, 0.75f, 1f, 1.25f };
    //public static readonly float[] spawnSpeedFactor = { 4f, 2f, 1.5f, 1f, 0.5f };
    //public static readonly float[] resourceFactor = { 1.25f, 1f, 0.75f, 0.5f };

    public static float stageSizeEx =0;//Fill Map Generator
    public static float waveNumEx = 0;//StageInfo
    public static float enmNumEx = 0;//StageInfo
    public static float enmSpeedEx = 0;//WaveManager
    public static int hpMaxEx = 0;//CastleSpawner
    public static float spawnSpeedEx = 0;//StageInfo
    public static float resourceEx = 0;//WaveManager

    public static void Init()
    {
        UpdateCustomizedData();
        int temp = PlayerPrefs.GetInt("IslandNow",0);
        switch (temp)
        {
            case 0:
                stageInfo = new StageAttr(5, PrepareEasyStageInfo(5));
                break;
            case 1:
                stageInfo = new StageAttr(15, PrepareNormalStageInfo(15));
                break;
            case 2:
                stageInfo = new StageAttr(30, PrepareHardStageInfo(30));
                break;
            case 3:
                stageInfo = new StageAttr((int)PlayerPrefs.GetFloat("waveNum",10),
                    PrepareCustomStageInfo((int)PlayerPrefs.GetFloat("waveNum",10)));
                break;
        }
    }

    public static void SaveDataInPrefs_DirectInput(int infoID, float input)
    {
        switch ((StageInfoID)infoID)
        {
            case StageInfoID.Enum_waveNum:
                PlayerPrefs.SetFloat("waveNum", input);
                break;
            case StageInfoID.Enum_stageSize:
                PlayerPrefs.SetFloat("stageSize", (input > MaxMapDepth * MaxMapDepth) ? MaxMapDepth * MaxMapDepth :
                    (input < MinMapDepth * MinMapDepth ? MinMapDepth * MinMapDepth : input));
                break;
            case StageInfoID.Enum_enmNum:
                PlayerPrefs.SetFloat("enmNum", input);
                break;
            case StageInfoID.Enum_enmSpeed:
                PlayerPrefs.SetFloat("enmSpeed", input);
                break;
            case StageInfoID.Enum_spawnSpeed:
                PlayerPrefs.SetFloat("spawnSpeed", input);
                break;
            case StageInfoID.Enum_hpMax:
                PlayerPrefs.SetFloat("hpMax", input);
                break;
            case StageInfoID.Enum_resource:
                PlayerPrefs.SetFloat("resource", input);
                break;
        }
    }
    private static int GetNearestElement(int infoID) {
        int elementID = 0;
        float tempVal;
        switch ((StageInfoID)infoID)
        {
            case StageInfoID.Enum_waveNum:
                tempVal = PlayerPrefs.GetFloat("waveNum", 1);
                for (; elementID < waveNumFactor.Length; elementID++)
                {
                    if (waveNumFactor[elementID] > tempVal) break;
                }
                break;
            case StageInfoID.Enum_stageSize:
                tempVal = PlayerPrefs.GetFloat("stageSize", 1);
                for (; elementID < stageSizeFactor.Length; elementID++)
                {
                    if (stageSizeFactor[elementID] > tempVal) break;
                }
                break;
            case StageInfoID.Enum_enmNum:
                tempVal = PlayerPrefs.GetFloat("enmNum", 1);
                for (; elementID < enmNumFactor.Length; elementID++)
                {
                    if (enmNumFactor[elementID] > tempVal) break;
                }
                break;
            case StageInfoID.Enum_enmSpeed:
                tempVal = PlayerPrefs.GetFloat("enmSpeed", 1);
                for (; elementID < enmSpeedFactor.Length; elementID++)
                {
                    if (enmSpeedFactor[elementID] > tempVal) break;
                }
                break;
            case StageInfoID.Enum_spawnSpeed:
                tempVal = PlayerPrefs.GetFloat("spawnSpeed", 1);
                for (; elementID < spawnSpeedFactor.Length; elementID++)
                {
                    if (spawnSpeedFactor[elementID] > tempVal) break;
                }
                break;
            case StageInfoID.Enum_hpMax:
                tempVal = PlayerPrefs.GetFloat("hpMax", 5);
                for (; elementID < hpMaxFactor.Length; elementID++)
                {
                    if (hpMaxFactor[elementID] > tempVal) break;
                }
                break;
            case StageInfoID.Enum_resource:
                tempVal = PlayerPrefs.GetFloat("resource", 1);
                for (; elementID < resourceFactor.Length; elementID++)
                {
                    if (resourceFactor[elementID] > tempVal) break;
                }
                break;
        }
        return Mathf.Max(elementID - 1, 0);
    }
        public static float SaveDataInPrefs(int infoID, int chg)
    {
        float tempVal = 0;
        int currentID = GetNearestElement(infoID);
        switch ((StageInfoID)infoID) {
            case StageInfoID.Enum_waveNum:
                tempVal = (currentID + chg + waveNumFactor.Length) % waveNumFactor.Length;
                PlayerPrefs.SetFloat("waveNum", waveNumFactor[(int)tempVal]);
                return waveNumFactor[(int)tempVal];
            case StageInfoID.Enum_stageSize:
                tempVal = (currentID + chg + stageSizeFactor.Length) % stageSizeFactor.Length;
                PlayerPrefs.SetFloat("stageSize", stageSizeFactor[(int)tempVal]);
                return stageSizeFactor[(int)tempVal];
            case StageInfoID.Enum_enmNum:
                tempVal = (currentID + chg + enmNumFactor.Length) % enmNumFactor.Length;
                PlayerPrefs.SetFloat("enmNum", enmNumFactor[(int)tempVal]);
                return enmNumFactor[(int)tempVal];
            case StageInfoID.Enum_enmSpeed:
                tempVal = (currentID + chg + enmSpeedFactor.Length) % enmSpeedFactor.Length;
                PlayerPrefs.SetFloat("enmSpeed", enmSpeedFactor[(int)tempVal]);
                return enmSpeedFactor[(int)tempVal];
            case StageInfoID.Enum_spawnSpeed:
                tempVal = (currentID + chg + spawnSpeedFactor.Length) % spawnSpeedFactor.Length;
                PlayerPrefs.SetFloat("spawnSpeed", spawnSpeedFactor[(int)tempVal]);
                return spawnSpeedFactor[(int)tempVal];
            case StageInfoID.Enum_hpMax:
                tempVal = (currentID + chg + hpMaxFactor.Length) % hpMaxFactor.Length;
                PlayerPrefs.SetFloat("hpMax", hpMaxFactor[(int)tempVal]);
                return hpMaxFactor[(int)tempVal];
            case StageInfoID.Enum_resource:
                tempVal = (currentID + chg + resourceFactor.Length) % resourceFactor.Length;
                PlayerPrefs.SetFloat("resource", resourceFactor[(int)tempVal]);
                return resourceFactor[(int)tempVal];
        }
        return 0f;
    }

    private static void UpdateCustomizedData() {
        waveNumEx = (int)PlayerPrefs.GetFloat("waveNum",1);
        stageSizeEx = (int)PlayerPrefs.GetFloat("stageSize",100);
        enmNumEx = (int)PlayerPrefs.GetFloat("enmNum",1);
        enmSpeedEx = (int)PlayerPrefs.GetFloat("enmSpeed",1);
        spawnSpeedEx = (int)PlayerPrefs.GetFloat("spawnSpeed",5);
        hpMaxEx = (int)PlayerPrefs.GetFloat("hpMax",1);
        resourceEx = (int)PlayerPrefs.GetFloat("resource",1);
    }

    public static StageAttr GetStageInfo()
    {
        return stageInfo;
    }



    private static WaveAttr[] PrepareEasyStageInfo(int waveNum) {
        WaveAttr[] waveArray = new WaveAttr[waveNum];

        List<EnmDetail> detail = new List<EnmDetail>();
        detail.Add(new EnmDetail(1, 5, 1, "Slime"));
        detail.Add(new EnmDetail(2, 5, 1, "Mushroom"));
        detail.Add(new EnmDetail(3, 10, 1, "Slime"));
        detail.Add(new EnmDetail(4, 10, 1, "TurtleShell"));
        detail.Add(new EnmDetail(5, 1, 1, "StoneMonster"));

        int j = 0;

        for (int i = 0; i < waveNum; ++i)
        {
            List<EnmDetail> detailPerWave = new List<EnmDetail>();
            for (; j < detail.Count && detail[j].waveID <= i + 1; ++j)
            {
                if (detail[j].waveID == i + 1)
                {
                    detailPerWave.Add(detail[j]);
                }
            }
            waveArray[i] = new WaveAttr(2, 2.0f, detailPerWave);
        }
        return waveArray;

    }

    private static WaveAttr[] PrepareNormalStageInfo(int waveNum)
    {
        WaveAttr[] waveArray = new WaveAttr[waveNum];

        List<EnmDetail> detail = new List<EnmDetail>();
        detail.Add(new EnmDetail(1, 5, 1, "Slime"));
        detail.Add(new EnmDetail(2, 5, 1, "Mushroom"));
        detail.Add(new EnmDetail(3, 10, 2, "Slime"));
        detail.Add(new EnmDetail(4, 10, 1, "Mushroom"));
        detail.Add(new EnmDetail(5, 10, 0, "TurtleShell"));
        detail.Add(new EnmDetail(6, 10, 1, "Footman"));
        detail.Add(new EnmDetail(7, 20, 1, "Slime"));
        detail.Add(new EnmDetail(8, 20, 2, "Grunt"));
        detail.Add(new EnmDetail(9, 30, 2, "Skeleton"));
        detail.Add(new EnmDetail(10, 30, 0, "Footman"));
        detail.Add(new EnmDetail(11, 20, 0, "FootmanS"));
        detail.Add(new EnmDetail(12, 30, 1, "Skeleton"));
        detail.Add(new EnmDetail(13, 20, 0, "GruntS"));
        detail.Add(new EnmDetail(14, 20, 2, "TurtleShell"));
        detail.Add(new EnmDetail(15, 2, 1, "Bull"));

        int j = 0;

        for (int i = 0; i < waveNum; ++i)
        {
            List<EnmDetail> detailPerWave = new List<EnmDetail>();
            for (; j < detail.Count && detail[j].waveID <= i + 1; ++j)
            {
                if (detail[j].waveID == i + 1)
                {
                    detailPerWave.Add(detail[j]);
                }
            }
            waveArray[i] = new WaveAttr(3, 1.0f, detailPerWave);
        }
        return waveArray;
    }

    private static WaveAttr[] PrepareHardStageInfo(int waveNum)
    {
        WaveAttr[] waveArray = new WaveAttr[waveNum];
        List<EnmDetail> detail = new List<EnmDetail>();
        detail.Add(new EnmDetail(1, 5, 1, "Slime"));
        detail.Add(new EnmDetail(2, 5, 1, "Mushroom"));
        detail.Add(new EnmDetail(3, 10, 2, "TurtleShell"));
        detail.Add(new EnmDetail(4, 10, 2, "Footman"));
        detail.Add(new EnmDetail(5, 10, 0, "Mushroom"));
        detail.Add(new EnmDetail(6, 10, 0, "Grunt"));
        detail.Add(new EnmDetail(7, 20, 1, "Skeleton"));
        detail.Add(new EnmDetail(8, 20, 2, "Slime"));
        detail.Add(new EnmDetail(9, 30, 0, "Grunt"));
        detail.Add(new EnmDetail(10, 30, 0, "TurtleShell"));
        detail.Add(new EnmDetail(11, 20, 1, "Mushroom"));
        detail.Add(new EnmDetail(12, 30, 1, "Grunt"));
        detail.Add(new EnmDetail(13, 20, 2, "Skeleton"));
        detail.Add(new EnmDetail(14, 20, 2, "Footman"));
        detail.Add(new EnmDetail(15, 10, 0, "StoneMonster"));
        detail.Add(new EnmDetail(16, 20, 2, "SpiderGhost"));
        detail.Add(new EnmDetail(17, 20, 1, "Golem"));
        detail.Add(new EnmDetail(18, 25, 1, "SkeletonArmed"));
        detail.Add(new EnmDetail(19, 30, 1, "FreeLich"));
        detail.Add(new EnmDetail(20, 35, 0, "SpiderGhost"));
        detail.Add(new EnmDetail(21, 15, 0, "Golem"));
        detail.Add(new EnmDetail(22, 30, 2, "SkeletonArmed"));
        detail.Add(new EnmDetail(23, 20, 2, "FreeLich"));
        detail.Add(new EnmDetail(24, 40, 1, "Slime"));
        detail.Add(new EnmDetail(25, 10, 1, "Bull"));
        detail.Add(new EnmDetail(26, 20, 0, "FootmanS"));
        detail.Add(new EnmDetail(27, 30, 0, "SpiderGhost"));
        detail.Add(new EnmDetail(28, 20, 2, "GolemS"));
        detail.Add(new EnmDetail(29, 20, 2, "FreeLichS"));
        detail.Add(new EnmDetail(30, 1, 1, "Dragon"));

        int j = 0;

        for (int i = 0; i < waveNum; ++i)
        {
            List<EnmDetail> detailPerWave = new List<EnmDetail>();
            for (; j < detail.Count && detail[j].waveID <= i+1; ++j)
            {
                if (detail[j].waveID == i + 1)
                {
                    detailPerWave.Add(detail[j]);
                }
            }
            waveArray[i] = new WaveAttr(3, 0.5f, detailPerWave);
        }
        return waveArray;
    }

    private static WaveAttr[] PrepareCustomStageInfo(int waveNum)
    {
        WaveAttr[] waveArray = new WaveAttr[waveNum];

        List<EnmDetail> detail = new List<EnmDetail>();
       // detail.Add(new EnmDetail(1, 1 * (int)enmNumEx, 0, monsterCat0[Random.Range(0, monsterCat0.Length)]));
        detail.Add(new EnmDetail(1, 1 * (int)enmNumEx, 1, monsterCat0[Random.Range(0, monsterCat0.Length)]));
       // detail.Add(new EnmDetail(1, 1 * (int)enmNumEx, 2, monsterCat0[Random.Range(0, monsterCat0.Length)]));

        for (int k = 2; k < waveNum; k++) {
            if (k < 10 - 1)
            {
                detail.Add(new EnmDetail(k, 10 * (int)enmNumEx, Random.Range(0,3), monsterCat0[Random.Range(0,monsterCat0.Length)]));
            }
            else if (k < 30 - 1)
            {
                detail.Add(new EnmDetail(k, Random.Range(15, 25)* (int)enmNumEx, Random.Range(0, 3), monsterCat1[Random.Range(0, monsterCat1.Length)]));
                detail.Add(new EnmDetail(k, Random.Range(15, 25) * (int)enmNumEx, Random.Range(0, 3), monsterCat1[Random.Range(0, monsterCat1.Length)]));
            }
            else if (k < 50 - 1)
            {
                detail.Add(new EnmDetail(k, Random.Range(15, 25)* (int)enmNumEx, Random.Range(0, 3), monsterCat2[Random.Range(0, monsterCat2.Length)]));
                detail.Add(new EnmDetail(k, Random.Range(15, 25)* (int)enmNumEx, Random.Range(0, 3), monsterCat2[Random.Range(0, monsterCat2.Length)]));
                detail.Add(new EnmDetail(k, Random.Range(15, 25) * (int)enmNumEx, Random.Range(0, 3), monsterCat2[Random.Range(0, monsterCat2.Length)]));
            }
            else
            {
                if (k == 49)
                    detail.Add(new EnmDetail(k, 3, 1, "RobotSphere"));
                else 
                {
                    switch (k % 10) {
                        case 9:
                            if (Random.Range(0, 5) == 0)
                                detail.Add(new EnmDetail(k, 1 * (int)enmNumEx, 1, "AttackBot"));
                            else
                            {
                                detail.Add(new EnmDetail(k, Random.Range(3, 5)* (int)enmNumEx, Random.Range(0, 3), "RobotSphere"));
                                detail.Add(new EnmDetail(k, Random.Range(3, 5)* (int)enmNumEx, Random.Range(0, 3), "RobotSphere"));
                                detail.Add(new EnmDetail(k, Random.Range(3, 5) * (int)enmNumEx, Random.Range(0, 3), "RobotSphere"));
                            }
                            break;
                        default:
                            detail.Add(new EnmDetail(k, Random.Range(20, 35)* (int)enmNumEx, Random.Range(0, 3), monsterCat3[Random.Range(0, monsterCat3.Length)]));
                            detail.Add(new EnmDetail(k, Random.Range(20, 35)* (int)enmNumEx, Random.Range(0, 3), monsterCat3[Random.Range(0, monsterCat3.Length)]));
                            detail.Add(new EnmDetail(k, Random.Range(20, 35) * (int)enmNumEx, Random.Range(0, 3), monsterCat3[Random.Range(0, monsterCat3.Length)]));
                            break;
                    }
                
                }
            }
        }

        int j = 0;

        for (int i = 0; i < waveNum; ++i)
        {
            List<EnmDetail> detailPerWave = new List<EnmDetail>();
            for (; j < detail.Count && detail[j].waveID <= i + 1; ++j)
            {
                if (detail[j].waveID == i + 1)
                {
                    detailPerWave.Add(detail[j]);
                }
            }
            waveArray[i] = new WaveAttr(3, 1f, detailPerWave);
        }
        return waveArray;
    }
}
