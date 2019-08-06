using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using UnityEditor.Graphing;
using UnityEditor.ShaderGraph.Drawing;
using UnityEditor.Experimental.GraphView;
using Edge = UnityEditor.Experimental.GraphView.Edge;

namespace UnityEditor.ShaderGraph
{
    class RedirectNodeView : RedirectNode, IShaderNodeView
    {
        IEdgeConnectorListener m_ConnectorListener;
        VisualElement m_TitleContainer;
        GraphView m_GraphView;

        public RedirectNodeView() : base()
        {
        }

        public override void InitializeFromEdge(Edge edge)
        {
            orientation = edge.output.orientation;
        }

        public void Initialize(AbstractMaterialNode inNode, PreviewManager previewManager, IEdgeConnectorListener connectorListener, GraphView graphView)
        {
            if (inNode == null)
                return;
            
            // Set references
            node = inNode;
            title = node.name;
            m_GraphView = graphView;
            m_ConnectorListener = connectorListener;

            viewDataKey = node.guid.ToString();
        
            SetPosition(new Rect(node.drawState.position.x, node.drawState.position.y, 0, 0));
            AddSlots(node.GetSlots<MaterialSlot>());
        }

        #region Helper functions
        public void AddSlots(IEnumerable<MaterialSlot> slots)
        {
            foreach (var slot in slots)
            {
                if (slot.hidden)
                    continue;

                var port = ShaderPort.Create(slot, m_ConnectorListener);
                if (slot.isOutputSlot)
                    outputContainer.Add(port);
                else
                    inputContainer.Add(port);
            }
        }
        #endregion

        #region IShaderNodeView interface
        public Node gvNode => this;
        public AbstractMaterialNode node { get; private set; }
        public VisualElement colorElement { get { return this; } }

        public void Dispose()
        {
            node = null;
            ((VisualElement)this).userData = null;
        }

        public void OnModified(ModificationScope scope)
        {
            
        }

        public void UpdatePortInputTypes()
        {

        }

        public void SetColor(Color newColor)
        {

        }

        public void ResetColor()
        {

        }
        #endregion
    }
}
