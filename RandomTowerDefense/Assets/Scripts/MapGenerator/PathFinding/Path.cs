using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path {

	public readonly bool showPath;
	public readonly Vector3[] lookPoints;
	public readonly Line[] turnBoundaries;
	public readonly int finishLineIndex;
	public readonly int slowDownIndex;

	public Path(Vector3[] waypoints, Vector3 startPos, float turnDst, float stoppingDst, bool show= false) {
		lookPoints = waypoints;
		turnBoundaries = new Line[lookPoints.Length];
		finishLineIndex = turnBoundaries.Length - 1;
		showPath = show;
		Vector3 previousPoint = startPos;
		for (int i = 0; i < lookPoints.Length; i++) {
			Vector3 currentPoint = lookPoints [i];
			Vector3 dirToCurrentPoint = (currentPoint - previousPoint).normalized;
			Vector3 turnBoundaryPoint = (i == finishLineIndex)?currentPoint : currentPoint - dirToCurrentPoint * turnDst;
			turnBoundaries [i] = new Line (turnBoundaryPoint, previousPoint - dirToCurrentPoint * turnDst);
			previousPoint = turnBoundaryPoint;
		}

		float dstFromEndPoint = 0;
		for (int i = lookPoints.Length - 1; i > 0; i--) {
			dstFromEndPoint += Vector3.Distance (lookPoints [i], lookPoints [i - 1]);
			if (dstFromEndPoint > stoppingDst) {
				slowDownIndex = i;
				break;
			}
		}
	}

	Vector2 V3ToV2(Vector3 v3) {
		return new Vector2 (v3.x, v3.z);
	}

	public void DrawWithGizmos() {
		if (!showPath) return;
		Gizmos.color = Color.black;
		foreach (Vector3 p in lookPoints) {
			Gizmos.DrawCube (p + Vector3.up, Vector3.one);
		}

		Gizmos.color = Color.white;
		foreach (Line l in turnBoundaries) {
			l.DrawWithGizmos (10);
		}

	}

}
