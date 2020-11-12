using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class StockOperator : MonoBehaviour
{
    private readonly Vector3 TargetScale = new Vector3(0.3f, 0.3f, 0.3f);
    private readonly int Rotation = 1;

    public List<SpriteRenderer> StockSlot;

    public Sprite SprBoss1;
    public Sprite SprBoss2;
    public Sprite SprBoss3;

    public Sprite SprMeteor;
    public Sprite SprBlizzard;
    public Sprite SprPetrification;
    public Sprite SprMinions;

    public Material MatNull;
    public Material MatBoss1;
    public Material MatBoss2;
    public Material MatBoss3;
                    
    public Material MatMeteor;
    public Material MatBlizzard;
    public Material MatPetrification;
    public Material MatMinions;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartAnimation());

        for (int i = 0; i < StockSlot.Count; ++i)
        {
            switch (SkillStack.GetStock(i))
            {
                default:
                    StockSlot[i].sprite = null;
                    StockSlot[i].material = MatNull;
                    break;
                case (int)Upgrades.StoreItems.BonusBoss1:
                    StockSlot[i].sprite = SprBoss1;
                    StockSlot[i].material = MatBoss1;
                    break;
                case (int)Upgrades.StoreItems.BonusBoss2: 
                    StockSlot[i].sprite = SprBoss2;
                    StockSlot[i].material = MatBoss2;
                    break;
                case (int)Upgrades.StoreItems.BonusBoss3:
                    StockSlot[i].sprite = SprBoss3;
                    StockSlot[i].material = MatBoss3;
                    break;
                case (int)Upgrades.StoreItems.MagicMeteor: 
                    StockSlot[i].sprite = SprMeteor;
                    StockSlot[i].material = MatMeteor;
                    break;
                case (int)Upgrades.StoreItems.MagicBlizzard:
                    StockSlot[i].sprite = SprBlizzard;
                    StockSlot[i].material = MatBlizzard;
                    break;
                case (int)Upgrades.StoreItems.MagicPetrification:
                    StockSlot[i].sprite = SprPetrification;
                    StockSlot[i].material = MatPetrification;
                    break;
                case (int)Upgrades.StoreItems.MagicMinions: 
                    StockSlot[i].sprite = SprMinions;
                    StockSlot[i].material = MatMinions;
                    break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator StartAnimation()
    {
        int frame = 20;
        float rotateChgsbyFrame = (Rotation * 360f - this.transform.localEulerAngles.z) / frame;
        float scaleChgsbyFrame = (TargetScale.x *(1-PlayerPrefs.GetFloat("zoomRate",0)*0.7f)*2f - this.transform.localScale.x) / frame;
        while (frame-- > 0)
        {
            this.transform.localEulerAngles = new Vector3(this.transform.localEulerAngles.x,
                this.transform.localEulerAngles.y, this.transform.localEulerAngles.z+ rotateChgsbyFrame);
            this.transform.localScale = new Vector3(this.transform.localScale.x + scaleChgsbyFrame,
    this.transform.localScale.y + scaleChgsbyFrame, this.transform.localScale.z + scaleChgsbyFrame);
            yield return new WaitForSeconds(0f);
        }
    }
}
