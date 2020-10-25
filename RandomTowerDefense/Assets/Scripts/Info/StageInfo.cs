using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
public class StageAttr
{
    public int waveNum;
    public float waveTime;

    public int[] enmNumPerWave;
    public string[] enmType;
    
    public int stageSize;

    public StageAttr(int waveNum,int stageSize, int[] enmNumPerWave, string[] enmType, float waveTime = 30f)
    {
        this.waveNum = waveNum;
        this.waveTime = waveTime;
        this.enmNumPerWave = enmNumPerWave;
        this.enmType = enmType;
        this.stageSize = stageSize;
    }
}

public static class StageInfo
{
    static readonly int MaxMapDepth = 20;
    static readonly int MinMapDepth = 6;

    static Dictionary<string, StageAttr> stageInfo;

    //Default Stage Info
    static readonly int[] invalidChoices = { 5, 10, 15, 20, 25, 30, 35, 40, 45, 50 };
    static readonly int[] EasyEnmNum = { 5, 10, 10, 20, 1 };
    static readonly string[] EasyEnmType = { "Slime", "Slime", "Mushroom", "TurtleShell", "StoneMonster" };
    static readonly int[] NormEnmNum = {
            10, 10, 15, 15, 15,
            20, 20, 20, 30, 30,
            30, 30, 30, 30, 2
        };
    static readonly string[] NormEnmType = {
            "Mushroom", "TurtleShell", "Footman", "Grunt", "Footman",
            "Grunt", "TurtleShell", "Skeleton", "Mushroom", "Grunt",
            "Footman", "Skeleton", "GruntS", "FootmanS", "Bull"
        };
    static readonly int[] HardEnmNum = {
            10, 10, 15, 15, 15,
            20, 20, 20, 30, 10,
            30, 30, 30, 30, 30,
            30, 30, 30, 30, 10,
            30, 30, 30, 30, 30,
            30, 30, 30, 30, 1,
        };
    static readonly string[] HardEnmType = {
            "Slime", "Mushroom", "Slime", "TurtleShell", "Footman",
            "SpiderGhost", "Skeleton", "Grunt", "SkeletonArmed", "StoneMonster",
            "Golem", "FreeLich", "FootmanS", "Mushroom", "SkeletonArmed",
            "GruntS", "Footman", "Golem", "GruntS", "Bull",
            "SpiderGhost", "SkeletonArmed", "FootmanS", "FreeLichS", "TurtleShell",
            "GolemS", "GruntS", "FreeLichS", "SkeletonArmed", "Dragon"
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
    public static readonly int[] waveNumFactor = { 30, 35, 50, 51 };
    public static readonly float[] enmNumFactor = { 0.5f, 1f, 2f, 4f, 8f };
    public static readonly float[] enmSpeedFactor = { 0.5f, 1f,1.5f, 2f, 4f };
    public static readonly int[] hpMaxFactor = { 1, 5, 10, 30 };
    public static readonly float[] spawnSpeedFactor = { 4f, 2f, 1.5f, 1f, 0.5f };
    public static readonly float[] resourceFactor = { 1.25f, 1f, 0.75f, 0.5f };


    static public float stageSizeEx =0;
    static public float waveNumEx = 0;
    static public float enmNumEx = 0;
    static public float enmSpeedEx = 0;
    static public float hpMaxEx = 0;
    static public float spawnSpeedEx = 0;
    static public float resourceEx = 0;

    static List<int> ExtraEnmNum;
    static List<string> ExtraEnmType;

    public static void Init()
    {
        stageInfo = new Dictionary<string, StageAttr>();
        stageInfo.Add("Easy", new StageAttr(5, 24, EasyEnmNum, EasyEnmType));//6*4
        stageInfo.Add("Normal", new StageAttr(15, 42, NormEnmNum, NormEnmType));//7*6
        stageInfo.Add("Hard", new StageAttr(30, 64, HardEnmNum, HardEnmType));//8*8

        stageInfo.Add("Extra", new StageAttr(1, 1, null, null));
    }

    static void Release()
    {
        stageInfo.Clear();
    }

    public static void SaveDataInPrefs_DirectInput(int infoID, float input)
    {
        switch ((StageInfoID)infoID)
        {
            case StageInfoID.Enum_waveNum:
                PlayerPrefs.SetFloat("waveNum", (input> MaxMapDepth* MaxMapDepth) ? MaxMapDepth * MaxMapDepth : 
                    (input< MinMapDepth * MinMapDepth ? MinMapDepth * MinMapDepth : input));
                break;
            case StageInfoID.Enum_stageSize:
                PlayerPrefs.SetFloat("stageSize", input);
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

    public static void UpdateCustomizedData() {

        waveNumEx = waveNumFactor[(int)PlayerPrefs.GetFloat("waveNum")];
        stageSizeEx = stageSizeFactor[(int)PlayerPrefs.GetFloat("stageSize")];
        enmNumEx = enmNumFactor[(int)PlayerPrefs.GetFloat("enmNum")];
        enmSpeedEx = enmSpeedFactor[(int)PlayerPrefs.GetFloat("enmSpeed")];
        spawnSpeedEx = spawnSpeedFactor[(int)PlayerPrefs.GetFloat("spawnSpeed")];
        hpMaxEx = hpMaxFactor[(int)PlayerPrefs.GetFloat("hpMax")];
        resourceEx = resourceFactor[(int)PlayerPrefs.GetFloat("resource")];

        //Stage Adjustment
        ResetExtraStage(); 

        //Game Adjustment (In other manager scripts)
        ////int hpMaxEx; //in game operation

        ////float enmNumEx; // enemy manager
        ////float spawnSpeedEx;//wave manager
        ////float enmSpeedEx; // enemyAI
        ////float resourceEx; // resource manager
    }

    static void ResetExtraStage() {
        ExtraEnmNum = new List<int>();
        ExtraEnmNum.AddRange(new int[]{10,10,15,20,20});
        ExtraEnmType = new List<string>();
        ExtraEnmType.AddRange(new string[] { EasyEnmType[Random.Range(0, 4)], EasyEnmType[Random.Range(0, 4)]
            , NormEnmType[Random.Range(0,15)],NormEnmType[Random.Range(0,15)], NormEnmType[Random.Range(0,15)] });

        for (int i = 5; i < waveNumEx; ++i) {
            if (i + 1 > 30 && (i+1) % 5 == 0)
            {
                switch (i + 1)
                {
                    case 50:
                        ExtraEnmNum.Add(1);
                        ExtraEnmType.Add("AttackBot");
                        break;
                    case 40:
                        ExtraEnmNum.Add(3);
                        ExtraEnmType.Add("RobotSphere");
                        break;
                    default:
                        ExtraEnmNum.Add(10);
                        switch (Random.Range(0, 3))
                        {
                            case 0: ExtraEnmType.Add("Dragon"); break;
                            case 1: ExtraEnmType.Add("Bull"); break;
                            case 2: ExtraEnmType.Add("StoneMonster"); break;
                        }
                        break;
                }
            }
            else
            {
                ExtraEnmNum.Add(30);
                ExtraEnmType.Add(HardEnmType[GetRandom()]);
            }
        
        }

        stageInfo["Extra"] = new StageAttr((int)waveNumEx, (int)stageSizeEx, ExtraEnmNum.ToArray(), ExtraEnmType.ToArray());
    }

    public static StageAttr GetStageInfo(string stgName)
    {
        if (stageInfo.ContainsKey(stgName))
            return stageInfo[stgName];
        return null;
    }
    public static int GetStageNum()
    {
        return stageInfo.Count;
    }

    static int GetRandom()
    {
        int temp = Random.Range(0, 30);
        if (invalidChoices.Contains<int>(temp)) return GetRandom();
        return temp;
    }
}
