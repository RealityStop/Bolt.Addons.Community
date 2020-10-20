using Bolt.Addons.Community.Fundamentals.Units.Documenting;
using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals.Editor.Editor.Descriptors
{
    [Descriptor(typeof(CommentUnit))]
    public class CommentDescriptor : UnitDescriptor<CommentUnit>
    {
        public CommentDescriptor(CommentUnit unit) : base(unit) { }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("comments", CommunityEditorPath.Fundamentals);
        }
    }
}