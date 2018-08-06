using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Addons.Community.Fundamentals.Editor.Editor.Descriptors
{
    [Descriptor(typeof(UnifiedVariableUnit))]
    public class OnVariableChangedDescriptor : UnitDescriptor<OnVariableChanged>
    {
        public OnVariableChangedDescriptor(OnVariableChanged unit) : base(unit) { }

        protected override EditorTexture DefinedIcon()
        {
            return BoltCore.Icons.VariableKind(unit.kind);
        }
    }
}