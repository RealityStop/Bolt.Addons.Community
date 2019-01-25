using Bolt.Addons.Community.Fundamentals.Units.Documenting;
using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Addons.Community.Fundamentals.Editor.Editor.Widgets
{
    [Widget(typeof(SomeValue))]
    public sealed class SomeValueWidget : UnitWidget<SomeValue>
    {

        public SomeValueWidget(FlowCanvas canvas, SomeValue unit) : base(canvas, unit)
        {
        }

        protected override NodeColorMix baseColor
        {
            get
            {
                return new NodeColorMix() { red = 0.6578709f, green = 1f };
            }
        }
    }
}