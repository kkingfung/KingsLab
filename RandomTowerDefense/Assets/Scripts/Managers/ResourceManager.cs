using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    private readonly int StartingMaterialNum = 300;

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
}
