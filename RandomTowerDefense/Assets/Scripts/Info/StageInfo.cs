using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities.Editor;
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
            "Grunt", "TurtleShell", "FootmanS", "Mushroom", "Grunt",
            "Footman", "Footman", "GruntS", "FootmanS", "Bull"
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
            "Slime", "FreeLich", "FootmanS", "Mushroom", "SkeletonArmed",
            "GruntS", "Footman", "FootmanS", "GruntS", "Bull",
            "SpiderGhost", "SkeletonArmed", "FootmanS", "FreeLichS", "TurtleShell",
            "SkeletonArmed", "GruntS", "FreeLichS", "SkeletonArmed", "Dragon"
        };

    //Extra Stage Customization
    static readonly int[] waveNumFactor = { 30, 35, 50, 51 };
    static readonly int[] stageSizeFactor = { 36, 72, 108, 144 };//6*6 + 9*8 + 12*9 + 16*9
    static readonly float[] enmNumFactor = { 0.5f, 1f, 2f, 4f, 8f };
    static readonly float[] enmSpeedFactor = { 0.5f, 1f,1.5f, 2f, 4f };
    static readonly float[] spawnSpeedFactor = { 4f, 2f, 1.5f, 1f, 0.5f };
    static readonly int[] hpMaxFactor = { 1, 5, 10, 30 };
    static readonly float[] resourceFactor = { 1.25f, 1f, 0.75f, 0.5f };

    static int waveNumEx=0;
    static int stageSizeEx=0;
    static float enmNumEx = 0;
    static float enmSpeedEx = 0;
    static float spawnSpeedEx = 0;
    static int hpMaxEx = 0;
    static float resourceEx = 0;

    static List<int> ExtraEnmNum;
    static List<string> ExtraEnmType;

    static void Init()
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

    public static void UpdateCustomizedData(int waveNumCustom,int stageSizeCustom,
         int enmNumCustom, int enmSpeedCustom, int spawnSpeedCustom, int hpMaxCustom, int resourceCustom) {

        waveNumEx = waveNumFactor[waveNumCustom];
        stageSizeEx = stageSizeFactor[stageSizeCustom];
        enmNumEx = enmNumFactor[enmNumCustom];
        enmSpeedEx = enmSpeedFactor[enmSpeedCustom];
        spawnSpeedEx = spawnSpeedFactor[spawnSpeedCustom];
        hpMaxEx = hpMaxFactor[hpMaxCustom];
        resourceEx = resourceFactor[resourceCustom];

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

        stageInfo["Extra"] = new StageAttr(waveNumEx, stageSizeEx, ExtraEnmNum.ToArray(), ExtraEnmType.ToArray());
    }

    public static StageAttr GetStageInfo(string enmName)
    {
        if (stageInfo.ContainsKey(enmName))
            return stageInfo[enmName];
        return null;
    }

    static int GetRandom()
    {
        int temp = Random.Range(0, 30);
        if (ArrayUtility.Contains(invalidChoices, temp)) return GetRandom();
        return temp;
    }
}
