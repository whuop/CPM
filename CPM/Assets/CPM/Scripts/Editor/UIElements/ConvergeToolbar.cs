using NugetForUnity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


namespace CPM.Editor.UIElements
{
    public class ConvergeToolbar : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ConvergeToolbar> { }

        public delegate void OnSearchDelegate(ChangeEvent<string> evt);
        
        public OnSearchDelegate OnSearchForPackages { get; set; }
        
        private ToolbarPopupSearchField m_searchField;
        
        private static string TargetFrameworkText = "Target Framework: ";
        
        public ConvergeToolbar()
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/CPM/Scripts/Editor/UIElements/ConvergeToolbar.uss");
            styleSheets.Add(styleSheet);
        }
        
        
        public void Initialize()
        {
            var toolbarMenu = this.Query<ToolbarMenu>("converge-toolbar--target-framework").First();
            toolbarMenu.menu.AppendAction(".Net 4.6", action => { toolbarMenu.text = TargetFrameworkText + ".Net 4.6"; });
            toolbarMenu.menu.AppendAction(".Net 2.0", action => { toolbarMenu.text = TargetFrameworkText + ".Net 2.0";});
            toolbarMenu.menu.AppendAction(".Net 2.0 Subset", action => { toolbarMenu.text = TargetFrameworkText + ".Net 2.0 Subset";});
            
            var toolbarNugetConfig = this.Query<ToolbarButton>("converge--toolbar-nuget-config").First();

            toolbarNugetConfig.clickable.clicked += () => { NugetConfigEditor.ShowWindow(); };
        }

        public void BindSearchField(OnSearchDelegate searchCallback)
        {
            m_searchField = this.Query<ToolbarPopupSearchField>("converge-toolbar--search-field");
            m_searchField.tooltip = "Search by NuGet package name";
            m_searchField.RegisterValueChangedCallback(SearchForPackages);
            OnSearchForPackages = searchCallback;
        }

        public void SearchForPackages(ChangeEvent<string> evt)
        {
            OnSearchForPackages?.Invoke(evt);
        }
    }    
}

