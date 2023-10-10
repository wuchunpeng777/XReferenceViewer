using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace XReferenceViewer.Editor
{
    public class XReferenceViewer : EditorWindow
    {
        [MenuItem("Assets/XReferenceViewer", false, 0)]
        static void Open()
        {
            GetWindow<XReferenceViewer>();
        }

        [MenuItem("Assets/XReferenceViewer", true)]
        static bool OpenValidate()
        {
            var valid = false;
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
                style = { flexGrow = 1}
            });
        }

        public class SampleGraphView : GraphView
        {
            public SampleGraphView() : base()
            {
                AddElement(new SampleNode());
                this.AddManipulator(new SelectionDragger());
                SetupZoom(ContentZoomer.DefaultMinScale,ContentZoomer.DefaultMaxScale);

                nodeCreationRequest += _context =>
                {
                    AddElement(new SampleNode());
                };
            }

            public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
            {
                return ports.ToList();
            }
        }

        public class SampleNode : Node
        {
            public SampleNode()
            {
                title = "Sample Node";
                
                var inputPort = Port.Create<Edge>(Orientation.Horizontal,Direction.Input, Port.Capacity.Single, typeof(Port));
                inputContainer.Add(inputPort);
                
                var outputPort = Port.Create<Edge>(Orientation.Horizontal,Direction.Output, Port.Capacity.Single, typeof(Port));
                outputContainer.Add(outputPort);
            }
        }
    }
}