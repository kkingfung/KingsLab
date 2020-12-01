using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.RemoteConfig;

using System.IO;

public class SkillAttr
{
    public float radius;
    public float damage;
    public float cycleTime; //for each instantiation
    public float waitTime;//for the gameobj lifetime except for Blizzard
    public float lifeTime;//for the whole skill lifetime
    public float slowRate;//for Blizzard only
    public float buffTime;//for Blizzard and Petricfication only

    public SkillAttr(float area, float damage, float cycleTime, float waitTime, float lifeTime, float slowRate = 0, float buffTime = 0)
    {
        this.radius = area;
        this.damage = damage;
        this.waitTime = waitTime;
        this.lifeTime = lifeTime;
        this.cycleTime = cycleTime;
        this.slowRate = slowRate;
        this.buffTime = buffTime;
    }

    public SkillAttr(SkillAttr attr)
    {
        this.radius = attr.radius;
        this.damage = attr.damage;
        this.waitTime = attr.waitTime;
        this.lifeTime = attr.lifeTime;
        this.cycleTime = attr.cycleTime;
    }
}

public static class SkillInfo
{
    static Dictionary<string, SkillAttr> skillInfo;

    public static void Init()
    {
        skillInfo = new Dictionary<string, SkillAttr>();

        //Wait time Refer to ParticleSystem Lifetime
        skillInfo.Add("SkillMeteor", new SkillAttr(
            2.5f, 8f,//radius,damage
            2.5f, 0.5f,//respawn cycle,wait to action
            15, 0,//lifetime,slowrate(include petrify)
            0));//bufftime

        skillInfo.Add("SkillBlizzard", new SkillAttr(
            5, 0.1f,//radius,damage
            5, 0f, //respawn cycle,wait to action
            15, 0.01f,//lifetime,slowrate(include petrify)
            3.0f));//bufftime

        skillInfo.Add("SkillMinions", new SkillAttr(
            1, 3f,//radius,damage
            0.1f, 1f, //respawn cycle,wait to action
            10, 0, //lifetime,slowrate(include petrify)
            0));//bufftime

        skillInfo.Add("SkillPetrification", new SkillAttr(
            1000, 0,//radius,damage
            0.2f, 0f,//respawn cycle,wait to action
            5, 1.0f,//lifetime,slowrate(include petrify)
            0.0015f));//bufftime
    }

    public static void InitByFile(string filepath)
    {
        StreamReader inp_stm = new StreamReader(filepath);

        skillInfo = new Dictionary<string, SkillAttr>();

        while (!inp_stm.EndOfStream)
        {
            string inp_ln = inp_stm.ReadLine();
            string[] seperateInfo = inp_ln.Split(':');
            if (seperateInfo.Length == 8)
                skillInfo.Add(seperateInfo[0], new SkillAttr(
                    float.Parse(seperateInfo[1]), float.Parse(seperateInfo[2]),
                    float.Parse(seperateInfo[3]), float.Parse(seperateInfo[4]),
                    float.Parse(seperateInfo[5]), float.Parse(seperateInfo[6]),
                    float.Parse(seperateInfo[7])));
            //Debug.Log(seperateInfo.Length);
        }

        inp_stm.Close();
    }

    public static void InitByRemote(ConfigResponse response)
    {
        skillInfo = new Dictionary<string, SkillAttr>();

        string[] allName = { "SkillMeteor", "SkillMinions", "SkillBlizzard" , "SkillPetrification" };
        foreach (string name in allName)
        {
            skillInfo.Add(name, new SkillAttr(
                      ConfigManager.appConfig.GetFloat(name+"Radius"), ConfigManager.appConfig.GetFloat(name+"Damage"),//radius,damage
                      ConfigManager.appConfig.GetFloat(name+"SpawnCycle"), ConfigManager.appConfig.GetFloat(name+"Wait"),//respawn cycle,wait to action
                       ConfigManager.appConfig.GetFloat(name+"Life"), ConfigManager.appConfig.GetFloat(name+"SlowRate"),//lifetime,slowrate(include petrify)
                        ConfigManager.appConfig.GetFloat(name+"BuffTime")));//bufftime
        }
    }

    static void Release()
    {
        skillInfo.Clear();
    }

    public static SkillAttr GetSkillInfo(string skillName)
    {
        if (skillInfo.ContainsKey(skillName))
            return skillInfo[skillName];
        return null;
    }
}