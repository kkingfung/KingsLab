using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private List<AnimationCurve> SpawnRateCurves;

    public GameObject MetalonGreen;
    public GameObject MetalonPurple;
    public GameObject MetalonRed;

    public GameObject AttackBot;
    public GameObject RobotSphere;

    public GameObject Dragon;
    public GameObject Bull;
    public GameObject StoneMonster;

    public GameObject FreeLichS;
    public GameObject FreeLich;
    public GameObject GolemS;
    public GameObject Golem;
    public GameObject SkeletonArmed;
    public GameObject SpiderGhost;

    public GameObject Skeleton;
    public GameObject GruntS;
    public GameObject FootmanS;
    public GameObject Grunt;
    public GameObject Footman;

    public GameObject TurtleShell;
    public GameObject Mushroom;
    public GameObject Slime;

    Dictionary<string, GameObject> allMonsterList;

    // Start is called before the first frame update
    void Start()
    {
        allMonsterList = new Dictionary<string, GameObject>();

        //Bonus
        allMonsterList.Add("MetalonGreen", MetalonGreen);
        allMonsterList.Add("MetalonPurple", MetalonPurple);
        allMonsterList.Add("MetalonRed", MetalonRed);

        //Stage 4
        allMonsterList.Add("AttackBot", AttackBot);
        allMonsterList.Add("RobotSphere", RobotSphere);

        //Bosses
        allMonsterList.Add("Dragon", Dragon);
        allMonsterList.Add("Bull", Bull);
        allMonsterList.Add("StoneMonster", StoneMonster);

        //Stage 3
        allMonsterList.Add("FreeLichS", FreeLichS);
        allMonsterList.Add("FreeLich", FreeLich);
        allMonsterList.Add("GolemS", GolemS);
        allMonsterList.Add("Golem", Golem);
        allMonsterList.Add("SkeletonArmed", SkeletonArmed);
        allMonsterList.Add("SpiderGhost", SpiderGhost);

        //Stage 2
        allMonsterList.Add("Skeleton", Skeleton);
        allMonsterList.Add("GruntS", GruntS);
        allMonsterList.Add("FootmanS", FootmanS);
        allMonsterList.Add("Grunt", Grunt);
        allMonsterList.Add("Footman", Footman);

        //Stage 1
        allMonsterList.Add("TurtleShell", TurtleShell);
        allMonsterList.Add("Mushroom", Mushroom);
        allMonsterList.Add("Slime", Slime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnMonster(string monsterName)
    {

    }

    public void SpawnBonusBoss(string bossID) { 
    
    }

}
