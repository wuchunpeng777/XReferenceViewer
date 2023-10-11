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

        private void OnEnable()
        {
            rootVisualElement.Add(new SampleGraphView()
            {
                style = {flexGrow = 1}
            });
        }

        public class SampleGraphView : GraphView
        {
            public SampleGraphView() : base()
            {
                var styleSheet = LoadAssetFromPackage<StyleSheet>("XReferenceViewer/PackageResource/Style.uss");
                styleSheets.Add(styleSheet);
                var gridBackground = new GridBackground();
                Insert(0, gridBackground);
                gridBackground.StretchToParentSize();
                var node1 = new SampleNode();
                var node2 = new SampleNode();
                AddElement(node1);
                AddElement(node2);
                AddEdgeByPorts(node1.outputContainer[0] as Port, node2.inputContainer[0] as Port);
                this.AddManipulator(new SelectionDragger());
                this.AddManipulator(new ContentDragger());
                this.AddManipulator(new ContentZoomer());
                var click = new ClickSelector();
                this.AddManipulator(click);
                click.target.RegisterCallback(new EventCallback<MouseDownEvent>(this.OnMouseDown));
            }

            void AddEdgeByPorts(Port outputPort, Port inputPort)
            {
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

        public class SampleNode : Node
        {
            public SampleNode()
            {
                title = "Sample Node";

                var inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single,
                    typeof(Port));
                inputContainer.Add(inputPort);
                inputPort.RemoveManipulator(inputPort.edgeConnector);
                
                var outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single,
                    typeof(Port));
                outputPort.RemoveManipulator(outputPort.edgeConnector);
                outputContainer.Add(outputPort);
                var clickable2 = new Clickable(OnDoubleClick);
                clickable2.activators.Clear();
                clickable2.activators.Add(new ManipulatorActivationFilter
                    {button = MouseButton.LeftMouse, clickCount = 2});

                this.capabilities ^= Capabilities.Deletable;

                this.AddManipulator(clickable2);
                
                RegisterCallback<ContextualMenuPopulateEvent>(MyMenuPopulateCB);

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
            }

            public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
            {
            }

            void MyMenuPopulateCB(ContextualMenuPopulateEvent evt)
            {
                evt.menu.MenuItems().Clear();
                evt.menu.AppendAction("Ping", (a) => { Debug.Log("test"); }, DropdownMenuAction.AlwaysEnabled);
            }

            void OnDoubleClick(EventBase evt)
            {
                Debug.Log("Double Click");
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