using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillAttr
{
    public float area;
    public float damage;
    public float cycleTime; // time to spawn
    public float frameWait; // Frame to ApplyDmg/ApplyEffect(buff time)
    public float activeTime;

    public SkillAttr(float area, float damage, float cycleTime, float frameWait, float activeTime)
    {
        this.area = area;
        this.damage = damage;
        this.frameWait = frameWait;
        this.activeTime = activeTime;
        this.cycleTime = cycleTime;
    }

    public SkillAttr(SkillAttr attr)
    {
        this.area = attr.area;
        this.damage = attr.damage;
        this.frameWait = attr.frameWait;
        this.activeTime = attr.activeTime;
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
        skillInfo.Add("SkillMeteor", new SkillAttr(5, 80, 2.5f,2.5f,15));
        skillInfo.Add("SkillBlizzard", new SkillAttr(30, 10,5, 4,15));
        skillInfo.Add("SkillMinions", new SkillAttr(1, 1,0.1f, 3f,10));
        skillInfo.Add("SkillPetrification", new SkillAttr(1000, 0,0.5f,2,3));
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