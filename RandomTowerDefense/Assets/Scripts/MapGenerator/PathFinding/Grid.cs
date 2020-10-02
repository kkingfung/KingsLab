using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{
	public GameObject model = null;
	Dictionary<Vector3Int, Node> edgeDictionary;

	public bool displayGridGizmos;
	public LayerMask unwalkableMask;
	public Vector3 gridWorldSize;
	public float nodeRadius;
	public TerrainType[] walkableRegions;
	public int obstacleProximityPenalty = 10;
	Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
	LayerMask walkableMask;

	Node[,,] grid;

	float nodeDiameter;
	int gridSizeX, gridSizeY, gridSizeZ;

	int penaltyMin = int.MaxValue;
	int penaltyMax = int.MinValue;

	void Awake()
	{
		nodeDiameter = nodeRadius * 2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
		gridSizeZ = Mathf.RoundToInt(gridWorldSize.z / nodeDiameter);
		foreach (TerrainType region in walkableRegions)
		{
			walkableMask.value |= region.terrainMask.value;
			walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.terrainPenalty);
		}

		if (model == null) CreateGrid();
		else CreateModifiedGrid();
	}

	public int MaxSize
	{
		get
		{
			return gridSizeX * gridSizeY * gridSizeZ;
		}
	}

	#region CreateModifiedGrid
	void CreateModifiedGrid()
	{
		MeshFilter temp = model.GetComponent<MeshFilter>();
		List<Vector3> normal = new List<Vector3>();
		temp.sharedMesh.GetNormals(normal);
		List<Vector3> vertice = new List<Vector3>();
		temp.sharedMesh.GetVertices(vertice);
		int size = vertice.Count;
		grid = new Node[size, 1,1];
		for (int i = 0; i < size; i++)
		{
            Vector3 worldPosition = model.transform.position + new Vector3(vertice[i].x*temp.transform.localScale.x, 
				vertice[i].y * temp.transform.localScale.y, vertice[i].z * temp.transform.localScale.z)
				+ nodeRadius * new Vector3(normal[i].x, normal[i].y, normal[i].z);
            bool walkable = !(Physics.CheckSphere(worldPosition, nodeRadius, unwalkableMask));
            int movementPenalty = 0;

            Ray ray = new Ray(worldPosition + new Vector3(normal[i].x, normal[i].y, normal[i].z) * 50, -1.0f * new Vector3(normal[i].x, normal[i].y, normal[i].z));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, walkableMask))
            {
                walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
            }

            if (!walkable)
            {
                movementPenalty += obstacleProximityPenalty;
            }
            grid[i,0,0] = new Node(walkable, worldPosition,i,0,0, movementPenalty);
        }
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				if (i!=j && grid[j, 0, 0].walkable)
				{
					if((grid[j, 0, 0].worldPosition- grid[i, 0, 0].worldPosition).sqrMagnitude< nodeRadius*10)
					grid[i, 0, 0].neighbours.Add(grid[j, 0, 0]);
				}
			}
		}
	}
#endregion

    #region CreateCubeGrid
    void CreateGrid()
	{
		grid = new Node[gridSizeX, gridSizeY, gridSizeZ];
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

		for (int z = 0; z < gridSizeZ; z++)
		{
			for (int x = 0; x < gridSizeX; x++)
			{
				for (int y = 0; y < gridSizeY; y++)
				{
					Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius) + Vector3.up * (z * nodeDiameter);
					bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));

					int movementPenalty = 0;

					Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
					RaycastHit hit;
					if (Physics.Raycast(ray, out hit, 100, walkableMask))
					{
						walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
					}

					if (!walkable)
					{
						movementPenalty += obstacleProximityPenalty;
					}

					grid[x, y, z] = new Node(walkable, worldPoint, x, y, z, movementPenalty);
				}
			}
		}
		for (int z = 0; z < gridSizeZ; z++)
		{
			for (int x = 0; x < gridSizeX; x++)
			{
				for (int y = 0; y < gridSizeY; y++)
				{
					AddNeighbours(grid[x, y, z]);
				}
			}
		}
		BlurPenaltyMap(3);

	}

	void BlurPenaltyMap(int blurSize)
	{
		int kernelSize = blurSize * 2 + 1;
		int kernelExtents = (kernelSize - 1) / 2;

		int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeY];
		int[,] penaltiesVerticalPass = new int[gridSizeX, gridSizeY];
		for (int z = 0; z < gridSizeZ; z++)
		{
			for (int y = 0; y < gridSizeY; y++)
			{
				for (int x = -kernelExtents; x <= kernelExtents; x++)
				{
					int sampleX = Mathf.Clamp(x, 0, kernelExtents);
					penaltiesHorizontalPass[0, y] += grid[sampleX, y, z].movementPenalty;
				}

				for (int x = 1; x < gridSizeX; x++)
				{
					int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, gridSizeX);
					int addIndex = Mathf.Clamp(x + kernelExtents, 0, gridSizeX - 1);

					penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - grid[removeIndex, y, z].movementPenalty + grid[addIndex, y, z].movementPenalty;
				}
			}
		}
		for (int z = 0; z < gridSizeZ; z++)
		{
			for (int x = 0; x < gridSizeX; x++)
			{
				for (int y = -kernelExtents; y <= kernelExtents; y++)
				{
					int sampleY = Mathf.Clamp(y, 0, kernelExtents);
					penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
				}

				int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));
				grid[x, 0, z].movementPenalty = blurredPenalty;

				for (int y = 1; y < gridSizeY; y++)
				{
					int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, gridSizeY);
					int addIndex = Mathf.Clamp(y + kernelExtents, 0, gridSizeY - 1);

					penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
					blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
					grid[x, y, z].movementPenalty = blurredPenalty;

					if (blurredPenalty > penaltyMax)
					{
						penaltyMax = blurredPenalty;
					}
					if (blurredPenalty < penaltyMin)
					{
						penaltyMin = blurredPenalty;
					}
				}
			}
		}

	}

	public void AddNeighbours(Node node)
	{
		for (int z = -1; z <= 1; z++)
		{
			for (int x = -1; x <= 1; x++)
			{
				for (int y = -1; y <= 1; y++)
				{
					if (x == 0 && y == 0)
						continue;

					int checkX = node.gridX + x;
					int checkY = node.gridY + y;
					int checkZ = node.gridZ + z;

					if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY && checkZ >= 0 && checkZ < gridSizeZ)
					{
						node.neighbours.Add(grid[checkX, checkY, checkZ]);
					}
				}
			}
		}
	}
    #endregion

    public Node NodeFromWorldPoint(Vector3 worldPosition)
	{
		float closest = float.MaxValue;
		Node closestNode = null;
		for (int z = 0; z < grid.GetLength(2); z++)
		{
			for (int y = 0; y < grid.GetLength(1); y++)
			{
				for (int x = 0; x < grid.GetLength(0); x++)
				{
					var n = grid[x, y,z];
					float sqrDst = (worldPosition - n.worldPosition).sqrMagnitude;
					if (sqrDst < closest)
					{
						closest = sqrDst;
						closestNode = n;
					}
				}
			}
		}
		return closestNode;
		//float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
		//float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
		//float percentZ = (worldPosition.y + gridWorldSize.z / 2) / gridWorldSize.z;
		//percentX = Mathf.Clamp01(percentX);
		//percentY = Mathf.Clamp01(percentY);
		//percentZ = Mathf.Clamp01(percentZ);

		//int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
		//int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
		//int z = Mathf.RoundToInt((gridSizeZ - 1) * percentZ);
		//return grid[x, y, z];
	}

	void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(transform.position+ Vector3.up* nodeRadius * Mathf.RoundToInt(gridWorldSize.z / (nodeRadius * 2)),
			new Vector3(gridWorldSize.x, gridWorldSize.z, gridWorldSize.y));
		if (grid != null && displayGridGizmos)
		{
			foreach (Node n in grid)
			{

				Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, n.movementPenalty));
				Gizmos.color = (n.walkable) ? Gizmos.color : Color.red;
				Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter));
			}
		}
	}

	[System.Serializable]
	public class TerrainType
	{
		public LayerMask terrainMask;
		public int terrainPenalty;
	}


}