using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Graphing;

using Edge = UnityEditor.Experimental.GraphView.Edge;

namespace UnityEditor.ShaderGraph
{
    [Title("Utility", "Redirect")]
    class RedirectNodeData : CodeFunctionNode
    {
        const int m_inSlotID = 0;
        const int m_outSlotID = 1;
        const string m_inSlotName = "In";
        const string m_outSlotName = "Out";

        const int m_tempSlotID = 2;
        const string m_tempSlotName = "Add";

        public RedirectNodeData() : base()
        {
            name = "Redirect Node";

            //Set the default state to collapsed
            DrawState temp = drawState;
            temp.expanded = false;
            drawState = temp;
        }

        protected override MethodInfo GetFunctionToConvert()
        {
            return GetType().GetMethod("Unity_Redirect", BindingFlags.Static | BindingFlags.NonPublic);
        }

        static string Unity_Redirect(
            [Slot(0, Binding.None)] DynamicDimensionVector In,
            [Slot(1, Binding.None)] out DynamicDimensionVector Out)
            //[Slot(2, Binding.None)] DynamicDimensionVector Add)
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
