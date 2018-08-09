using Bolt.Addons.Community.Fundamentals.Units.Documenting;
using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bolt.Addons.Community.Fundamentals.Editor.Editor.Descriptors
{
    [Descriptor(typeof(CommentUnit))]
    public class CommentDescriptor : UnitDescriptor<CommentUnit>
    {
        public CommentDescriptor(CommentUnit unit) : base(unit) { }

        protected override EditorTexture DefinedIcon()
        {
            return EditorTexture.Load(new AssemblyResourceProvider(Assembly.GetExecutingAssembly(), "Bolt.Addons.Community.Fundamentals.Editor", "Resources"), "comments.png", CreateTextureOptions.PixelPerfect, true);
        }
    }
}