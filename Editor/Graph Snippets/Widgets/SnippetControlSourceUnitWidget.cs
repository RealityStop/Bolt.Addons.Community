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
        protected override IEnumerable<DropdownOption> contextOptions => Enumerable.Empty<DropdownOption>();

        protected override NodeColorMix baseColor => NodeColor.Green;

        public override bool canDrag => false;
    }
}