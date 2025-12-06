using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

namespace RandomTowerDefense.FileSystem
{
    /// <summary>
    /// アセットバンドル読み込みユーティリティ - Unity AssetBundle システムの管理
    ///
    /// 主な機能:
    /// - AssetBundle ファイルの動的読み込み
    /// - TextAsset データの抽出とファイル出力
    /// - ローカルファイルシステムへの書き込み処理
    /// - バンドル形式からプレーンテキストへの変換
    /// - ゲーム設定とデータファイルの管理統合
    /// </summary>
    public static class LoadBundle
    {
        #region Private Fields

        private static AssetBundle _bundle;

        #endregion

        #region Public API

        /// <summary>
        /// アセットバンドル読み込みと出力処理
        /// </summary>
        /// <param name="bundleUrl">バンドルファイルパス</param>
        /// <param name="filename">読み込むファイル名</param>
        /// <param name="filepath">出力先ディレクトリパス</param>
        public static void LoadAssetBundle(string bundleUrl, string filename, string filepath)
        {
            if (string.IsNullOrEmpty(bundleUrl) || string.IsNullOrEmpty(filename) || string.IsNullOrEmpty(filepath))
            {
                Debug.LogError("LoadAssetBundle: Invalid parameters provided.");
                return;
            }

            try
            {
                _bundle = AssetBundle.LoadFromFile(bundleUrl);

                if (_bundle == null)
                {
                    Debug.LogError($"Failed to load bundle from: {bundleUrl}");
                    return;
                }

                TextAsset dataFile = _bundle.LoadAsset(filename) as TextAsset;

                if (dataFile == null)
                {
                    Debug.LogError($"Failed to load asset '{filename}' from bundle.");
                    return;
                }

                string outputPath = Path.Combine(filepath, filename);
                EnsureDirectoryExists(filepath);

                File.WriteAllText(outputPath, dataFile.text);
                Debug.Log($"Successfully loaded and saved: {outputPath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error loading asset bundle: {ex.Message}");
            }
            finally
            {
                if (_bundle != null)
                {
                    _bundle.Unload(false);
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// ディレクトリ存在確認と作成
        /// </summary>
        /// <param name="directoryPath">ディレクトリパス</param>
        private static void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        #endregion
    }
}