using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ジェネリックグリッドシステム - 任意の型でグリッドデータを管理
/// パスフィンディング、マップ生成、タイル配置などに使用
/// </summary>
/// <typeparam name="TGridObject">グリッドに格納するオブジェクトの型</typeparam>
public class Grid<TGridObject>
{
    /// <summary>
    /// グリッドオブジェクトが変更されたときに発生するイベント
    /// </summary>
    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;

    /// <summary>
    /// グリッドオブジェクト変更イベントの引数
    /// </summary>
    public class OnGridObjectChangedEventArgs : EventArgs
    {
        /// <summary>変更されたグリッドのX座標</summary>
        public int x;
        /// <summary>変更されたグリッドのY座標</summary>
        public int y;
    }

    private int _width;
    private int _height;
    private float _cellSize;
    private Vector3 _originPosition;
    private TGridObject[,] _gridArray;

    /// <summary>
    /// グリッドシステムを初期化
    /// </summary>
    /// <param name="width">グリッドの幅</param>
    /// <param name="height">グリッドの高さ</param>
    /// <param name="cellSize">各セルのサイズ</param>
    /// <param name="originPosition">グリッドの原点位置</param>
    /// <param name="createGridObject">グリッドオブジェクト生成関数</param>
    public Grid(int width, int height, float cellSize, Vector3 originPosition, Func<Grid<TGridObject>, int, int, TGridObject> createGridObject)
    {
        _width = width;
        _height = height;
        _cellSize = cellSize;
        _originPosition = originPosition;

        _gridArray = new TGridObject[width, height];
        for (int x = 0; x < _gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < _gridArray.GetLength(1); y++)
            {
                _gridArray[x, y] = createGridObject(this, x, y);
            }
        }
    }

    /// <summary>
    /// グリッドの幅を取得
    /// </summary>
    /// <returns>グリッドの幅</returns>
    public int GetWidth()
    {
        return _width;
    }

    /// <summary>
    /// グリッドの高さを取得
    /// </summary>
    /// <returns>グリッドの高さ</returns>
    public int GetHeight()
    {
        return _height;
    }

    /// <summary>
    /// グリッドセルのサイズを取得
    /// </summary>
    /// <returns>セルサイズ</returns>
    public float GetCellSize()
    {
        return _cellSize;
    }

    /// <summary>
    /// グリッド座標からワールド座標を取得
    /// </summary>
    /// <param name="x">グリッドX座標</param>
    /// <param name="y">グリッドY座標</param>
    /// <returns>ワールド座標</returns>
    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, 0, y) * _cellSize + _originPosition;
    }

    /// <summary>
    /// ワールド座標からグリッド座標を取得
    /// </summary>
    /// <param name="worldPosition">ワールド座標</param>
    /// <param name="x">出力グリッドX座標</param>
    /// <param name="y">出力グリッドY座標</param>
    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - _originPosition).x / _cellSize);
        y = Mathf.FloorToInt((worldPosition - _originPosition).y / _cellSize);
    }

    /// <summary>
    /// 指定座標にグリッドオブジェクトを設定
    /// </summary>
    /// <param name="x">グリッドX座標</param>
    /// <param name="y">グリッドY座標</param>
    /// <param name="value">設定する値</param>
    public void SetGridObject(int x, int y, TGridObject value)
    {
        if (x >= 0 && y >= 0 && x < _width && y < _height)
        {
            _gridArray[x, y] = value;
            TriggerGridObjectChanged(x, y);
            //OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, y = y });
            //if (OnGridObjectChanged != null) OnGridObjectChanged(this, new OnGridObjectChangedEventArgs { x = x, y = y });
        }
    }

    /// <summary>
    /// グリッドオブジェクト変更イベントを発火
    /// </summary>
    /// <param name="x">変更されたX座標</param>
    /// <param name="y">変更されたY座標</param>
    public void TriggerGridObjectChanged(int x, int y)
    {
        OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, y = y });
       // if (OnGridObjectChanged != null) OnGridObjectChanged(this, new OnGridObjectChangedEventArgs { x = x, y = y });
    }

    /// <summary>
    /// ワールド座標を使用してグリッドオブジェクトを設定
    /// </summary>
    /// <param name="worldPosition">ワールド座標</param>
    /// <param name="value">設定する値</param>
    public void SetGridObject(Vector3 worldPosition, TGridObject value)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        SetGridObject(x, y, value);
    }

    /// <summary>
    /// 指定座標のグリッドオブジェクトを取得
    /// </summary>
    /// <param name="x">グリッドX座標</param>
    /// <param name="y">グリッドY座標</param>
    /// <returns>グリッドオブジェクト</returns>
    public TGridObject GetGridObject(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < _width && y < _height)
        {
            return _gridArray[x, y];
        }
        else
        {
            return default(TGridObject);
        }
    }

    /// <summary>
    /// ワールド座標を使用してグリッドオブジェクトを取得
    /// </summary>
    /// <param name="worldPosition">ワールド座標</param>
    /// <returns>グリッドオブジェクト</returns>
    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetGridObject(x, y);
    }

}
