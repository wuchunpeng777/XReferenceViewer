using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace XReferenceViewer.Editor
{
    public class XReferenceViewerNode : GraphElement, ICollectibleElement
    {
        public VisualElement mainContainer { get; private set; }
        public VisualElement titleContainer { get; private set; }
        public VisualElement inputContainer { get; private set; }
        public VisualElement outputContainer { get; private set; }

        private VisualElement m_InputContainerParent;
        private VisualElement m_OutputContainerParent;

        //This directly contains input and output containers
        public VisualElement topContainer { get; private set; }

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

        private readonly Label m_TitleLabel;
        public override string title
        {
            get { return m_TitleLabel != null ? m_TitleLabel.text : string.Empty; }
            set { if (m_TitleLabel != null) m_TitleLabel.text = value; }
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

        protected void UseDefaultStyling()
        {
            //反射调用AddStyleSheetPath方法
            Type type = typeof(GraphElement);
            var method = type.GetMethod("AddStyleSheetPath", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(this, new object[] { "StyleSheets/GraphView/Node.uss" });
        }

        public XReferenceViewerNode() : this("UXML/GraphView/Node.uxml")
        {
            // UseDefaultStyling();
            var tpl = EditorGUIUtility.Load("Assets/Scenes/ee.uxml") as VisualTreeAsset;
            tpl.CloneTree(this);
        }

        public XReferenceViewerNode(string uiFile)
        {
            var tpl = EditorGUIUtility.Load(uiFile) as VisualTreeAsset;
            tpl.CloneTree(this);

            VisualElement main = this;
            VisualElement borderContainer = main.Q(name: "node-border");

            if (borderContainer != null)
            {
                borderContainer.style.overflow = Overflow.Hidden;
                mainContainer = borderContainer;
                var selection = main.Q(name: "selection-border");
                if (selection != null)
                {
                    selection.style.overflow = Overflow.Visible; //fixes issues with selection border being clipped when zooming out
                }
            }
            else
            {
                mainContainer = main;
            }

            titleContainer = main.Q(name: "title");
            inputContainer = main.Q(name: "input");

            if (inputContainer != null)
            {
                m_InputContainerParent = inputContainer.hierarchy.parent;
            }

            VisualElement output = main.Q(name: "output");
            outputContainer = output;

            if (outputContainer != null)
            {
                m_OutputContainerParent = outputContainer.hierarchy.parent;
                topContainer = output.parent;
            }

            m_TitleLabel = main.Q<Label>(name: "title-label");

            elementTypeColor = new Color(0.9f, 0.9f, 0.9f, 0.5f);

            if (main != this)
            {
                Add(main);
            }

            capabilities |= Capabilities.Selectable | Capabilities.Movable | Capabilities.Deletable | Capabilities.Ascendable | Capabilities.Copiable | Capabilities.Snappable | Capabilities.Groupable;
            usageHints = UsageHints.DynamicTransform;

            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));

            style.position = Position.Absolute;
        }

        public virtual void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
           
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

        public virtual void CollectElements(HashSet<GraphElement> collectedElementSet, Func<GraphElement, bool> conditionFunc)
        {
            CollectConnectedEdges(collectedElementSet);
        }
    }
}