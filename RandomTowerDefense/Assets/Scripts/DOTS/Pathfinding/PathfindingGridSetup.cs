﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class PathfindingGridSetup : MonoBehaviour {

    private FilledMapGenerator mapGenerator;
    public static PathfindingGridSetup Instance { private set; get; }

    public Grid<GridNode> pathfindingGrid;

    private bool isActived;
    private void Awake() {
        Instance = this;
        isActived = false;
    }

    private void Update() {
        if (isActived == false)
        {
            mapGenerator = FindObjectOfType<FilledMapGenerator>();
            pathfindingGrid = new Grid<GridNode>(mapGenerator.CurrMapX(),
                mapGenerator.CurrMapY(), mapGenerator.tileSize, mapGenerator.transform.position,
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
        }
    }


}
