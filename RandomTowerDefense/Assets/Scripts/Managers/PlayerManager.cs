using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [HideInInspector]
    public bool isSkillActive;

    [HideInInspector]
    public bool isSaling;

    [Header("Skill Settings")]
    public GameObject FireSkillPrefab;
    public GameObject IceSkillPrefab;
    public GameObject MindSkillPrefab;
    public GameObject SummonSkillPrefab;

    GameObject StockOperatorPrefab;
    bool isChecking;

    // Start is called before the first frame update
    void Awake()
    {
        SkillStack.init();
        Upgrades.init();
    }


    public void CheckStock(Vector2 DragPos) 
    { 
    
    }

    public void UseStock(Vector2 DragPos)
    {
        int StockSelected = 0;

        //For non-sale
        switch (SkillStack.UseStock(StockSelected)) {
            case (int)Upgrades.StoreItems.BonusBoss1:
                isSkillActive = true;
                break;
            case (int)Upgrades.StoreItems.BonusBoss2:
                isSkillActive = true;
                break;
            case (int)Upgrades.StoreItems.BonusBoss3:
                isSkillActive = true;
                break;
            case (int)Upgrades.StoreItems.MagicMeteor:
                isSkillActive = true;
                break;
            case (int)Upgrades.StoreItems.MagicBlizzard:
                isSkillActive = true;
                break;
            case (int)Upgrades.StoreItems.MagicSummon:
                isSkillActive = true;
                break;
            case (int)Upgrades.StoreItems.MagicPetrification:
                isSkillActive = true;
                break;
        }
    }
}
