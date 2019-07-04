using NugetForUnity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace CPM.Editor.UIElements
{
    [System.Serializable]
    public class PackageViewModel : ScriptableObject
    {
        
        [SerializeField]
        public NugetPackage Package;
    }
    
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

        public void Initialize()
        {
            this.Query<Button>("package-view--primary-action").First().clickable.clicked += PrimaryActionSelector;
            
        }
        

        public void SetPreviewedPackage(NugetPackage package)
        {
            m_nugetPackage = package;

            var model = ScriptableObject.CreateInstance<PackageViewModel>();
            model.Package = package;
            
            SerializedObject serializedObject = new SerializedObject(model);
            LogPropertyPaths(serializedObject);
            this.Bind(serializedObject);
            
            this.Query<Label>("package-view--title").First().text = package.Title;
            this.Query<Label>("package-view--author-text").First().text = package.Authors;
            this.Query<Label>("package-view--description-text").First().text = package.Description;
            this.Query<Label>("package-view--license-text").First().text = package.LicenseUrl;
            this.Query<VisualElement>("package-view--image").First().style.backgroundImage = package.Icon;

            bool isInstalled = NugetHelper.IsInstalled(package);
            
            this.Query<Button>("package-view--primary-action").First().text = 
                isInstalled
                ? "Remove Package from Project"
                : "Add Package to Project";
        }


        private void PrimaryActionSelector()
        {
            if (NugetHelper.IsInstalled(m_nugetPackage))
            {
                UninstallPackage();
            }
            else
            {
                InstallPackage();
            }
        }
        
        public void InstallPackage()
        {
            NugetHelper.Install(m_nugetPackage);
        }

        public void UninstallPackage()
        {
            NugetHelper.Uninstall(m_nugetPackage);
        }
        
        private void LogPropertyPaths(SerializedObject obj)
        {
            // Loop through properties and create one field (including children) for each top level property.
            SerializedProperty property = obj.GetIterator();
            bool expanded = true;
            while (property.NextVisible(expanded))
            {
                Debug.Log(string.Format("Property: {0}", property.propertyPath));
                expanded = true;
            }
        }
    }
}

