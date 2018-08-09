using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bolt.Addons.Community.Fundamentals.Editor.Editor.Descriptors
{
    [Descriptor(typeof(OnVariableChanged))]
    public class OnVariableChangedDescriptor : UnitDescriptor<OnVariableChanged>
    {
        public OnVariableChangedDescriptor(OnVariableChanged unit) : base(unit) { }

        protected override EditorTexture DefinedIcon()
        {
            return EditorTexture.Load(new AssemblyResourceProvider(Assembly.GetExecutingAssembly(), "Bolt.Addons.Community.Fundamentals.Editor", "Resources"), "variableevent.png", CreateTextureOptions.PixelPerfect, true);
        }
    }
}