using NugetForUnity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[System.Serializable]
public class NugetConfigModel : ScriptableObject
{
    [SerializeField]
    public NugetConfigFile NuGetConfig;
}

public class NugetConfigEditor : EditorWindow
{
    public static void ShowWindow()
    {
        NugetConfigEditor wnd = GetWindow<NugetConfigEditor>();
        wnd.titleContent = new GUIContent("NuGet Config Properties");
    }
    
    

    public void OnEnable()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;
        
        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/CPM/Scripts/Editor/NugetConfigEditor/NugetConfigEditor.uss");

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/CPM/Scripts/Editor/NugetConfigEditor/NugetConfigEditor.uxml");
        VisualElement labelFromUXML = visualTree.CloneTree();
        root.Add(labelFromUXML);
        
        BindConfig(root);
    }

    private void BindConfig(VisualElement root)
    {
        var model = ScriptableObject.CreateInstance<NugetConfigModel>();
        model.NuGetConfig = NugetHelper.NugetConfigFile;
        
        SerializedObject serializedModel = new SerializedObject(model);
        root.Bind(serializedModel);
    }
}