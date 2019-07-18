using UnityEngine.UIElements;

namespace UnityEditor.ShaderGraph.Drawing
{
    class EdgeViewManipulator : MouseManipulator
    {
        public EdgeViewManipulator()
        {
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, clickCount = 2 });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        }

        protected void OnMouseDown(MouseDownEvent evt)
        {
            if(evt.button == (int)MouseButton.LeftMouse && evt.clickCount == 2)
            {
                var edge = target as EdgeView;

                if(edge != null)
                    edge.AddRedirectNode(evt.localMousePosition);
            }
        }
    }
}
