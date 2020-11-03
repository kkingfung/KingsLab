using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

//[CustomEditor(typeof(),true)]
public class CustomEditorInspector : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement container = new VisualElement();

        SerializedProperty it = serializedObject.GetIterator();
        it.Next(true);

        while (it.NextVisible(false))
        {
            PropertyField prop = new PropertyField(it);
            prop.SetEnabled(it.name != "m_Script");
            container.Add(prop);
        }

        return container;
    }
}
