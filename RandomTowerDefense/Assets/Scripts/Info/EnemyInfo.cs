using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttr {
    public float health;
    public float speed;
    public int damage;
    public int frameWait;
    public int money;

    public EnemyAttr(float health, float speed, int damage, int money, int frameWait=0) {
        this.health = health;
        this.speed = speed;
        this.damage = damage;
        this.frameWait = frameWait;
        this.money = money;
    }
}

 public static class EnemyInfo
{
    static Dictionary<string, EnemyAttr> enemyInfo;
    static void Init()
    {
        enemyInfo = new Dictionary<string, EnemyAttr>();

        EnemyAttr BaseAttr = new EnemyAttr(10,1,1,5,0);

        enemyInfo.Add("MetalonGreen", BaseAttr);
        enemyInfo.Add("MetalonPurple", BaseAttr);
        enemyInfo.Add("MetalonRed", BaseAttr);

        enemyInfo.Add("AttackBot", BaseAttr);
        enemyInfo.Add("RobotSphere", BaseAttr);

        enemyInfo.Add("Bull", BaseAttr);
        enemyInfo.Add("Dragon", BaseAttr);
        enemyInfo.Add("Footman", BaseAttr);
        enemyInfo.Add("FootmanS", BaseAttr);
        enemyInfo.Add("FreeLich", BaseAttr);
        enemyInfo.Add("FreeLichS", BaseAttr);
        enemyInfo.Add("Grunt", BaseAttr);
        enemyInfo.Add("GruntS", BaseAttr);

        enemyInfo.Add("Mushroom", BaseAttr);

        enemyInfo.Add("Skeleton", BaseAttr);
        enemyInfo.Add("SkeletonArmed", BaseAttr);
        enemyInfo.Add("Slime", BaseAttr);
        enemyInfo.Add("SpiderGhost", BaseAttr);
        enemyInfo.Add("StoneMonster", BaseAttr);
        enemyInfo.Add("TurtleShell", BaseAttr);
    }
    static void Release()
    {
        enemyInfo.Clear();
    }

    public static EnemyAttr GetEnemyInfo(string enmName) {
        if(enemyInfo.ContainsKey(enmName))
             return enemyInfo[enmName];
        return null;
    }
}
