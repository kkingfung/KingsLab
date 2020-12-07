using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class PathfindingGridSetup : MonoBehaviour {

    private FilledMapGenerator mapGenerator;
    public static PathfindingGridSetup Instance { private set; get; }

    public Grid<GridNode> pathfindingGrid;

    public bool isActived;
    public bool Reset;

    private void Awake() {
        Instance = this;
        isActived = false;
        Reset = true;
        mapGenerator = FindObjectOfType<FilledMapGenerator>();
    }

    private void Update() {
        if (isActived == false)
        {
            pathfindingGrid = new Grid<GridNode>(mapGenerator.CurrMapX(),
                mapGenerator.CurrMapY(), mapGenerator.tileSize, mapGenerator.originPos,
                (Grid<GridNode> grid, int x, int y) => new GridNode(grid, x, y));
            pathfindingGrid.GetGridObject(2, 0).SetIsWalkable(false);

            for (int y = 0; y < mapGenerator.CurrMapY(); ++y)
            {
                for (int x = 0; x < mapGenerator.CurrMapX(); ++x)
                {
                    pathfindingGrid.GetGridObject(x, y).SetIsWalkable(mapGenerator.GetMapWalkable(x, y));
                }
            }
            isActived = true;
            Reset = true;
        }

        if (isActived) {
            for (int y = 0; y < mapGenerator.CurrMapY(); ++y)
            {
                for (int x = 0; x < mapGenerator.CurrMapX(); ++x)
                {
                    Vector3 temp = pathfindingGrid.GetWorldPosition(x, y);
                    temp.y = mapGenerator.transform.position.y;
                  //  Debug.DrawLine(temp, temp+Vector3.up,(pathfindingGrid.GetGridObject(x, y).IsWalkable())?Color.white:Color.red);
                }
            }
        }
    }

}
