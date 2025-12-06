using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using System.Linq;
using RandomTowerDefense.Scene;
using RandomTowerDefense.Managers.Macro;
using RandomTowerDefense.Info;
using RandomTowerDefense.Utilities;

namespace RandomTowerDefense.MapGenerator
{
    /// <summary>
    /// 埋め尽くされたマップ生成器 - プロシージュラルレベル生成システム
    ///
    /// 主な機能:
    /// - タイルベースのプロシージュラルマップ生成システム
    /// - 障害物配置とアクセシビリティ検証アルゴリズム
    /// - シードベースの再現可能なランダム生成システム
    /// - スポーンポイントと城配置の自動計算
    /// - AI訓練用動的マップサイズ対応
    /// - 柱状態管理とナビメッシュ連携システム
    /// - フラッドフィルアルゴリズムによるアクセシビリティ検証
    /// </summary>
    public class FilledMapGenerator : MonoBehaviour
    {
        #region Serialized Fields

        /// <summary>
        /// マップ設定配列
        /// </summary>
        [Header("マップ設定")]
        [SerializeField] public FilledMapInfo[] maps;

        /// <summary>
        /// 現在のマップインデックス
        /// </summary>
        [SerializeField] public int mapIndex;

        /// <summary>
        /// ランダムシード使用フラグ
        /// </summary>
        [SerializeField] public bool Randomize;

        /// <summary>
        /// タイルプレハブ
        /// </summary>
        [Header("プレファブ")]
        [SerializeField] public Transform tilePrefab;

        /// <summary>
        /// 障害物プレハブ
        /// </summary>
        [SerializeField] public Transform obstaclePrefab;

        /// <summary>
        /// マップフロアオブジェクト
        /// </summary>
        [SerializeField] public Transform mapFloor;

        /// <summary>
        /// ナビメッシュフロアオブジェクト
        /// </summary>
        [SerializeField] public Transform navmeshFloor;

        /// <summary>
        /// 最大マップサイズ
        /// </summary>
        [Header("マップサイズ")]
        [SerializeField] public Vector2 maxMapSize;

        /// <summary>
        /// 現在のマップサイズ
        /// </summary>
        [SerializeField] public int2 MapSize;

        /// <summary>
        /// タイルアウトライン表示率（0.0～1.0）
        /// </summary>
        [Header("外観設定")]
        [SerializeField][Range(0, 1)] public float outlinePercent;

        /// <summary>
        /// タイルサイズ
        /// </summary>
        [SerializeField] public float tileSize;

        /// <summary>
        /// 障害物密集配置フラグ
        /// </summary>
        [SerializeField] public bool packedObstacles;

        /// <summary>
        /// マップ原点位置
        /// </summary>
        [SerializeField] public Vector3 originPos;

        /// <summary>
        /// インゲームシーンマネージャー参照
        /// </summary>
        [Header("システム参照")]
        [SerializeField] public InGameOperation sceneManager;

        /// <summary>
        /// ステージマネージャー参照
        /// </summary>
        [SerializeField] public StageManager stageManager;

        #endregion

        #region Private Fields

        private List<FilledMapCoord> allTileCoords;
        private Queue<FilledMapCoord> shuffledTileCoords;
        private Queue<FilledMapCoord> shuffledOpenTileCoords;
        private Transform[,] tileMap;
        private FilledMapInfo currentMap;
        private Transform mapHolder;
        private System.Random prng;
        private List<Pillar> pillarList;

        #endregion

        #region Public API

        /// <summary>
        /// 柱リスト取得処理 - 現在のマップに存在する柱オブジェクトのリストを返す
        /// </summary>
        /// <returns>柱オブジェクトのリスト</returns>
        public List<Pillar> GetPillarList() => pillarList;

        /// <summary>
        /// 現在のマップのXサイズ取得処理 - 現在生成されているマップの幅を返す
        /// </summary>
        /// <returns>マップの幅(int)</returns>
        public int CurrMapX() => currentMap.mapSize.x;

        /// <summary>
        /// 現在のマップのYサイズ取得処理 - 現在生成されているマップの高さを返す
        /// </summary>
        /// <returns>マップの高さ(int)</returns>
        public int CurrMapY() => currentMap.mapSize.y;

        /// <summary>
        /// マップの歩行可能性取得処理 - 指定座標のタイルが歩行可能かどうかを返す
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <returns>タイルが歩行可能であるかどうかの真偽値</returns>
        public bool GetMapWalkable(int x, int y)
        {
            return shuffledOpenTileCoords.Contains(new FilledMapCoord(x, y));
        }

        /// <summary>
        /// 新ステージ開始時処理 - 指定ステージ番号のマップ生成
        /// </summary>
        /// <param name="stageNumber">ステージ番号</param>
        public void OnNewStage(int stageNumber)
        {
            mapIndex = stageNumber;
            GenerateMap();
        }

        /// <summary>
        /// メインマップ生成処理 - タイル配置から障害物生成まで統合実行
        /// </summary>
        public void GenerateMap()
        {
            pillarList = new List<Pillar>();
            currentMap = maps[mapIndex];

            // AI訓練用の動的マップサイズ設定
            if (sceneManager && (sceneManager.GetCurrIsland() == StageInfoDetail.IslandNum - 1))
            {
                float width = Mathf.Sqrt(StageInfoDetail.customStageInfo.StageSizeFactor);
                currentMap.mapSize.x = (int)(width);
                currentMap.mapSize.y = (int)(StageInfoDetail.customStageInfo.StageSizeFactor / width);
                currentMap.obstaclePercent = StageInfoDetail.customStageInfo.ObstacleFactor;
                MapSize = new int2(currentMap.mapSize.x, currentMap.mapSize.y);
            }

            // ランダムシード設定
            if (Randomize)
            {
                currentMap.seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            }

            tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];
            prng = new System.Random(currentMap.seed);

            // 座標リスト生成
            allTileCoords = new List<FilledMapCoord>();
            for (int x = 0; x < currentMap.mapSize.x; x++)
            {
                for (int y = 0; y < currentMap.mapSize.y; y++)
                {
                    allTileCoords.Add(new FilledMapCoord(x, y));
                }
            }

            // 城とテレポートポイント用のスペース確保
            if (stageManager && stageManager.SpawnPoint.Length >= 4)
            {
                stageManager.SpawnPoint[0] = new FilledMapCoord(0, 0);
                stageManager.SpawnPoint[1] = new FilledMapCoord(currentMap.mapSize.x - 1, 0);
                stageManager.SpawnPoint[2] = new FilledMapCoord(currentMap.mapSize.x - 1, currentMap.mapSize.y - 1);
                stageManager.SpawnPoint[3] = new FilledMapCoord(0, currentMap.mapSize.y - 1);
            }

            shuffledTileCoords = new Queue<FilledMapCoord>(
                RandomTowerDefense.Utilities.Utility.ShuffleArray<FilledMapCoord>(
                    allTileCoords.ToArray(), currentMap.seed));

            // Create map holder object
            string holderName = "Generated Map";
            if (transform.Find(holderName))
            {
                DestroyImmediate(transform.Find(holderName).gameObject);
            }

            mapHolder = new GameObject(holderName).transform;
            mapHolder.parent = transform;

            // Spawning tiles
            for (int x = 0; x < currentMap.mapSize.x; x++)
            {
                for (int y = 0; y < currentMap.mapSize.y; y++)
                {
                    Vector3 tilePosition = CoordToPosition(x, y);
                    Transform newTile = Instantiate(tilePrefab,
                        this.transform.position + tilePosition,
                        Quaternion.Euler(Vector3.right * 90)) as Transform;
                    newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                    newTile.parent = mapHolder;
                    tileMap[x, y] = newTile;
                }
            }

            // Spawning obstacles
            bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];

            int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
            int currentObstacleCount = 0;
            List<FilledMapCoord> allOpenCoords = new List<FilledMapCoord>(allTileCoords);

            for (int i = 0; i < obstacleCount; i++)
            {
                FilledMapCoord randomCoord = GetRandomCoord();
                if (randomCoord.x == 0 && randomCoord.y == 0) continue;
                if (randomCoord.x == currentMap.mapSize.x - 1 && randomCoord.y == 0) continue;
                if (randomCoord.x == 0 && randomCoord.y == currentMap.mapSize.y - 1) continue;
                if (randomCoord.x == currentMap.mapSize.x - 1 && randomCoord.y == currentMap.mapSize.y - 1) continue;

                obstacleMap[randomCoord.x, randomCoord.y] = true;
                currentObstacleCount++;

                if (randomCoord != currentMap.MapCentre && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
                {
                    float obstacleHeight = Mathf.Lerp(
                        currentMap.minObstacleHeight,
                        currentMap.maxObstacleHeight,
                        (float)prng.NextDouble());
                    Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);

                    Transform newObstacle = Instantiate(obstaclePrefab,
                        this.transform.position + obstaclePosition + Vector3.up * obstacleHeight / 2,
                        Quaternion.identity) as Transform;
                    newObstacle.parent = mapHolder;
                    newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize,
                        obstacleHeight, (1 - outlinePercent) * tileSize);
                    pillarList.Add(new Pillar(newObstacle.gameObject,
                        randomCoord.x, randomCoord.y, obstacleHeight));

                    Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                    Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                    float colourPercent = randomCoord.y / (float)currentMap.mapSize.y;
                    obstacleMaterial.color = Color.Lerp(
                        currentMap.foregroundColour,
                        currentMap.backgroundColour, colourPercent);
                    obstacleRenderer.sharedMaterial = obstacleMaterial;

                    allOpenCoords.Remove(randomCoord);
                }
                else
                {
                    obstacleMap[randomCoord.x, randomCoord.y] = false;
                    currentObstacleCount--;
                }
            }

            shuffledOpenTileCoords = new Queue<FilledMapCoord>(
                RandomTowerDefense.Utilities.Utility.ShuffleArray<FilledMapCoord>(
                    allOpenCoords.ToArray(), currentMap.seed));
            shuffledOpenTileCoords.Enqueue(new FilledMapCoord(0, 0));

            shuffledOpenTileCoords.Enqueue(new FilledMapCoord(currentMap.mapSize.x - 1, currentMap.mapSize.y - 1));
            shuffledOpenTileCoords.Enqueue(new FilledMapCoord(currentMap.mapSize.x - 1, 0));
            shuffledOpenTileCoords.Enqueue(new FilledMapCoord(0, currentMap.mapSize.y - 1));

            //Ensure ONE and ONLY ONE road
            if (packedObstacles)
                FillingNonNecessary(shuffledOpenTileCoords, obstacleMap);

            foreach (Pillar pillar in pillarList)
            {
                int counter = 0;

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        int neighbourX = pillar.mapSize.x + x;
                        int neighbourY = pillar.mapSize.y + y;

                        if (x == 0 ^ y == 0)
                        {
                            if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0)
                            && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
                            {
                                if (obstacleMap[neighbourX, neighbourY] == false)
                                {
                                    counter++;
                                }
                            }
                        }
                    }
                }
                pillar.surroundSpace = counter;
            }

            navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
            mapFloor.localScale = new Vector3(
                currentMap.mapSize.x * tileSize,
                currentMap.mapSize.y * tileSize);

            originPos = this.transform.position + CoordToPosition(0, 0);

            PathfindingGridSetup.Instance.isActivated = false;
        }

        /// <summary>
        /// 不要な開放タイルを障害物で埋める処理 - マップ内の通路を一本化
        /// </summary>
        /// <param name="openTileList">開放タイルのキュー</param>
        /// <param name="obstacleMap">障害物マップの2D配列</param>
        void FillingNonNecessary(Queue<FilledMapCoord> openTileList, bool[,] obstacleMap)
        {
            List<FilledMapCoord> tempList = openTileList.ToList<FilledMapCoord>();
            bool openTileRemoved = true;
            while (openTileRemoved)
            {
                openTileRemoved = false;
                foreach (FilledMapCoord i in tempList)
                {
                    if (i.x == 0 && i.y == 0) continue;
                    if (i.x == currentMap.mapSize.x - 1 && i.y == 0) continue;
                    if (i.x == 0 && i.y == currentMap.mapSize.y - 1) continue;
                    if (i.x == currentMap.mapSize.x - 1 && i.y == currentMap.mapSize.y - 1) continue;

                    int CntSurrounding = 0;
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            int neighbourX = i.x + x;
                            int neighbourY = i.y + y;
                            if (x == 0 ^ y == 0)
                            {
                                if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0)
                                    && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
                                {
                                    if (obstacleMap[neighbourX, neighbourY] == false)
                                    {
                                        CntSurrounding++;
                                    }
                                }
                            }
                            if (CntSurrounding > 1) break;
                        }
                        if (CntSurrounding > 1) break;
                    }

                    if (CntSurrounding == 1)
                    {
                        //Build Obstacle
                        float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight,
                            currentMap.maxObstacleHeight, (float)prng.NextDouble());
                        Vector3 obstaclePosition = CoordToPosition(i.x, i.y);

                        Transform newObstacle = Instantiate(obstaclePrefab,
                            this.transform.position + obstaclePosition + Vector3.up * obstacleHeight / 2,
                            Quaternion.identity) as Transform;
                        newObstacle.parent = mapHolder;
                        newObstacle.localScale = new Vector3(
                            (1 - outlinePercent) * tileSize,
                            obstacleHeight,
                            (1 - outlinePercent) * tileSize);
                        pillarList.Add(new Pillar(newObstacle.gameObject, i.x, i.y, obstacleHeight));

                        Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                        Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                        float colourPercent = i.y / (float)currentMap.mapSize.y;
                        obstacleMaterial.color = Color.Lerp(
                            currentMap.foregroundColour,
                            currentMap.backgroundColour,
                            colourPercent);
                        obstacleRenderer.sharedMaterial = obstacleMaterial;

                        //Restart Processing
                        openTileRemoved = true;
                        tempList.Remove(i);
                        obstacleMap[i.x, i.y] = true;
                        break;
                    }
                }
            }
            openTileList = new Queue<FilledMapCoord>(tempList);
        }

        /// <summary>
        /// マップの完全なアクセシビリティ検証 - フラッドフィルアルゴリズムを使用して全タイルへの到達可能性を確認
        /// </summary>
        /// <param name="obstacleMap">障害物マップの2D配列</param>
        /// <param name="currentObstacleCount">現在の障害物数</param>
        /// <returns>マップが完全にアクセシブルであるかどうかの真偽値</returns>
        bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
        {
            bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];

            Queue<FilledMapCoord> queue = new Queue<FilledMapCoord>();
            queue.Enqueue(currentMap.MapCentre);
            mapFlags[currentMap.MapCentre.x, currentMap.MapCentre.y] = true;

            int accessibleTileCount = 1;

            while (queue.Count > 0)
            {
                FilledMapCoord tile = queue.Dequeue();

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        int neighbourX = tile.x + x;
                        int neighbourY = tile.y + y;
                        if (x == 0 ^ y == 0)
                        {
                            if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0)
                                && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
                            {
                                if (!mapFlags[neighbourX, neighbourY]
                                    && !obstacleMap[neighbourX, neighbourY])
                                {
                                    mapFlags[neighbourX, neighbourY] = true;
                                    queue.Enqueue(new FilledMapCoord(neighbourX, neighbourY));
                                    accessibleTileCount++;
                                }
                            }
                        }
                    }
                }
            }
            int targetAccessibleTileCount =
                (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
            return targetAccessibleTileCount == accessibleTileCount;
        }

        /// <summary>
        /// 座標からワールド位置への変換処理
        /// </summary>
        /// <param name="coord">マップ座標</param>
        /// <returns>ワールド位置ベクトル</returns>
        public Vector3 CoordToPosition(FilledMapCoord coord)
        {
            return CoordToPosition(coord.x, coord.y);
        }

        /// <summary>
        /// ワールド位置からタイル取得処理
        /// </summary>
        /// <param name="position">ワールド位置ベクトル</param>
        /// <returns>対応するタイルのTransform</returns>

        public Transform GetTileFromPosition(Vector3 position)
        {
            int x = Mathf.RoundToInt((position.x - transform.position.x)
                / tileSize + (currentMap.mapSize.x - 1) / 2f);
            int y = Mathf.RoundToInt((position.z - transform.position.z)
                / tileSize + (currentMap.mapSize.y - 1) / 2f);
            x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
            y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);
            return tileMap[x, y];
        }

        /// <summary>
        /// ワールド位置からタイルID取得処理
        /// </summary>
        /// <param name="position">ワールド位置ベクトル</param>
        /// <returns>対応するタイルのID(int2)</returns>
        public int2 GetTileIDFromPosition(Vector3 position)
        {
            int x = Mathf.RoundToInt((position.x - transform.position.x)
                / tileSize + (currentMap.mapSize.x - 1) / 2f);
            int y = Mathf.RoundToInt((position.z - transform.position.z)
                / tileSize + (currentMap.mapSize.y - 1) / 2f);
            x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
            y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);
            return new int2(x, y);
        }

        /// <summary>
        /// ランダム座標取得処理
        /// </summary>
        /// <returns>ランダムに選ばれたマップ座標</returns>
        public FilledMapCoord GetRandomCoord()
        {
            FilledMapCoord randomCoord = shuffledTileCoords.Dequeue();
            shuffledTileCoords.Enqueue(randomCoord);
            return randomCoord;
        }

        /// <summary>
        /// ランダム開放タイル取得処理
        /// </summary>
        /// <returns>ランダムに選ばれた開放タイルのTransform</returns>
        public Transform GetRandomOpenTile()
        {
            FilledMapCoord randomCoord = shuffledOpenTileCoords.Dequeue();
            shuffledOpenTileCoords.Enqueue(randomCoord);
            return tileMap[randomCoord.x, randomCoord.y];
        }

        /// <summary>
        /// マップカスタマイズと生成処理 - 指定サイズでのマップ生成を実行
        /// </summary>
        /// <param name="width">マップ幅</param>
        /// <param name="depth">マップ奥行き</param>
        public void CustomizeMapAndCreate(int width, int depth)
        {
            maps[3].mapSize = new FilledMapCoord(width, depth);
            OnNewStage(3);
        }

        /// <summary>
        /// 柱状態更新処理 - 指定柱の状態を更新し、高さを返す
        /// </summary>
        /// <param name="targetPillar">対象の柱オブジェクト</param>
        /// <param name="toState">更新先の状態（デフォルトは1）</param>
        /// <returns>更新された柱の高さ</returns>
        public float UpdatePillarStatus(GameObject targetPillar, int toState = 1)
        {
            foreach (Pillar i in pillarList)
            {
                if (i.obj != targetPillar) continue;
                i.state = toState;
                return i.height;
            }
            return 0;
        }

        /// <summary>
        /// 柱状態確認処理 - 指定柱が空状態かどうかを確認
        /// </summary>
        /// <param name="targetPillar">対象の柱オブジェクト</param>
        /// <returns>柱が空状態であるかどうかの真偽値</
        public bool ChkPillarStatusEmpty(GameObject targetPillar)
        {
            foreach (Pillar i in pillarList)
            {
                if (i.obj == null) continue;
                if (i.obj != targetPillar) continue;
                return (i.state == 0);
            }
            return false;
        }

        /// <summary>
        /// 座標からワールド位置への変換処理
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <returns>ワールド位置ベクトル</returns>
        Vector3 CoordToPosition(int x, int y)
        {
            var basePosition = new Vector3(
                -currentMap.mapSize.x / 2f + 0.5f + x,
                0,
                -currentMap.mapSize.y / 2f + 0.5f + y);
            return basePosition * tileSize;
        }

        #endregion
    }
}
