using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Node : IHeapItem<Node> {
	
	public bool walkable;
	public Vector3 worldPosition;
	public int gridX;
	public int gridY;
	public int gridZ;
	public int movementPenalty;

	public int gCost;
	public int hCost;
	public Node parent;
	public List<Node> neighbours;
	int heapIndex;

	public Node()
	{
		neighbours = new List<Node>();
	}
	public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY, int _gridZ, int _penalty) {
		walkable = _walkable;
		worldPosition = _worldPos;
		gridX = _gridX;
		gridY = _gridY;
		gridZ = _gridZ;
		movementPenalty = _penalty;
		neighbours = new List<Node>();
	}

	public int fCost {
		get {
			return gCost + hCost;
		}
	}

	public int HeapIndex {
		get {
			return heapIndex;
		}
		set {
			heapIndex = value;
		}
	}

	public int CompareTo(Node nodeToCompare) {
		int compare = fCost.CompareTo(nodeToCompare.fCost);
		if (compare == 0) {
			compare = hCost.CompareTo(nodeToCompare.hCost);
		}
		return -compare;
	}
}
