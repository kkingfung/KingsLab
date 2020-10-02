using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Line {

	const float verticalLineGradient = 1e5f;

	float gradient;
	float y_intercept;
	Vector3 pointOnLine_1;
	Vector3 pointOnLine_2;

	float gradientPerpendicular;

	bool approachSide;

	public Line(Vector3 pointOnLine, Vector3 pointPerpendicularToLine) {
		float dx = pointOnLine.x - pointPerpendicularToLine.x;
		float dy = pointOnLine.z - pointPerpendicularToLine.z;

		if (dx == 0) {
			gradientPerpendicular = verticalLineGradient;
		} else {
			gradientPerpendicular = dy / dx;
		}

		if (gradientPerpendicular == 0) {
			gradient = verticalLineGradient;
		} else {
			gradient = -1 / gradientPerpendicular;
		}

		y_intercept = pointOnLine.z - gradient * pointOnLine.x;
		pointOnLine_1 = pointOnLine;
		pointOnLine_2 = pointOnLine + new Vector3 (1,0, gradient);

		approachSide = false;
		approachSide = GetSide (pointPerpendicularToLine);
	}

	bool GetSide(Vector3 p) {
		return (p.x - pointOnLine_1.x) * (pointOnLine_2.z - pointOnLine_1.z) > (p.z - pointOnLine_1.z) * (pointOnLine_2.x - pointOnLine_1.x);
	}

	public bool HasCrossedLine(Vector3 p) {
		return GetSide (p) != approachSide;
	}

	public float DistanceFromPoint(Vector3 p) {
		float yInterceptPerpendicular = p.z - gradientPerpendicular * p.x;
		float intersectX = (yInterceptPerpendicular - y_intercept) / (gradient - gradientPerpendicular);
		float intersectY = gradient * intersectX + y_intercept;
		return Vector3.Distance (p, new Vector3 (intersectX, p.y,intersectY));
	}

	public void DrawWithGizmos(float length) {
        Vector3 lineDir = new Vector3(1, 0, gradient).normalized;
        Vector3 lineCentre = new Vector3(pointOnLine_1.x, 0, pointOnLine_1.z) + Vector3.up * pointOnLine_1.y;
        Gizmos.DrawLine(lineCentre - lineDir * length / 2f, lineCentre + lineDir * length / 2f);
    }

}
