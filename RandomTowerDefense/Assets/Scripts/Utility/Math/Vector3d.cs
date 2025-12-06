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
using UnityEngine;
using UnityMeshSimplifier;

namespace RandomTowerDefense.Utility.Math
{
    /// <summary>
    /// Vector3d - 倍精度3次元ベクトル構造体
    ///
    /// 主な機能:
    /// - 高精度な3次元ベクトル演算
    /// - Unity の Vector3 との相互変換
    /// - 演算子オーバーロードによる直感的な演算
    /// - IEquatable による効率的な比較処理
    /// - MethodImpl によるパフォーマンス最適化
    /// - 大規模シミュレーション向けの精度確保
    /// </summary>
    public struct Vector3d : IEquatable<Vector3d>
    {
        #region Static Read-Only
        /// <summary>
        /// ゼロベクトル。
        /// </summary>
        public static readonly Vector3d zero = new Vector3d(0, 0, 0);
        #endregion

#region Constants
        /// <summary>
        /// ベクトルのイプシロン（極小値）。
        /// </summary>
        public const double Epsilon = double.Epsilon;
        #endregion

        #region Fields
        /// <summary>
        /// x 成分。
        /// </summary>
        public double x;
        /// <summary>
        /// y 成分。
        /// </summary>
        public double y;
        /// <summary>
        /// z 成分。
        /// </summary>
        public double z;
        #endregion

#region Public Properties
        /// <summary>
        /// このベクトルの長さ（大きさ）を取得します。
        /// </summary>
        public double Magnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return System.Math.Sqrt(x * x + y * y + z * z); }
        }

        /// <summary>
        /// このベクトルの二乗長（長さの二乗）を取得します。
        /// </summary>
        public double MagnitudeSqr
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (x * x + y * y + z * z); }
        }

        /// <summary>
        /// このベクトルの正規化ベクトルを取得します。
        /// </summary>
        public Vector3d Normalized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Vector3d result;
                Normalize(ref this, out result);
                return result;
            }
        }

        /// <summary>
        /// インデックスで指定した成分（0=x,1=y,2=z）を取得または設定します。
        /// </summary>
        /// <param name="index">成分のインデックス。</param>
        public double this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                switch (index)
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3d index!");
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                switch (index)
                {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        y = value;
                        break;
                    case 2:
                        z = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3d index!");
                }
            }
        }
        #endregion

#region Constructors
        /// <summary>
        /// すべての成分に同じ値を設定して新しいベクトルを作成します。
        /// </summary>
        /// <param name="value">成分に設定する値。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3d(double value)
        {
            this.x = value;
            this.y = value;
            this.z = value;
        }

        /// <summary>
        /// 指定した x, y, z 値で新しいベクトルを作成します。
        /// </summary>
        /// <param name="x">x 成分。</param>
        /// <param name="y">y 成分。</param>
        /// <param name="z">z 成分。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3d(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// 単精度ベクトル（UnityEngine.Vector3）から新しい倍精度ベクトルを作成します。
        /// </summary>
        /// <param name="vector">変換元の単精度ベクトル。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3d(Vector3 vector)
        {
            this.x = vector.x;
            this.y = vector.y;
            this.z = vector.z;
        }
        #endregion

        #region Operators
        /// <summary>
        /// 2 つのベクトルを加算します。
        /// </summary>
        /// <param name="a">左オペランド。</param>
        /// <param name="b">右オペランド。</param>
        /// <returns>加算結果のベクトル。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator +(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        /// <summary>
        /// 2 つのベクトルを減算します。
        /// </summary>
        /// <param name="a">左オペランド。</param>
        /// <param name="b">右オペランド。</param>
        /// <returns>減算結果のベクトル。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator -(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        /// <summary>
        /// ベクトルをスカラーで乗算します（右スカラー）。
        /// </summary>
        /// <param name="a">ベクトル。</param>
        /// <param name="d">スカラー値。</param>
        /// <returns>スケーリング後のベクトル。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator *(Vector3d a, double d)
        {
            return new Vector3d(a.x * d, a.y * d, a.z * d);
        }

        /// <summary>
        /// ベクトルをスカラーで乗算します（左スカラー）。
        /// </summary>
        /// <param name="d">スカラー値。</param>
        /// <param name="a">ベクトル。</param>
        /// <returns>スケーリング後のベクトル。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator *(double d, Vector3d a)
        {
            return new Vector3d(a.x * d, a.y * d, a.z * d);
        }

        /// <summary>
        /// ベクトルをスカラーで除算します。
        /// </summary>
        /// <param name="a">ベクトル。</param>
        /// <param name="d">除算値（スカラー）。</param>
        /// <returns>除算結果のベクトル。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator /(Vector3d a, double d)
        {
            return new Vector3d(a.x / d, a.y / d, a.z / d);
        }

        /// <summary>
        /// ベクトルを符号反転します。
        /// </summary>
        /// <param name="a">ベクトル。</param>
        /// <returns>反転されたベクトル。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator -(Vector3d a)
        {
            return new Vector3d(-a.x, -a.y, -a.z);
        }

        /// <summary>
        /// 2 つのベクトルが等しいかどうかを返します。
        /// </summary>
        /// <param name="lhs">左辺ベクトル。</param>
        /// <param name="rhs">右辺ベクトル。</param>
        /// <returns>等しい場合は true。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector3d lhs, Vector3d rhs)
        {
            return (lhs - rhs).MagnitudeSqr < Epsilon;
        }

        /// <summary>
        /// 2 つのベクトルが等しくないかどうかを返します。
        /// </summary>
        /// <param name="lhs">左辺ベクトル。</param>
        /// <param name="rhs">右辺ベクトル。</param>
        /// <returns>等しくない場合は true。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector3d lhs, Vector3d rhs)
        {
            return (lhs - rhs).MagnitudeSqr >= Epsilon;
        }

        /// <summary>
        /// 単精度ベクトルから倍精度ベクトルへの暗黙変換。
        /// </summary>
        /// <param name="v">単精度ベクトル。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector3d(Vector3 v)
        {
            return new Vector3d(v.x, v.y, v.z);
        }

        /// <summary>
        /// 倍精度ベクトルから単精度ベクトルへの明示的変換。
        /// </summary>
        /// <param name="v">倍精度ベクトル。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector3(Vector3d v)
        {
            return new Vector3((float)v.x, (float)v.y, (float)v.z);
        }
        #endregion

        #region Public API
        #region Instance
        /// <summary>
        /// 既存ベクトルの x,y,z 成分を設定します。
        /// </summary>
        /// <param name="x">x 値。</param>
        /// <param name="y">y 値。</param>
        /// <param name="z">z 値。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// 他ベクトルと成分ごとに乗算します。
        /// </summary>
        /// <param name="scale">乗算するベクトル。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(ref Vector3d scale)
        {
            x *= scale.x;
            y *= scale.y;
            z *= scale.z;
        }

        /// <summary>
        /// このベクトルを正規化します。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            double mag = this.Magnitude;
            if (mag > Epsilon)
            {
                x /= mag;
                y /= mag;
                z /= mag;
            }
            else
            {
                x = y = z = 0;
            }
        }

        /// <summary>
        /// ベクトルを指定範囲でクランプします。
        /// </summary>
        /// <param name="min">最小成分値。</param>
        /// <param name="max">最大成分値。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clamp(double min, double max)
        {
            if (x < min) x = min;
            else if (x > max) x = max;

            if (y < min) y = min;
            else if (y > max) y = max;

            if (z < min) z = min;
            else if (z > max) z = max;
        }
        #endregion

        #region Object
        /// <summary>
        /// このベクトルのハッシュコードを返します。
        /// </summary>
        /// <returns>ハッシュコード。</returns>
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() << 2 ^ z.GetHashCode() >> 2;
        }

        /// <summary>
        /// 別のオブジェクトと等価かどうかを返します。
        /// </summary>
        /// <param name="other">比較対象のオブジェクト。</param>
        /// <returns>等価なら true。</returns>
        public override bool Equals(object other)
        {
            if (!(other is Vector3d))
            {
                return false;
            }
            Vector3d vector = (Vector3d)other;
            return (x == vector.x && y == vector.y && z == vector.z);
        }

        /// <summary>
        /// 別の Vector3d と等価かどうかを返します。
        /// </summary>
        /// <param name="other">比較対象のベクトル。</param>
        /// <returns>等価なら true。</returns>
        public bool Equals(Vector3d other)
        {
            return (x == other.x && y == other.y && z == other.z);
        }

        /// <summary>
        /// フォーマット済みの文字列を返します。
        /// </summary>
        /// <returns>フォーマットされた文字列。</returns>
        public override string ToString()
        {
            return string.Format("({0:F1}, {1:F1}, {2:F1})", x, y, z);
        }

        /// <summary>
        /// 指定フォーマットで文字列を返します。
        /// </summary>
        /// <param name="format">フォーマット指定子。</param>
        /// <returns>フォーマットされた文字列。</returns>
        public string ToString(string format)
        {
            return string.Format("({0}, {1}, {2})", x.ToString(format), y.ToString(format), z.ToString(format));
        }
        #endregion

        #region Static
        /// <summary>
        /// 2 つのベクトルのドット積を計算します。
        /// </summary>
        /// <param name="lhs">左辺ベクトル。</param>
        /// <param name="rhs">右辺ベクトル。</param>
        /// <returns>ドット積の値。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Dot(ref Vector3d lhs, ref Vector3d rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
        }

        /// <summary>
        /// 2 つのベクトルのクロス積を計算します。
        /// </summary>
        /// <param name="lhs">左辺ベクトル。</param>
        /// <param name="rhs">右辺ベクトル。</param>
        /// <param name="result">結果のベクトル（出力）。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Cross(ref Vector3d lhs, ref Vector3d rhs, out Vector3d result)
        {
            result = new Vector3d(lhs.y * rhs.z - lhs.z * rhs.y, lhs.z * rhs.x - lhs.x * rhs.z, lhs.x * rhs.y - lhs.y * rhs.x);
        }

        /// <summary>
        /// 2 つのベクトル間の角度を計算します。
        /// </summary>
        /// <param name="from">始点ベクトル。</param>
        /// <param name="to">終点ベクトル。</param>
        /// <returns>角度（度）。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Angle(ref Vector3d from, ref Vector3d to)
        {
            Vector3d fromNormalized = from.Normalized;
            Vector3d toNormalized = to.Normalized;
            return System.Math.Acos(MathHelper.Clamp(Vector3d.Dot(ref fromNormalized, ref toNormalized), -1.0, 1.0)) * MathHelper.Rad2Degd;
        }

        /// <summary>
        /// 2 つのベクトルを線形補間します。
        /// </summary>
        /// <param name="a">補間開始ベクトル。</param>
        /// <param name="b">補間終了ベクトル。</param>
        /// <param name="t">補間係数（0..1）。</param>
        /// <param name="result">補間結果（出力）。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Lerp(ref Vector3d a, ref Vector3d b, double t, out Vector3d result)
        {
            result = new Vector3d(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
        }

        /// <summary>
        /// 2 つのベクトルを成分ごとに乗算します。
        /// </summary>
        /// <param name="a">1 つ目のベクトル。</param>
        /// <param name="b">2 つ目のベクトル。</param>
        /// <param name="result">結果のベクトル（出力）。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Scale(ref Vector3d a, ref Vector3d b, out Vector3d result)
        {
            result = new Vector3d(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        /// <summary>
        /// ベクトルを正規化します（出力パラメータ形式）。
        /// </summary>
        /// <param name="value">正規化するベクトル。</param>
        /// <param name="result">正規化結果（出力）。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Normalize(ref Vector3d value, out Vector3d result)
        {
            double mag = value.Magnitude;
            if (mag > Epsilon)
            {
                result = new Vector3d(value.x / mag, value.y / mag, value.z / mag);
            }
            else
            {
                result = Vector3d.zero;
            }
        }
        #endregion
        #endregion
    }
}
