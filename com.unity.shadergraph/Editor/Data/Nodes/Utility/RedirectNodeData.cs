using System.Reflection;
using UnityEngine;
using Edge = UnityEditor.Experimental.GraphView.Edge;

namespace UnityEditor.ShaderGraph
{
    // As soon as traversal can skip RedirectNodes, make this NOT a CodeFunctionNode
    class RedirectNodeData : CodeFunctionNode
    {
        public Edge m_Edge;

        public RedirectNodeData() : base()
        {
            name = "Redirect Node";
        }

        protected override MethodInfo GetFunctionToConvert()
        {
            return GetType().GetMethod("Unity_Redirect", BindingFlags.Static | BindingFlags.NonPublic);
        }

        RedirectNodeView m_nodeView;
        public RedirectNodeView nodeView
        {
            get { return m_nodeView; }
            set
            {
                if (value != m_nodeView)
                    m_nodeView = value;
            }
        }

        // Center the node's position?
        public void SetPosition(Vector2 pos)
        {
            var temp = drawState;
            temp.position = new Rect(pos, Vector2.zero);
            drawState = temp;
        }

        static string Unity_Redirect(
            [Slot(0, Binding.None)] DynamicDimensionMatrix In,
            [Slot(1, Binding.None)] out DynamicDimensionMatrix Out)
        {
            return
                @"
{
    Out = In;
}
";
        }
    }
}
