using Bolt.Addons.Community.Fundamentals.Units.logic;
using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bolt.Addons.Community.Fundamentals.Editor.Editor.Descriptors
{
    [Descriptor(typeof(LogUnit))]
    public class LogUnitDescriptor : UnitDescriptor<LogUnit>
    {
        public LogUnitDescriptor(LogUnit target) : base(target)
        {
        }

        protected override EditorTexture DefinedIcon()
        {
            return EditorTexture.Load(new AssemblyResourceProvider(Assembly.GetExecutingAssembly(), "Bolt.Addons.Community.Fundamentals.Editor", "Resources"), "debug.png", CreateTextureOptions.PixelPerfect, true);
        }
    }
}
