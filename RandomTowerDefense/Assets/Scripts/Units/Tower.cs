using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.VFX;

public class Tower : MonoBehaviour
{
    private readonly int[] MaxLevel = { 10, 30, 50, 70, 100 };

    public TowerAttr attr;
    public int Level;
    public int rank;
    public int type;

    bool isBuilding;
    GameObject CurrTarget;
    GameObject AtkVFX;
    GameObject LevelUpVFX;

    EnemyManager enemyManager;

    private void Start()
    {
        isBuilding = true;
        enemyManager = FindObjectOfType<EnemyManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (CurrTarget == null) CurrTarget = detectEnemy();
        if (CurrTarget != null && isBuilding==false) Attack();
    }

    public GameObject detectEnemy() {
        GameObject nearestMonster=null;
        float dist = -1;
        foreach (GameObject i in enemyManager.allAliveMonsters)
        {
            float tempDist = (i.transform.position - this.transform.position).sqrMagnitude;
            if (tempDist > attr.areaSq) continue;
            if (tempDist < dist) {
                dist = tempDist;
                nearestMonster = i;
            }
        }

        return nearestMonster;
    }

    public void Attack()
    {
        GameObject atk = Instantiate(AtkVFX, this.transform);
        atk.GetComponent<VisualEffect>().SetVector3("TargetEnm", CurrTarget.transform.position);

        CurrTarget.GetComponent<EnemyAI>().Damaged(attr.damage);
    }

    public void newTower(GameObject AtkVFX, GameObject LevelUpVFX, GameObject AuraVFX, int type,int lv=1,int rank=1) {
        this.AtkVFX = AtkVFX;
        this.LevelUpVFX = LevelUpVFX;
        this.type = type;
        this.Level = lv;
        this.rank = rank;
        GameObject.Instantiate(AuraVFX, this.transform);
        isBuilding = true;
    }

    public void SetLevel(int lv) {
        Level = lv;
        GameObject.Instantiate(LevelUpVFX,this.transform);
    }
    public bool CheckMaxLevel() {
        return Level == MaxLevel[rank - 1];
    }
}
