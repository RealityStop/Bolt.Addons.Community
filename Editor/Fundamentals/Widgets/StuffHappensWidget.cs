using Bolt.Addons.Community.Fundamentals.Units.Documenting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals.Editor.Editor.Widgets
{
    [Widget(typeof(StuffHappens))]
    public sealed class StuffHappensUnitWidget : UnitWidget<StuffHappens>
    {

        public StuffHappensUnitWidget(FlowCanvas canvas, StuffHappens unit) : base(canvas, unit)
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