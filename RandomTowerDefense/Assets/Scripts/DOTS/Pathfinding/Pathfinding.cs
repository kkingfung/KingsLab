using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Burst;
using RandomTowerDefense.DOTS.Tags;

namespace RandomTowerDefense.DOTS.Pathfinding
{
    /// <summary>
    /// Unity ECSを使用したA*パスフィンディングシステム
    /// 敵エンティティの経路計算とパス設定を効率的に処理
    /// マルチスレッドジョブシステムとBurstコンパイラーを活用
    /// </summary>
    public class Pathfinding : ComponentSystem
    {
        #region Constants
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;
        #endregion

        #region Private Fields
        private static int _gridWidth;
        private static int _gridHeight;

        private Dictionary<int2, PathNode[]> _pathNodeArrayList;
        private NativeArray<PathNode> _pathNodeArray;

        private bool _chgPath;

        // ソースコードのみで調整
        private bool _goalFixed = true;
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// システムの初期化処理
        /// パス変更フラグとパスノード配列リストを初期化
        /// </summary>
        protected override void OnCreate()
        {
            _chgPath = false;
            _pathNodeArrayList = new Dictionary<int2, PathNode[]>();
        }

        /// <summary>
        /// システムの破棄処理
        /// ネイティブ配列のメモリリークを防ぐため適切に解放
        /// </summary>
        protected override void OnDestroy()
        {
            if (_pathNodeArray.IsCreated)
                _pathNodeArray.Dispose();
        }

        /// <summary>
        /// メインの更新ループ
        /// パスフィンディングリクエストを処理し、ジョブをスケジュール
        /// </summary>
        protected override void OnUpdate()
        {
            if (PathfindingGridSetup.Instance == null) return;
            if (PathfindingGridSetup.Instance.Reset)
            {
                _pathNodeArrayList = new Dictionary<int2, PathNode[]>();
                if (_pathNodeArray.IsCreated)
                    _pathNodeArray.Dispose();
                PathfindingGridSetup.Instance.Reset = false;
            }
            _gridWidth = PathfindingGridSetup.Instance.pathfindingGrid.GetWidth();
            _gridHeight = PathfindingGridSetup.Instance.pathfindingGrid.GetHeight();
            int2 gridSize = new int2(_gridWidth, _gridHeight);

            List<FindPathJob> findPathJobList = new List<FindPathJob>();
            NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);

            Entities.WithAll<EnemyTag>().ForEach((Entity entity, ref PathfindingParams pathfindingParams) =>
            {
                FindPathJob findPathJob;
                if (_goalFixed == false)
                {
                    _pathNodeArray = GetPathNodeArray();
                    NativeArray<PathNode> tmpPathNodeArray = new NativeArray<PathNode>(_pathNodeArray, Allocator.TempJob);

                    findPathJob = new FindPathJob
                    {
                        gridSize = gridSize,
                        pathNodeArray = tmpPathNodeArray,
                        startPosition = pathfindingParams.startPosition,
                        endPosition = pathfindingParams.endPosition,
                        entity = entity,
                        chgPath = _chgPath
                    };
                    findPathJobList.Add(findPathJob);
                    jobHandleList.Add(findPathJob.Schedule());
                    JobHandle.CompleteAll(jobHandleList);

                    PathNode[] array = findPathJob.pathNodeArray.ToArray();
                    _pathNodeArrayList.Add(pathfindingParams.startPosition, array);
                    _chgPath = !_chgPath;
                }
                else
                {
                    if (_pathNodeArrayList.ContainsKey(pathfindingParams.startPosition))
                    {
                        NativeArray<PathNode> tmpPathNodeArray = new NativeArray<PathNode>(_pathNodeArrayList[pathfindingParams.startPosition], Allocator.TempJob);

                        findPathJob = new FindPathJob
                        {
                            gridSize = gridSize,
                            pathNodeArray = tmpPathNodeArray,
                            startPosition = pathfindingParams.startPosition,
                            endPosition = pathfindingParams.endPosition,
                            entity = entity,
                            chgPath = _chgPath
                        };
                        findPathJobList.Add(findPathJob);
                    }
                    else
                    {
                        _pathNodeArray = GetPathNodeArray();
                        NativeArray<PathNode> tmpPathNodeArray = new NativeArray<PathNode>(_pathNodeArray, Allocator.TempJob);

                        findPathJob = new FindPathJob
                        {
                            gridSize = gridSize,
                            pathNodeArray = tmpPathNodeArray,
                            startPosition = pathfindingParams.startPosition,
                            endPosition = pathfindingParams.endPosition,
                            entity = entity,
                            chgPath = _chgPath
                        };
                        findPathJobList.Add(findPathJob);
                        jobHandleList.Add(findPathJob.Schedule());

                        JobHandle.CompleteAll(jobHandleList);

                        //PathNode[] array = findPathJob.pathNodeArray.ToArray();
                        //_pathNodeArrayList.Add(pathfindingParams.startPosition, array);
                        _chgPath = !_chgPath;
                    }
                }
                PostUpdateCommands.RemoveComponent<PathfindingParams>(entity);
            });

            foreach (FindPathJob findPathJob in findPathJobList)
            {
                new SetBufferPathJob
                {
                    entity = findPathJob.entity,
                    gridSize = findPathJob.gridSize,
                    pathNodeArray = findPathJob.pathNodeArray,
                    pathfindingParamsComponentDataFromEntity = GetComponentDataFromEntity<PathfindingParams>(),
                    pathFollowComponentDataFromEntity = GetComponentDataFromEntity<PathFollow>(),
                    pathPositionBufferFromEntity = GetBufferFromEntity<PathPosition>(),
                }.Run();
            }

            if (_pathNodeArray.IsCreated)
                _pathNodeArray.Dispose();
        }
        #endregion

        #region Public API
        /// <summary>
        /// パスフィンディンググリッドからパスノード配列を生成
        /// 歩行可能性とコストを各ノードに設定
        /// </summary>
        /// <returns>初期化されたパスノード配列</returns>
        private NativeArray<PathNode> GetPathNodeArray()
        {
            Grid<GridNode> grid = PathfindingGridSetup.Instance.pathfindingGrid;

            int2 gridSize = new int2(grid.GetWidth(), grid.GetHeight());
            NativeArray<PathNode> pathNodeArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.TempJob);

            for (int x = 0; x < grid.GetWidth(); x++)
            {
                for (int y = 0; y < grid.GetHeight(); y++)
                {
                    PathNode pathNode = new PathNode();
                    pathNode.x = x;
                    pathNode.y = y;
                    pathNode.index = CalculateIndex(x, y, gridSize.x);

                    pathNode.gCost = int.MaxValue;

                    pathNode.isWalkable = grid.GetGridObject(x, y).IsWalkable();
                    pathNode.cameFromNodeIndex = -1;
                    pathNodeArray[pathNode.index] = pathNode;
                }
            }

            return pathNodeArray;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 計算されたパスをパスポジションバッファーに設定
        /// パスが見つからない場合は適切にハンドル
        /// </summary>
        /// <param name="pathNodeArray">パスノード配列</param>
        /// <param name="endNode">終点ノード</param>
        /// <param name="pathPositionBuffer">パス位置バッファー</param>
        private static void CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode, DynamicBuffer<PathPosition> pathPositionBuffer)
        {
            if (endNode.cameFromNodeIndex == -1)
            {
                // Couldn't find a path!
            }
            else
            {
                // Found a path
                pathPositionBuffer.Add(new PathPosition { position = new int2(endNode.x, endNode.y) });

                PathNode currentNode = endNode;
                while (currentNode.cameFromNodeIndex != -1)
                {
                    PathNode cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                    pathPositionBuffer.Add(new PathPosition { position = new int2(cameFromNode.x, cameFromNode.y) });
                    currentNode = cameFromNode;
                }
            }
        }

        /// <summary>
        /// パスの計算（オーバーロード版）
        /// ネイティブリストを使用してパスを返す
        /// </summary>
        /// <param name="pathNodeArray">パスノード配列</param>
        /// <param name="endNode">終点ノード</param>
        /// <returns>計算されたパス</returns>
        private static NativeList<int2> CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode)
        {
            if (endNode.cameFromNodeIndex == -1)
            {
                // Couldn't find a path!
                return new NativeList<int2>(Allocator.Temp);
            }
            else
            {
                // Found a path
                NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
                path.Add(new int2(endNode.x, endNode.y));

                PathNode currentNode = endNode;
                while (currentNode.cameFromNodeIndex != -1)
                {
                    PathNode cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                    path.Add(new int2(cameFromNode.x, cameFromNode.y));
                    currentNode = cameFromNode;
                }

                return SimplifyPath(path);
            }
        }

        /// <summary>
        /// パスを簡略化してウェイポイントを削減
        /// 同じ方向の連続する移動をまとめて効率化
        /// </summary>
        /// <param name="path">元のパス</param>
        /// <returns>簡略化されたパス</returns>
        private static NativeList<int2> SimplifyPath(NativeList<int2> path)
        {
            NativeList<int2> waypoints = new NativeList<int2>(Allocator.Temp);
            Vector2 directionOld = Vector2.zero;

            for (int i = 1; i < path.Length; ++i)
            {
                Vector2 directionNew = new Vector2(path[i - 1].x - path[i].x, path[i - 1].y - path[i].y);
                if (directionNew != directionOld)
                {
                    waypoints.Add(path[i]);
                }
                directionOld = directionNew;
            }
            return waypoints;
        }

        /// <summary>
        /// 指定された位置がグリッド内にあるかチェック
        /// </summary>
        /// <param name="gridPosition">チェックする位置</param>
        /// <param name="gridSize">グリッドサイズ</param>
        /// <returns>位置がグリッド内にある場合true</returns>
        private static bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize)
        {
            return
                gridPosition.x >= 0 &&
                gridPosition.y >= 0 &&
                gridPosition.x < gridSize.x &&
                gridPosition.y < gridSize.y;
        }

        /// <summary>
        /// グリッド座標を1次元配列インデックスに変換
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="gridWidth">グリッド幅</param>
        /// <returns>配列インデックス</returns>
        private static int CalculateIndex(int x, int y, int gridWidth)
        {
            return x + y * gridWidth;
        }

        /// <summary>
        /// 2つの位置間の移動コストを計算
        /// 直線移動と対角移動のコストを考慮
        /// </summary>
        /// <param name="aPosition">開始位置</param>
        /// <param name="bPosition">終了位置</param>
        /// <returns>移動コスト</returns>
        private static int CalculateDistanceCost(int2 aPosition, int2 bPosition)
        {
            int xDistance = math.abs(aPosition.x - bPosition.x);
            int yDistance = math.abs(aPosition.y - bPosition.y);
            int remaining = math.abs(xDistance - yDistance);
            return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }

        /// <summary>
        /// オープンリストから最低コストのFノードのインデックスを取得
        /// A*アルゴリズムで次に探索するノードを決定
        /// </summary>
        /// <param name="openList">オープンリスト</param>
        /// <param name="pathNodeArray">パスノード配列</param>
        /// <returns>最低コストノードのインデックス</returns>
        private static int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray)
        {
            PathNode lowestCostPathNode = pathNodeArray[openList[0]];
            for (int i = 1; i < openList.Length; ++i)
            {
                PathNode testPathNode = pathNodeArray[openList[i]];
                if (testPathNode.fCost < lowestCostPathNode.fCost)
                {
                    lowestCostPathNode = testPathNode;
                }
            }
            return lowestCostPathNode.index;
        }
        #endregion

        #region Nested Types
        /// <summary>
        /// パス設定をバッファーに適用するジョブ
        /// 計算されたパスをエンティティのバッファーコンポーネントに設定
        /// </summary>
        [BurstCompile]
        private struct SetBufferPathJob : IJob
        {
            public int2 gridSize;

            [DeallocateOnJobCompletion]
            public NativeArray<PathNode> pathNodeArray;

            public Entity entity;

            public ComponentDataFromEntity<PathfindingParams> pathfindingParamsComponentDataFromEntity;
            public ComponentDataFromEntity<PathFollow> pathFollowComponentDataFromEntity;
            public BufferFromEntity<PathPosition> pathPositionBufferFromEntity;

            /// <summary>
            /// ジョブの実行処理
            /// パスを計算してエンティティに設定
            /// </summary>
            public void Execute()
            {
                DynamicBuffer<PathPosition> pathPositionBuffer = pathPositionBufferFromEntity[entity];
                pathPositionBuffer.Clear();

                PathfindingParams pathfindingParams = pathfindingParamsComponentDataFromEntity[entity];
                int endNodeIndex = CalculateIndex(pathfindingParams.endPosition.x, pathfindingParams.endPosition.y, gridSize.x);
                PathNode endNode = pathNodeArray[endNodeIndex];
                if (endNode.cameFromNodeIndex == -1)
                {
                    // Didn't find a path!
                    //Debug.Log("Didn't find a path!");
                    pathFollowComponentDataFromEntity[entity] = new PathFollow { pathIndex = -1 };
                }
                else
                {
                    // Found a path
                    CalculatePath(pathNodeArray, endNode, pathPositionBuffer);

                    pathFollowComponentDataFromEntity[entity] = new PathFollow { pathIndex = pathPositionBuffer.Length - 1 };
                }
            }
        }

        /// <summary>
        /// A*パスフィンディングアルゴリズムを実行するジョブ
        /// 開始点から終点への最適パスを計算
        /// オープン・クローズドリストを使用した効率的な探索
        /// </summary>
        [BurstCompile]
        private struct FindPathJob : IJob
        {
            public int2 gridSize;
            public NativeArray<PathNode> pathNodeArray;

            public int2 startPosition;
            public int2 endPosition;

            public bool chgPath;

            public Entity entity;

            /// <summary>
            /// A*アルゴリズムの実行
            /// 隣接ノード探索の順序を交互に変更してパスの多様性を確保
            /// </summary>
            public void Execute()
            {
                for (int i = 0; i < pathNodeArray.Length; ++i)
                {
                    PathNode pathNode = pathNodeArray[i];
                    pathNode.hCost = CalculateDistanceCost(new int2(pathNode.x, pathNode.y), endPosition);
                    pathNode.cameFromNodeIndex = -1;

                    pathNodeArray[i] = pathNode;
                }

                NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(4, Allocator.Temp);
                if (chgPath)
                {
                    neighbourOffsetArray[0] = new int2(-1, 0); // Left
                    neighbourOffsetArray[1] = new int2(+1, 0); // Right
                    neighbourOffsetArray[2] = new int2(0, +1); // Up
                    neighbourOffsetArray[3] = new int2(0, -1); // Down
                }
                else
                {
                    neighbourOffsetArray[0] = new int2(0, -1); // Down
                    neighbourOffsetArray[1] = new int2(0, +1); // Up
                    neighbourOffsetArray[2] = new int2(+1, 0); // Right
                    neighbourOffsetArray[3] = new int2(-1, 0); // Left
                }

                int endNodeIndex = CalculateIndex(endPosition.x, endPosition.y, gridSize.x);
                PathNode startNode = pathNodeArray[CalculateIndex(startPosition.x, startPosition.y, gridSize.x)];

                startNode.gCost = 0;
                startNode.CalculateFCost();
                pathNodeArray[startNode.index] = startNode;

                NativeList<int> openList = new NativeList<int>(Allocator.Temp);
                NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

                openList.Add(startNode.index);

                while (openList.Length > 0)
                {
                    int currentNodeIndex = GetLowestCostFNodeIndex(openList, pathNodeArray);
                    PathNode currentNode = pathNodeArray[currentNodeIndex];

                    if (currentNodeIndex == endNodeIndex)
                    {
                        // Reached our destination!
                        break;
                    }

                    // Remove current node from Open List
                    for (int i = 0; i < openList.Length; ++i)
                    {
                        if (openList[i] == currentNodeIndex)
                        {
                            openList.RemoveAtSwapBack(i);
                            break;
                        }
                    }

                    closedList.Add(currentNodeIndex);

                    for (int i = 0; i < neighbourOffsetArray.Length; ++i)
                    {
                        int2 neighbourOffset = neighbourOffsetArray[i];
                        int2 neighbourPosition = new int2(currentNode.x + neighbourOffset.x, currentNode.y + neighbourOffset.y);

                        if (!IsPositionInsideGrid(neighbourPosition, gridSize))
                        {
                            // Neighbour not valid position
                            continue;
                        }

                        int neighbourNodeIndex = CalculateIndex(neighbourPosition.x, neighbourPosition.y, gridSize.x);

                        if (closedList.Contains(neighbourNodeIndex))
                        {
                            // Already searched this node
                            continue;
                        }

                        PathNode neighbourNode = pathNodeArray[neighbourNodeIndex];
                        if (!neighbourNode.isWalkable)
                        {
                            // Not walkable
                            continue;
                        }

                        int2 currentNodePosition = new int2(currentNode.x, currentNode.y);

                        int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNodePosition, neighbourPosition);
                        if (tentativeGCost < neighbourNode.gCost)
                        {
                            neighbourNode.cameFromNodeIndex = currentNodeIndex;
                            neighbourNode.gCost = tentativeGCost;
                            neighbourNode.CalculateFCost();
                            pathNodeArray[neighbourNodeIndex] = neighbourNode;

                            if (!openList.Contains(neighbourNode.index))
                            {
                                openList.Add(neighbourNode.index);
                            }
                        }
                    }
                }

                neighbourOffsetArray.Dispose();
                openList.Dispose();
                closedList.Dispose();
            }
        }

        /// <summary>
        /// パスフィンディングで使用される個々のノード
        /// A*アルゴリズムのG、H、Fコストとヒープ機能を実装
        /// </summary>
        public struct PathNode : IHeapItem<PathNode>
        {
            /// <summary>グリッドX座標</summary>
            public int x;
            /// <summary>グリッドY座標</summary>
            public int y;

            /// <summary>配列内のインデックス</summary>
            public int index;

            /// <summary>開始点からこのノードまでのコスト</summary>
            public int gCost;
            /// <summary>このノードから目標までの推定コスト</summary>
            public int hCost;
            /// <summary>総合コスト（g + h）</summary>
            public int fCost;

            /// <summary>歩行可能かどうか</summary>
            public bool isWalkable;
            /// <summary>このノードに到達した元ノードのインデックス</summary>
            public int cameFromNodeIndex;
            int heapIndex;

            /// <summary>
            /// G + H = F コストを計算
            /// </summary>
            public void CalculateFCost()
            {
                fCost = gCost + hCost;
            }

            /// <summary>
            /// ノードの歩行可能性を設定
            /// </summary>
            /// <param name="isWalkable">歩行可能かどうか</param>
            public void SetIsWalkable(bool isWalkable)
            {
                this.isWalkable = isWalkable;
            }

            /// <summary>
            /// ヒープ内のインデックス
            /// </summary>
            public int HeapIndex
            {
                get
                {
                    return heapIndex;
                }
                set
                {
                    heapIndex = value;
                }
            }

            /// <summary>
            /// パスノードの比較（ヒープ用）
            /// Fコスト、次にHコストで比較
            /// </summary>
            /// <param name="nodeToCompare">比較対象ノード</param>
            /// <returns>比較結果</returns>
            public int CompareTo(PathNode nodeToCompare)
            {
                int compare = fCost.CompareTo(nodeToCompare.fCost);
                if (compare == 0)
                {
                    compare = hCost.CompareTo(nodeToCompare.hCost);
                }
                return -compare;
            }
        }
        #endregion
    }
}
