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

        public override bool canDelete => false;

        public override bool canCopy => false;

        public override bool canSelect => false;

        public override bool canDrag => false;

        protected override NodeColorMix color => NodeColor.Green;

        protected override IEnumerable<DropdownOption> contextOptions => Enumerable.Empty<DropdownOption>();
    }
}