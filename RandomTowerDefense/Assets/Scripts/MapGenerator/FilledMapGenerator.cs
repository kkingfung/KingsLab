using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FilledMapGenerator : MonoBehaviour {
	private Coord[] FixedFreeSpace;

	public Map[] maps;
	public int mapIndex;
	public bool Randomize;
	public Transform tilePrefab;
	public Transform obstaclePrefab;
	public Transform mapFloor;
	public Transform navmeshFloor;
	public Transform navmeshMaskPrefab;
	public Vector2 maxMapSize;

	public GameObject FakeParentCam;
	private Vector3 RelativePosition;
	private bool FollowFakeParent=false;

	[Range(0,1)]
	public float outlinePercent;
	
	public float tileSize;
	List<Coord> allTileCoords;
	Queue<Coord> shuffledTileCoords;
	Queue<Coord> shuffledOpenTileCoords;
	Transform[,] tileMap;
	List<Pillar> PillarList;

	Map currentMap;

	private void Start() {
		PillarList = new List<Pillar>();
	}
	public void followParentCam(bool toFollow) {
		FollowFakeParent = toFollow;
		RelativePosition = this.transform.position - FakeParentCam.transform.position;
	}
	private void Update()
	{
		if (FollowFakeParent) {
			this.transform.position = RelativePosition + FakeParentCam.transform.position;
		}
	}

	public void OnNewStage(int stageNumber) {
		mapIndex = stageNumber;
		GenerateMap ();
	}

	public void GenerateMap() {
		currentMap = maps[mapIndex];

		if (Randomize)
			currentMap.seed = Random.Range(int.MinValue, int.MaxValue);
		tileMap = new Transform[currentMap.mapSize.x,currentMap.mapSize.y];
		System.Random prng = new System.Random (currentMap.seed);

		// Generating coords
		allTileCoords = new List<Coord> ();
		for (int x = 0; x < currentMap.mapSize.x; x ++) {
			for (int y = 0; y < currentMap.mapSize.y; y ++) {
				allTileCoords.Add(new Coord(x,y));
			}
		}

		//Space for Castle and TP
		StageManager stageManager = FindObjectOfType<StageManager>();
		if (stageManager.SpawnPoint.Length >= 4)
		{
			stageManager.SpawnPoint[0] = new Coord(0, 0);
			stageManager.SpawnPoint[1] = new Coord(currentMap.mapSize.x - 1, 0);
			stageManager.SpawnPoint[2] = new Coord(0, currentMap.mapSize.y - 1);
			stageManager.SpawnPoint[3] = new Coord(currentMap.mapSize.x - 1, currentMap.mapSize.y - 1);

			//Remove for Confirmed Open Space
			foreach (Coord i in stageManager.SpawnPoint)
				allTileCoords.Remove(i);

			allTileCoords.Remove(new Coord(0, 1));
			allTileCoords.Remove(new Coord(1, 1));
			allTileCoords.Remove(new Coord(1, 0));
		}

		shuffledTileCoords = new Queue<Coord> (Utility.ShuffleArray (allTileCoords.ToArray (), currentMap.seed));

		// Create map holder object
		string holderName = "Generated Map";
		if (transform.Find (holderName)) {
			DestroyImmediate (transform.Find (holderName).gameObject);
		}
		
		Transform mapHolder = new GameObject (holderName).transform;
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

		// Creating navmesh mask
		Transform maskLeft = Instantiate (navmeshMaskPrefab, this.transform.position + Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
		maskLeft.parent = mapHolder;
		maskLeft.localScale = new Vector3 ((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;
		
		Transform maskRight = Instantiate (navmeshMaskPrefab, this.transform.position + Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
		maskRight.parent = mapHolder;
		maskRight.localScale = new Vector3 ((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;
		
		Transform maskTop = Instantiate (navmeshMaskPrefab, this.transform.position + Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
		maskTop.parent = mapHolder;
		maskTop.localScale = new Vector3 (maxMapSize.x, 1, (maxMapSize.y-currentMap.mapSize.y)/2f) * tileSize;
	
		Transform maskBottom = Instantiate (navmeshMaskPrefab, this.transform.position + Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
		maskBottom.parent = mapHolder;
		maskBottom.localScale = new Vector3 (maxMapSize.x, 1, (maxMapSize.y-currentMap.mapSize.y)/2f) * tileSize;
		
		navmeshFloor.localScale = new Vector3 (maxMapSize.x, maxMapSize.y) * tileSize;
		mapFloor.localScale =  new Vector3 (currentMap.mapSize.x * tileSize, currentMap.mapSize.y * tileSize);
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
					if (x == 0 || y == 0) {
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

	public Transform GetTileFromPosition(Vector3 position) {
		int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
		int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1) / 2f);
		x = Mathf.Clamp (x, 0, tileMap.GetLength (0) -1);
		y = Mathf.Clamp (y, 0, tileMap.GetLength (1) -1);
		return tileMap [x, y];
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

	public float UpdatePillarStatus(GameObject targetPillar) {
		foreach (Pillar i in PillarList)
		{
			if (i.obj != targetPillar) continue;
			i.state = 1;
			return i.height;
		}
		return 0;
	}

	public bool ChkPillarStatusEmpty(GameObject targetPillar) {
		Debug.Log(0);
		foreach (Pillar i in PillarList) {
			Debug.Log(1);
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
	public Pillar(GameObject obj, int _x, int _y,float height, int state = 0)
	{
		this.obj = obj;
		mapSize.x = _x;
		mapSize.y = _y;
		this.state = state;
		this.height = height;
	}
}