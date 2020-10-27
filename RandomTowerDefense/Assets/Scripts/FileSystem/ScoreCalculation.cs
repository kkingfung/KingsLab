using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCalculation : MonoBehaviour
{
    public List<Text> ScoreObj;
    public List<Text> RankObj;
    public List<Text> NameObj;

    private int rank;
    private string name;
    private int score;
    private string scoreStr;

    [HideInInspector]
    public bool Inputting;

    private TouchScreenKeyboard keyboard;
    private bool CancelKeybroad = false;
    private InGameOperation sceneManager;
    private RecordManager recordManager;
    private StageManager stageManager;

    //For Calculation
    private ResourceManager resourceManager;

    // Start is called before the first frame update
    private void Start()
    {
        Inputting = false;
        name = "AAAAA";
        rank = 99;
        sceneManager = FindObjectOfType<InGameOperation>();
        recordManager = FindObjectOfType<RecordManager>();
        stageManager = FindObjectOfType<StageManager>();
    }
    private void LateUpdate()
    {
        foreach (Text i in RankObj)
            i.text = rank+ ".";
        foreach (Text i in NameObj)
            i.text = name;
    }

    public void CalculationScore() {

        score = 0;
        scoreStr = "";

        int scoreChg = 0;
        int result = stageManager.GetResult();
        int currIsland = sceneManager.GetCurrIsland();

        //Clear
        if (result > 0) {
            score += 2000 + 50 * currIsland;
            scoreStr += score+"\n";
        }

        //CastleHP
        scoreChg = 20 * stageManager.GetCurrHP();
        score += scoreChg;
        scoreStr += "+" + scoreChg + "\n";


        //Upgrades
        scoreChg = 100 * Upgrades.allLevel();
        score += scoreChg;
        scoreStr += "+" + scoreChg + "\n";

        //Resource
        scoreChg = resourceManager.GetCurrMaterial();
        score += scoreChg;
        scoreStr += "+" + scoreChg + "\n";


        //Remark: Special Function for extra mode
        if (currIsland != sceneManager.IslandNum - 1 && result==-1) {
            scoreStr += "-" + score + "\n";
            score = 0;
        }

        scoreStr += "=" + score;

        if (score <= 0) return;
        rank=recordManager.RecordComparison(currIsland, "ZYXWV", score);

        foreach (Text i in ScoreObj)
        {
                i.text = scoreStr;
                i.gameObject.GetComponent<UIEffect>().ResetForScoreShow();
        }
    }

    public void TouchKeybroad(int infoID)
    {
        if (rank > 5) return;

        Inputting = true;

        keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, true);

        if (keyboard != null)
        {
            keyboard.characterLimit = 5;
            StartCoroutine(TouchScreenInputUpdate(infoID));
            CancelKeybroad = false;
        }
    }

    private IEnumerator TouchScreenInputUpdate(int infoID)
    {
        if (keyboard != null)
        {
            while (keyboard.status == TouchScreenKeyboard.Status.Visible && CancelKeybroad == false)
            {
                foreach(Text i in NameObj)
                    i.text = keyboard.text;
                yield return new WaitForSeconds(0f);
            }

           //if (keyboard.status == TouchScreenKeyboard.Status.Done || keyboard.status == TouchScreenKeyboard.Status.Canceled)
             
            keyboard = null;
        }

        recordManager.UpdateRecordName(rank, name);
        Inputting = false;
    }
}
