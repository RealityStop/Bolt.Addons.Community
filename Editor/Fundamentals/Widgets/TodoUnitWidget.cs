using Bolt.Addons.Community.Fundamentals.Units.Documenting;
using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Addons.Community.Fundamentals.Editor.Editor.Widgets
{
    [Widget(typeof(Todo))]
    public sealed class TodoUnitWidget : UnitWidget<Todo>
    {

        public TodoUnitWidget(FlowCanvas canvas, Todo unit) : base(canvas, unit)
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