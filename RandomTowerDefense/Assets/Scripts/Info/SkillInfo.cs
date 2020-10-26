using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillAttr
{
    public float area;
    public float damage;
    public int cycleTime; // time to spawn
    public int frameWait; // Frame to ApplyDmg/ApplyEffect(buff time)
    public int activeTime;

    public SkillAttr(float area, float damage, int cycleTime, int activeTime, int frameWait)
    {
        this.area = area;
        this.damage = damage;
        this.frameWait = frameWait;
        this.activeTime = activeTime;
        this.cycleTime = cycleTime;
    }
}

public static class SkillInfo
{
    static Dictionary<string, SkillAttr> skillInfo;
    public static void Init()
    {
        skillInfo = new Dictionary<string, SkillAttr>();

        //Wait time Refer to ParticleSystem Lifetime
        skillInfo.Add("SkillMeteor", new SkillAttr(5, 80, 30,200,3));
        skillInfo.Add("SkillBlizzard", new SkillAttr(30, 1,300, 300,5));
        skillInfo.Add("SkillMinions", new SkillAttr(1, 5,5, 240,30));
        skillInfo.Add("SkillPetrification", new SkillAttr(1000, 0,20, 100,2));
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