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
            return PathUtil.Load("weather_clouds", CommunityEditorPath.Fundamentals);
        }
    }
}