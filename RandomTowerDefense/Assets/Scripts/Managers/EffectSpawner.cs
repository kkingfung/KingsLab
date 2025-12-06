using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.VFX;

namespace RandomTowerDefense.Managers.System
{
    /// <summary>
    /// エフェクトスポーナーシステム - VFXエフェクトの動的生成と管理
    ///
    /// 主な機能:
    /// - 6種類エフェクトタイプ（ビルド、デス、マネードロップ、消失、インパクト、売却）
    /// - VFXグラフコンポーネント統合と自動再生システム
    /// - ゲームオブジェクトプールでパフォーマンス最適化
    /// - シングルトンパターンでグローバルアクセス管理
    /// - 位置指定でエフェクトスポーンシステム
    /// </summary>
    public class EffectSpawner : MonoBehaviour
{
    /// <summary>ビルドエフェクトプレハブ</summary>
    public GameObject PrefabBuild;
    /// <summary>死亡エフェクトプレハブ</summary>
    public GameObject PrefabDieVfx;
    /// <summary>マネードロップエフェクトプレハブ</summary>
    public GameObject PrefabMoneyDropVfx;
    /// <summary>消失エフェクトプレハブ</summary>
    public GameObject PrefabDisappearVfx;
    /// <summary>インパクトエフェクトプレハブ</summary>
    public GameObject PrefabImpactVfx;
    /// <summary>売却エフェクトプレハブ</summary>
    public GameObject PrefabSellVfx;

    /// <summary>ビルドVFXプールリスト</summary>
    public List<GameObject> BuildVFXList;
    /// <summary>死亡VFXプールリスト</summary>
    public List<GameObject> DieVFXList;
    /// <summary>マネードロップVFXプールリスト</summary>
    public List<GameObject> MoneyDropVFXList;
    /// <summary>消失VFXプールリスト</summary>
    public List<GameObject> DisappearVFXList;
    /// <summary>インパクトVFXプールリスト</summary>
    public List<GameObject> ImpactVFXList;
    /// <summary>売却VFXプールリスト</summary>
    public List<GameObject> SellVFXList;

    /// <summary>シングルトンインスタンス</summary>
    public static EffectSpawner Instance { get; private set; }

    /// <summary>
    /// シングルトンインスタンスの初期化
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// エフェクトプールの初期化処理
    /// </summary>
    private void Start()
    {
        //BuildVFXList = new List<GameObject>();
        //DieVFXList = new List<GameObject>();
        //MoneyDropVFXList = new List<GameObject>();
        //DisappearVFXList = new List<GameObject>();
        //ImpactVFXList = new List<GameObject>();
        //SellVFXList = new List<GameObject>();
    }

    /// <summary>
    /// 指定タイプのエフェクトを指定位置にスポーン
    /// </summary>
    /// <param name="prefabID">エフェクトタイプID（0:ビルド、1:死亡、2:マネードロップ、3:消失、4:インパクト、5:売却）</param>
    /// <param name="Position">スポーン位置</param>
    /// <returns>スポーンされたエフェクトGameObject（タイプ3以外の場合null）</returns>
    public GameObject Spawn(int prefabID, float3 Position)
    {
        if (prefabID != 3)
            return null;

        GameObject newObj = null;
        switch (prefabID)
        {
            case 0:
                foreach (GameObject j in BuildVFXList)
                {
                    if (j.activeSelf) continue;
                    newObj = j;
                    break;
                }
                break;
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
}
