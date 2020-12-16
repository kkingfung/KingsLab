using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCalculation : MonoBehaviour
{
    private readonly int ScoreForBase = 5000; 
    private readonly int ScoreForStage = 500;
    private readonly int ScoreForStageEx = 10;
    private readonly int ScoreForCastleHP = 20;
    private readonly int ScoreForStartHP = 10;
    private readonly int ScoreForStartHPEx = 100;
    private readonly int ScoreForUpgrades = 100;

    private readonly int RecordCharNum = 5;

    public List<Text> ScoreObj;
    public List<Text> RankObj;
    public List<Text> NameObj;

    private int rank;
    private string playerName;
    private int score;
    private string scoreStr;

    [HideInInspector]
    public bool Inputting;

    private TouchScreenKeyboard keyboard;
    private bool CancelKeybroad = false;
    public InGameOperation sceneManager;
    public RecordManager recordManager;
    public StageManager stageManager;

    private List<UIEffect> uiEffect;
    //For Calculation
    public ResourceManager resourceManager;

    // Start is called before the first frame update
    private void Start()
    {
        Inputting = false;
        playerName = "AAAAA";
        rank = 0;
        //sceneManager = FindObjectOfType<InGameOperation>();
        //recordManager = FindObjectOfType<RecordManager>();
        //stageManager = FindObjectOfType<StageManager>();
        //resourceManager = FindObjectOfType<ResourceManager>();
        uiEffect = new List<UIEffect>();
        for (int i = 0; i < ScoreObj.Count; ++i)
            uiEffect.Add(ScoreObj[i].gameObject.GetComponent<UIEffect>());
    }

    private void OnDisable()
    {
        recordManager.UpdateRecordName(recordManager.rank, playerName);
    }
    private void LateUpdate()
    {
        if (rank <= RecordCharNum)
        {
            foreach (Text i in RankObj)
                i.text = rank == 0 ? "" : rank + ".";

            foreach (Text i in NameObj)
                i.text = rank == 0 ? "" : playerName;
        }
        else 
        {
            foreach (Text i in RankObj)
                i.text = "-";

            foreach (Text i in NameObj)
                i.text = "" ;
        }
    }

    public void CalculationScore() 
    {
        score = 0;
        scoreStr = "";

        int scoreChg = 0;
        int result = stageManager.GetResult();
        int currIsland = sceneManager.GetCurrIsland();

        //Clear
        if (result == (int)StageManager.GameResult.Won)
        {
            score += ScoreForBase + ScoreForStage * currIsland;
            score += ((currIsland != StageInfo.IslandNum - 1) ? 0 :
                (int)((StageInfo.MaxMapDepth * StageInfo.MaxMapDepth - StageInfo.stageSizeEx) * ScoreForStageEx * (1 / Mathf.Max(0.1f, StageInfo.obstacleEx))));
            scoreStr += score + "\n";
        }
        else {
            scoreStr += 0 + "\n";
        }

        //CastleHP
        scoreChg = ScoreForCastleHP * stageManager.GetCurrHP();
         score += scoreChg;
        scoreStr += "+" + scoreChg + "\n";

        //Resource
        scoreChg = resourceManager.GetCurrMaterial();
        scoreChg = (int)(scoreChg*((currIsland != StageInfo.IslandNum - 1) ? 1f :
                (1f/Mathf.Max(StageInfo.resourceEx,0.5f))));
        score += scoreChg;
        scoreStr += "+" + scoreChg + "\n";

        //Upgrades
        scoreChg = ScoreForUpgrades * Upgrades.allLevel();
        score += scoreChg;
        scoreStr += "+" + scoreChg + "\n";

        //Remark: Special Function for extra mode
        if (currIsland != StageInfo.IslandNum - 1)
        {
            if (result == (int)StageManager.GameResult.Won)
            {
                score -= StageInfo.hpMaxEx * ScoreForStartHP;
                scoreStr += "-" + StageInfo.hpMaxEx * ScoreForStartHP + "\n";
            }
            else 
            {
                scoreStr += "-" + score + "\n";
                score = 0;
            }
        }
        else {
            if (StageInfo.waveNumEx > 50 || (result == (int)StageManager.GameResult.Won))
            {
                score -= StageInfo.hpMaxEx * ScoreForStartHPEx;
                scoreStr += "-" + StageInfo.hpMaxEx * ScoreForStartHPEx + "\n";
            }
            else 
            {
                scoreStr += "-" + score + "\n";
                score = 0;
            }
        }

        scoreStr += "=" + score;

        for (int i = 0; i < ScoreObj.Count; ++i)
        {
            ScoreObj[i].text = scoreStr;
            if (uiEffect[i])
                uiEffect[i].ResetForScoreShow();

        }

        if (score <= 0) return;

        rank =recordManager.RecordComparison(currIsland, "ZYXWV", score);
    }

    public void TouchKeybroad()
    {
        if (rank > RecordCharNum)
        {
            return;
        }

        Inputting = true;

        keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, true);

        if (keyboard != null)
        {
            keyboard.characterLimit = RecordCharNum;
            StartCoroutine(TouchScreenInputUpdate());
            CancelKeybroad = false;
        }
    }

    private IEnumerator TouchScreenInputUpdate()
    {
        if (keyboard != null)
        {
            while (keyboard.status == TouchScreenKeyboard.Status.Visible && CancelKeybroad == false)
            {
                playerName = keyboard.text;
                foreach (Text i in NameObj)
                    i.text = playerName;
                yield return new WaitForSeconds(0f);
            }
            //if (keyboard.status == TouchScreenKeyboard.Status.Done || keyboard.status == TouchScreenKeyboard.Status.Canceled)

            keyboard = null;
        }
        recordManager.UpdateRecordName(rank, playerName);
        Inputting = false;
    }
}
