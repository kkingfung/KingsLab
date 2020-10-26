using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : MonoBehaviour
{
    private Upgrades.StoreItems ActionID;
    private SkillAttr attr;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        switch (ActionID) {
            case Upgrades.StoreItems.MagicMeteor:
                break;
            case Upgrades.StoreItems.MagicBlizzard:
                break;
            case Upgrades.StoreItems.MagicPetrification:
                break;
            case Upgrades.StoreItems.MagicSummon:
                break;
        }
    }

    public void init(Upgrades.StoreItems ActionID, SkillAttr attr) {
        this.ActionID = ActionID;
        this.attr = attr;
    }

    private void DamageEnemy(Collider other)
    {
        if (other.gameObject.layer == LayerMask.GetMask("Enemy"))
        {
            switch (ActionID)
            {
                case Upgrades.StoreItems.MagicMeteor:
                    break;
                case Upgrades.StoreItems.MagicBlizzard:
                    break;
                case Upgrades.StoreItems.MagicPetrification:
                    break;
                case Upgrades.StoreItems.MagicSummon:
                    break;
            }
            other.gameObject.GetComponent<EnemyAI>().Damaged(1);
        }
    }

}
