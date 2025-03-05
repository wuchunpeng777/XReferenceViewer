using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace XReferenceViewer.Editor
{
    public partial class XReferenceViewer
    {
        private class NodeGraphView : GraphView
        {
            public Action OnGraphViewReady;

            public NodeGraphView() : base()
            {
                var styleSheet = LoadAssetFromPackage<StyleSheet>("XReferenceViewer/PackageResource/Style.uss");
                if (styleSheet != null)
                {
                    styleSheets.Add(styleSheet);
                }
                else
                {
                    Debug.LogError("无法加载Style.uss样式表，请检查资源路径是否正确");
                }
                var gridBackground = new GridBackground();
                Insert(0, gridBackground);
                gridBackground.StretchToParentSize();
                
                this.AddManipulator(new SelectionDragger());
                this.AddManipulator(new ContentDragger());
                this.AddManipulator(new ContentZoomer());

                var click = new ClickSelector();
                this.AddManipulator(click);
                click.target.RegisterCallback(new EventCallback<MouseDownEvent>(this.OnMouseDown));

                var timer = this.schedule.Execute(() => { OnGraphViewReady?.Invoke(); });
                timer.ExecuteLater(1L);
            }

            public Edge LinkNode(XReferenceViewerNode outputNode, XReferenceViewerNode inputNode)
            {
                var outputPort = outputNode.outputContainer[0] as Port;
                var inputPort = inputNode.inputContainer[0] as Port;
                var edge = new Edge()
                {
                    output = outputPort,
                    input = inputPort
                };
                edge.capabilities ^= Capabilities.Selectable;
                edge.capabilities ^= Capabilities.Deletable;
                edge.input.Connect(edge);
                edge.output.Connect(edge);
                AddElement(edge);
                return edge;
            }

            public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
            {
            }

            protected void OnMouseDown(MouseDownEvent e)
            {
                ClearSelection();
            }
        }
    }
}