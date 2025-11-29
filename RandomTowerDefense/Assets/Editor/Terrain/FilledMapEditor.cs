using UnityEngine;
using System.Collections;
using UnityEditor;
using RandomTowerDefense.MapGenerator;

/// <summary>
/// FilledMapGeneratorのカスタムエディタ
/// </summary>
[CustomEditor(typeof(FilledMapGenerator))]
public class FilledMapEditor : Editor
{
    /// <summary>
    /// インスペクタのGUI描画
    /// </summary>
    public override void OnInspectorGUI()
    {
        FilledMapGenerator map = target as FilledMapGenerator;

        if (DrawDefaultInspector())
        {
            map.GenerateMap();
        }

        if (GUILayout.Button("Generate Map"))
        {
            map.GenerateMap();
        }
    }
}
