using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace UnityEditor.ShaderGraph.Drawing
{
    class EdgeView : Edge
    {
        public EdgeView() : base()
        {
            this.AddManipulator(new EdgeViewManipulator());
        }

        public void AddRedirectNode(Vector2 pos)
        {
            var matGraph = GetFirstOfType<MaterialGraphView>();
            matGraph.AddRedirectNode(this, pos);
        }
    }
}
