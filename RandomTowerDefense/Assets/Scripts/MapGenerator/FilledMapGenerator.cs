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

        [Header("マップ設定")]
        [SerializeField] public Map[] maps;
        [SerializeField] public int mapIndex;
        [SerializeField] public bool Randomize;

        [Header("プレファブ")]
        [SerializeField] public Transform tilePrefab;
        [SerializeField] public Transform obstaclePrefab;
        [SerializeField] public Transform mapFloor;
        [SerializeField] public Transform navmeshFloor;
        [SerializeField] public Transform navmeshMaskPrefab;

        [Header("マップサイズ")]
        [SerializeField] public Vector2 maxMapSize;
        [SerializeField] public int2 MapSize;

        [Header("外観設定")]
        [SerializeField][Range(0, 1)] public float outlinePercent;
        [SerializeField] public float tileSize;
        [SerializeField] public bool packedObstacles;

        [Header("システム参照")]
        [SerializeField] public InGameOperation sceneManager;
        [SerializeField] public StageManager stageManager;

        [Header("生成情報")]
        [HideInInspector] public List<Pillar> PillarList;
        [HideInInspector] public Vector3 originPos;

        #endregion

        #region Private Fields

        private List<Coord> _allTileCoords;
        private Queue<Coord> _shuffledTileCoords;
        private Queue<Coord> _shuffledOpenTileCoords;
        private Transform[,] _tileMap;
        private Map _currentMap;
        private Transform _mapHolder;
        private System.Random _prng;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Start()
        {
            // システム参照はインスペクターで設定
        }

        /// <summary>
        /// 毎フレーム更新
        /// </summary>
        private void Update()
        {
            // 現在使用中の更新処理なし
        }

        #endregion

        #region Public API

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
            PillarList = new List<Pillar>();
            _currentMap = maps[mapIndex];

            // AI訓練用の動的マップサイズ設定
            if (sceneManager && (sceneManager.GetCurrIsland() == StageInfo.IslandNum - 1))
            {
                float width = Mathf.Sqrt(StageInfo.stageSizeEx);
                _currentMap.mapSize.x = (int)(width);
                _currentMap.mapSize.y = (int)(StageInfo.stageSizeEx / width);
                _currentMap.obstaclePercent = StageInfo.obstacleEx;
                MapSize = new int2(_currentMap.mapSize.x, _currentMap.mapSize.y);
            }

            // ランダムシード設定
            if (Randomize)
            {
                _currentMap.seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            }

            _tileMap = new Transform[_currentMap.mapSize.x, _currentMap.mapSize.y];
            _prng = new System.Random(_currentMap.seed);

            // 座標リスト生成
            _allTileCoords = new List<Coord>();
            for (int x = 0; x < _currentMap.mapSize.x; x++)
            {
                for (int y = 0; y < _currentMap.mapSize.y; y++)
                {
                    _allTileCoords.Add(new Coord(x, y));
                }
            }

            // 城とテレポートポイント用のスペース確保
            if (stageManager && stageManager.SpawnPoint.Length >= 4)
            {
                stageManager.SpawnPoint[0] = new Coord(0, 0);
                stageManager.SpawnPoint[1] = new Coord(_currentMap.mapSize.x - 1, 0);
                stageManager.SpawnPoint[2] = new Coord(_currentMap.mapSize.x - 1, _currentMap.mapSize.y - 1);
                stageManager.SpawnPoint[3] = new Coord(0, _currentMap.mapSize.y - 1);
            }

            _shuffledTileCoords = new Queue<Coord>(RandomTowerDefense.Utilities.Utility.ShuffleArray<Coord>(_allTileCoords.ToArray(), _currentMap.seed));

            // Create map holder object
            string holderName = "Generated Map";
            if (transform.Find(holderName))
            {
                DestroyImmediate(transform.Find(holderName).gameObject);
            }

            _mapHolder = new GameObject(holderName).transform;
            _mapHolder.parent = transform;

            // Spawning tiles
            for (int x = 0; x < _currentMap.mapSize.x; x++)
            {
                for (int y = 0; y < _currentMap.mapSize.y; y++)
                {
                    Vector3 tilePosition = CoordToPosition(x, y);
                    Transform newTile = Instantiate(tilePrefab, this.transform.position + tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                    newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                    newTile.parent = _mapHolder;
                    _tileMap[x, y] = newTile;
                }
            }

            // Spawning obstacles
            bool[,] obstacleMap = new bool[(int)_currentMap.mapSize.x, (int)_currentMap.mapSize.y];

            int obstacleCount = (int)(_currentMap.mapSize.x * _currentMap.mapSize.y * _currentMap.obstaclePercent);
            int currentObstacleCount = 0;
            List<Coord> allOpenCoords = new List<Coord>(_allTileCoords);

            for (int i = 0; i < obstacleCount; i++)
            {
                Coord randomCoord = GetRandomCoord();
                if (randomCoord.x == 0 && randomCoord.y == 0) continue;
                if (randomCoord.x == _currentMap.mapSize.x - 1 && randomCoord.y == 0) continue;
                if (randomCoord.x == 0 && randomCoord.y == _currentMap.mapSize.y - 1) continue;
                if (randomCoord.x == _currentMap.mapSize.x - 1 && randomCoord.y == _currentMap.mapSize.y - 1) continue;

                obstacleMap[randomCoord.x, randomCoord.y] = true;
                currentObstacleCount++;

                if (randomCoord != _currentMap.MapCentre && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
                {
                    float obstacleHeight = Mathf.Lerp(_currentMap.minObstacleHeight, _currentMap.maxObstacleHeight, (float)_prng.NextDouble());
                    Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);

                    Transform newObstacle = Instantiate(obstaclePrefab, this.transform.position + obstaclePosition + Vector3.up * obstacleHeight / 2, Quaternion.identity) as Transform;
                    newObstacle.parent = _mapHolder;
                    newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent) * tileSize);
                    PillarList.Add(new Pillar(newObstacle.gameObject, randomCoord.x, randomCoord.y, obstacleHeight));

                    Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                    Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                    float colourPercent = randomCoord.y / (float)_currentMap.mapSize.y;
                    //obstacleMaterial.color = Color.Lerp(_currentMap.foregroundColour,_currentMap.backgroundColour,colourPercent);
                    obstacleRenderer.sharedMaterial = obstacleMaterial;

                    allOpenCoords.Remove(randomCoord);
                }
                else
                {
                    obstacleMap[randomCoord.x, randomCoord.y] = false;
                    currentObstacleCount--;
                }
            }

            _shuffledOpenTileCoords = new Queue<Coord>(RandomTowerDefense.Utilities.Utility.ShuffleArray<Coord>(allOpenCoords.ToArray(), _currentMap.seed));
            _shuffledOpenTileCoords.Enqueue(new Coord(0, 0));

            _shuffledOpenTileCoords.Enqueue(new Coord(_currentMap.mapSize.x - 1, _currentMap.mapSize.y - 1));
            _shuffledOpenTileCoords.Enqueue(new Coord(_currentMap.mapSize.x - 1, 0));
            _shuffledOpenTileCoords.Enqueue(new Coord(0, _currentMap.mapSize.y - 1));

            //Ensure ONE and ONLY ONE road
            if (packedObstacles)
                FillingNonNecessary(_shuffledOpenTileCoords, obstacleMap);

            foreach (Pillar pillar in PillarList)
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
                            if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
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

            //// Creating navmesh mask
            //Transform maskLeft = Instantiate (navmeshMaskPrefab, this.transform.position + Vector3.left * (_currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
            //maskLeft.parent = _mapHolder;
            //maskLeft.localScale = new Vector3 ((maxMapSize.x - _currentMap.mapSize.x) / 2f, 1, _currentMap.mapSize.y) * tileSize;

            //Transform maskRight = Instantiate (navmeshMaskPrefab, this.transform.position + Vector3.right * (_currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
            //maskRight.parent = _mapHolder;
            //maskRight.localScale = new Vector3 ((maxMapSize.x - _currentMap.mapSize.x) / 2f, 1, _currentMap.mapSize.y) * tileSize;

            //Transform maskTop = Instantiate (navmeshMaskPrefab, this.transform.position + Vector3.forward * (_currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
            //maskTop.parent = _mapHolder;
            //maskTop.localScale = new Vector3 (maxMapSize.x, 1, (maxMapSize.y-_currentMap.mapSize.y)/2f) * tileSize;

            //Transform maskBottom = Instantiate (navmeshMaskPrefab, this.transform.position + Vector3.back * (_currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
            //maskBottom.parent = _mapHolder;
            //maskBottom.localScale = new Vector3 (maxMapSize.x, 1, (maxMapSize.y-_currentMap.mapSize.y)/2f) * tileSize;

            navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
            mapFloor.localScale = new Vector3(_currentMap.mapSize.x * tileSize, _currentMap.mapSize.y * tileSize);

            originPos = this.transform.position + CoordToPosition(0, 0);

            PathfindingGridSetup.Instance.isActivated = false;
        }

        void FillingNonNecessary(Queue<Coord> openTileList, bool[,] obstacleMap)
        {
            List<Coord> tempList = openTileList.ToList<Coord>();
            bool openTileRemoved = true;
            while (openTileRemoved)
            {
                openTileRemoved = false;
                foreach (Coord i in tempList)
                {
                    if (i.x == 0 && i.y == 0) continue;
                    if (i.x == _currentMap.mapSize.x - 1 && i.y == 0) continue;
                    if (i.x == 0 && i.y == _currentMap.mapSize.y - 1) continue;
                    if (i.x == _currentMap.mapSize.x - 1 && i.y == _currentMap.mapSize.y - 1) continue;

                    int CntSurrounding = 0;
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            int neighbourX = i.x + x;
                            int neighbourY = i.y + y;
                            if (x == 0 ^ y == 0)
                            {
                                if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
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
                        float obstacleHeight = Mathf.Lerp(_currentMap.minObstacleHeight, _currentMap.maxObstacleHeight, (float)_prng.NextDouble());
                        Vector3 obstaclePosition = CoordToPosition(i.x, i.y);

                        Transform newObstacle = Instantiate(obstaclePrefab, this.transform.position + obstaclePosition + Vector3.up * obstacleHeight / 2, Quaternion.identity) as Transform;
                        newObstacle.parent = _mapHolder;
                        newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent) * tileSize);
                        PillarList.Add(new Pillar(newObstacle.gameObject, i.x, i.y, obstacleHeight));

                        Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                        Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                        float colourPercent = i.y / (float)_currentMap.mapSize.y;
                        //obstacleMaterial.color = Color.Lerp(_currentMap.foregroundColour, _currentMap.backgroundColour, colourPercent);
                        obstacleRenderer.sharedMaterial = obstacleMaterial;

                        //Restart Processing
                        openTileRemoved = true;
                        tempList.Remove(i);
                        obstacleMap[i.x, i.y] = true;
                        break;
                    }
                }
            }
            openTileList = new Queue<Coord>(tempList);
        }

        bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
        {
            bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];

            Queue<Coord> queue = new Queue<Coord>();
            queue.Enqueue(_currentMap.MapCentre);
            mapFlags[_currentMap.MapCentre.x, _currentMap.MapCentre.y] = true;

            int accessibleTileCount = 1;

            while (queue.Count > 0)
            {
                Coord tile = queue.Dequeue();

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        int neighbourX = tile.x + x;
                        int neighbourY = tile.y + y;
                        if (x == 0 ^ y == 0)
                        {
                            if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
                            {
                                if (!mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY])
                                {
                                    mapFlags[neighbourX, neighbourY] = true;
                                    queue.Enqueue(new Coord(neighbourX, neighbourY));
                                    accessibleTileCount++;
                                }
                            }
                        }
                    }
                }
            }
            int targetAccessibleTileCount = (int)(_currentMap.mapSize.x * _currentMap.mapSize.y - currentObstacleCount);
            return targetAccessibleTileCount == accessibleTileCount;
        }

        public Vector3 CoordToPosition(Coord coord)
        {
            return CoordToPosition(coord.x, coord.y);
        }

        Vector3 CoordToPosition(int x, int y)
        {
            return new Vector3(-_currentMap.mapSize.x / 2f + 0.5f + x, 0, -_currentMap.mapSize.y / 2f + 0.5f + y) * tileSize;
        }

        public Transform GetTileFromPosition(Vector3 position)
        {
            int x = Mathf.RoundToInt((position.x - transform.position.x) / tileSize + (_currentMap.mapSize.x - 1) / 2f);
            int y = Mathf.RoundToInt((position.z - transform.position.z) / tileSize + (_currentMap.mapSize.y - 1) / 2f);
            x = Mathf.Clamp(x, 0, _tileMap.GetLength(0) - 1);
            y = Mathf.Clamp(y, 0, _tileMap.GetLength(1) - 1);
            return _tileMap[x, y];
        }

        public int2 GetTileIDFromPosition(Vector3 position)
        {
            int x = Mathf.RoundToInt((position.x - transform.position.x) / tileSize + (_currentMap.mapSize.x - 1) / 2f);
            int y = Mathf.RoundToInt((position.z - transform.position.z) / tileSize + (_currentMap.mapSize.y - 1) / 2f);
            x = Mathf.Clamp(x, 0, _tileMap.GetLength(0) - 1);
            y = Mathf.Clamp(y, 0, _tileMap.GetLength(1) - 1);
            return new int2(x, y);
        }

        public Coord GetRandomCoord()
        {
            Coord randomCoord = _shuffledTileCoords.Dequeue();
            _shuffledTileCoords.Enqueue(randomCoord);
            return randomCoord;
        }

        public Transform GetRandomOpenTile()
        {
            Coord randomCoord = _shuffledOpenTileCoords.Dequeue();
            _shuffledOpenTileCoords.Enqueue(randomCoord);
            return _tileMap[randomCoord.x, randomCoord.y];
        }

        public void CustomizeMapAndCreate(int width, int depth)
        {
            maps[3].mapSize = new Coord(width, depth);
            OnNewStage(3);
        }

        public float UpdatePillarStatus(GameObject targetPillar, int toState = 1)
        {
            foreach (Pillar i in PillarList)
            {
                if (i.obj != targetPillar) continue;
                i.state = toState;
                return i.height;
            }
            return 0;
        }

        public bool ChkPillarStatusEmpty(GameObject targetPillar)
        {
            foreach (Pillar i in PillarList)
            {
                if (i.obj == null) continue;
                if (i.obj != targetPillar) continue;
                return (i.state == 0);
            }
            return false;
        }

        #endregion

        #region Nested Types

        /// <summary>
        /// マップ設定データクラス - SerializableObject対応マップパラメーター
        /// </summary>
        [System.Serializable]
        public class Map
        {
            [Header("マップサイズ")]
            public Coord mapSize;

            [Header("障害物設定")]
            [Range(0, 1)] public float obstaclePercent;
            public float minObstacleHeight;
            public float maxObstacleHeight;

            [Header("生成設定")]
            public int seed;

            [Header("カラー設定")]
            public Color foregroundColour;
            public Color backgroundColour;

            /// <summary>
            /// マップ中心座標の自動計算
            /// </summary>
            public Coord MapCentre
            {
                get
                {
                    return new Coord(mapSize.x / 2, mapSize.y / 2);
                }
            }
        }

        public int CurrMapX()
        {
            return _currentMap.mapSize.x;
        }
        public int CurrMapY()
        {
            return _currentMap.mapSize.y;
        }

        public bool GetMapWalkable(int x, int y)
        {
            if (_shuffledOpenTileCoords.Contains(new Coord(x, y)))
                return true;
            return false;
        }

        #endregion
    }


    [System.Serializable]
    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int _x, int _y)
        {
            x = _x;
            y = _y;
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Coord c1, Coord c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coord c1, Coord c2)
        {
            return !(c1 == c2);
        }


    }


    public class Pillar
    {
        public GameObject obj;
        public Coord mapSize;
        public int state;//0: Empty 1: Occupied
        public float height;
        public int surroundSpace;
        public Pillar(GameObject obj, int _x, int _y, float height, int state = 0)
        {
            this.obj = obj;
            mapSize.x = _x;
            mapSize.y = _y;
            this.state = state;
            this.height = height;
        }
    }
}