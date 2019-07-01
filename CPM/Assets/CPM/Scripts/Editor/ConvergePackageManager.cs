using Boo.Lang;
using CPM.Editor.UIElements;
using NugetForUnity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class ConvergePackageManager : EditorWindow
{
    [MenuItem("Window/UIElements/ConvergePackageManager")]
    public static void ShowExample()
    {
        ConvergePackageManager wnd = GetWindow<ConvergePackageManager>();
        wnd.titleContent = new GUIContent("ConvergePackageManager");
    }
    
    private List<NugetPackage> m_currentlyBrowsing = new List<NugetPackage>();

    private PackageView m_packageView;

    public void OnEnable()
    {
        var packages = NugetHelper.Search();

        foreach (var package in packages)
        {
            Debug.Log("Found Package: " + package.Title);
            m_currentlyBrowsing.Add(package);
        }
        
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/CPM/Scripts/Editor/ConvergePackageManager.uxml");
        VisualElement twoPaneSplitViewTemplate = visualTree.CloneTree();
        root.Add(twoPaneSplitViewTemplate.ElementAt(0));

        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/CPM/Scripts/Editor/ConvergePackageManager.uss");
        root.styleSheets.Add(styleSheet);
        
        BindBrowseList();
        BindPackageView();

        m_packageView = rootVisualElement.Query<PackageView>("package-view").First();
    }

    private void BindBrowseList()
    {
        var listView = rootVisualElement.Query<ListView>("browse-packages-list").First();
        listView.itemHeight = 30;
        listView.itemsSource = m_currentlyBrowsing;
        listView.makeItem += () =>
        {
            var box = new VisualElement();
            box.style.flexDirection = FlexDirection.Row;
            box.style.flexGrow = 1.0f;
            box.style.flexShrink = 1f;
            box.style.flexBasis = 0f;
            box.style.borderTopWidth = 2;
            box.style.borderBottomWidth = 2;
        
            var label = new Label("PlaceHolder");
            label.style.flexGrow = 0;
            label.style.flexShrink = 1;
            label.style.overflow = Overflow.Hidden;
            box.Add(label);

            //box.Add(new Button() { text = "Install"});
            return box;
        };

        listView.bindItem += (element, i) =>
        {
            (element.ElementAt(0) as Label).text = m_currentlyBrowsing[i].Title;
        };

        listView.onItemChosen += o =>
        {
            Debug.Log("Chose Item: " + o.GetType());
        };

        listView.onSelectionChanged += list =>
        {
            Debug.Log(("Selection Changed: " + list.GetType()));
            m_packageView.SetPreviewedPackage(list[0] as NugetPackage);
        };
    }

    private void BindPackageView()
    {
        var flexedPane = rootVisualElement.Query<VisualElement>("flexed-pane").First();
        
        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/CPM/Scripts/Editor/UIElements/PackageView.uxml");
        VisualElement packageViewTemplate = visualTree.CloneTree();
        flexedPane.Add(packageViewTemplate);
    }
}