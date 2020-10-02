using System.Net;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Pathfinding))]
public class PosSetEditor:Editor {
    void OnSceneGUI () {
        var pathfinder = (Pathfinding)target;
        if (Event.current.shift && !Event.current.alt)
        {
            Vector2 mouse = Event.current.mousePosition;
            mouse = new Vector2(mouse.x, Camera.current.pixelHeight - mouse.y);
            Ray ray = Camera.current.ScreenPointToRay(mouse);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, pathfinder.unwalkableMask))
            {
                var t = pathfinder.testA;
                    t.transform.position = hit.point;
                    t.transform.up = hit.normal.normalized;
            }
        }
        if (Event.current.control && !Event.current.alt)
        {
            Vector2 mouse = Event.current.mousePosition;
            mouse = new Vector2(mouse.x, Camera.current.pixelHeight - mouse.y);
            Ray ray = Camera.current.ScreenPointToRay(mouse);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit,pathfinder.unwalkableMask))
            {
                var t = pathfinder.testB;
                    t.transform.position = hit.point;
                    t.transform.up = hit.normal.normalized;
            }
        }
    }
}