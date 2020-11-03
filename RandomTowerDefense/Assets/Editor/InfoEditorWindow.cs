//using UnityEditor;
//using UnityEngine;
//using UnityEditor.UIElements;
//using UnityEngine.UIElements;

//public class InfoEditorWindow : EditorWindow
//{
//    [MenuItem("Tools/InfoEditorWindow")]
//    public static void ShowWindow()
//    {
//        var window = GetWindow<InfoEditorWindow>();
//        window.titleContent = new GUIContent("Info Editor");
//        window.minSize = new Vector2(800,600);
//    }

//    private void OnEnable()
//    {
//        VisualTreeAsset original = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/InfoEditorWindow.uxml");
//        TemplateContainer treeAsset = original.CloneTree();
//        rootVisualElement.Add(treeAsset);

//        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/InfoEditorStyle.uss");
//        rootVisualElement.styleSheets.Add(styleSheet);

//        CreateListView();
//    }


//    private void CreateListView()
//    {
//        //Get Info
//        FindAllInfos(out Data[] infos);


//        ListView list = rootVisualElement.Query<ListView>("info-list").First();
//        list.makeItem=()=>new Label();
//        list.bindItem=(element,i)=>(element as Label).text = infos[i].name;

//        list.itemsSource = null;
//        list.itemHeight = 16;
//        list.selectionType = SelectionType.Single;

//        list.onSelectionChange += (enumerable) =>
//        {
//            foreach (object it in enumerable)
//            {
//                Box infoBox = rootVisualElement.Query<Box>("detail-info").First();
//                infoBox.Clear();

//                //Data
//                Data info = it as Data;

//                SerializedObject serializedInfo = new SerializedObject(info);
//                SerializedProperty property = serializedInfo.GetIterator();
//                property.Next(true);

//                while (property.NextVisible(false))
//                {
//                    PropertyField prop = new PropertyField(property);

//                    prop.SetEnabled(property.name != "m_Script");
//                    prop.Bind(serializedInfo);
//                    infoBox.Add(prop);

//                    if (property.name == "")
//                    {
//                        prop.RegisterCallback<ChangeEvent<UnityEngine.Object>>((changeEvt) => LoadImage());
//                    }
//                }

//                //ChangeTexture
//                LoadImage();
//            }
//        };

//        list.Refresh();
//    }

//    private void FindAllInfos(out Data[] infos)
//    {
//        var guids = AssetDatabase.FindAssets("");

//        infos = new Data[guids.Length];

//        for (int i = 0; i < guids.Length; i++)
//        {
//            var path = AssetDatabase.GUIDToAssetPath(guids[i]);
//            infos[i] = AssetDatabase.LoadAssetAtPath<Data>(path);
//        }
//    }

//    private void LoadImage(Texture texture)
//    {
//        var PreviewImage = rootVisualElement.Query<Image>("preview").First();
//        PreviewImage.image = texture;
//    }
//}
