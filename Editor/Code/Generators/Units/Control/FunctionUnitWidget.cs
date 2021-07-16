using Bolt.Addons.Libraries.Humility;
using Unity.VisualScripting;
using UnityEngine;

namespace Bolt.Addons.Community.Code
{
    [Widget(typeof(FunctionUnit))]
    public sealed class FunctionUnitWidget : UnitWidget<FunctionUnit>
    {
        public FunctionUnitWidget(FlowCanvas canvas, FunctionUnit unit) : base(canvas, unit)
        {
        }

        private bool isDeleting;
        public override bool canDelete => isDeleting;

        protected override NodeColorMix color => new NodeColorMix(NodeColor.Green);

        public override void HandleInput()
        {
            var firstFunction = canvas.graph.units[0];
            if (unit != firstFunction)
            {
                isDeleting = true;
                selection.Clear();
                selection.Add(unit);
                Delete();
                Debug.Log(reference.macro.GetType());
                if (reference.macro != null && reference.macro.GetType().Inherits<MethodDeclaration>())
                {
                    Debug.LogWarning("You cannot have more then one EntryUnit in a Method. Auto deleting.");
                }
                else
                {
                    Debug.LogWarning("You cannot have an EntryUnit outside of a Method. Auto deleting.");
                }
            }
            else
            {
                isDeleting = false;
            }

            GraphWindow.active.Repaint();
            base.HandleInput();
        }
    }
}