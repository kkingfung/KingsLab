using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.RemoteConfig;

using System.IO;
public class EnemyAttr {
    public float health;
    public float speed;
    public float damage;
    public float radius;
    public float time;
    public int money;

    const float healthBase=1;

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
    public static string[] allName = {
            "MetalonGreen", "MetalonPurple", "MetalonRed", "AttackBot" , "RobotSphere",
                        "Dragon", "Bull", "StoneMonster", "FreeLichS" , "FreeLich",
                            "GolemS", "Golem", "SkeletonArmed", "SpiderGhost" , "Skeleton",
                              "GruntS", "FootmanS", "Grunt", "Footman" , "TurtleShell",
                               "Mushroom" , "Slime", "PigChef", "PhoenixChick" , "RockCritter"

        };

    public static void Init()
    {
        enemyInfo = new Dictionary<string, EnemyAttr>();

        //Bonus
        enemyInfo.Add("MetalonGreen", new EnemyAttr(50, 100, 30, 0.5f, 5, 0.5f));
        enemyInfo.Add("MetalonPurple", new EnemyAttr(150, 200, 30, 0.5f, 5, 0.5f));
        enemyInfo.Add("MetalonRed", new EnemyAttr(300, 500, 30, 0.5f, 5, 0.5f));

        //Stage 4
        enemyInfo.Add("AttackBot", new EnemyAttr(150, 300, 15, 0.5f, 5, 0.5f));
        enemyInfo.Add("RobotSphere", new EnemyAttr(100, 250, 20, 0.5f, 5, 0.5f));

        //Bosses
        enemyInfo.Add("Dragon", new EnemyAttr(50, 160, 12, 0.5f, 5, 0.5f));
        enemyInfo.Add("Bull", new EnemyAttr(30, 200, 8, 0.5f, 5, 0.5f));
        enemyInfo.Add("StoneMonster", new EnemyAttr(15, 60, 3, 0.5f, 5, 0.5f));

        //Stage 3
        enemyInfo.Add("FreeLichS", new EnemyAttr(5, 150, 7.5f, 0.5f, 1, 0.5f));
        enemyInfo.Add("FreeLich", new EnemyAttr(3, 80, 6, 0.5f, 1, 0.5f));
        enemyInfo.Add("GolemS", new EnemyAttr(5, 220, 7.5f, 0.5f, 1, 0.5f));
        enemyInfo.Add("Golem", new EnemyAttr(3, 180, 6, 0.5f, 1, 0.5f));
        enemyInfo.Add("SkeletonArmed", new EnemyAttr(3, 120, 9, 0.5f, 1, 0.5f));
        enemyInfo.Add("SpiderGhost", new EnemyAttr(3, 40, 12, 0.5f, 1, 0.5f));

        //Stage 2
        enemyInfo.Add("Skeleton", new EnemyAttr(3, 60, 9.5f, 0.5f, 1, 0.5f));
        enemyInfo.Add("GruntS", new EnemyAttr(3, 80, 15, 0.5f, 1, 0.5f));
        enemyInfo.Add("FootmanS", new EnemyAttr(3, 120, 8, 0.5f, 1, 0.5f));
        enemyInfo.Add("Grunt", new EnemyAttr(2, 30, 10, 0.5f, 1, 0.5f));
        enemyInfo.Add("Footman", new EnemyAttr(2, 50, 5, 0.5f, 1, 0.5f));

        //Stage 1
        enemyInfo.Add("TurtleShell", new EnemyAttr(10, 100, 0.5f, 0.5f, 1, 0.5f));
        enemyInfo.Add("Mushroom", new EnemyAttr(5, 50, 1f, 1.0f, 1, 0.5f));
        enemyInfo.Add("Slime", new EnemyAttr(5, 30, 0.8f, 0.08f, 1, 0.5f));

        //Addition
        enemyInfo.Add("PigChef", new EnemyAttr(10, 100, 0.5f, 0.5f, 1, 0.5f));
        enemyInfo.Add("PhoenixChick", new EnemyAttr(5, 50, 1f, 1.0f, 1, 0.5f));
        enemyInfo.Add("RockCritter", new EnemyAttr(5, 30, 0.8f, 0.08f, 1, 0.5f));
    }

    public static void InitByFile(string filepath)
    {
        StreamReader inp_stm = new StreamReader(filepath);

        enemyInfo = new Dictionary<string, EnemyAttr>();

        while (!inp_stm.EndOfStream)
        {
            string inp_ln = inp_stm.ReadLine();
            string[] seperateInfo = inp_ln.Split(':');
            if (seperateInfo.Length == 7)
            {
                enemyInfo.Add(seperateInfo[0], new EnemyAttr(
                    int.Parse(seperateInfo[1]), float.Parse(seperateInfo[2]),
                    float.Parse(seperateInfo[3]), float.Parse(seperateInfo[4]),
                    int.Parse(seperateInfo[5]), float.Parse(seperateInfo[6])));
            }
           // Debug.Log(seperateInfo.Length);
        }

        inp_stm.Close();
    }
    public static void InitByRemote(ConfigResponse response)
    {
        enemyInfo = new Dictionary<string, EnemyAttr>();

        foreach (string name in allName)
        {
            enemyInfo.Add(name, new EnemyAttr(
                      ConfigManager.appConfig.GetInt("Enemy"+name + "Money"), ConfigManager.appConfig.GetFloat("Enemy" + name + "Health"),
                      ConfigManager.appConfig.GetFloat("Enemy" + name + "Speed"), ConfigManager.appConfig.GetFloat("Enemy" + name + "Time"),
                       ConfigManager.appConfig.GetInt("Enemy" + name + "Damage"), ConfigManager.appConfig.GetFloat("Enemy" + name + "Radius")));
        }
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
