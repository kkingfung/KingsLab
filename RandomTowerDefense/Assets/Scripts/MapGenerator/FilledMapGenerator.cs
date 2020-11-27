using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using System.Linq;

public class FilledMapGenerator : MonoBehaviour 
{
	public Map[] maps;
	public int mapIndex;
	public bool Randomize;
	public Transform tilePrefab;
	public Transform obstaclePrefab;
	public Transform mapFloor;
	public Transform navmeshFloor;
	public Transform navmeshMaskPrefab;
	public Vector2 maxMapSize;
	public int2 MapSize;
	//public GameObject FakeParentCam;
	//private Vector3 RelativePosition;
	//private bool FollowFakeParent=false;

	[Range(0,1)]
	public float outlinePercent;
	
	public float tileSize;
	public bool packedObstacles;

	private List<Coord> allTileCoords;
	private Queue<Coord> shuffledTileCoords;
	private Queue<Coord> shuffledOpenTileCoords;
	private Transform[,] tileMap;

	[HideInInspector]
	public List<Pillar> PillarList;

	private Map currentMap;
	private Transform mapHolder;

	[HideInInspector]
	public Vector3 originPos;
	public InGameOperation sceneManager;
	public StageManager stageManager;

	private System.Random prng;

	private void Start() {
		//sceneManager = FindObjectOfType<InGameOperation>();
		//stageManager = FindObjectOfType<StageManager>();
	}
	//public void followParentCam(bool toFollow) {
	//	FollowFakeParent = toFollow;
	//	RelativePosition = this.transform.position - FakeParentCam.transform.position;
	//}
	private void Update()
	{
		//if (FollowFakeParent) {
		//	this.transform.position = RelativePosition + FakeParentCam.transform.position;
		//}
	}

	public void OnNewStage(int stageNumber) {
		mapIndex = stageNumber;
		GenerateMap ();
	}

	public void GenerateMap() {
		PillarList = new List<Pillar>();
		currentMap = maps[mapIndex];
		if (sceneManager && (sceneManager.GetCurrIsland() == StageInfo.IslandNum - 1))
		{
			float width = Mathf.Sqrt(StageInfo.stageSizeEx);
			currentMap.mapSize.x = (int)(width);
			currentMap.mapSize.y = (int)(StageInfo.stageSizeEx / width);
			currentMap.obstaclePercent = StageInfo.obstacleEx;
			MapSize = new int2(currentMap.mapSize.x, currentMap.mapSize.y);
		}

		if (Randomize)
			currentMap.seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		tileMap = new Transform[currentMap.mapSize.x,currentMap.mapSize.y];
		prng = new System.Random (currentMap.seed);

		// Generating coords
		allTileCoords = new List<Coord> ();
		for (int x = 0; x < currentMap.mapSize.x; x ++) {
			for (int y = 0; y < currentMap.mapSize.y; y ++) {
				allTileCoords.Add(new Coord(x,y));
			}
		}

		//Space for Castle and TP
		if (stageManager && stageManager.SpawnPoint.Length >= 4)
		{
			stageManager.SpawnPoint[0] = new Coord(0, 0);
			stageManager.SpawnPoint[1] = new Coord(currentMap.mapSize.x - 1, 0);
			stageManager.SpawnPoint[2] = new Coord(currentMap.mapSize.x - 1, currentMap.mapSize.y - 1);
			stageManager.SpawnPoint[3] = new Coord(0, currentMap.mapSize.y - 1);

			////Remove for Confirmed Open Space
			//foreach (Coord i in stageManager.SpawnPoint)
			//	allTileCoords.Remove(i);
		}

		shuffledTileCoords = new Queue<Coord> (Utility.ShuffleArray (allTileCoords.ToArray (), currentMap.seed));

		// Create map holder object
		string holderName = "Generated Map";
		if (transform.Find (holderName)) {
			DestroyImmediate (transform.Find (holderName).gameObject);
		}
		
		mapHolder = new GameObject (holderName).transform;
		mapHolder.parent = transform;

		// Spawning tiles
		for (int x = 0; x < currentMap.mapSize.x; x ++) {
			for (int y = 0; y < currentMap.mapSize.y; y ++) {
				Vector3 tilePosition = CoordToPosition(x,y);
				Transform newTile = Instantiate (tilePrefab, this.transform.position + tilePosition, Quaternion.Euler (Vector3.right * 90)) as Transform;
				newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
				newTile.parent = mapHolder;
				tileMap[x,y] = newTile;
			}
		}

		// Spawning obstacles
		bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x,(int)currentMap.mapSize.y];
		
		int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
		int currentObstacleCount = 0;
		List<Coord> allOpenCoords = new List<Coord> (allTileCoords);
		
		for (int i =0; i < obstacleCount; i ++) {
			Coord randomCoord = GetRandomCoord();
			if (randomCoord.x == 0 && randomCoord.y == 0) continue;
			if (randomCoord.x == currentMap.mapSize.x - 1 && randomCoord.y == 0) continue;
			if (randomCoord.x == 0 && randomCoord.y == currentMap.mapSize.y - 1) continue;
			if (randomCoord.x == currentMap.mapSize.x - 1 && randomCoord.y == currentMap.mapSize.y - 1) continue;

			obstacleMap[randomCoord.x,randomCoord.y] = true;
			currentObstacleCount ++;
			
			if (randomCoord != currentMap.mapCentre && MapIsFullyAccessible(obstacleMap, currentObstacleCount)) {
				float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight,currentMap.maxObstacleHeight,(float)prng.NextDouble());
				Vector3 obstaclePosition = CoordToPosition(randomCoord.x,randomCoord.y);
				
				Transform newObstacle = Instantiate(obstaclePrefab, this.transform.position+obstaclePosition + Vector3.up * obstacleHeight/2, Quaternion.identity) as Transform;
				newObstacle.parent = mapHolder;
				newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent) * tileSize);
				PillarList.Add(new Pillar(newObstacle.gameObject, randomCoord.x, randomCoord.y, obstacleHeight));

				Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
				Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
				float colourPercent = randomCoord.y / (float)currentMap.mapSize.y;
				obstacleMaterial.color = Color.Lerp(currentMap.foregroundColour,currentMap.backgroundColour,colourPercent);
				obstacleRenderer.sharedMaterial = obstacleMaterial;

				allOpenCoords.Remove(randomCoord);
			}
			else {
				obstacleMap[randomCoord.x,randomCoord.y] = false;
				currentObstacleCount --;
			}
		}

		shuffledOpenTileCoords = new Queue<Coord> (Utility.ShuffleArray (allOpenCoords.ToArray (), currentMap.seed));
		shuffledOpenTileCoords.Enqueue(new Coord(0, 0));

		shuffledOpenTileCoords.Enqueue(new Coord(currentMap.mapSize.x - 1, currentMap.mapSize.y - 1));
		shuffledOpenTileCoords.Enqueue(new Coord(currentMap.mapSize.x - 1, 0));
		shuffledOpenTileCoords.Enqueue(new Coord(0, currentMap.mapSize.y - 1));

		//Ensure ONE and ONLY ONE road
		if(packedObstacles)
		FillingNonNecessary(shuffledOpenTileCoords,obstacleMap);

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
		//Transform maskLeft = Instantiate (navmeshMaskPrefab, this.transform.position + Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
		//maskLeft.parent = mapHolder;
		//maskLeft.localScale = new Vector3 ((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

			//Transform maskRight = Instantiate (navmeshMaskPrefab, this.transform.position + Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
			//maskRight.parent = mapHolder;
			//maskRight.localScale = new Vector3 ((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

			//Transform maskTop = Instantiate (navmeshMaskPrefab, this.transform.position + Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
			//maskTop.parent = mapHolder;
			//maskTop.localScale = new Vector3 (maxMapSize.x, 1, (maxMapSize.y-currentMap.mapSize.y)/2f) * tileSize;

			//Transform maskBottom = Instantiate (navmeshMaskPrefab, this.transform.position + Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
			//maskBottom.parent = mapHolder;
			//maskBottom.localScale = new Vector3 (maxMapSize.x, 1, (maxMapSize.y-currentMap.mapSize.y)/2f) * tileSize;

		navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
        mapFloor.localScale = new Vector3(currentMap.mapSize.x * tileSize, currentMap.mapSize.y * tileSize);

		originPos = this.transform.position + CoordToPosition(0, 0);
	}

	void FillingNonNecessary(Queue<Coord> openTileList, bool[,] obstacleMap) {
		List<Coord> tempList = openTileList.ToList<Coord>();
		bool openTileRemoved = true;
		while (openTileRemoved)
		{
			openTileRemoved = false;
			foreach (Coord i in tempList)
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
					float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble());
					Vector3 obstaclePosition = CoordToPosition(i.x, i.y);

					Transform newObstacle = Instantiate(obstaclePrefab, this.transform.position + obstaclePosition + Vector3.up * obstacleHeight / 2, Quaternion.identity) as Transform;
					newObstacle.parent = mapHolder;
					newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent) * tileSize);
					PillarList.Add(new Pillar(newObstacle.gameObject, i.x, i.y, obstacleHeight));

					Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
					Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
					float colourPercent = i.y / (float)currentMap.mapSize.y;
					obstacleMaterial.color = Color.Lerp(currentMap.foregroundColour, currentMap.backgroundColour, colourPercent);
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

	bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount) {
		bool[,] mapFlags = new bool[obstacleMap.GetLength(0),obstacleMap.GetLength(1)];

		Queue<Coord> queue = new Queue<Coord> ();
		queue.Enqueue (currentMap.mapCentre);
		mapFlags [currentMap.mapCentre.x, currentMap.mapCentre.y] = true;
		
		int accessibleTileCount = 1;
		
		while (queue.Count > 0) {
			Coord tile = queue.Dequeue();
			
			for (int x = -1; x <= 1; x ++) {
				for (int y = -1; y <= 1; y ++) {
					int neighbourX = tile.x + x;
					int neighbourY = tile.y + y;
					if (x == 0 ^ y == 0) {
						if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1)) {
							if (!mapFlags[neighbourX,neighbourY] && !obstacleMap[neighbourX,neighbourY]) {
								mapFlags[neighbourX,neighbourY] = true;
								queue.Enqueue(new Coord(neighbourX,neighbourY));
								accessibleTileCount ++;
							}
						}
					}
				}
			}
		}
		int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
		return targetAccessibleTileCount == accessibleTileCount;
	}

	public Vector3 CoordToPosition(Coord coord)
	{
		return CoordToPosition(coord.x,coord.y);
	}

	Vector3 CoordToPosition(int x, int y) {
		return new Vector3 (-currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y) * tileSize;
	}

	public Transform GetTileFromPosition(Vector3 position)
	{
		int x = Mathf.RoundToInt((position.x - transform.position.x) / tileSize + (currentMap.mapSize.x - 1) / 2f);
		int y = Mathf.RoundToInt((position.z - transform.position.z) / tileSize + (currentMap.mapSize.y - 1) / 2f);
		x = Mathf.Clamp (x, 0, tileMap.GetLength (0) -1);
		y = Mathf.Clamp (y, 0, tileMap.GetLength (1) -1);
		return tileMap [x, y];
	}

	public int2 GetTileIDFromPosition(Vector3 position)
	{
		int x = Mathf.RoundToInt((position.x - transform.position.x) / tileSize + (currentMap.mapSize.x - 1) / 2f);
		int y = Mathf.RoundToInt((position.z - transform.position.z) / tileSize + (currentMap.mapSize.y - 1) / 2f);
		x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
		y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);
		return new int2(x,y); ;
	}

	public Coord GetRandomCoord() {
		Coord randomCoord = shuffledTileCoords.Dequeue ();
		shuffledTileCoords.Enqueue (randomCoord);
		return randomCoord;
	}

	public Transform GetRandomOpenTile() {
		Coord randomCoord = shuffledOpenTileCoords.Dequeue ();
		shuffledOpenTileCoords.Enqueue (randomCoord);
		return tileMap[randomCoord.x,randomCoord.y];
	}
	
	public void CustomizeMapAndCreate(int width,int depth) {
		maps[3].mapSize=new Coord(width,depth);
		OnNewStage(3);
	}

	public float UpdatePillarStatus(GameObject targetPillar,int toState=1) {
		foreach (Pillar i in PillarList)
		{
			if (i.obj != targetPillar) continue;
			i.state = toState;
			return i.height;
		}
		return 0;
	}

	public bool ChkPillarStatusEmpty(GameObject targetPillar) {
		foreach (Pillar i in PillarList) {
			if (i.obj == null) continue;
			if (i.obj != targetPillar) continue;
			return (i.state == 0);
		}
		return false;
	}

	[System.Serializable]
	public class Map {
		
		public Coord mapSize;
		[Range(0,1)]
		public float obstaclePercent;
		public int seed;
		public float minObstacleHeight;
		public float maxObstacleHeight;
		public Color foregroundColour;
		public Color backgroundColour;
		
		public Coord mapCentre {
			get {
				return new Coord(mapSize.x/2,mapSize.y/2);
			}
		}
		
	}

	public int CurrMapX() {
		return currentMap.mapSize.x;
	}
	public int CurrMapY() {
		return currentMap.mapSize.y;
	}

	public bool GetMapWalkable(int x, int y) {
		if (shuffledOpenTileCoords.Contains(new Coord(x, y))) 
			return true;
		return false;
	}
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
	public Pillar(GameObject obj, int _x, int _y,float height, int state = 0)
	{
		this.obj = obj;
		mapSize.x = _x;
		mapSize.y = _y;
		this.state = state;
		this.height = height;
	}
}