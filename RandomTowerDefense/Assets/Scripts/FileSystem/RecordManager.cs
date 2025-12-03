using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RandomTowerDefense.Scene;
using RandomTowerDefense.Info;
using RandomTowerDefense.FileSystem;

namespace RandomTowerDefense.FileSystem
{
    /// <summary>
    /// UIレコードリストクラス - TextMeshコンポーネント群の管理
    /// </summary>
    [System.Serializable]
    public class UIRecordList
    {
        [SerializeField] public List<TextMesh> Records;
    }

    /// <summary>
    /// レコードマネージャークラス - スコアランキングの表示と管理
    ///
    /// 主な機能:
    /// - ステージ別スコアランキングの管理と表示更新
    /// - SaveSystem統合による永続化データの操作
    /// - UI要素との自動同期とリアルタイム表示
    /// - 新記録挿入時のランク計算と表示更新
    /// - プレイヤー名変更機能とデータ保存
    /// - マルチステージ対応とスコア比較機能
    /// </summary>
    public class RecordManager : MonoBehaviour
    {
        #region Serialized Fields

        [Header("UI Record Display")]
        [SerializeField] public List<UIRecordList> AllRecordsName;
        [SerializeField] public List<UIRecordList> AllRecordsScore;

        [Header("Settings")]
        [SerializeField] public bool newRecordList;

        #endregion

        #region Public Properties

        public int rank { get; private set; }

        #endregion

        #region Private Fields

        private List<SaveObject> _stageRecords;
        private InGameOperation _sceneManager;

        #endregion
        #region Unity Lifecycle

        /// <summary>
        /// ゲームオブジェクト有効化時処理 - レコードデータ初期化と読み込み
        /// </summary>
        private void OnEnable()
        {
            InitializeStageRecords();
            LoadAllStageRecords();
            newRecordList = false;

            if (AllRecordsName.Count > 0)
            {
                UpdateUI();
            }
        }

        /// <summary>
        /// ゲームオブジェクト無効化時処理 - レコードデータ保存
        /// </summary>
        private void OnDisable()
        {
            SaveAllStageRecords();
            _stageRecords.Clear();
        }

        /// <summary>
        /// スタート処理 - シーンマネージャー参照取得
        /// </summary>
        private void Start()
        {
            _sceneManager = FindObjectOfType<InGameOperation>();
        }

        #endregion
        #region Public Methods

        /// <summary>
        /// スコア比較とランキング挿入処理
        /// </summary>
        /// <param name="stageID">ステージID</param>
        /// <param name="name">プレイヤー名</param>
        /// <param name="score">スコア</param>
        /// <returns>達成したランク位置</returns>
        public int RecordComparison(int stageID, string name, int score)
        {
            if (stageID < 0 || stageID >= _stageRecords.Count)
            {
                Debug.LogError($"Invalid stageID: {stageID}");
                return -1;
            }

            rank = _stageRecords[stageID].InsertObject(stageID, name, score);

            if (AllRecordsName.Count > 0)
            {
                UpdateUI();
            }

            return rank;
        }

        /// <summary>
        /// プレイヤー名更新処理
        /// </summary>
        /// <param name="rank">更新対象のランク</param>
        /// <param name="name">新しいプレイヤー名</param>
        public void UpdateRecordName(int rank, string name)
        {
            int currentIsland = _sceneManager.GetCurrIsland();
            if (currentIsland < 0 || currentIsland >= _stageRecords.Count)
            {
                Debug.LogError($"Invalid island ID: {currentIsland}");
                return;
            }

            if (UpdateRecordByRank(currentIsland, rank, name))
            {
                SaveSystem.SaveObject($"Record{currentIsland}", _stageRecords[currentIsland], true);
                UpdateUI();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// ステージレコード初期化
        /// </summary>
        private void InitializeStageRecords()
        {
            _stageRecords = new List<SaveObject>();
            _stageRecords.Clear();
        }

        /// <summary>
        /// 全ステージレコード読み込み
        /// </summary>
        private void LoadAllStageRecords()
        {
            for (int i = 0; i < StageInfoDetail.IslandNum; ++i)
            {
                SaveSystem.Init($"Record{i}", newRecordList);
                _stageRecords.Add(SaveSystem.LoadObject<SaveObject>($"Record{i}"));
            }
        }

        /// <summary>
        /// 全ステージレコード保存
        /// </summary>
        private void SaveAllStageRecords()
        {
            for (int i = 0; i < StageInfoDetail.IslandNum; ++i)
            {
                SaveSystem.SaveObject($"Record{i}", _stageRecords[i], true);
            }
        }

        /// <summary>
        /// UI表示更新処理
        /// </summary>
        private void UpdateUI()
        {
            for (int i = 0; i < StageInfoDetail.IslandNum; ++i)
            {
                if (i >= AllRecordsName.Count || i >= AllRecordsScore.Count) continue;

                UpdateStageUI(i);
            }
        }

        /// <summary>
        /// 指定ステージのUI更新
        /// </summary>
        /// <param name="stageIndex">ステージインデックス</param>
        private void UpdateStageUI(int stageIndex)
        {
            var nameRecords = AllRecordsName[stageIndex].Records;
            var scoreRecords = AllRecordsScore[stageIndex].Records;
            var stageRecord = _stageRecords[stageIndex];

            var records = new Record[]
            {
                stageRecord.record1,
                stageRecord.record2,
                stageRecord.record3,
                stageRecord.record4,
                stageRecord.record5
            };

            for (int rank = 0; rank < records.Length && rank < nameRecords.Count; rank++)
            {
                string displayName = records[rank].name.Length >= 5 ?
                    records[rank].name.Substring(0, 5).ToUpper() :
                    records[rank].name.ToUpper();

                nameRecords[rank].text = $"{rank + 1}.{displayName}";
                scoreRecords[rank].text = records[rank].score.ToString("000000");
            }
        }

        /// <summary>
        /// ランク指定レコード更新
        /// </summary>
        /// <param name="stageID">ステージID</param>
        /// <param name="rank">ランク</param>
        /// <param name="name">新しい名前</param>
        /// <returns>更新成功フラグ</returns>
        private bool UpdateRecordByRank(int stageID, int rank, string name)
        {
            switch (rank)
            {
                case 1: _stageRecords[stageID].record1.name = name; return true;
                case 2: _stageRecords[stageID].record2.name = name; return true;
                case 3: _stageRecords[stageID].record3.name = name; return true;
                case 4: _stageRecords[stageID].record4.name = name; return true;
                case 5: _stageRecords[stageID].record5.name = name; return true;
                default:
                    Debug.LogWarning($"Invalid rank: {rank}");
                    return false;
            }
        }

        #endregion
    }
}

