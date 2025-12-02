using UnityEngine;

namespace RandomTowerDefense.Utility.Math
{
    /// <summary>
    /// 2D数学ライブラリ - 2次元幾何学計算とアルゴリズム
    ///
    /// 主な機能:
    /// - 点と直線の距離計算（疑似距離版）
    /// - 線分の位置関係判定システム
    /// - 三角形内部点判定アルゴリズム
    /// - 線分交差判定システム
    /// - 高精度な幾何学的計算
    /// - ゲーム最適化向けパフォーマンス設計
    /// </summary>
    public static class Maths2D
    {
        #region Distance and Position Calculations

        /// <summary>
        /// 点から直線への疑似距離を計算します（最適化版）
        /// </summary>
        /// <param name="a">直線の開始点</param>
        /// <param name="b">直線の終了点</param>
        /// <param name="c">距離を測る点</param>
        /// <returns>疑似距離値</returns>
        public static float PseudoDistanceFromPointToLine(Vector2 a, Vector2 b, Vector2 c)
        {
            return Mathf.Abs((c.x - a.x) * (-b.y + a.y) + (c.y - a.y) * (b.x - a.x));
        }

        /// <summary>
        /// 点が直線のどちら側にあるかを判定します
        /// </summary>
        /// <param name="a">直線の開始点</param>
        /// <param name="b">直線の終了点</param>
        /// <param name="c">判定する点</param>
        /// <returns>-1（左側）、0（線上）、1（右側）</returns>
        public static int SideOfLine(Vector2 a, Vector2 b, Vector2 c)
        {
            return (int)Mathf.Sign((c.x - a.x) * (-b.y + a.y) + (c.y - a.y) * (b.x - a.x));
        }

        /// <summary>
        /// 点が直線のどちら側にあるかを判定します（最適化版）
        /// </summary>
        /// <param name="ax">直線開始点のX座標</param>
        /// <param name="ay">直線開始点のY座標</param>
        /// <param name="bx">直線終了点のX座標</param>
        /// <param name="by">直線終了点のY座標</param>
        /// <param name="cx">判定点のX座標</param>
        /// <param name="cy">判定点のY座標</param>
        /// <returns>-1（左側）、0（線上）、1（右側）</returns>
        public static int SideOfLine(float ax, float ay, float bx, float by, float cx, float cy)
        {
            return (int)Mathf.Sign((cx - ax) * (-by + ay) + (cy - ay) * (bx - ax));
        }

        #endregion

        #region Geometric Shape Calculations

        /// <summary>
        /// 点が三角形の内部にあるかを判定します（重心座標系使用）
        /// </summary>
        /// <param name="a">三角形の頂点A</param>
        /// <param name="b">三角形の頂点B</param>
        /// <param name="c">三角形の頂点C</param>
        /// <param name="p">判定する点</param>
        /// <returns>true: 内部、false: 外部</returns>
        public static bool PointInTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
        {
            float area = 0.5f * (-b.y * c.x + a.y * (-b.x + c.x) + a.x * (b.y - c.y) + b.x * c.y);
            float s = 1 / (2 * area) * (a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y);
            float t = 1 / (2 * area) * (a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y);
            return s >= 0 && t >= 0 && (s + t) <= 1;

        }

        /// <summary>
        /// 二つの線分が交差するかを判定します
        /// </summary>
        /// <param name="a">線分1の開始点</param>
        /// <param name="b">線分1の終了点</param>
        /// <param name="c">線分2の開始点</param>
        /// <param name="d">線分2の終了点</param>
        /// <returns>true: 交差する、false: 交差しない</returns>
        public static bool LineSegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float denominator = ((b.x - a.x) * (d.y - c.y)) - ((b.y - a.y) * (d.x - c.x));
            if (Mathf.Approximately(denominator, 0))
            {
                return false;
            }

            float numerator1 = ((a.y - c.y) * (d.x - c.x)) - ((a.x - c.x) * (d.y - c.y));
            float numerator2 = ((a.y - c.y) * (b.x - a.x)) - ((a.x - c.x) * (b.y - a.y));

            if (Mathf.Approximately(numerator1, 0) || Mathf.Approximately(numerator2, 0))
            {
                return false;
            }

            float r = numerator1 / denominator;
            float s = numerator2 / denominator;

            return (r > 0 && r < 1) && (s > 0 && s < 1);
        }

        #endregion
    }
}