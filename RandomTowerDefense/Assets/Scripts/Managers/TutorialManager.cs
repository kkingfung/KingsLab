using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public enum TutorialStageID {
        TutorialProgress_Info = 0,
        TutorialProgress_FirstWave,
        TutorialProgress_StoreSkill,
        TutorialProgress_Finish,
        TutorialProgress_FreeBattle,
    }

    [HideInInspector]
    public bool FreeToBuild;

    public bool WaitingResponds;

    private TutorialStageID tutorialStage;
    private int StageProgress;

    private string fullText;
    private int textCnt;

    public List<Text> InstructionText_Landscape;
    public List<Text> InstructionText_Protrait;
    public List<GameObject> InstructionSprite_Landscape;
    public List<GameObject> InstructionSprite_Protrait;

    private InGameOperation SceneManager;
    private TowerSpawner towerSpawner;
    private EnemySpawner enemySpawner;
    private SkillSpawner skillSpawner;

    // Start is called before the first frame update
    void Start()
    {
        WaitingResponds = false;
        FreeToBuild = false;
        tutorialStage = 0;
        StageProgress = 0;
        textCnt = 0;
        fullText = "";

        SceneManager = FindObjectOfType<InGameOperation>();
        towerSpawner = FindObjectOfType<TowerSpawner>();
        enemySpawner = FindObjectOfType<EnemySpawner>();
        skillSpawner = FindObjectOfType<SkillSpawner>();
        if (SceneManager.CheckIfTutorial() == false) {
            DestroyAllRelated();
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (tutorialStage) { 
            case TutorialStageID.TutorialProgress_Info:
                WaitingResponds = true;
                Update_TutorialInfo();
                break;
            case TutorialStageID.TutorialProgress_FirstWave:
                WaitingResponds = false;
                Update_TutorialFirstWave();
                break;
            case TutorialStageID.TutorialProgress_StoreSkill:
                WaitingResponds = true;
                Update_TutorialStoreSkill();
                break;
            case TutorialStageID.TutorialProgress_Finish:
                WaitingResponds = true;
                Update_TutorialFinish();
                break;
            case TutorialStageID.TutorialProgress_FreeBattle:
                WaitingResponds = false;
                DestroyAllRelated();
                return;
        }
        FixedUpdateText();
        UpdateActiveness();
    }

    private void UpdateActiveness() {
        foreach (Text i in InstructionText_Landscape)
            i.enabled = SceneManager.OrientationLand && WaitingResponds && fullText!="";
        foreach (GameObject i in InstructionSprite_Landscape)
            i.SetActive(SceneManager.OrientationLand && WaitingResponds && fullText != "") ;

        foreach (Text i in InstructionText_Protrait)
            i.enabled = !SceneManager.OrientationLand && WaitingResponds && fullText != "";
        foreach (GameObject i in InstructionSprite_Protrait)
            i.SetActive(!SceneManager.OrientationLand && WaitingResponds && fullText != "");
    }

    private void ChangeText(string fulltext)
    {
        textCnt = 0;
        fullText = fulltext;
    }

    private void FixedUpdateText()
    {
        textCnt = Mathf.Min(textCnt + 1, fullText.Length);
        foreach (Text i in InstructionText_Landscape)
        {
            i.text = fullText.Substring(0, textCnt);

        }
        foreach (Text i in InstructionText_Protrait)
        {
            i.text = fullText.Substring(0, textCnt);
        }
    }

    private void Update_TutorialInfo() {
        switch (StageProgress)
        {
            case 0:
                ChangeText("新人，ハンは第九師団団長だぞ");
                StageProgress++;
                break;
            case 1:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount>0 && Input.touches[0].phase==TouchPhase.Ended))
                {
                    ChangeText("今日は初任務だけど、\n心配要らないだぞ");
                    StageProgress++;
                }
                break;
            case 2:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("ハンは傍にいるだぞ");
                    StageProgress++;
                }
                break;
            case 3:
                if (Input.GetMouseButtonUp(0) ||
                 (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("自分を信じなくていい、\nハンを信じるだぞ");
                    StageProgress++;
                }
                break;
            case 4:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("では早速準備するだぞ");
                    StageProgress++;
                }
                break;
            case 5:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("真中の印で柱に狙って、\n2回押すだぞ");
                    StageProgress++;
                }
                break;
            case 6:
                WaitingResponds = false;
                if (((Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))) 
                    && towerSpawner.GameObjects[0]!=null)
                {
                    ChangeText("それは新人の部隊だぞ");
                    StageProgress++;
                }
                break;
            case 7:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("同じ種類を3つ集まるだぞ");
                    StageProgress++;
                }
                break;
            case 8:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("そしてもう一回狙って\n2回押すと");
                    StageProgress++;
                }
                break;
            case 9:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("更に強くなれるだぞ");
                    StageProgress++;
                }
                break;
            case 10:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("静かに、敵が来た");
                    StageProgress++;
                }
                break;
            case 11:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("");
                    StageProgress = 0;
                    tutorialStage++;
                }
                break;
        }
    }

    private void Update_TutorialFirstWave()
    {
        switch (StageProgress)
        {
            case 0:
                if (enemySpawner.GameObjects[0]!=null)
                    StageProgress++;
                break;
            case 1:
                if (enemySpawner.AllAliveMonstersList().Count <= 0) {
                    ChangeText("悪くない\n褒めてやるだぞ");
                    StageProgress = 0;
                    tutorialStage++;
                }
                break;
        }
    }

    private void Update_TutorialStoreSkill()
    {
        switch (StageProgress)
        {
            case 0:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("画面矢印を押すか、\n指示のよう振るだぞ");
                    StageProgress++;
                }
                break;
            case 1:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("");
                    StageProgress++;
                }
                break;
            case 2:
                WaitingResponds = false;
                if (SceneManager.currScreenShown==2)
                {
                    ChangeText("これは基地通信だぞ、\n追加ミッションを要求");
                    StageProgress++;
                }
                break;
            case 3:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("お金で、兵士の訓練、\n道具の購入もできるだぞ");
                    StageProgress++;
                }
                break;
            case 4:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("けど、今回は私の道具を\n貸してやるだぞ");
                    SkillStack.AddStock(Upgrades.StoreItems.MagicMeteor);
                    StageProgress++;
                }
                break;
            case 5:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("早く戦場に戻るだぞ");
                    StageProgress++;
                }
                break;
            case 6:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("");
                    StageProgress++;
                }
                break;
            case 7:
                WaitingResponds = false;
                if (SceneManager.currScreenShown == 0)
                {
                    ChangeText("道具の使用は長押しながら、\nアイテムを選ぶだぞ");
                    StageProgress++;
                }
                break;
            case 8:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("");
                    StageProgress++;
                }
                break;
            case 9:
                WaitingResponds = false;
                if (skillSpawner.GameObjects[0] != null)
                {
                    StageProgress = 0;
                    tutorialStage++;
                }
                break;
        }
    }

    private void Update_TutorialFinish()
    {
        switch (StageProgress)
        {
            case 0:
                if (enemySpawner.AllAliveMonstersList().Count <= 0)
                {
                    ChangeText("ハンのおかけで、\nどうやら大丈夫だ");
                    StageProgress++;
                }
                break;
            case 1:
                if (Input.GetMouseButtonUp(0) ||
                     (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("ハンは忙しいから、\n雑魚は新人に任すだぞ");
                    StageProgress++;
                }
                break;
            case 2:
                if (Input.GetMouseButtonUp(0) ||
                        (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("さよなら");
                    StageProgress++;
                }
                break;
            case 3:
                if (Input.GetMouseButtonUp(0) ||
                        (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("");
                    StageProgress = 0;
                    tutorialStage++;
                }
                break;
        }
    }

    public void SetTutorialStage(TutorialStageID stage)
    {
        tutorialStage = stage;
    }

    private void DestroyAllRelated() {
        foreach (Text i in InstructionText_Landscape)
            Destroy(i.gameObject);
        foreach (GameObject i in InstructionSprite_Landscape)
            Destroy(i);
        foreach (Text i in InstructionText_Protrait)
            Destroy(i.gameObject);
        foreach (GameObject i in InstructionSprite_Protrait)
            Destroy(i);
        Destroy(this);
    }
}
