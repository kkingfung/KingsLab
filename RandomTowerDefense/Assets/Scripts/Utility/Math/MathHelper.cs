#region License
/*
MIT License

Copyright(c) 2017 Mattias Edlund

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

using System;
using System.Runtime.CompilerServices;
using RandomTowerDefense.Utility.Math;

namespace UnityMeshSimplifier
{
    /// <summary>
    /// 数学ヘルパー
    /// </summary>
    public static class MathHelper
    {
        public static int RandAdj(int x, int y, int range)
        {
            UnityEngine.Random.InitState(y + (x << 4) + (x << 1) + (y >> 2));

            return ((int)UnityEngine.Random.value & (range - 1));
        }

        #region Consts
        /// <summary>
        /// 円周率（float）
        /// </summary>
        public const float PI = 3.14159274f;

        /// <summary>
        /// 円周率（double）
        /// </summary>
        public const double PId = 3.1415926535897932384626433832795;

        /// <summary>
        /// 度からラジアンへの変換定数（float）
        /// </summary>
        public const float Deg2Rad = PI / 180f;

        /// <summary>
        /// 度からラジアンへの変換定数（double）
        /// </summary>
        public const double Deg2Radd = PId / 180.0;

        /// <summary>
        /// ラジアンから度への変換定数（float）
        /// </summary>
        public const float Rad2Deg = 180f / PI;

        /// <summary>
        /// ラジアンから度への変換定数（double）
        /// </summary>
        public const double Rad2Degd = 180.0 / PId;
        #endregion

        #region Min
        /// <summary>
        /// 3つの値のうち最小の値を返す
        /// </summary>
        /// <param name="val1">1つ目の値</param>
        /// <param name="val2">2つ目の値</param>
        /// <param name="val3">3つ目の値</param>
        /// <returns>最小値</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Min(double val1, double val2, double val3)
        {
            return (val1 < val2 ? (val1 < val3 ? val1 : val3) : (val2 < val3 ? val2 : val3));
        }
        #endregion

        #region Clamping
        /// <summary>
        /// 値を最小値と最大値の範囲にクランプする
        /// </summary>
        /// <param name="value">クランプする値</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <returns>クランプ後の値</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp(double value, double min, double max)
        {
            return (value >= min ? (value <= max ? value : max) : min);
        }
        #endregion

        #region Triangle Area
        /// <summary>
        /// 三角形の面積を計算する
        /// </summary>
        /// <param name="p0">1点目</param>
        /// <param name="p1">2点目</param>
        /// <param name="p2">3点目</param>
        /// <returns>三角形の面積</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double TriangleArea(ref Vector3d p0, ref Vector3d p1, ref Vector3d p2)
        {
            var dx = p1 - p0;
            var dy = p2 - p0;
            return dx.Magnitude * (Math.Sin(Vector3d.Angle(ref dx, ref dy) * Deg2Radd) * dy.Magnitude) * 0.5f;
        }
        #endregion
    }
}