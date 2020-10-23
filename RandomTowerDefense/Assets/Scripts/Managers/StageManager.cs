using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    private const int EnemyNum=3;

    [HideInInspector]
    public Coord[] SpawnPoint;

    public GameObject EnemySpawnPortPrefab;
    public GameObject CastlePrefab;

    private int MaxCastleHP;
    private int CurrCastleHP;
    public List<TextMesh> CastleHPText;

    private InGameOperation sceneManager;
    public List<GameObject> GameClearCanva;
    public List<GameObject> GameOverCanva;

    public Material CoveringMateral;
    private readonly float FadeRate = 0.02f;

    private FilledMapGenerator mapGenerator;

    private GameObject[] EnemySpawnPort;
    private GameObject CastlePointer;

    private int result = 0;
    private bool isReady = false;
    void Awake() {
        SpawnPoint = new Coord[EnemyNum + 1];
        EnemySpawnPort = new GameObject[EnemyNum];
    }
    // Start is called before the first frame update
    void Start()
    {
        sceneManager = FindObjectOfType<InGameOperation>();
        mapGenerator = FindObjectOfType<FilledMapGenerator>();
           result = 0;
        foreach (GameObject i in GameClearCanva) { 
            i.SetActive(false);
        }
        foreach (GameObject i in GameOverCanva)
            i.SetActive(false);

        int CurrIsland = sceneManager.GetCurrIsland();
        if (CurrIsland == sceneManager.GetTotalIslandNum() - 1) 
            MaxCastleHP = (int)PlayerPrefs.GetFloat("hpMaxDir");
        else
            MaxCastleHP = (int)PlayerPrefs.GetFloat("hpMax");

        CurrCastleHP = MaxCastleHP;
        foreach (TextMesh i in CastleHPText)
        {
            i.text = CurrCastleHP.ToString() + "/" + MaxCastleHP.ToString();
        }

        if (mapGenerator)
        {
            if (sceneManager.GetCurrIsland() != 3)
            {
                mapGenerator.OnNewStage(sceneManager.GetCurrIsland());
            }
            else
            {
                float TotalSize = PlayerPrefs.GetFloat("stageSize");
                TotalSize = Mathf.Sqrt(TotalSize);
                mapGenerator.CustomizeMapAndCreate((int)(TotalSize + 0.9f), (int)(TotalSize + 0.9f));
            }

            CastlePointer = Instantiate(CastlePrefab, mapGenerator.CoordToPosition(SpawnPoint[0]) + mapGenerator.transform.position, Quaternion.Euler(0f,90f,0f));
            for (int i = 0; i < EnemyNum; ++i) 
            EnemySpawnPort[i] = Instantiate(EnemySpawnPortPrefab, mapGenerator.CoordToPosition(SpawnPoint[i+1]) + mapGenerator.transform.position, Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Damaged(1);
        }
        if (result != 0 && isReady) {
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
        CurrCastleHP -= Val;
        foreach (TextMesh i in CastleHPText)
        {
            i.text = CurrCastleHP.ToString()+"/"+MaxCastleHP.ToString();
        }

        if (CurrCastleHP <= 0) {
            result = -1;
            isReady = false;
            StartCoroutine(FadeInRoutine());
        }
    }
    public bool SetWin() {
        if (result == -1)
            return false;

        result = 1;
        return true;
    }
    public int GetMaxHP() { return MaxCastleHP; }
    public int GetCurrHP() { return CurrCastleHP; }

    private IEnumerator FadeOutRoutine()
    {
        float Threshold = 0f;
        while (Threshold < 1f)
        {
            CoveringMateral.SetFloat("_FadeThreshold", Threshold);
            Threshold += FadeRate;
            yield return new WaitForSeconds(0f);
        }
        CoveringMateral.SetFloat("_FadeThreshold", Threshold);
    }

    private IEnumerator FadeInRoutine()
    {
        float Threshold = 1f;
        while (Threshold > 0f)
        {
            CoveringMateral.SetFloat("_FadeThreshold", Threshold);
            Threshold -= FadeRate;
            yield return new WaitForSeconds(0f);
        }
        CoveringMateral.SetFloat("_FadeThreshold", Threshold);
        isReady = true;

        StartCoroutine(FadeOutRoutine());
    }
}
