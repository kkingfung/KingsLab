using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RandomTowerDefense.Tools
{
    /// <summary>
    /// カスタムグラデーションクラス - 時間ベースのカラーグラデーション管理システム
    ///
    /// 主な機能:
    /// - 線形および離散カラーブレンディングモード
    /// - 時間軸に沿ったカラーキー管理
    /// - リアルタイムカラー評価とテクスチャ生成
    /// - ランダムカラー生成オプション
    /// - エディター統合とシリアライゼーション対応
    /// </summary>
    [System.Serializable]
    public class CustomGradient
    {
        /// <summary>
        /// ブレンドモード列挙型
        /// </summary>
        public enum BlendMode { Linear, Discrete };

        /// <summary>
        /// ブレンドモード
        /// </summary>
        public BlendMode blendMode;

        /// <summary>
        /// ランダムカラー生成フラグ
        /// </summary>
        public bool randomizeColour;

        [SerializeField]
        private List<ColourKey> keys = new List<ColourKey>();

        /// <summary>
        /// コンストラクタ - デフォルトの白黒グラデーション初期化
        /// </summary>
        public CustomGradient()
        {
            AddKey(Color.white, 0);
            AddKey(Color.black, 1);
        }

        /// <summary>
        /// グラデーション評価 - 指定時間のカラーを取得
        /// </summary>
        /// <param name="time">評価時間（0.0～1.0）</param>
        /// <returns>評価されたカラー</returns>
        public Color Evaluate(float time)
        {
            ColourKey keyLeft = keys[0];
            ColourKey keyRight = keys[keys.Count - 1];

            for (int i = 0; i < keys.Count; ++i)
            {
                if (keys[i].Time < time)
                {
                    keyLeft = keys[i];
                }
                if (keys[i].Time > time)
                {
                    keyRight = keys[i];
                    break;
                }
            }

            if (blendMode == BlendMode.Linear)
            {
                float blendTime = Mathf.InverseLerp(keyLeft.Time, keyRight.Time, time);
                return Color.Lerp(keyLeft.Colour, keyRight.Colour, blendTime);
            }
            return keyRight.Colour;
        }

        /// <summary>
        /// カラーキー追加 - 指定時間にカラーキーを追加
        /// </summary>
        /// <param name="colour">カラー</param>
        /// <param name="time">時間（0.0～1.0）</param>
        /// <returns>追加されたキーのインデックス</returns>
        public int AddKey(Color colour, float time)
        {
            ColourKey newKey = new ColourKey(colour, time);
            for (int i = 0; i < keys.Count; ++i)
            {
                if (newKey.Time < keys[i].Time)
                {
                    keys.Insert(i, newKey);
                    return i;
                }
            }

            keys.Add(newKey);
            return keys.Count - 1;
        }

        /// <summary>
        /// カラーキー削除 - 指定インデックスのキーを削除
        /// </summary>
        /// <param name="index">キーインデックス</param>
        public void RemoveKey(int index)
        {
            if (keys.Count >= 2)
            {
                keys.RemoveAt(index);
            }
        }

        /// <summary>
        /// キー時間更新 - 指定キーの時間を更新
        /// </summary>
        /// <param name="index">キーインデックス</param>
        /// <param name="time">新しい時間</param>
        /// <returns>更新後のキーインデックス</returns>
        public int UpdateKeyTime(int index, float time)
        {
            Color col = keys[index].Colour;
            RemoveKey(index);
            return AddKey(col, time);
        }

        /// <summary>
        /// キーカラー更新 - 指定キーのカラーを更新
        /// </summary>
        /// <param name="index">キーインデックス</param>
        /// <param name="col">新しいカラー</param>
        public void UpdateKeyColour(int index, Color col)
        {
            keys[index] = new ColourKey(col, keys[index].Time);
        }

        /// <summary>
        /// キー数取得プロパティ
        /// </summary>
        public int NumKeys
        {
            get
            {
                return keys.Count;
            }
        }

        /// <summary>
        /// キー取得 - 指定インデックスのキーを取得
        /// </summary>
        /// <param name="i">キーインデックス</param>
        /// <returns>カラーキー</returns>
        public ColourKey GetKey(int i)
        {
            return keys[i];
        }

        /// <summary>
        /// テクスチャ生成 - グラデーションテクスチャを生成
        /// </summary>
        /// <param name="width">テクスチャ幅</param>
        /// <returns>グラデーションテクスチャ</returns>
        public Texture2D GetTexture(int width)
        {
            Texture2D texture = new Texture2D(width, 1);
            Color[] colours = new Color[width];
            for (int i = 0; i < width; ++i)
            {
                colours[i] = Evaluate((float)i / (width - 1));
            }
            texture.SetPixels(colours);
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// カラーキー構造体 - グラデーションのカラーと時間の組み合わせ
        /// </summary>
        [System.Serializable]
        public struct ColourKey
        {
            [SerializeField]
            private Color colour;
            [SerializeField]
            private float time;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="colour">カラー</param>
            /// <param name="time">時間</param>
            public ColourKey(Color colour, float time)
            {
                this.colour = colour;
                this.time = time;
            }

            /// <summary>
            /// カラープロパティ
            /// </summary>
            public Color Colour
            {
                get
                {
                    return colour;
                }
            }

            /// <summary>
            /// 時間プロパティ
            /// </summary>
            public float Time
            {
                get
                {
                    return time;
                }
            }
        }
    }
}
