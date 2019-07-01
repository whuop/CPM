using NugetForUnity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace CPM.Editor.UIElements
{
    public class PackageView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<PackageView> {};

        private NugetPackage m_nugetPackage;    
        public PackageView()
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/CPM/Scripts/Editor/UIElements/PackageView.uss");
            styleSheets.Add(styleSheet);

            this.style.flexGrow = 1;
            this.style.paddingBottom = 25;
            this.style.paddingLeft = 20;
            this.style.paddingRight = 10;
            this.style.paddingBottom = 10;
        }

        public void SetPreviewedPackage(NugetPackage package)
        {
            m_nugetPackage = package;

            this.Query<Label>("package-view--title").First().text = package.Title;
            this.Query<Label>("package-view--author-text").First().text = package.Authors;
            this.Query<Label>("package-view--description-text").First().text = package.Description;
        }

        public void InstallPackage()
        {
            
        }

        public void UninstallPackage()
        {
            
        }
    }
}

