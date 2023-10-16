using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace XReferenceViewer.Editor
{
    public partial class XReferenceViewer
    {
        private class OwnerNode : XReferenceViewerNode
        {
            public OwnerNode(string assetPath) : base(assetPath)
            {
                AddOutputPort();
            }
        }

        private class DependentNode : XReferenceViewerNode
        {
            public DependentNode(string assetPath) : base(assetPath)
            {
                AddInputPort();
            }
        }

        private class SourceNode : XReferenceViewerNode
        {
            public SourceNode(string assetPath) : base(assetPath)
            {
                AddInputPort();
                AddOutputPort();
            }
        }

        private class XReferenceViewerNode : GraphElement, ICollectibleElement
        {
            public VisualElement inputContainer { get; private set; }
            public VisualElement outputContainer { get; private set; }

            private GraphView m_GraphView;

            // TODO Maybe make protected and move to GraphElement!
            private GraphView graphView
            {
                get
                {
                    if (m_GraphView == null)
                    {
                        m_GraphView = GetFirstAncestorOfType<GraphView>();
                    }

                    return m_GraphView;
                }
            }

            private Label _TitleLabel;

            public override string title
            {
                get { return _TitleLabel != null ? _TitleLabel.text : string.Empty; }
                set
                {
                    if (_TitleLabel != null) _TitleLabel.text = value;
                }
            }

            private Label _TypeLabel;

            public string TypeText
            {
                get { return _TypeLabel != null ? _TypeLabel.text : string.Empty; }
                set
                {
                    if (_TypeLabel != null) _TypeLabel.text = value;
                }
            }

            public override Rect GetPosition()
            {
                if (resolvedStyle.position == Position.Absolute)
                    return new Rect(resolvedStyle.left, resolvedStyle.top, layout.width, layout.height);
                return layout;
            }

            public override void SetPosition(Rect newPos)
            {
                style.position = Position.Absolute;
                style.left = newPos.x;
                style.top = newPos.y;
            }

            public readonly string AssetPath;

            public XReferenceViewerNode(string assetPath)
            {
                AssetPath = assetPath;
                tooltip = AssetPath;

                var treeAsset =
                    XReferenceViewer.LoadAssetFromPackage<VisualTreeAsset>(
                        "XReferenceViewer/PackageResource/Node.uxml");
                treeAsset.CloneTree(this);

                OnInit();
            }

            void OnInit()
            {
                elementTypeColor = new Color(0.9f, 0.9f, 0.9f, 0.5f);
                usageHints = UsageHints.DynamicTransform;

                VisualElement main = this;
                inputContainer = main.Q(name: "input");
                outputContainer = main.Q(name: "output");
                _TitleLabel = main.Q<Label>(name: "title-label");
                _TypeLabel = main.Q<Label>(name: "type-label");

                capabilities |= Capabilities.Selectable | Capabilities.Movable |
                                Capabilities.Ascendable | Capabilities.Copiable | Capabilities.Snappable |
                                Capabilities.Groupable;

                this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
                style.position = Position.Absolute;

                AddPreview();
                AddDoubleClick();

                RefreshTitle();
                RefreshObjectType();
            }

            void AddPreview()
            {
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

            void RefreshTitle()
            {
                var fileName = Path.GetFileNameWithoutExtension(AssetPath);
                title = fileName;
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

            void OnDoubleClick(EventBase evt)
            {
                //wtodo:双击打开资源
            }

            public override void OnSelected()
            {
                base.OnSelected();
            }

            public override void OnUnselected()
            {
                base.OnUnselected();
            }

            public virtual void BuildContextualMenu(ContextualMenuPopulateEvent evt)
            {
                evt.menu.MenuItems().Clear();
                evt.menu.AppendAction("Ping", (a) =>
                {
                    //wtodo
                }, DropdownMenuAction.AlwaysEnabled);
            }

            void CollectConnectedEdges(HashSet<GraphElement> edgeSet)
            {
                edgeSet.UnionWith(inputContainer.Children().OfType<Port>().SelectMany(c => c.connections)
                    .Where(d => (d.capabilities & Capabilities.Deletable) != 0)
                    .Cast<GraphElement>());
                edgeSet.UnionWith(outputContainer.Children().OfType<Port>().SelectMany(c => c.connections)
                    .Where(d => (d.capabilities & Capabilities.Deletable) != 0)
                    .Cast<GraphElement>());
            }

            public virtual void CollectElements(HashSet<GraphElement> collectedElementSet,
                Func<GraphElement, bool> conditionFunc)
            {
                CollectConnectedEdges(collectedElementSet);
            }
        }
    }
}