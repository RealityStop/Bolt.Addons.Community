using Bolt.Addons.Community.Fundamentals.Units.Documenting;
using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bolt.Addons.Community.Fundamentals.Editor.Editor.Descriptors
{
    [Descriptor(typeof(Todo))]
    public class TodoDescriptor : UnitDescriptor<Todo>
    {
        public TodoDescriptor(Todo unit) : base(unit) { }

        protected override EditorTexture DefinedIcon()
        {
            if (unit.ErrorIfHit)
                PathUtil.Load("construction_alarm", CommunityEditorPath.Fundamentals);
            return PathUtil.Load("construction", CommunityEditorPath.Fundamentals);
        }
    }
}