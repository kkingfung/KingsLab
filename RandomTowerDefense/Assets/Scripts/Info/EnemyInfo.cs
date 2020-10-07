using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttr {
    public float health;
    public float speed;
    public int damage;
    public int frameWait;
    public int money;

    const float healthBase=10;

    public EnemyAttr(int money, float health, float speed, int damage = 1, int frameWait=0) {
        this.health = healthBase*health;
        this.speed =  speed;
        this.damage =  damage;
        this.frameWait =  frameWait;
        this.money =  money;
    }
}

 public static class EnemyInfo
{
    static Dictionary<string, EnemyAttr> enemyInfo;
    static void Init()
    {
        enemyInfo = new Dictionary<string, EnemyAttr>();

        //Bonus
        enemyInfo.Add("MetalonGreen", new EnemyAttr(100, 100, 30, 10, 0));
        enemyInfo.Add("MetalonPurple", new EnemyAttr(200, 200, 30, 10, 0));
        enemyInfo.Add("MetalonRed", new EnemyAttr(500, 500, 30, 10, 0));

        //Stage 4
        enemyInfo.Add("RobotSphere", new EnemyAttr(108, 250, 20, 5, 0));

        //Bosses
        enemyInfo.Add("AttackBot", new EnemyAttr(92, 300, 15, 5, 0));
        enemyInfo.Add("Bull", new EnemyAttr(75, 200, 8, 5, 0));
        enemyInfo.Add("Dragon", new EnemyAttr(75, 160, 12, 5, 0));

        //Stage 3
        enemyInfo.Add("FreeLich", new EnemyAttr(32, 80, 6, 1, 0));
        enemyInfo.Add("FreeLichS", new EnemyAttr(58, 150, 7.5f, 1, 0));
        enemyInfo.Add("Skeleton", new EnemyAttr(30, 60, 9.5f, 1, 0));
        enemyInfo.Add("SkeletonArmed", new EnemyAttr(32, 120, 9, 1, 0));
        enemyInfo.Add("SpiderGhost", new EnemyAttr(25, 40, 12, 1, 0));

        //Stage 2
        enemyInfo.Add("Grunt", new EnemyAttr(15, 30, 10, 1, 0));
        enemyInfo.Add("GruntS", new EnemyAttr(35, 80, 15, 1, 0));
        enemyInfo.Add("Footman", new EnemyAttr(12, 50, 5, 1, 0));
        enemyInfo.Add("FootmanS", new EnemyAttr(25, 120, 8, 1, 0));

        //Stage 1
        enemyInfo.Add("StoneMonster", new EnemyAttr(10, 60, 3, 1, 0));
        enemyInfo.Add("Mushroom", new EnemyAttr(2, 10, 5, 1, 0));
        enemyInfo.Add("TurtleShell", new EnemyAttr(5, 50, 2, 1, 0));
        enemyInfo.Add("Slime", new EnemyAttr(1, 5, 3, 1, 0));
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
