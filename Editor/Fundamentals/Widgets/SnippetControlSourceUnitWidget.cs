using System.Collections.Generic;
using System.Linq;


namespace Unity.VisualScripting.Community
{
    [Widget(typeof(SnippetControlSourceUnit))]
    public class SnippetControlSourceUnitWidget : UnitWidget<SnippetControlSourceUnit>
    {
        public SnippetControlSourceUnitWidget(FlowCanvas canvas, SnippetControlSourceUnit unit) : base(canvas, unit)
        {
        }

        private bool isDeleting;
        public override bool canDelete => isDeleting;
        public override bool canCopy => false;
        protected override IEnumerable<DropdownOption> contextOptions => Enumerable.Empty<DropdownOption>();

        protected override NodeColorMix baseColor => NodeColor.Green;

        public override void HandleInput()
        {
            var firstSource = canvas.graph.units.FirstOrDefault(u => u is SnippetControlSourceUnit);
            if (unit != firstSource)
            {
                isDeleting = true;
                selection.Clear();
                selection.Add(unit);
                Delete();
            }
            else
            {
                isDeleting = false;
            }

            base.HandleInput();
        }

        public override bool canSelect => false;

        public override bool canDrag => false;
    }
}