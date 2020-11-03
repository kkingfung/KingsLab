using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(DisabledFloatAttribute))]
public class DisabledPropertyDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        FloatField floatField = new FloatField(property.displayName)
        {
            value = property.floatValue,
        };

        floatField.SetEnabled(false);

        return floatField;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.FloatField(position, label, property.floatValue);
        GUI.enabled = true;
    }
}
