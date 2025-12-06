using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using RandomTowerDefense.MapGenerator;

/// <summary>
/// パスフィンディンググリッドのセットアップと管理
/// マップ生成システムと連携してナビゲーショングリッドを構築
/// </summary>
public class PathfindingGridSetup : MonoBehaviour
{
    private FilledMapGenerator _mapGenerator;

    /// <summary>
    /// シングルトンインスタンス
    /// </summary>
    public static PathfindingGridSetup Instance { private set; get; }

    /// <summary>
    /// パスフィンディング用グリッド
    /// </summary>
    public Grid<GridNode> pathfindingGrid;

    /// <summary>
    /// グリッドが初期化済みかどうか
    /// </summary>
    public bool isActivated;

    /// <summary>
    /// グリッドをリセットするフラグ
    /// </summary>
    public bool Reset;

    private void Awake()
    {
        Instance = this;
        isActivated = false;
        Reset = true;
        _mapGenerator = FindObjectOfType<FilledMapGenerator>();
    }

    private void Start()
    {
        if (isActivated == false)
        {
            pathfindingGrid = new Grid<GridNode>(_mapGenerator.CurrMapX(),
                _mapGenerator.CurrMapY(), _mapGenerator.tileSize, _mapGenerator.originPos,
                (Grid<GridNode> grid, int x, int y) => new GridNode(grid, x, y));
            pathfindingGrid.GetGridObject(2, 0).SetIsWalkable(false);

            for (int y = 0; y < _mapGenerator.CurrMapY(); ++y)
            {
                for (int x = 0; x < _mapGenerator.CurrMapX(); ++x)
                {
                    pathfindingGrid.GetGridObject(x, y).SetIsWalkable(_mapGenerator.GetMapWalkable(x, y));
                }
            }
            isActivated = true;
            Reset = true;
        }

        if (isActivated)
        {
            for (int y = 0; y < _mapGenerator.CurrMapY(); ++y)
            {
                for (int x = 0; x < _mapGenerator.CurrMapX(); ++x)
                {
                    Vector3 temp = pathfindingGrid.GetWorldPosition(x, y);
                    temp.y = _mapGenerator.transform.position.y;
                    //  Debug.DrawLine(temp, temp+Vector3.up,(pathfindingGrid.GetGridObject(x, y).IsWalkable())?Color.white:Color.red);
                }
            }
        }
    }

}
