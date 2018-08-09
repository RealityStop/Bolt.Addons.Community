using Bolt.Addons.Community.Fundamentals.Units.Documenting;
using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bolt.Addons.Community.Fundamentals.Editor.Editor.Descriptors
{
    [Descriptor(typeof(StuffHappens))]
    public class StuffHappensDescriptor : UnitDescriptor<StuffHappens>
    {
        public StuffHappensDescriptor(StuffHappens unit) : base(unit) { }

        protected override EditorTexture DefinedIcon()
        {
            return EditorTexture.Load(new AssemblyResourceProvider(Assembly.GetExecutingAssembly(), "Bolt.Addons.Community.Fundamentals.Editor", "Resources"), "weather_clouds.png", CreateTextureOptions.PixelPerfect, true);
        }
    }
}