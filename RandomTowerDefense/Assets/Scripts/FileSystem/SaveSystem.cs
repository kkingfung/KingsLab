using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace RandomTowerDefense.FileSystem
{
    /// <summary>
    /// セーブシステムユーティリティ - ゲームデータの永続化と復元管理
    ///
    /// 主な機能:
    /// - JSONベースセーブデータの永続化処理
    /// - スコアランキングデータの管理と更新
    /// - ファイルシステム操作とエラーハンドリング
    /// - 最新ファイル検出とバージョン管理
    /// - 自動バックアップと上書き保護機能
    /// - プラットフォーム独立パス管理
    /// </summary>
    public static class SaveSystem
    {
        #region Constants

        private static readonly string _saveFolder = Application.persistentDataPath + "/RandomDefender/";
        private const string _saveExtension = "txt";
        private const int _defaultRecordCount = 5;
        private const int _stageCount = 4;

        #endregion

        #region Public API

        /// <summary>
        /// セーブシステム初期化 - ディレクトリ作成とデフォルトデータセットアップ
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <param name="newBuild">新規ビルドフラグ</param>
        public static void Init(string fileName, bool newBuild)
        {
            EnsureSaveFolderExists();

            string fullPath = GetFullFilePath(fileName);
            if (newBuild || !File.Exists(fullPath))
            {
                CreateDefaultRecords();
            }
        }

        /// <summary>
        /// 文字列データの保存処理
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <param name="saveString">保存する文字列</param>
        /// <param name="overwrite">上書き許可フラグ</param>
        public static void Save(string fileName, string saveString, bool overwrite = true)
        {
            try
            {
                EnsureSaveFolderExists();
                string saveFileName = GetUniqueFileName(fileName, overwrite);
                string fullPath = GetFullFilePath(saveFileName);
                File.WriteAllText(fullPath, saveString);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to save file '{fileName}': {ex.Message}");
            }
        }

        /// <summary>
        /// 指定ファイルの読み込み処理
        /// </summary>
        /// <param name="fileName">読み込むファイル名</param>
        /// <returns>ファイル内容、またはnull</returns>
        public static string Load(string fileName)
        {
            try
            {
                string fullPath = GetFullFilePath(fileName);
                if (File.Exists(fullPath))
                {
                    return File.ReadAllText(fullPath);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load file '{fileName}': {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// 最新ファイルの読み込み処理
        /// </summary>
        /// <returns>最新ファイルの内容、またはnull</returns>
        public static string LoadMostRecentFile()
        {
            try
            {
                if (!Directory.Exists(_saveFolder))
                {
                    return null;
                }

                DirectoryInfo directoryInfo = new DirectoryInfo(_saveFolder);
                FileInfo[] saveFiles = directoryInfo.GetFiles($"*.{_saveExtension}");

                FileInfo mostRecentFile = saveFiles
                    .OrderByDescending(f => f.LastWriteTime)
                    .FirstOrDefault();

                return mostRecentFile != null ? File.ReadAllText(mostRecentFile.FullName) : null;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load most recent file: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// SaveObjectのデフォルト保存処理
        /// </summary>
        /// <param name="saveObject">保存するSaveObject</param>
        public static void SaveObject(SaveObject saveObject)
        {
            SaveObject("save", saveObject, false);
        }

        /// <summary>
        /// SaveObjectのJSON形式保存処理
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <param name="saveObject">保存するSaveObject</param>
        /// <param name="overwrite">上書き許可フラグ</param>
        public static void SaveObject(string fileName, SaveObject saveObject, bool overwrite)
        {
            try
            {
                string json = JsonUtility.ToJson(saveObject, true);
                Save(fileName, json, overwrite);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to save object '{fileName}': {ex.Message}");
            }
        }

        /// <summary>
        /// 最新オブジェクトの読み込み処理
        /// </summary>
        /// <typeparam name="TSaveObject">読み込むオブジェクト型</typeparam>
        /// <returns>デシリアライズされたオブジェクト</returns>
        public static TSaveObject LoadMostRecentObject<TSaveObject>()
        {
            try
            {
                string saveString = LoadMostRecentFile();
                return saveString != null ? JsonUtility.FromJson<TSaveObject>(saveString) : default(TSaveObject);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load most recent object: {ex.Message}");
                return default(TSaveObject);
            }
        }

        /// <summary>
        /// 指定オブジェクトの読み込み処理
        /// </summary>
        /// <typeparam name="TSaveObject">読み込むオブジェクト型</typeparam>
        /// <param name="fileName">ファイル名</param>
        /// <returns>デシリアライズされたオブジェクト</returns>
        public static TSaveObject LoadObject<TSaveObject>(string fileName)
        {
            try
            {
                string saveString = Load(fileName);
                return saveString != null ? JsonUtility.FromJson<TSaveObject>(saveString) : default(TSaveObject);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load object '{fileName}': {ex.Message}");
                return default(TSaveObject);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// セーブフォルダ存在確認と作成
        /// </summary>
        private static void EnsureSaveFolderExists()
        {
            if (!Directory.Exists(_saveFolder))
            {
                Directory.CreateDirectory(_saveFolder);
            }
        }

        /// <summary>
        /// フルファイルパス取得
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <returns>フルパス</returns>
        private static string GetFullFilePath(string fileName)
        {
            return Path.Combine(_saveFolder, $"{fileName}.{_saveExtension}");
        }

        /// <summary>
        /// 一意なファイル名取得
        /// </summary>
        /// <param name="fileName">ベースファイル名</param>
        /// <param name="overwrite">上書き許可フラグ</param>
        /// <returns>一意ファイル名</returns>
        private static string GetUniqueFileName(string fileName, bool overwrite)
        {
            if (overwrite)
            {
                return fileName;
            }

            string uniqueFileName = fileName;
            int saveNumber = 1;

            while (File.Exists(GetFullFilePath(uniqueFileName)))
            {
                uniqueFileName = $"{fileName}_{saveNumber}";
                saveNumber++;
            }

            return uniqueFileName;
        }

        /// <summary>
        /// デフォルトレコード作成
        /// </summary>
        private static void CreateDefaultRecords()
        {
            var defaultRecords = new Record[]
            {
                new Record("AAAAA", 5000),
                new Record("BBBBB", 1000),
                new Record("CCCCC", 500),
                new Record("DDDDD", 300),
                new Record("EEEEE", 100)
            };

            for (int i = 0; i < _stageCount; ++i)
            {
                SaveObject defaultRecord = new SaveObject
                {
                    stageID = i,
                    record1 = defaultRecords[0],
                    record2 = defaultRecords[1],
                    record3 = defaultRecords[2],
                    record4 = defaultRecords[3],
                    record5 = defaultRecords[4]
                };

                SaveObject($"Record{i}", defaultRecord, true);
            }
        }

        #endregion
    }

    /// <summary>
    /// レコード構造体 - スコア情報の基本単位
    /// </summary>
    [Serializable]
    public struct Record
    {
        /// <summary>プレイヤー名</summary>
        public string name;
        /// <summary>スコア値</summary>
        public int score;

        /// <summary>
        /// レコードコンストラクタ
        /// </summary>
        /// <param name="name">プレイヤー名</param>
        /// <param name="score">スコア</param>
        public Record(string name, int score)
        {
            this.name = name;
            this.score = score;
        }
    }

    /// <summary>
    /// セーブオブジェクトクラス - ステージ毎のスコアランキング管理
    /// </summary>
    [Serializable]
    public class SaveObject
    {
        #region Constants

        private const int _maxRankCount = 5;

        #endregion

        #region Public Fields

        /// <summary>ステージID</summary>
        public int stageID;
        /// <summary>1位レコード</summary>
        public Record record1;
        /// <summary>2位レコード</summary>
        public Record record2;
        /// <summary>3位レコード</summary>
        public Record record3;
        /// <summary>4位レコード</summary>
        public Record record4;
        /// <summary>5位レコード</summary>
        public Record record5;

        #endregion

        #region Public API

        /// <summary>
        /// 新しいスコアをランキングに挿入
        /// </summary>
        /// <param name="stageID">ステージID</param>
        /// <param name="name">プレイヤー名</param>
        /// <param name="score">スコア</param>
        /// <returns>達成したランク位置</returns>
        public int InsertObject(int stageID, string name, int score)
        {
            var recordList = GetRecordArray();
            int rank = CalculateRank(recordList, score);

            PlayerPrefs.SetInt("PlayerRank", rank + 1);

            if (rank < _maxRankCount)
            {
                InsertRecordAtRank(recordList, rank, name, score);
                UpdateRecordFields(recordList);
            }

            return rank + 1;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// レコード配列取得
        /// </summary>
        /// <returns>レコード配列</returns>
        private Record[] GetRecordArray()
        {
            return new Record[] { record1, record2, record3, record4, record5 };
        }

        /// <summary>
        /// スコアに基づくランク計算
        /// </summary>
        /// <param name="recordList">現在のレコードリスト</param>
        /// <param name="score">新しいスコア</param>
        /// <returns>ランク位置</returns>
        private int CalculateRank(Record[] recordList, int score)
        {
            for (int rank = recordList.Length; rank > 0; rank--)
            {
                if (recordList[rank - 1].score >= score)
                {
                    return rank;
                }
            }
            return 0;
        }

        /// <summary>
        /// 指定ランクにレコード挿入
        /// </summary>
        /// <param name="recordList">レコードリスト</param>
        /// <param name="rank">挿入位置</param>
        /// <param name="name">プレイヤー名</param>
        /// <param name="score">スコア</param>
        private void InsertRecordAtRank(Record[] recordList, int rank, string name, int score)
        {
            // 下位ランクを下へシフト
            for (int i = recordList.Length - 2; i >= rank; i--)
            {
                recordList[i + 1] = new Record(recordList[i].name, recordList[i].score);
            }

            // 新しいレコードを挿入
            recordList[rank] = new Record(name, score);
        }

        /// <summary>
        /// フィールド更新処理
        /// </summary>
        /// <param name="recordList">更新後のレコードリスト</param>
        private void UpdateRecordFields(Record[] recordList)
        {
            record1 = recordList[0];
            record2 = recordList[1];
            record3 = recordList[2];
            record4 = recordList[3];
            record5 = recordList[4];
        }

        #endregion
    }
}
