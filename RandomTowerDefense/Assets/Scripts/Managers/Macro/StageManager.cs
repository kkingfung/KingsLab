using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    private const int EnemySpawnPtNum = 3;

    [HideInInspector]
    public Coord[] SpawnPoint;

    public GameObject EnemySpawnPortPrefab;

    private InGameOperation sceneManager;
    private AudioManager audioManager;
    private ScoreCalculation scoreCalculation;
    public List<GameObject> GameClearCanva;
    public List<GameObject> GameOverCanva;

    public Material CoveringMateral;
    private readonly float FadeRate = 0.02f;

    private FilledMapGenerator mapGenerator;

    private GameObject[] EnemySpawnPort;
    [HideInInspector]
    public int CastleEntityID;
    private CastleSpawner castleSpawner;

    private int result = 0;
    private bool isReady = false;

    void Awake()
    {
        SpawnPoint = new Coord[EnemySpawnPtNum + 1];
        EnemySpawnPort = new GameObject[EnemySpawnPtNum];
    }
    // Start is called before the first frame update
    void Start()
    {
        sceneManager = FindObjectOfType<InGameOperation>();
        mapGenerator = FindObjectOfType<FilledMapGenerator>();
        audioManager = FindObjectOfType<AudioManager>();
        scoreCalculation = FindObjectOfType<ScoreCalculation>();
        castleSpawner = FindObjectOfType<CastleSpawner>();

        result = 0;
        foreach (GameObject i in GameClearCanva)
        {
            i.SetActive(false);
        }
        foreach (GameObject i in GameOverCanva)
            i.SetActive(false);

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

            //Fixed CastleMapPos
            int[] entityID = castleSpawner.Spawn(mapGenerator.CoordToPosition(SpawnPoint[0]) + mapGenerator.transform.position, Quaternion.Euler(0f, 90f, 0f),
                (int)PlayerPrefs.GetFloat("hpMax"), 1);
            CastleEntityID = entityID[0];
            for (int i = 0; i < EnemySpawnPtNum; ++i)
            {
                Vector3 pos = mapGenerator.CoordToPosition(SpawnPoint[i + 1]) + mapGenerator.transform.position;
                EnemySpawnPort[i] = Instantiate(EnemySpawnPortPrefab, pos, Quaternion.identity);
                PlayerPrefs.SetFloat("SpawnPointx" + i, pos.x);
                PlayerPrefs.SetFloat("SpawnPointy" + i, pos.y);
                PlayerPrefs.SetFloat("SpawnPointz" + i, pos.z);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckLose();
        if (result != 0 && isReady)
        {
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
            else
            {
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

    public void CheckLose()
    {
        if (GetCurrHP() <= 0 && result == 0)
        {
            result = -1;
            isReady = false;
            audioManager.PlayAudio("se_Lose");
            castleSpawner.GameObjects[CastleEntityID].GetComponent<MeshDestroy>().DestroyMesh();
            scoreCalculation.CalculationScore();
            StartCoroutine(FadeInRoutine());
        }
    }

    public bool SetWin()
    {
        if (result == -1)
            return false;

        result = 1;

        scoreCalculation.CalculationScore();
        if (sceneManager.GetEnabledIsland() == sceneManager.GetCurrIsland() && sceneManager.GetEnabledIsland() < StageInfo.IslandNum - 1)
            PlayerPrefs.SetInt("IslandEnabled", sceneManager.GetCurrIsland() + 1);
        audioManager.PlayAudio("se_Clear");
        return true;
    }
    public int GetMaxHP() { return castleSpawner.GameObjects[CastleEntityID].GetComponent<Castle>().MaxCastleHP; }
    public int GetCurrHP() { return Mathf.Max(0, castleSpawner.castleHPArray[0]); }

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

    public int GetResult() { return result; }

    public Vector3[] GetPortalPosition()
    {
        Vector3[] pos = new Vector3[EnemySpawnPort.Length];

        for (int i = 0; i < EnemySpawnPort.Length; ++i)
        {
            pos[i] = EnemySpawnPort[i].transform.position;
        }

        return pos;
    }
}
