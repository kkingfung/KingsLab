using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public EnmDetail[] enmDetail;
    public WaveAttr(float enmStartTime, float enmSpawnPeriod, EnmDetail[] enmDetail)
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

    public StageAttr(int waveNum, WaveAttr[] waveAttr, float waveTime = 30f)
    {
        this.waveNum = waveNum;
        this.waveWaitTime = waveTime;
        this.waveDetail = waveAttr;
    }
}

public static class StageInfo
{
    public static readonly int IslandNum = 4;
    private static readonly int MaxMapDepth = 20;
    private static readonly int MinMapDepth = 6;

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
    public static readonly float[] spawnSpeedFactor = { 4f, 2f, 1.5f, 1f, 0.5f };
    public static readonly float[] resourceFactor = { 1.25f, 1f, 0.75f, 0.5f };

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
        int temp = PlayerPrefs.GetInt("IslandNow");
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
                stageInfo = new StageAttr((int)PlayerPrefs.GetFloat("waveNum"),
                    PrepareCustomStageInfo((int)PlayerPrefs.GetFloat("waveNum")));
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
        public static float SaveDataInPrefs(int infoID, int chg)
    {
        float tempVal = 0;
        switch ((StageInfoID)infoID) {
            case StageInfoID.Enum_waveNum:
                tempVal = PlayerPrefs.GetFloat("waveNum");
                tempVal = (tempVal + chg + waveNumFactor.Length) % waveNumFactor.Length;
                PlayerPrefs.SetFloat("waveNum", tempVal);
                return waveNumFactor[(int)tempVal];
            case StageInfoID.Enum_stageSize:
                tempVal = PlayerPrefs.GetFloat("stageSize");
                tempVal = (tempVal + chg + stageSizeFactor.Length) % stageSizeFactor.Length;
                PlayerPrefs.SetFloat("stageSize", tempVal);
                return stageSizeFactor[(int)tempVal];
            case StageInfoID.Enum_enmNum:
                tempVal = PlayerPrefs.GetFloat("enmNum");
                tempVal = (tempVal + chg + enmNumFactor.Length) % enmNumFactor.Length;
                PlayerPrefs.SetFloat("enmNum", tempVal);
                return enmNumFactor[(int)tempVal];
            case StageInfoID.Enum_enmSpeed:
                tempVal = PlayerPrefs.GetFloat("enmSpeed");
                tempVal = (tempVal + chg + enmSpeedFactor.Length) % enmSpeedFactor.Length;
                PlayerPrefs.SetFloat("enmSpeed", tempVal);
                return enmSpeedFactor[(int)tempVal];
            case StageInfoID.Enum_spawnSpeed:
                tempVal = PlayerPrefs.GetFloat("spawnSpeed");
                tempVal = (tempVal + chg + spawnSpeedFactor.Length) % spawnSpeedFactor.Length;
                PlayerPrefs.SetFloat("spawnSpeed", tempVal);
                return spawnSpeedFactor[(int)tempVal];
            case StageInfoID.Enum_hpMax:
                tempVal = PlayerPrefs.GetFloat("hpMax");
                tempVal = (tempVal + chg + hpMaxFactor.Length) % hpMaxFactor.Length;
                PlayerPrefs.SetFloat("hpMax", tempVal);
                return hpMaxFactor[(int)tempVal];
            case StageInfoID.Enum_resource:
                tempVal = PlayerPrefs.GetFloat("resource");
                tempVal = (tempVal + chg + resourceFactor.Length) % resourceFactor.Length;
                PlayerPrefs.SetFloat("resource", tempVal);
                return resourceFactor[(int)tempVal];
        }
        return 0f;
    }

    private static void UpdateCustomizedData() {
        waveNumEx = waveNumFactor[(int)PlayerPrefs.GetFloat("waveNum")];
        stageSizeEx = stageSizeFactor[(int)PlayerPrefs.GetFloat("stageSize")];
        enmNumEx = enmNumFactor[(int)PlayerPrefs.GetFloat("enmNum")];
        enmSpeedEx = enmSpeedFactor[(int)PlayerPrefs.GetFloat("enmSpeed")];
        spawnSpeedEx = spawnSpeedFactor[(int)PlayerPrefs.GetFloat("spawnSpeed")];
        hpMaxEx = hpMaxFactor[(int)PlayerPrefs.GetFloat("hpMax")];
        resourceEx = resourceFactor[(int)PlayerPrefs.GetFloat("resource")];
    }

    public static StageAttr GetStageInfo()
    {
        return stageInfo;
    }



    private static WaveAttr[] PrepareEasyStageInfo(int waveNum) {
        WaveAttr[] waveArray = new WaveAttr[waveNum];

        List<EnmDetail> detail = new List<EnmDetail>();
        detail.Add(new EnmDetail(0, 5, 0, "Slime"));
        detail.Add(new EnmDetail(1, 5, 1, "Mushroom"));
        detail.Add(new EnmDetail(2, 10, 2, "Slime"));
        detail.Add(new EnmDetail(3, 10, 1, "TurtleShell"));
        detail.Add(new EnmDetail(4, 1, 1, "StoneMonster"));

        int j = 0;

        for (int i = 0; i < waveNum; ++i)
        {
            List<EnmDetail> detailPerWave = new List<EnmDetail>();
            for (; j < detail.Count && detail[j].waveID <= i; ++j)
            {
                detailPerWave.Add(detail[j]);

            }
            waveArray[i] = new WaveAttr(5, 0.5f, detailPerWave.ToArray());
        }
        return waveArray;

    }

    private static WaveAttr[] PrepareNormalStageInfo(int waveNum)
    {
        WaveAttr[] waveArray = new WaveAttr[waveNum];

        List<EnmDetail> detail = new List<EnmDetail>();
        detail.Add(new EnmDetail(0, 5, 1, "Slime"));
        detail.Add(new EnmDetail(1, 5, 1, "Mushroom"));
        detail.Add(new EnmDetail(2, 10, 2, "Slime"));
        detail.Add(new EnmDetail(3, 10, 1, "Mushroom"));
        detail.Add(new EnmDetail(4, 10, 0, "TurtleShell"));
        detail.Add(new EnmDetail(5, 10, 1, "Footman"));
        detail.Add(new EnmDetail(6, 20, 1, "Slime"));
        detail.Add(new EnmDetail(7, 20, 2, "Grunt"));
        detail.Add(new EnmDetail(8, 30, 2, "Skeleton"));
        detail.Add(new EnmDetail(9, 30, 0, "Footman"));
        detail.Add(new EnmDetail(10, 20, 0, "FootmanS"));
        detail.Add(new EnmDetail(11, 30, 1, "Skeleton"));
        detail.Add(new EnmDetail(12, 20, 0, "GruntS"));
        detail.Add(new EnmDetail(13, 20, 2, "TurtleShell"));
        detail.Add(new EnmDetail(14, 2, 1, "Bull"));

        int j = 0;

        for (int i = 0; i < waveNum; ++i) 
        {
            List<EnmDetail> detailPerWave = new List<EnmDetail>();
            for (; j < detail.Count && detail[j].waveID <= i; ++j)
            {
                detailPerWave.Add(detail[j]);

            }
            waveArray[i] = new WaveAttr(5, 0.5f, detailPerWave.ToArray());
        }
        return waveArray;
    }

    private static WaveAttr[] PrepareHardStageInfo(int waveNum)
    {
        Debug.Log(waveNum);
        WaveAttr[] waveArray = new WaveAttr[waveNum];
        List<EnmDetail> detail = new List<EnmDetail>();
        detail.Add(new EnmDetail(0, 5, 1, "Slime"));
        detail.Add(new EnmDetail(1, 5, 1, "Mushroom"));
        detail.Add(new EnmDetail(2, 10, 2, "TurtleShell"));
        detail.Add(new EnmDetail(3, 10, 2, "Footman"));
        detail.Add(new EnmDetail(4, 10, 0, "Mushroom"));
        detail.Add(new EnmDetail(5, 10, 0, "Grunt"));
        detail.Add(new EnmDetail(6, 20, 1, "Skeleton"));
        detail.Add(new EnmDetail(7, 20, 2, "Slime"));
        detail.Add(new EnmDetail(8, 30, 0, "Grunt"));
        detail.Add(new EnmDetail(9, 30, 0, "TurtleShell"));
        detail.Add(new EnmDetail(10, 20, 1, "Mushroom"));
        detail.Add(new EnmDetail(11, 30, 1, "Grunt"));
        detail.Add(new EnmDetail(12, 20, 2, "Skeleton"));
        detail.Add(new EnmDetail(13, 20, 2, "Footman"));
        detail.Add(new EnmDetail(14, 10, 0, "StoneMonster"));
        detail.Add(new EnmDetail(15, 20, 2, "SpiderGhost"));
        detail.Add(new EnmDetail(16, 20, 1, "Golem"));
        detail.Add(new EnmDetail(17, 25, 1, "SkeletonArmed"));
        detail.Add(new EnmDetail(18, 30, 1, "FreeLich"));
        detail.Add(new EnmDetail(19, 35, 0, "SpiderGhost"));
        detail.Add(new EnmDetail(20, 15, 0, "Golem"));
        detail.Add(new EnmDetail(21, 30, 2, "SkeletonArmed"));
        detail.Add(new EnmDetail(22, 20, 2, "FreeLich"));
        detail.Add(new EnmDetail(23, 40, 1, "Slime"));
        detail.Add(new EnmDetail(24, 10, 1, "Bull"));
        detail.Add(new EnmDetail(25, 20, 0, "FootmanS"));
        detail.Add(new EnmDetail(26, 30, 0, "SpiderGhost"));
        detail.Add(new EnmDetail(27, 20, 2, "GolemS"));
        detail.Add(new EnmDetail(28, 20, 2, "FreeLichS"));
        detail.Add(new EnmDetail(29, 1, 1, "Dragon"));

        int j = 0;

        for (int i = 0; i < waveNum; ++i)
        {
            List<EnmDetail> detailPerWave = new List<EnmDetail>();
            for (; j < detail.Count && detail[j].waveID <= i; ++j)
            {
                detailPerWave.Add(detail[j]);

            }
            waveArray[i] = new WaveAttr(5, 0.5f, detailPerWave.ToArray());
        }
        return waveArray;
    }

    private static WaveAttr[] PrepareCustomStageInfo(int waveNum)
    {
        Debug.Log(waveNum);
        WaveAttr[] waveArray = new WaveAttr[waveNum];

        List<EnmDetail> detail = new List<EnmDetail>();
        for (int k = 0; k < waveNum; k++) {
            if (k < 10 - 1)
            {
                detail.Add(new EnmDetail(k, 10 * (int)enmNumEx, Random.Range(0,3), monsterCat0[Random.Range(0,monsterCat0.Length)]));
            }
            else if (k < 30 - 1)
            {
                detail.Add(new EnmDetail(k, Random.Range(15, 25)* (int)enmNumEx, Random.Range(0, 3), monsterCat0[Random.Range(0, monsterCat1.Length)]));
                detail.Add(new EnmDetail(k, Random.Range(15, 25) * (int)enmNumEx, Random.Range(0, 3), monsterCat0[Random.Range(0, monsterCat1.Length)]));
            }
            else if (k < 50 - 1)
            {
                detail.Add(new EnmDetail(k, Random.Range(15, 25)* (int)enmNumEx, Random.Range(0, 3), monsterCat0[Random.Range(0, monsterCat2.Length)]));
                detail.Add(new EnmDetail(k, Random.Range(15, 25)* (int)enmNumEx, Random.Range(0, 3), monsterCat0[Random.Range(0, monsterCat2.Length)]));
                detail.Add(new EnmDetail(k, Random.Range(15, 25) * (int)enmNumEx, Random.Range(0, 3), monsterCat0[Random.Range(0, monsterCat2.Length)]));
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
                            detail.Add(new EnmDetail(k, Random.Range(20, 35)* (int)enmNumEx, Random.Range(0, 3), monsterCat0[Random.Range(0, monsterCat3.Length)]));
                            detail.Add(new EnmDetail(k, Random.Range(20, 35)* (int)enmNumEx, Random.Range(0, 3), monsterCat0[Random.Range(0, monsterCat3.Length)]));
                            detail.Add(new EnmDetail(k, Random.Range(20, 35) * (int)enmNumEx, Random.Range(0, 3), monsterCat0[Random.Range(0, monsterCat3.Length)]));
                            break;
                    }
                
                }
            }
        }

        int j = 0;

        for (int i = 0; i < waveNum; ++i)
        {
            List<EnmDetail> detailPerWave = new List<EnmDetail>();
            for (; j < detail.Count && detail[j].waveID <= i; ++j)
            {
                detailPerWave.Add(detail[j]);

            }
            waveArray[i] = new WaveAttr(5, 0.5f * spawnSpeedEx, detailPerWave.ToArray());
        }
        return waveArray;
    }
}
