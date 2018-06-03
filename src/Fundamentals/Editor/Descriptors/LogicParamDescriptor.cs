using Bolt.Addons.Community.Fundamentals;
using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Addons.Community.Editor.Editor.Descriptors
{
    [Descriptor(typeof(LogicParams))]
    public class LogicParamDescriptor : UnitDescriptor<LogicParams>
    {
        public LogicParamDescriptor(LogicParams unit) : base(unit) { }

        protected override void DefinedPort(IUnitPort port, UnitPortDescription description)
        {
            base.DefinedPort(port, description);

            var index = unit.arguments.IndexOf(port as ValueInput);

            if (index >= 0)
            {
                description.label = "Arg. " + index;
            }
        }
    }
}