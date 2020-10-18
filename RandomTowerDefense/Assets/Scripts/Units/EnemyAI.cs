using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class EnemyAI : MonoBehaviour
{
    EnemyAttr attr;
    GameObject DieEffect;
    GameObject DropEffect;

    Vector3 oriScale;
    int DamagedCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        oriScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (DamagedCount > 0) { DamagedCount--;
            if(DamagedCount == 0)
                transform.localScale = oriScale;
        }
     
    }

    public void init(GameObject DieEffect, GameObject DropEffect)
    {
        this.DieEffect = DieEffect;
        this.DropEffect = DropEffect;
    }

    public void Damaged(int dmg)
    {
        attr.health -= dmg;
        transform.localScale = oriScale*0.5f;
        DamagedCount = 1;

        if (attr.health <= 0) {
            GameObject.Instantiate(DieEffect,this.transform.position,Quaternion.identity);

            GameObject vfx=Instantiate(DropEffect, this.transform.position, Quaternion.identity);
            vfx.GetComponent<VisualEffect>().SetFloat("SpawnCount", attr.money);

            FindObjectOfType<EnemyManager>().allAliveMonsters.Remove(this.gameObject);
        }
    }
}
