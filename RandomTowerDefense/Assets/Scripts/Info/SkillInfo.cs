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

    public SkillAttr(float area, float damage, int cycleTime, int frameWait, int activeTime)
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
        skillInfo.Add("SkillMeteor", new SkillAttr(5, 80, 150,150,900));
        skillInfo.Add("SkillBlizzard", new SkillAttr(30, 10,300, 200,800));
        skillInfo.Add("SkillMinions", new SkillAttr(1, 1,5, 90,240));
        skillInfo.Add("SkillPetrification", new SkillAttr(1000, 0,10,120,250));
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