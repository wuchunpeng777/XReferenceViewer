using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace XReferenceViewer.Editor
{
    public class XReferenceViewer : EditorWindow
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

        [MenuItem("Assets/XReferenceViewer", false, 0)]
        static void Open()
        {
            GetWindow<XReferenceViewer>();
        }

        [MenuItem("Assets/XReferenceViewer", true)]
        static bool OpenValidate()
        {
            var valid = false;
            //wtodo:需要排除package路径
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
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

        private NodeGraphView graphView;
        private void OnEnable()
        {
            graphView = new NodeGraphView()
            {
                style = {flexGrow = 1}
            };
            graphView.OnGraphViewReady = OnGraphViewReady;
            rootVisualElement.Add(graphView);
        }

        void OnGraphViewReady()
        {
            graphView.AddElement(new OwnerNode("Assets/Scenes/Directional Light.prefab"));
        }

        public class NodeGraphView : GraphView
        {
            public Action OnGraphViewReady;
            public NodeGraphView() : base()
            {
                var styleSheet = LoadAssetFromPackage<StyleSheet>("XReferenceViewer/PackageResource/Style.uss");
                styleSheets.Add(styleSheet);
                var gridBackground = new GridBackground();
                Insert(0, gridBackground);
                gridBackground.StretchToParentSize();
                
                this.AddManipulator(new SelectionDragger());
                this.AddManipulator(new ContentDragger());
                this.AddManipulator(new ContentZoomer());
                
                var click = new ClickSelector();
                this.AddManipulator(click);
                click.target.RegisterCallback(new EventCallback<MouseDownEvent>(this.OnMouseDown));

                var timer = this.schedule.Execute(() =>
                {
                    OnGraphViewReady?.Invoke();
                });
                timer.ExecuteLater(1L);
            }

            void LinkNode(Node nodeLeft, Node nodeRight)
            {
                var outputPort = nodeLeft.outputContainer[0] as Port;
                var inputPort = nodeRight.inputContainer[0] as Port;
                var edge = new Edge()
                {
                    output = outputPort,
                    input = inputPort
                };
                edge.capabilities ^= Capabilities.Selectable;
                edge.capabilities ^= Capabilities.Deletable;
                edge.input.Connect(edge);
                edge.output.Connect(edge);
                Add(edge);
            }

            public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
            {
            }

            protected void OnMouseDown(MouseDownEvent e)
            {
                ClearSelection();
            }
        }

        public class BaseAssetNode : XReferenceViewerNode
        {
            protected string AssetPath;

            public BaseAssetNode(string assetPath) : base()
            {
                AssetPath = assetPath;

                this.capabilities ^= Capabilities.Deletable;

                RegisterCallback<ContextualMenuPopulateEvent>(MenuPopulateCallback);

                var preview = new Image();
                var previewContainer = new UnityEngine.UIElements.VisualElement();
                previewContainer.style.width = StyleKeyword.Auto; //100;
                previewContainer.style.height = 100;
                previewContainer.style.backgroundColor = Color.black;
                previewContainer.style.flexDirection = FlexDirection.Row;
                previewContainer.style.justifyContent = Justify.Center;
                this.Add(previewContainer);
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetPath);
                preview.image = AssetPreview.GetAssetPreview(obj) ??
                                AssetPreview.GetMiniThumbnail(obj);
                previewContainer.Add(preview);
                preview.StretchToParentSize();

                AddObjectTypeArea();
                AddDoubleClick();
                RemoveCollapseButton();
                
                RefreshTitle();
                RefreshObjectType();
                
                tooltip = AssetPath;
            }

            void RemoveCollapseButton()
            {
                var collapseButton = this.Q("collapse-button");
                collapseButton.parent.Remove(collapseButton);
            }

            protected void RemoveInputArea()
            {
                var inputArea = this.Q("input");
                inputArea.parent.Remove(inputArea);
            }
            
            protected void RemoveOutputArea()
            {
                var outputArea = this.Q("output");
                outputArea.parent.Remove(outputArea);
            }

            void RefreshTitle()
            {
                var fileName = Path.GetFileNameWithoutExtension(AssetPath);
                title = fileName;
            }

            void AddObjectTypeArea()
            {
                //wtodo
            }

            void RefreshObjectType()
            {
                //wtodo
            }

            void AddDoubleClick()
            {
                var clickable = new Clickable(OnDoubleClick);
                clickable.activators.Clear();
                clickable.activators.Add(new ManipulatorActivationFilter
                    {button = MouseButton.LeftMouse, clickCount = 2});

                this.AddManipulator(clickable);
            }

            protected void AddInputPort(Color color = default)
            {
                var port = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single,
                    typeof(Port));
                if (color != default)
                    port.portColor = color;
                port.RemoveManipulator(port.edgeConnector);
                inputContainer.Add(port);
            }

            protected void AddOutputPort(Color color = default)
            {
                var port = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single,
                    typeof(Port));
                if (color != default)
                    port.portColor = color;
                port.RemoveManipulator(port.edgeConnector);
                outputContainer.Add(port);
            }

            void OnDoubleClick(EventBase evt)
            {
                //wtodo:双击打开资源
            }

            void MenuPopulateCallback(ContextualMenuPopulateEvent evt)
            {
                evt.menu.MenuItems().Clear();
                evt.menu.AppendAction("Ping", (a) =>
                {
                    //wtodo
                }, DropdownMenuAction.AlwaysEnabled);
            }
        }

        public class OwnerNode : BaseAssetNode
        {
            public OwnerNode(string assetPath) : base(assetPath)
            {
                // AddOutputPort(Color.yellow);
                // RemoveInputArea();
            }
        }

        public class DependentNode : BaseAssetNode
        {
            public DependentNode(string assetPath) : base(assetPath)
            {
                AddInputPort(Color.cyan);
                RemoveOutputArea();
            }
        }

        public class SourceNode : BaseAssetNode
        {
            public SourceNode(string assetPath) : base(assetPath)
            {
                AddInputPort();
                AddOutputPort();


                var objField = new ObjectField()
                {
                    objectType = typeof(GameObject),
                    allowSceneObjects = false,
                    value = null,
                };

                var preview = new Image();
                var previewContainer = new UnityEngine.UIElements.VisualElement();
                previewContainer.style.width = StyleKeyword.Auto; //100;
                previewContainer.style.height = 100;
                previewContainer.style.backgroundColor = Color.black;
                previewContainer.style.flexDirection = FlexDirection.Row;
                previewContainer.style.justifyContent = Justify.Center;
                this.Add(previewContainer);

                objField.RegisterValueChangedCallback(v =>
                {
                    preview.image = AssetPreview.GetAssetPreview(objField.value) ??
                                    AssetPreview.GetMiniThumbnail(objField.value);
                });
                this.Add(objField);

                previewContainer.Add(preview);
                preview.StretchToParentSize();

                var collapseButton = this.Q("collapse-button");
                collapseButton.parent.Remove(collapseButton);
            }

            public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
            {
            }
        }

        private static T LoadAssetFromPackage<T>(string packageFilePath) where T : UnityEngine.Object
        {
            // try to load as a package path
            var possibleAssetFilePath = $"Assets/{packageFilePath}";
            var asset = AssetDatabase.LoadAssetAtPath<T>(possibleAssetFilePath);
            if (asset != null)
                return asset;

            // try to convert path to a package path from a presumed package display path
            var splits = packageFilePath.Split('/');
            if (splits.Length >= 1)
            {
                var possiblePackageDisplayName = splits[0];
                var possiblePackageName = PackageDisplayNameToPackageName(possiblePackageDisplayName);
                if (!string.IsNullOrEmpty(possiblePackageName))
                {
                    splits[1] = possiblePackageName;
                    var possiblePackageFilePath = string.Join('/', splits);

                    var possibleAsset = AssetDatabase.LoadAssetAtPath<T>(possiblePackageFilePath);
                    if (possibleAsset != null)
                        return possibleAsset;
                }
            }

            return null;
        }

        private static string PackageDisplayNameToPackageName(string packageDisplayName)
        {
            var packages = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages();

            return packages
                .Where(package => package.displayName == packageDisplayName)
                .Select(package => package.name)
                .FirstOrDefault();
        }
    }
}