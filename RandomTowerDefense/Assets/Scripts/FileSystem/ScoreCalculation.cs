using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RandomTowerDefense.Managers.Macro;
using RandomTowerDefense.Scene;
using RandomTowerDefense.Common;
using RandomTowerDefense.Info;
using RandomTowerDefense.Units;
using RandomTowerDefense.FileSystem;

/// <summary>
/// スコア計算システム - ゲーム終了時のスコア算出とランキング管理
///
/// 主な機能:
/// - ステージクリア時のスコア計算処理
/// - 城HP、リソース、アップグレードレベルのスコア換算
/// - エクストラモード対応スコア補正システム
/// - プレイヤー名入力とランキング登録
/// - タッチスクリーンキーボード統合
/// - スコア表示UI更新とエフェクト管理
/// </summary>
public class ScoreCalculation : MonoBehaviour
{
    // スコア計算用定数
    private readonly int ScoreForBase = 5000;
    private readonly int ScoreForStage = 500;
    private readonly int ScoreForStageEx = 10;
    private readonly int ScoreForCastleHP = 20;
    private readonly int ScoreForStartHP = 10;
    private readonly int ScoreForStartHPEx = 100;
    private readonly int ScoreForUpgrades = 100;

    private readonly int RecordCharNum = 5;
    // スコア計算用定数
    private const float MIN_OBSTACLE_FACTOR = 0.1f;
    private const float RESOURCE_FACTOR_MULTIPLIER = 1.0f;
    private const float MIN_RESOURCE_FACTOR = 0.5f;
    private const int WAVE_NUM_FACTOR_THRESHOLD = 50;
    private const float WAIT_TIME_SECONDS = 0f;

    /// <summary>
    /// スコア表示用UIテキストリスト
    /// </summary>
    public List<Text> ScoreObj;

    /// <summary>
    /// ランク表示用UIテキストリスト
    /// </summary>
    public List<Text> RankObj;

    /// <summary>
    /// プレイヤー名表示用UIテキストリスト
    /// </summary>
    public List<Text> NameObj;

    private int rank;
    private string playerName;
    private int score;
    private string scoreStr;

    /// <summary>
    /// キーボード入力中フラグ
    /// </summary>
    [HideInInspector]
    public bool Inputting;

    private TouchScreenKeyboard keyboard;
    private bool CancelKeybroad = false;

    /// <summary>
    /// インゲームシーンマネージャー参照
    /// </summary>
    public InGameOperation sceneManager;

    /// <summary>
    /// レコードマネージャー参照
    /// </summary>
    public RecordManager recordManager;

    /// <summary>
    /// ステージマネージャー参照
    /// </summary>
    public StageManager stageManager;

    private List<UIEffect> uiEffect;

    /// <summary>
    /// リソースマネージャー参照（スコア計算用）
    /// </summary>
    public ResourceManager resourceManager;

    /// <summary>
    /// アップグレードマネージャー参照（スコア計算用）
    /// </summary>
    public UpgradesManager upgradesManager;

    /// <summary>
    /// 開始時処理 - 初期化とUIエフェクト設定
    /// </summary>
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

    /// <summary>
    /// 無効化時処理 - プレイヤー名の最終保存
    /// </summary>
    private void OnDisable()
    {
        recordManager.UpdateRecordName(recordManager.rank, playerName);
    }

    /// <summary>
    /// 遅延更新処理 - ランクと名前表示の更新
    /// </summary>
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
                i.text = "";
        }
    }

    /// <summary>
    /// スコア計算とランキング登録処理
    /// </summary>
    public void CalculationScore()
    {
        score = 0;
        scoreStr = "";

        int scoreChg = 0;
        int result = stageManager.GetResult();
        int currIsland = sceneManager.GetCurrIsland();

        // クリア
        if (result == (int)StageManager.GameResult.Won)
        {
            score += ScoreForBase + ScoreForStage * currIsland;
            score += ((currIsland != StageInfoDetail.IslandNum - 1) ? 0 :
                (int)((DefaultStageInfos.MaxMapDepth * DefaultStageInfos.MaxMapDepth - StageInfoDetail.customStageInfo.StageSizeFactor) * ScoreForStageEx * (1 / Mathf.Max(MIN_OBSTACLE_FACTOR, StageInfoDetail.customStageInfo.ObstacleFactor))));
            scoreStr += score + "\n";
        }
        else
        {
            scoreStr += 0 + "\n";
        }

        // 城のHP
        scoreChg = ScoreForCastleHP * stageManager.GetCurrHP();
        score += scoreChg;
        scoreStr += "+" + scoreChg + "\n";

        // リソース
        scoreChg = resourceManager.GetCurrMaterial();
        scoreChg = (int)(scoreChg * ((currIsland != StageInfoDetail.IslandNum - 1) ? RESOURCE_FACTOR_MULTIPLIER :
                (RESOURCE_FACTOR_MULTIPLIER / Mathf.Max(StageInfoDetail.customStageInfo.ResourceFactor, 0.5f))));
        score += scoreChg;
        scoreStr += "+" + scoreChg + "\n";

        // アップグレード
        scoreChg = ScoreForUpgrades * (upgradesManager ? upgradesManager.GetTotalLevel() : 0);
        score += scoreChg;
        scoreStr += "+" + scoreChg + "\n";

        // 備考: エクストラモード用特別関数
        if (currIsland != StageInfoDetail.IslandNum - 1)
        {
            if (result == (int)StageManager.GameResult.Won)
            {
                score -= StageInfoDetail.customStageInfo.HpMaxFactor * ScoreForStartHP;
                scoreStr += "-" + StageInfoDetail.customStageInfo.HpMaxFactor * ScoreForStartHP + "\n";
            }
            else
            {
                scoreStr += "-" + score + "\n";
                score = 0;
            }
        }
        else
        {
            if (StageInfoDetail.customStageInfo.WaveNumFactor > WAVE_NUM_FACTOR_THRESHOLD || (result == (int)StageManager.GameResult.Won))
            {
                score -= StageInfoDetail.customStageInfo.HpMaxFactor * ScoreForStartHPEx;
                scoreStr += "-" + StageInfoDetail.customStageInfo.HpMaxFactor * ScoreForStartHPEx + "\n";
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

        rank = recordManager.RecordComparison(currIsland, "ZYXWV", score);
    }

    /// <summary>
    /// タッチスクリーンキーボードを開く処理
    /// </summary>
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

    /// <summary>
    /// タッチスクリーンキーボード入力更新コルーチン
    /// </summary>
    /// <returns>コルーチン</returns>
    private IEnumerator TouchScreenInputUpdate()
    {
        if (keyboard != null)
        {
            while (keyboard.status == TouchScreenKeyboard.Status.Visible && CancelKeybroad == false)
            {
                playerName = keyboard.text;
                foreach (Text i in NameObj)
                    i.text = playerName;
                yield return new WaitForSeconds(WAIT_TIME_SECONDS);
            }

            keyboard = null;
        }
        recordManager.UpdateRecordName(rank, playerName);
        Inputting = false;
    }
}
