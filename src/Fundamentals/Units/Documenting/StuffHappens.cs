using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Addons.Community.Fundamentals.Units.Documenting
{
    [UnitCategory("Community\\Documentation")]
    public class StuffHappens : Unit
    {

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter { get; private set; }


        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), (x) => exit);
            exit = ControlOutput(nameof(exit));

            Succession(enter, exit);
        }
    }
}