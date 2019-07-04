using System.Collections.Generic;
using System.Threading.Tasks;
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
    private string m_pendingSearchString = "";
    private Task m_searchTask;

    private PackageView m_packageView;
    private TwoPaneSplitView m_splitView;
    private ConvergeToolbar m_toolbar;

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
        
        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/CPM/Scripts/Editor/ConvergePackageManager.uss");
        root.styleSheets.Add(styleSheet);
        
        BindToolbar();
        BindTwoPaneSplitView();
        BindBrowseList();
        BindPackageView();
        
        m_packageView = rootVisualElement.Query<PackageView>("package-view").First();
    }

    private void BindTwoPaneSplitView()
    {
        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/CPM/Scripts/Editor/ConvergePackageManager.uxml");
        VisualElement twoPaneSplitViewTemplate = visualTree.CloneTree();
        rootVisualElement.Add(twoPaneSplitViewTemplate.ElementAt(0));
    }

    private void BindToolbar()
    {
        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/CPM/Scripts/Editor/UIElements/ConvergeToolbar.uxml");
        VisualElement toolbar = visualTree.CloneTree();
        rootVisualElement.Add(toolbar.ElementAt(0));

        m_toolbar = rootVisualElement.Query<ConvergeToolbar>("converge-toolbar");
        m_toolbar.Initialize();
        m_toolbar.RegisterCallback<KeyUpEvent>(ToolbarKeyUpEvent);
        m_toolbar.BindSearchField((evt =>
        {
            m_pendingSearchString = evt.newValue;
            // TODO: Convert this so that it doesn choke the main thread. Need to rewrite in NuGetHelper.cs WWW in line 1441
            
            // new Task(Search).Start();
        }));
        
    }

    private void ToolbarKeyUpEvent(KeyUpEvent evt)
    {
        if (evt.keyCode == KeyCode.Return)
        {
            Search();
        }
    }
    
    private async void Search()
    {
        Debug.Log("Initiated search");
        var list = NugetHelper.Search(m_pendingSearchString);
        Debug.Log("Finished search");
        foreach (var package in list)
        {
            Debug.Log("found package:" + package.Title);
        }
        
        m_currentlyBrowsing.Clear();
        m_currentlyBrowsing.AddRange(list);
        var listView = rootVisualElement.Query<ListView>("browse-packages-list").First();
        SelectFirstPackageOrShowMessages(listView);
    }

    private void SelectFirstPackageOrShowMessages(ListView listView)
    {
        if (listView.itemsSource.Count > 0)
            listView.selectedIndex = 0;
        listView.Refresh();
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
        //SelectFirstPackageOrShowMessages(listView);
    }

    private void BindPackageView()
    {
        var flexedPane = rootVisualElement.Query<VisualElement>("flexed-pane").First();
        
        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/CPM/Scripts/Editor/UIElements/PackageView.uxml");
        VisualElement packageViewTemplate = visualTree.CloneTree();
        packageViewTemplate.Query<PackageView>().First().Initialize();
        flexedPane.Add(packageViewTemplate);
    }
}