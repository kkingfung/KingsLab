using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using RandomTowerDefense.Tools;

/// <summary>
/// CustomGradientのPropertyDrawer
/// </summary>
[CustomPropertyDrawer(typeof(CustomGradient))]
public class GradientDrawer : PropertyDrawer
{
    /// <summary>
    /// GUIの描画
    /// </summary>
    /// <param name="position">描画位置</param>
    /// <param name="property">SerializedProperty</param>
    /// <param name="label">ラベル</param>
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Event guiEvent = Event.current;

        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        CustomGradient gradient = GetDataObject(property) as CustomGradient;
        if (gradient == null)
        {
            return;
        }
        Rect textureRect = new Rect(position.x, position.y, position.width, position.height);

        if (guiEvent.type == EventType.Repaint)
        {
            GUIStyle gradientStyle = new GUIStyle();
            gradientStyle.normal.background = gradient.GetTexture((int)position.width);
            GUI.Label(textureRect, GUIContent.none, gradientStyle);

        }
        else
        {
            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
            {
                if (textureRect.Contains(guiEvent.mousePosition))
                {
                    GradientEditor window = EditorWindow.GetWindow<GradientEditor>();
                    window.SetGradient(gradient);
                }
            }

        }

        EditorGUI.EndProperty();
    }

    /// <summary>
    /// データオブジェクトを取得する
    /// </summary>
    /// <param name="prop">SerializedProperty</param>
    /// <returns>データオブジェクト</returns>
    public object GetDataObject(SerializedProperty prop)
    {
        var path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;

        var elements = path.Split('.');
        foreach (var element in elements)
        {
            if (element.Contains("["))
            {
                var elementName = element.Substring(0, element.IndexOf("["));
                var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                obj = GetValue(obj, elementName, index);

            }
            else
            {
                obj = GetValue(obj, element);
            }
        }
        return obj;
    }

    /// <summary>
    /// 値を取得する
    /// </summary>
    /// <param name="source">ソースオブジェクト</param>
    /// <param name="name">フィールド名またはプロパティ名</param>
    /// <returns>値</returns>
    public object GetValue(object source, string name)
    {
        if (source == null)
            return null;
        var type = source.GetType();
        var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (f == null)
        {
            var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p == null)
                return null;
            return p.GetValue(source, null);
        }
        return f.GetValue(source);
    }

    public object GetValue(object source, string name, int index)
    {
        var enumerable = GetValue(source, name) as IEnumerable;
        var enemy = enumerable.GetEnumerator();
        try
        {
            while (index-- >= 0)
                enemy.MoveNext();
            return enemy.Current;
        }
        catch
        {
            return null;
        }
    }
}
