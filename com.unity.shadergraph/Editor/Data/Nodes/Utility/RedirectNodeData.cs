using System.Reflection;
using UnityEditor.Graphing;

namespace UnityEditor.ShaderGraph
{
    class RedirectNodeData : CodeFunctionNode
    {
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

        static string Unity_Redirect(
            [Slot(0, Binding.None)] DynamicDimensionVector In,
            [Slot(1, Binding.None)] out DynamicDimensionVector Out)
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
