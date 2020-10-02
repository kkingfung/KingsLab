using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor (typeof (FilledMapGenerator))]
public class FilledMapEditor : Editor {

	public override void OnInspectorGUI ()
	{

		FilledMapGenerator map = target as FilledMapGenerator;

		if (DrawDefaultInspector ()) {
			map.GenerateMap ();
		}

		if (GUILayout.Button("Generate Map")) {
			map.GenerateMap ();
		}


	}
	
}
