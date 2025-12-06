using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RandomTowerDefense.DOTS.Spawner;
using RandomTowerDefense.Scene;
using RandomTowerDefense.Managers.System;
using RandomTowerDefense.Managers.Macro;
using RandomTowerDefense.Systems;
using RandomTowerDefense.Units;

/// <summary>
/// チュートリアル管理システム - 新規プレイヤー向け段階的学習システム
///
/// 主な機能:
/// - 段階的チュートリアル進行管理（情報、初Wave、ストア、完了）
/// - ランドスケープ/ポートレート両対応UI表示制御
/// - 文字送りエフェクトによるインストラクション表示
/// - プレイヤー行動待機と自動進行制御
/// - 過去インストラクション振り返り機能
/// - チュートリアル完了後のフリーバトルへ遷移
/// - タイムスケール調整によるゲーム進行制御
/// </summary>
public class TutorialManager : MonoBehaviour
{
    #region Enums
    /// <summary>
    /// チュートリアル段階ID列挙型
    /// </summary>
    public enum TutorialStageID
    {
        /// <summary>初期情報表示段階</summary>
        TutorialProgress_Info = 0,
        /// <summary>初Wave体験段階</summary>
        TutorialProgress_FirstWave,
        /// <summary>ストア・スキル説明段階</summary>
        TutorialProgress_StoreSkill,
        /// <summary>チュートリアル完了段階</summary>
        TutorialProgress_Finish,
        /// <summary>フリーバトル段階</summary>
        TutorialProgress_FreeBattle,
    }
    #endregion

    #region Public Properties
    /// <summary>タワー建設可能フラグ</summary>
    [HideInInspector]
    public bool FreeToBuild;

    /// <summary>プレイヤー応答待ちフラグ</summary>
    public bool WaitingResponds;
    #endregion

    #region Private Fields
    private TutorialStageID tutorialStage;
    private int StageProgress;

    private string fullText;
    private float textCnt;

    private int reviewStage;
    #endregion

    #region Serialized Fields
    [Header("📱 UI Elements - Landscape")]
    public List<Text> InstructionText_Landscape;
    public List<GameObject> InstructionSprite_Landscape;

    [Header("📱 UI Elements - Portrait")]
    public List<Text> InstructionText_Protrait;
    public List<GameObject> InstructionSprite_Protrait;

    [Header("📜 History")]
    public List<Button> HistoryIcons;

    [Header("🎮 Manager References")]
    public InGameOperation SceneManager;
    public TowerSpawner towerSpawner;
    public EnemySpawner enemySpawner;
    public SkillSpawner skillSpawner;
    public TimeManager timeManager;
    public ResourceManager resourceManager;
    #endregion

    #region Private Fields (Continued)
    private float timeWait;
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// チュートリアルシステム初期化 - 全状態をリセットしてチュートリアル開始準備
    /// </summary>
    void Start()
    {
        WaitingResponds = false;
        FreeToBuild = false;
        tutorialStage = 0;
        StageProgress = 0;
        textCnt = 0;
        fullText = "";
        reviewStage = 0;
        timeWait = 0;
        //SceneManager = FindObjectOfType<InGameOperation>();
        //towerSpawner = FindObjectOfType<TowerSpawner>();
        //enemySpawner = FindObjectOfType<EnemySpawner>();
        //skillSpawner = FindObjectOfType<SkillSpawner>();
        //timeManager = FindObjectOfType<TimeManager>();
        if (SceneManager.CheckIfTutorial() == false)
        {
            DestroyAllRelated();
        }
    }

    /// <summary>
    /// 毎フレーム更新 - 現在のチュートリアル段階に応じた処理を実行
    /// </summary>
    void Update()
    {
        switch (tutorialStage)
        {
            case TutorialStageID.TutorialProgress_Info:
                WaitingResponds = true;
                Update_TutorialInfo();
                break;
            case TutorialStageID.TutorialProgress_FirstWave:
                WaitingResponds = true;
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
                timeManager.timeFactor = 0.05f;
                timeManager.TimeControl();
                reviewStage = 0;
                DestroyAllRelated();
                return;
        }
        foreach (Button i in HistoryIcons)
            i.interactable = !WaitingResponds && tutorialStage < TutorialStageID.TutorialProgress_FreeBattle;

        FixedUpdateText();
        UpdateActiveness();
    }

    #endregion

    #region Private Methods

    private void UpdateActiveness()
    {
        foreach (Text i in InstructionText_Landscape)
            i.enabled = SceneManager.OrientationLand && WaitingResponds && fullText != "";
        foreach (GameObject i in InstructionSprite_Landscape)
            i.SetActive(SceneManager.OrientationLand && WaitingResponds && fullText != "");

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
        textCnt = Mathf.Min(textCnt + 0.5f, fullText.Length);
        foreach (Text i in InstructionText_Landscape)
        {
            i.text = fullText.Substring(0, (int)textCnt);

        }
        foreach (Text i in InstructionText_Protrait)
        {
            i.text = fullText.Substring(0, (int)textCnt);
        }
    }

    private void Update_TutorialInfo()
    {
        switch (StageProgress)
        {
            case 0:
                resourceManager.CurrentMaterial = 0;
                ChangeText("新人，Kは第九師団団長なのだ");
                StageProgress++;
                reviewStage = 0;
                break;
            case 1:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("今日は初任務だけど、\n心配要らないのだ");
                    StageProgress++;
                }
                break;
            case 2:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("Kは傍にいるのだ");
                    StageProgress++;
                }
                break;
            case 3:
                if (Input.GetMouseButtonUp(0) ||
                 (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("自分を信じなくていい、\nKを信じるのだ");
                    StageProgress++;
                }
                break;
            case 4:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("では早速準備するのだ");
                    StageProgress++;
                }
                break;
            case 5:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("真中の印で柱に狙って、\n2回押すのだ");
                    StageProgress++;
                }
                break;
            case 6:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("");
                    StageProgress++;
                    resourceManager.ResetMaterial();
                }
                break;
            case 7:
                WaitingResponds = false;
                if (towerSpawner.AllAliveObjList().Count > 0)
                {
                    ChangeText("");
                    StageProgress++;
                    timeWait = Time.time;
                }
                break;
            case 8:
                WaitingResponds = false;
                if (Time.time - timeWait > 1)
                {
                    ChangeText("よくできた、\n120Gだが兵士が必要なのだ");
                    StageProgress++;
                    reviewStage = 7;
                }
                break;
            case 9:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("最高レベルになると、\n支援金がもらえるのだ");
                    StageProgress++;
                }
                break;
            case 10:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("その他にも支援金が\nもらえる方法が…");
                    StageProgress++;
                }
                break;
            case 11:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("…がある…\n…ある……あ…");
                    StageProgress++;
                }
                break;
            case 12:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("自分で色んな事を\n試せばいいのだ");
                    StageProgress++;
                }
                break;
            case 13:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("そして、\n同じ種類を3つ集まり");
                    StageProgress++;
                }
                break;
            case 14:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("もう一回狙って\n2回押すと");
                    StageProgress++;
                }
                break;
            case 15:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("更に強くなれるのだ");
                    StageProgress++;
                }
                break;
            case 16:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("…ちょっと静かに");
                    StageProgress++;
                }
                break;
            case 17:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("敵が来たのだ");
                    StageProgress++;
                }
                break;
            case 18:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
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
                reviewStage = 0;
                ChangeText("");
                StageProgress++;
                break;
            case 1:
                WaitingResponds = false;
                reviewStage = 1;
                if (enemySpawner.AllAliveMonstersList().Count > 0)
                {
                    StageProgress++;
                }
                break;
            case 2:
                WaitingResponds = false;
                reviewStage = 2;
                if (enemySpawner.AllAliveMonstersList().Count <= 0)
                {
                    StageProgress = 0;
                    tutorialStage++;
                    reviewStage = 0;
                }
                break;
        }
    }

    private void Update_TutorialStoreSkill()
    {
        switch (StageProgress)
        {
            case 0:
                ChangeText("悪くない\n褒めてやるのだ");
                StageProgress++;
                reviewStage = 0;
                break;
            case 1:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("画面矢印を押すか、\n上をみるのだ");
                    StageProgress++;
                }
                break;
            case 2:
                reviewStage = 0;
                ChangeText("上は基地通信だぞ、\n追加ミッションを要求");
                StageProgress++;
                break;
            case 3:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("お金で、兵士の訓練、\n道具の購入もできるのだ");
                    StageProgress++;
                }
                break;
            case 4:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("けど、今回は私の道具を\n貸してやるのだ");
                    SkillStack.AddStock(UpgradesManager.StoreItems.MagicMeteor);
                    StageProgress++;
                }
                break;
            case 5:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("");
                    StageProgress++;
                }
                break;
            case 6:
                reviewStage = 6;
                ChangeText("道具の使用は長押しながら、\nアイテムを選ぶのだ");
                StageProgress++;
                break;
            case 7:
                if (Input.GetMouseButtonUp(0) ||
                    (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("");
                    StageProgress++;
                }
                break;
            case 8:
                WaitingResponds = false;
                if (skillSpawner.AllAliveSkillsList().Count > 0)
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
                reviewStage = 0;
                ChangeText("");
                StageProgress++;
                break;
            case 1:
                WaitingResponds = false;
                if (enemySpawner.AllAliveMonstersList().Count > 0)
                {
                    StageProgress++;
                }
                break;
            case 2:
                WaitingResponds = false;
                if (enemySpawner.AllAliveMonstersList().Count <= 0)
                {
                    ChangeText("Kのおかけで、\nどうやら大丈夫のだ");
                    StageProgress++;
                }
                break;
            case 3:
                if (Input.GetMouseButtonUp(0) ||
                     (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("Kは忙しいから、\n雑魚は新人に任すのだ");
                    StageProgress++;
                }
                break;
            case 4:
                if (Input.GetMouseButtonUp(0) ||
                        (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
                {
                    ChangeText("さよなら");
                    StageProgress++;
                }
                break;
            case 5:
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

    #endregion

    #region Public API

    public void SetTutorialStage(TutorialStageID stage)
    {
        tutorialStage = stage;
    }
    public TutorialStageID GetTutorialStage()
    {
        return tutorialStage;
    }

    public int GetTutorialStageProgress()
    {
        return StageProgress;
    }

    public void DestroyAllRelated()
    {
        foreach (Text i in InstructionText_Landscape)
            if (i) Destroy(i.gameObject);
        foreach (GameObject i in InstructionSprite_Landscape)
            if (i) Destroy(i);
        foreach (Text i in InstructionText_Protrait)
            if (i) Destroy(i.gameObject);
        foreach (GameObject i in InstructionSprite_Protrait)
            if (i) Destroy(i);
        foreach (Button i in HistoryIcons)
            if (i) Destroy(i.gameObject);
        Destroy(this);
    }

    public void ViewHistory()
    {
        StageProgress = reviewStage;
    }

    #endregion
}
