using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttr {
    public float health;
    public float speed;
    public float damage;
    public float radius;
    public float time;
    public int money;

    const float healthBase=10;

    public EnemyAttr(int money, float health, float speed, float time = 10,
        int damage = 1, float radius = 1) {
        this.health = healthBase*health;
        this.speed =  speed;
        this.radius = radius;
        this.damage = damage;
        this.time =  time;
        this.money =  money;
    }
}

 public static class EnemyInfo
{
    //Remark: Money Carried is now Amended to WaveId except bosses/bonus
    static Dictionary<string, EnemyAttr> enemyInfo;
    public static void Init()
    {
        enemyInfo = new Dictionary<string, EnemyAttr>();

        //Bonus
        enemyInfo.Add("MetalonGreen", new EnemyAttr(50, 100, 30, 0.5f, 5, 1));
        enemyInfo.Add("MetalonPurple", new EnemyAttr(150, 200, 30, 0.5f, 5, 1));
        enemyInfo.Add("MetalonRed", new EnemyAttr(300, 500, 30, 0.5f, 5, 1));

        //Stage 4
        enemyInfo.Add("AttackBot", new EnemyAttr(150, 300, 15, 0.5f, 5, 1));
        enemyInfo.Add("RobotSphere", new EnemyAttr(100, 250, 20, 0.5f, 5, 1));

        //Bosses
        enemyInfo.Add("Dragon", new EnemyAttr(50, 160, 12, 0.5f, 5, 1));
        enemyInfo.Add("Bull", new EnemyAttr(30, 200, 8, 0.5f, 5, 1));
        enemyInfo.Add("StoneMonster", new EnemyAttr(15, 60, 3, 0.5f, 5, 1));

        //Stage 3
        enemyInfo.Add("FreeLichS", new EnemyAttr(5, 150, 7.5f, 0.5f, 1, 1));
        enemyInfo.Add("FreeLich", new EnemyAttr(3, 80, 6, 0.5f, 1, 1));
        enemyInfo.Add("GolemS", new EnemyAttr(5, 220, 7.5f, 0.5f, 1, 1));
        enemyInfo.Add("Golem", new EnemyAttr(3, 180, 6, 0.5f, 1, 1));
        enemyInfo.Add("SkeletonArmed", new EnemyAttr(3, 120, 9, 0.5f, 1, 1));
        enemyInfo.Add("SpiderGhost", new EnemyAttr(3, 40, 12, 0.5f, 1, 1));

        //Stage 2
        enemyInfo.Add("Skeleton", new EnemyAttr(3, 60, 9.5f, 0.5f, 1, 1));
        enemyInfo.Add("GruntS", new EnemyAttr(3, 80, 15, 0.5f, 1, 1));
        enemyInfo.Add("FootmanS", new EnemyAttr(3, 120, 8, 0.5f, 1, 1));
        enemyInfo.Add("Grunt", new EnemyAttr(2, 30, 10, 0.5f, 1, 1));
        enemyInfo.Add("Footman", new EnemyAttr(2, 50, 5, 0.5f, 1, 1));

        //Stage 1
        enemyInfo.Add("TurtleShell", new EnemyAttr(1, 30, 2, 0.5f, 1, 1));
        enemyInfo.Add("Mushroom", new EnemyAttr(1, 10, 5, 0.5f, 1, 1));
        enemyInfo.Add("Slime", new EnemyAttr(1, 5, 3, 0.5f, 1, 1));
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
