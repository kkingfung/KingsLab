using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// グリッドノード - パスフィンディンググリッドの個々のセルを表現
/// 歩行可能性とグリッド位置を管理
/// </summary>
public class GridNode
{
    private Grid<GridNode> _grid;
    private int _x;
    private int _y;

    private bool _isWalkable;

    /// <summary>
    /// グリッドノードを初期化
    /// </summary>
    /// <param name="grid">親グリッド</param>
    /// <param name="x">X座標</param>
    /// <param name="y">Y座標</param>
    public GridNode(Grid<GridNode> grid, int x, int y)
    {
        _grid = grid;
        _x = x;
        _y = y;
        _isWalkable = true;
    }

    /// <summary>
    /// ノードが歩行可能かどうかを取得
    /// </summary>
    /// <returns>歩行可能な場合true</returns>
    public bool IsWalkable()
    {
        return _isWalkable;
    }

    /// <summary>
    /// ノードの歩行可能性を設定
    /// </summary>
    /// <param name="isWalkable">歩行可能かどうか</param>
    public void SetIsWalkable(bool isWalkable)
    {
        _isWalkable = isWalkable;
        _grid.TriggerGridObjectChanged(_x, _y);
    }

}
