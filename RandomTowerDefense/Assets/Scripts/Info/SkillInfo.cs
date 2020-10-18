using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillAttr
{
    public int area;
    public int damage;
    public int flameWait;
    public float activeTime;

    public SkillAttr(int area, int damage, int flameWait, float activeTime)
    {
        this.area = area;
        this.damage = damage;
        this.flameWait = flameWait;
        this.activeTime = activeTime;
    }
}

public static class SkillInfo
{
    static Dictionary<string, SkillAttr> skillInfo;
    public static void Init()
    {
        skillInfo = new Dictionary<string, SkillAttr>();

        skillInfo.Add("SkillMeteor", new SkillAttr(5, 80, 30,600));
        skillInfo.Add("SkillBlizzard", new SkillAttr(30, 1,0, 600));
        skillInfo.Add("SkillMinions", new SkillAttr(1, 5,0, 200));
        skillInfo.Add("SkillPetrification", new SkillAttr(1000, 0,-1, 1));
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