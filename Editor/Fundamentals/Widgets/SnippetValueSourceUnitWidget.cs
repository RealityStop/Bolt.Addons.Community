using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Widget(typeof(SnippetSourceUnit))]
    public class SnippetSourceUnitWidget : UnitWidget<SnippetSourceUnit>
    {
        public SnippetSourceUnitWidget(FlowCanvas canvas, SnippetValueSourceUnit unit) : base(canvas, unit)
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
