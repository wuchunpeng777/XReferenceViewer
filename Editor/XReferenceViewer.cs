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

      
    }
}