using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    public Vector3 SpawnPoint;
    public GameObject EnemySpawnPort;
    public GameObject Castle;

    private int MaxCastleHP;
    private int CurrCastleHP;
    public List<TextMesh> CastleHPText;

    private InGameOperation sceneManager;
    public List<GameObject> GameClearCanva;
    public List<GameObject> GameOverCanva;

    private int result = 0;

    // Start is called before the first frame update
    void Start()
    {
        sceneManager = FindObjectOfType<InGameOperation>();
        result = 0;
        foreach (GameObject i in GameClearCanva) { 
            i.SetActive(false);
        }
        foreach (GameObject i in GameOverCanva)
            i.SetActive(false);

        int CurrIsland = sceneManager.GetCurrIsland();
        if (CurrIsland == sceneManager.GetTotalIslandNum() - 1)
            MaxCastleHP = (int)PlayerPrefs.GetFloat("hpMax");
        else
            MaxCastleHP = StageInfo.hpMaxFactor[sceneManager.GetCurrIsland()];
        CurrCastleHP = MaxCastleHP;
        foreach (TextMesh i in CastleHPText)
        {
            i.text = CurrCastleHP.ToString() + "/" + MaxCastleHP.ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (result != 0) {
            sceneManager.DarkenCam.SetActive(true);
            if (Screen.width > Screen.height)
            {
                if (result > 0)
                {
                    GameOverCanva[0].SetActive(false);
                    GameOverCanva[1].SetActive(false);
                    GameClearCanva[0].SetActive(false);
                    GameClearCanva[1].SetActive(true);
                }
                else
                {
                    GameOverCanva[0].SetActive(false);
                    GameOverCanva[1].SetActive(true);
                    GameClearCanva[0].SetActive(false);
                    GameClearCanva[1].SetActive(false);
                }
            }
            else {
                if (result > 0)
                {
                    GameOverCanva[0].SetActive(false);
                    GameOverCanva[1].SetActive(false);
                    GameClearCanva[0].SetActive(true);
                    GameClearCanva[1].SetActive(false);
                }
                else
                {
                    GameOverCanva[0].SetActive(true);
                    GameOverCanva[1].SetActive(false);
                    GameClearCanva[0].SetActive(false);
                    GameClearCanva[1].SetActive(false);
                }
            }

        }
    }

    public void Damaged(int Val=1)
    {
        Debug.Log(2);
        CurrCastleHP -= Val;
        foreach (TextMesh i in CastleHPText)
        {
            i.text = CurrCastleHP.ToString()+"/"+MaxCastleHP.ToString();
        }

        if (CurrCastleHP <= 0) {
            result = -1; 
        }
    }
    public bool SetWin() {
        if (result == -1)
            return false;

        result = 1;
        return true;
    }
}
