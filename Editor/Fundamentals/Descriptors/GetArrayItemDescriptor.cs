using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Ludiq;
using Bolt;

namespace Bolt.Addons.Community.Fundamentals.Units.Collections.Editor
{
    /// <summary>
    /// The descriptor that sets the icon for Get Array Item.
    /// </summary>
    [Descriptor(typeof(GetArrayItem))]
    public class GetArrayItemDescriptor : UnitDescriptor<GetArrayItem>
    {
        public GetArrayItemDescriptor(GetArrayItem unit) : base(unit)
        {

        }

        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("multi_array", CommunityEditorPath.Fundamentals);
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("multi_array", CommunityEditorPath.Fundamentals);
        }
    }
}