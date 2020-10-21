using UnityEngine;
using UnityEditor;
using Ludiq;
using Bolt;

namespace Bolt.Addons.Community.Fundamentals.Units.Collections.Editor
{
    /// <summary>
    /// The descriptor that sets the icon for CreateMultiArray.
    /// </summary>
    [Descriptor(typeof(CreateMultiArray))]
    public class CreateMultiArrayDescriptor : UnitDescriptor<CreateMultiArray>
    {
        public CreateMultiArrayDescriptor(CreateMultiArray unit) : base(unit)
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