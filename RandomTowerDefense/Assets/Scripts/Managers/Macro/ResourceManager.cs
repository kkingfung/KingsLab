using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    private readonly int StartingMaterialNum = 300;
    private readonly int[] BuildPrice = { 100,0,0,0,0};
    private readonly int[] SellPrice = { 10, 30, 60, 100, 150 };

    private int CurrentMaterial;

    // Start is called before the first frame update
    void Start()
    {
        CurrentMaterial = StartingMaterialNum;
    }

    public bool ChangeMaterial(int Chg) {
        if (Chg < 0 && CurrentMaterial < Chg) return false;
        CurrentMaterial += Chg;
        return true;
    }

    public int GetCurrMaterial() { return CurrentMaterial; }

    public bool ChkAndBuild(int rank) {
        if (rank > BuildPrice.Length) return false;
        bool enoughResource = CurrentMaterial >= BuildPrice[rank - 1];
        if (enoughResource == false) return false;
        CurrentMaterial -= BuildPrice[rank - 1];
        return enoughResource;
    }

    public bool SellTower(Tower targetTower) {
        if (targetTower == null) return false;
        CurrentMaterial += SellPrice[targetTower.rank - 1];
        return true;
    }
}
