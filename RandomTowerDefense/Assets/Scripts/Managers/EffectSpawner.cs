using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.VFX;

public class EffectSpawner : MonoBehaviour
{
    public GameObject PrefabBuild;
    public GameObject PrefabDieVfx;
    public GameObject PrefabMoneyDropVfx;
    public GameObject PrefabDisappearVfx;
    public GameObject PrefabImpactVfx;
    public GameObject PrefabSellVfx;

    public List<GameObject> BuildVFXList;
    public List<GameObject> DieVFXList;
    public List<GameObject> MoneyDropVFXList;
    public List<GameObject> DisappearVFXList;
    public List<GameObject> ImpactVFXList;
    public List<GameObject> SellVFXList;

    public static EffectSpawner Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        //BuildVFXList = new List<GameObject>();
        //DieVFXList = new List<GameObject>();
        //MoneyDropVFXList = new List<GameObject>();
        //DisappearVFXList = new List<GameObject>();
        //ImpactVFXList = new List<GameObject>();
        //SellVFXList = new List<GameObject>();
    }

    public GameObject Spawn(int prefabID, float3 Position)
    {
        GameObject newObj = null;
        switch (prefabID)
        {
            case 0:
                return null;
                //foreach (GameObject j in BuildVFXList)
                //{
                //    if (j.activeSelf) continue;
                //    newObj = j;
                //    break;
                //}
                //break;
            case 1:
                foreach (GameObject j in DieVFXList)
                {
                    if (j.activeSelf) continue;
                    newObj = j;
                    break;
                }
                break;
            case 2:
                foreach (GameObject j in MoneyDropVFXList)
                {
                    if (j.activeSelf) continue;
                    newObj = j;
                    break;
                }
                break;
            case 3:
                foreach (GameObject j in DisappearVFXList)
                {
                    if (j.activeSelf) continue;
                    newObj = j;
                    break;
                }
                break;
            case 4:
                foreach (GameObject j in ImpactVFXList)
                {
                    if (j.activeSelf) continue;
                    newObj = j;
                    break;
                }
                break;
            case 5:
                foreach (GameObject j in SellVFXList)
                {
                    if (j.activeSelf) continue;
                    newObj = j;
                    break;
                }
                break;
        }

        if (newObj == null)
        {
            switch (prefabID)
            {
                case 0:
                    newObj = Instantiate(PrefabBuild, transform);
                    BuildVFXList.Add(newObj);
                    break;
                case 1:
                    newObj = Instantiate(PrefabDieVfx, transform);
                    DieVFXList.Add(newObj);
                    break;
                case 2:
                    newObj = Instantiate(PrefabMoneyDropVfx, transform);
                    MoneyDropVFXList.Add(newObj);
                    break;
                case 3:
                    newObj = Instantiate(PrefabDisappearVfx, transform);
                    DisappearVFXList.Add(newObj);
                    break;
                case 4:
                    newObj = Instantiate(PrefabImpactVfx, transform);
                    ImpactVFXList.Add(newObj);
                    break;
                case 5:
                    newObj = Instantiate(PrefabSellVfx, transform);
                    SellVFXList.Add(newObj);
                    break;
            }
        }
        else
        {
            newObj.SetActive(true);
            newObj.GetComponent<VisualEffect>().Play();
        }
        newObj.transform.position = Position;
        newObj.transform.localRotation = Quaternion.identity;

        return newObj;
    }

}
