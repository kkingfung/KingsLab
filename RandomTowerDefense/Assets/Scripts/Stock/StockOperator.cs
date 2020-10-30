using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class StockOperator : MonoBehaviour
{
    private readonly Vector3 TargetScale = new Vector3(0.3f, 0.3f, 0.3f);
    private readonly int Rotation = 1;

    public List<GameObject> StockSlot;

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
                    StockSlot[i].GetComponent<SpriteRenderer>().sprite = null;
                    StockSlot[i].GetComponent<SpriteRenderer>().material = MatNull;
                    break;
                case (int)Upgrades.StoreItems.BonusBoss1:
                    StockSlot[i].GetComponent<SpriteRenderer>().sprite = SprBoss1;
                    StockSlot[i].GetComponent<SpriteRenderer>().material = MatBoss1;
                    break;
                case (int)Upgrades.StoreItems.BonusBoss2: 
                    StockSlot[i].GetComponent<SpriteRenderer>().sprite = SprBoss2;
                    StockSlot[i].GetComponent<SpriteRenderer>().material = MatBoss2;
                    break;
                case (int)Upgrades.StoreItems.BonusBoss3:
                    StockSlot[i].GetComponent<SpriteRenderer>().sprite = SprBoss3;
                    StockSlot[i].GetComponent<SpriteRenderer>().material = MatBoss3;
                    break;
                case (int)Upgrades.StoreItems.MagicMeteor: 
                    StockSlot[i].GetComponent<SpriteRenderer>().sprite = SprMeteor;
                    StockSlot[i].GetComponent<SpriteRenderer>().material = MatMeteor;
                    break;
                case (int)Upgrades.StoreItems.MagicBlizzard:
                    StockSlot[i].GetComponent<SpriteRenderer>().sprite = SprBlizzard;
                    StockSlot[i].GetComponent<SpriteRenderer>().material = MatBlizzard;
                    break;
                case (int)Upgrades.StoreItems.MagicPetrification:
                    StockSlot[i].GetComponent<SpriteRenderer>().sprite = SprPetrification;
                    StockSlot[i].GetComponent<SpriteRenderer>().material = MatPetrification;
                    break;
                case (int)Upgrades.StoreItems.MagicMinions: 
                    StockSlot[i].GetComponent<SpriteRenderer>().sprite = SprMinions;
                    StockSlot[i].GetComponent<SpriteRenderer>().material = MatMinions;
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
        int frame = 30;
        float rotateChgsbyFrame = (Rotation * 360f - this.transform.localEulerAngles.z) / frame;
        float scaleChgsbyFrame = (TargetScale.x *(1-PlayerPrefs.GetFloat("zoomRate")*0.7f)*2f - this.transform.localScale.x) / frame;
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
