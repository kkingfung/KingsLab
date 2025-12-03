using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;

namespace RandomTowerDefense.Tools
{
    /// <summary>
    /// ProfilerController - Unity Profiling APIを使用したリアルタイムパフォーマンス監視システム
    ///
    /// 主な機能:
    /// - フレームタイム測定とフレーム平均計算
    /// - GCメモリとシステムメモリ使用量監視
    /// - ドローコール数追跡
    /// - リアルタイムGUI表示システム
    /// - パフォーマンスデータ記録とエクスポート
    /// - ECS最適化向けプロファイリング統計
    ///
    /// 使用例:
    /// - 1000+エンティティ処理時のパフォーマンス監視
    /// - フレームレート60FPS維持の検証
    /// - メモリリーク検出とGC負荷測定
    /// </summary>
    public class ProfilerController : MonoBehaviour
    {
        #region Source Attribution

        /************************************************************************************************************
       * Source: https://docs.unity3d.com/2020.2/Documentation/ScriptReference/Unity.Profiling.ProfilerRecorder.html
       *************************************************************************************************************/

        #endregion

        #region Serialized Fields

        [Header("プロファイラー設定")]
        [SerializeField] private bool _enableProfiler = true;
        [SerializeField] private bool _showGUI = true;
        [SerializeField] private Vector2 _guiPosition = new Vector2(10, 30);
        [SerializeField] private Vector2 _guiSize = new Vector2(250, 100);

        #endregion

        #region Private Fields

        private string _statsText = "";
        private ProfilerRecorder _systemMemoryRecorder;
        private ProfilerRecorder _gcMemoryRecorder;
        private ProfilerRecorder _mainThreadTimeRecorder;
        private ProfilerRecorder _drawCallsCountRecorder;

        // プロファイラーマーカー（必要に応じて使用）
        //public static ProfilerMarker UpdatePlayerProfilerMarker = new ProfilerMarker("Player.Update");

        #endregion

        #region Properties

        /// <summary>
        /// プロファイラー有効状態
        /// </summary>
        public bool EnableProfiler
        {
            get => _enableProfiler;
            set => _enableProfiler = value;
        }

        /// <summary>
        /// GUI表示有効状態
        /// </summary>
        public bool ShowGUI
        {
            get => _showGUI;
            set => _showGUI = value;
        }

        /// <summary>
        /// 最新の統計テキスト
        /// </summary>
        public string StatsText => _statsText;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// プロファイラー有効化 - レコーダー開始と統計列挙
        /// </summary>
        private void OnEnable()
        {
            if (!_enableProfiler) return;

            _systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
            _gcMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
            _mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
            _drawCallsCountRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");

            // 利用可能なプロファイラー統計情報を列挙（デバッグ用）
            // GetAvailableProfilerStats.EnumerateProfilerStats();
        }

        /// <summary>
        /// プロファイラー無効化 - レコーダーのリソース解放
        /// </summary>
        private void OnDisable()
        {
            if (!_enableProfiler) return;

            _systemMemoryRecorder.Dispose();
            _gcMemoryRecorder.Dispose();
            _mainThreadTimeRecorder.Dispose();
            _drawCallsCountRecorder.Dispose();
        }

        /// <summary>
        /// 統計情報更新 - フレームごとのパフォーマンスデータ取得
        /// </summary>
        private void Update()
        {
            if (!_enableProfiler) return;

            var sb = new StringBuilder(500);
            sb.AppendLine($"Frame Time: {GetRecorderFrameAverage(_mainThreadTimeRecorder) * (1e-6f):F1} ms");
            sb.AppendLine($"GC Memory: {_gcMemoryRecorder.LastValue / (1024 * 1024)} MB");
            sb.AppendLine($"System Memory: {_systemMemoryRecorder.LastValue / (1024 * 1024)} MB");
            sb.AppendLine($"Draw Calls: {_drawCallsCountRecorder.LastValue}");
            sb.AppendLine($"FPS: {1.0f / Time.deltaTime:F1}");
            _statsText = sb.ToString();
        }

        /// <summary>
        /// GUI描画 - 統計情報のリアルタイム表示
        /// </summary>
        private void OnGUI()
        {
            if (!_showGUI || !_enableProfiler) return;

            GUI.TextArea(new Rect(_guiPosition.x, _guiPosition.y, _guiSize.x, _guiSize.y), _statsText);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 統計情報のCSVエクスポート（デバッグ用）
        /// </summary>
        /// <param name="filePath">出力ファイルパス</param>
        public void ExportStatsToCSV(string filePath)
        {
            try
            {
                var csvData = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{GetRecorderFrameAverage(_mainThreadTimeRecorder) * (1e-6f):F3}," +
                              $"{_gcMemoryRecorder.LastValue / (1024 * 1024)},{_systemMemoryRecorder.LastValue / (1024 * 1024)}," +
                              $"{_drawCallsCountRecorder.LastValue},{1.0f / Time.deltaTime:F1}\n";

                File.AppendAllText(filePath, csvData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to export profiler stats: {ex.Message}");
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// レコーダーフレーム平均値計算
        /// </summary>
        /// <param name="recorder">プロファイラーレコーダー</param>
        /// <returns>平均値</returns>
        private static double GetRecorderFrameAverage(ProfilerRecorder recorder)
        {
            var samplesCount = recorder.Capacity;
            if (samplesCount == 0)
                return 0;

            double r = 0;
            var samples = new List<ProfilerRecorderSample>(samplesCount);
            recorder.CopyTo(samples);
            for (var i = 0; i < samples.Count; ++i)
                r += samples[i].Value;
            r /= samplesCount;

            return r;
        }

        #endregion
    }
}