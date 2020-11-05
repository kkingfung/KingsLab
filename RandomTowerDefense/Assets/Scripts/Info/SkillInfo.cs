using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillAttr
{
    public float radius;
    public float damage;
    public float cycleTime; //for each instantiation
    public float waitTime;//for the gameobj lifetime except for Blizzard
    public float lifeTime;//for the whole skill lifetime
    public float slowRate;//for Blizzard only
    public float buffTime;//for Blizzard and Petricfication only

    public SkillAttr(float area, float damage, float cycleTime, float waitTime, float lifeTime, float slowRate=0, float buffTime=0)
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
        skillInfo.Add("SkillMeteor", new SkillAttr(5, 80, 2.5f,2.5f,15,0,0));
        skillInfo.Add("SkillMinions", new SkillAttr(1, 1,0.1f, 3f,10, 0, 0));

        skillInfo.Add("SkillBlizzard", new SkillAttr(30, 10, 5, 4, 15, 0.01f, 3.0f));
        skillInfo.Add("SkillPetrification", new SkillAttr(1000, 0,0.2f,2,5, 1, 0.02f));
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