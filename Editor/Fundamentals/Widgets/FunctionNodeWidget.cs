using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Unity.VisualScripting.Community
{
    [Widget(typeof(FunctionNode))]
    public sealed class FunctionNodeWidget : UnitWidget<FunctionNode>
    {
        public FunctionNodeWidget(FlowCanvas canvas, FunctionNode unit) : base(canvas, unit)
        {
        }

        private bool isDeleting;
        public override bool canDelete => isDeleting;
        public override bool canCopy => false;
        protected override IEnumerable<DropdownOption> contextOptions => Enumerable.Empty<DropdownOption>();

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