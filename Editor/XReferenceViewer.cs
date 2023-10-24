using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace XReferenceViewer.Editor
{
    public partial class XReferenceViewer : EditorWindow
    {
        static XReferenceViewer()
        {
            EditorApplication.projectWindowChanged += OnProjectChanged;
        }

        static void OnProjectChanged()
        {
            //wtodo:资源变化了
            Debug.Log("资源变化了");
        }

        private static List<Object> TargetObjects = new List<Object>();

        [MenuItem("Assets/XReferenceViewer", false, 0)]
        static void Open()
        {
            TargetObjects.Clear();

            var valid = false;
            //wtodo:需要排除package路径
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                TargetObjects.Add(obj);
            }

            var window = GetWindow<XReferenceViewer>();
            window.InitUI();
        }

        [MenuItem("Assets/XReferenceViewer", true)]
        static bool OpenValidate()
        {
            TargetObjects.Clear();

            var valid = false;
            //wtodo:需要排除package路径
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                TargetObjects.Add(obj);
                var path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path))
                    continue;
                if (File.Exists(path))
                {
                    valid = true;
                    break;
                }
                else if (Directory.Exists(path))
                {
                    valid = true;
                    break;
                }
            }

            return valid;
        }

        [SettingsProvider]
        static SettingsProvider CreateSettingProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Settings window for the Project scope.
            var provider = new SettingsProvider("Project/XReferenceViewer", SettingsScope.Project)
            {
                label = "XReferenceViewer",
                // activateHandler is called when the user clicks on the Settings item in the Settings window.
                activateHandler = (searchContext, rootElement) =>
                {
                    // var settings = MyCustomSettings.GetSerializedSettings();
                    //
                    // // rootElement is a VisualElement. If you add any children to it, the OnGUI function
                    // // isn't called because the SettingsProvider uses the UIElements drawing framework.
                    // var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/settings_ui.uss");
                    // rootElement.styleSheets.Add(styleSheet);
                    // var title = new Label()
                    // {
                    //     text = "Custom UI Elements"
                    // };
                    // title.AddToClassList("title");
                    // rootElement.Add(title);
                    //
                    // var properties = new VisualElement()
                    // {
                    //     style =
                    //     {
                    //         flexDirection = FlexDirection.Column
                    //     }
                    // };
                    // properties.AddToClassList("property-list");
                    // rootElement.Add(properties);
                    //
                    // properties.Add(new PropertyField(settings.FindProperty("m_SomeString")));
                    // properties.Add(new PropertyField(settings.FindProperty("m_Number")));
                    //
                    // rootElement.Bind(settings);
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] {"Number", "Some String"})
            };

            return provider;
        }


        private NodeGraphView graphView;
        private List<XReferenceViewerNode> SourceNodes = new List<XReferenceViewerNode>();
        private List<XReferenceViewerNode> DependentNodes = new List<XReferenceViewerNode>();
        private List<Edge> Edges = new List<Edge>();

        private OwnerNode ownerNode;

        void InitUI()
        {
            if (graphView == null)
            {
                graphView = new NodeGraphView()
                {
                    style = {flexGrow = 1}
                };
                graphView.OnGraphViewReady = OnGraphViewReady;
                rootVisualElement.Add(graphView);
            }
            else
            {
                OnGraphViewReady();
            }
        }

        void OnGraphViewReady()
        {
            ClearView();

            foreach (var target in TargetObjects)
            {
                var path = AssetDatabase.GetAssetPath(target);
                ownerNode = new OwnerNode(path);
                graphView.AddElement(ownerNode);
                ownerNode.SetPosition(new Rect(0, 0, ownerNode.GetPosition().width, ownerNode.GetPosition().height));
                FillDependenceNode(ownerNode);
            }

            var timer = graphView.schedule.Execute(() => { RefreshNodePosition(); });
            timer.ExecuteLater(1L);
        }

        void ClearView()
        {
            foreach (var node in SourceNodes)
            {
                graphView.RemoveElement(node);
            }

            SourceNodes.Clear();

            foreach (var node in DependentNodes)
            {
                graphView.RemoveElement(node);
            }

            DependentNodes.Clear();

            if (ownerNode != null)
            {
                graphView.RemoveElement(ownerNode);
            }

            ownerNode = null;

            foreach (var edge in Edges)
            {
                graphView.RemoveElement(edge);
            }

            Edges.Clear();
        }

        public static void HandleTarget(string assetPath)
        {
            var window = GetWindow<XReferenceViewer>();
            TargetObjects.Clear();
            TargetObjects.Add(AssetDatabase.LoadAssetAtPath<Object>(assetPath));
            window.graphView.OnGraphViewReady();
        }

        void RefreshNodePosition()
        {
            RefreshDependentNodePosition();
            RefreshSourceNodePosition();

            graphView.FrameAll();
        }

        void RefreshDependentNodePosition()
        {
            var nodeHeight = ownerNode.GetPosition().height;
            var nodeWidth = ownerNode.GetPosition().width;

            var basePositionX = ownerNode.GetPosition().x + nodeWidth / 2 +
                                XReferenceViewerSetting.Inst.HorizontalPadding;

            var totalHeight = nodeHeight +
                              (nodeHeight + XReferenceViewerSetting.Inst.VerticalPadding) * (DependentNodes.Count - 1);

            var basePositionY = -totalHeight / 2;

            for (int index = 0; index < DependentNodes.Count; index++)
            {
                var node = DependentNodes[index];
                var positionY = (nodeHeight + XReferenceViewerSetting.Inst.VerticalPadding) * index + basePositionY;
                node.SetPosition(
                    new Rect(basePositionX, positionY, node.GetPosition().width, node.GetPosition().height));
            }
        }

        void RefreshSourceNodePosition()
        {
            var nodeHeight = ownerNode.GetPosition().height;
            var nodeWidth = ownerNode.GetPosition().width;

            var basePositionX = ownerNode.GetPosition().x + nodeWidth / 2 +
                                XReferenceViewerSetting.Inst.HorizontalPadding;

            var totalHeight = nodeHeight +
                              (nodeHeight + XReferenceViewerSetting.Inst.VerticalPadding) * (SourceNodes.Count - 1);

            var basePositionY = -totalHeight / 2;

            for (int index = 0; index < SourceNodes.Count; index++)
            {
                var node = SourceNodes[index];
                var positionY = (nodeHeight + XReferenceViewerSetting.Inst.VerticalPadding) * index + basePositionY;
                node.SetPosition(
                    new Rect(basePositionX, positionY, node.GetPosition().width, node.GetPosition().height));
            }
        }

        void FillDependenceNode(OwnerNode owner)
        {
            var assetDependencies = AssetDatabase.GetDependencies(owner.AssetPath, false);
            foreach (var dependence in assetDependencies)
            {
                var dependenceNode = new DependentNode(dependence);
                DependentNodes.Add(dependenceNode);
                graphView.AddElement(dependenceNode);
                var edge = graphView.LinkNode(owner, dependenceNode);
                Edges.Add(edge);
            }
        }
    }
}