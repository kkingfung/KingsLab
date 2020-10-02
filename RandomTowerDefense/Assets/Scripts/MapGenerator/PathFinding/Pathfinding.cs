using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

public class Pathfinding : MonoBehaviour {

	[Header("Pos settings")]
	public LayerMask unwalkableMask;
	public GameObject testA;
	public GameObject testB;

	Grid grid;
	void Awake() {
		grid = GetComponent<Grid>();
	}
	

	public void FindPath(PathRequest request, Action<PathResult> callback) {
		
		//Stopwatch sw = new Stopwatch();
		//sw.Start();
		
		Vector3[] waypoints = new Vector3[0];
		bool pathSuccess = false;
		
		Node startNode = grid.NodeFromWorldPoint(request.pathStart);
		Node targetNode = grid.NodeFromWorldPoint(request.pathEnd);
		startNode.parent = startNode;
		
		
		if (startNode.walkable && targetNode.walkable) {
			Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
			HashSet<Node> closedSet = new HashSet<Node>();
			openSet.Add(startNode);
			
			while (openSet.Count > 0) {
				Node currentNode = openSet.RemoveFirst();
				closedSet.Add(currentNode);
				
				if (currentNode == targetNode) {
					//sw.Stop();
					//print ("Path found: " + sw.ElapsedMilliseconds + " ms");
					pathSuccess = true;
					break;
				}

				foreach (Node neighbour in currentNode.neighbours)
				{
					if (!neighbour.walkable || closedSet.Contains(neighbour))
					{
						continue;
					}

						int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) + neighbour.movementPenalty;
						if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
						{
							neighbour.gCost = newMovementCostToNeighbour;
							neighbour.hCost = GetDistance(neighbour, targetNode);
							neighbour.parent = currentNode;

							if (!openSet.Contains(neighbour))
								openSet.Add(neighbour);
							else
								openSet.UpdateItem(neighbour);
						}
				}
			}
		}
		if (pathSuccess) {
			waypoints = RetracePath(startNode,targetNode);
			pathSuccess = waypoints.Length > 0;
		}
		callback (new PathResult (waypoints, pathSuccess, request.callback));
		
	}
		
	
	Vector3[] RetracePath(Node startNode, Node endNode) {
		List<Node> path = new List<Node>();
		Node currentNode = endNode;
		
		while (currentNode != startNode) {
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}
		//UnityEngine.Debug.Log(path.Count);
		Vector3[] waypoints = SimplifyPath(path);
		Array.Reverse(waypoints);
		return waypoints;
		
	}
	
	Vector3[] SimplifyPath(List<Node> path) {
		List<Vector3> waypoints = new List<Vector3>();
		Vector3 directionOld = Vector3.zero;
		
		for (int i = 1; i < path.Count; i ++) {
			Vector3 directionNew = new Vector3(path[i-1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY, path[i - 1].gridZ - path[i].gridZ);
			if (directionNew != directionOld) {
				waypoints.Add(path[i].worldPosition);
			}
			directionOld = directionNew;
		}
		return waypoints.ToArray();
	}
	
	int GetDistance(Node nodeA, Node nodeB) {
        return (int)(nodeA.worldPosition - nodeB.worldPosition).magnitude;
  //      int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		//int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
		//int dstZ = Mathf.Abs(nodeA.gridZ - nodeB.gridZ);
		//if (dstX > dstY)
		//	return 14 * dstY + 10 * (dstX - dstY) + 0 * dstZ;
		//return 14*dstX + 10 * (dstY-dstX) + 0 * dstZ;
	}
	
	
}
