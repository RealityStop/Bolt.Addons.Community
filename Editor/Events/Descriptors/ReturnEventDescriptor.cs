using UnityEngine;
using Ludiq;
using Bolt;
using UnityEditor;

namespace Bolt.Addons.Community.ReturnEvents.Editor
{
    /// <summary>
    /// A descriptor that assigns the ReturnEvents icon.
    /// </summary>
    [Descriptor(typeof(ReturnEvent))]
    public sealed class ReturnEventDescriptor : EventUnitDescriptor<ReturnEvent>
    {
        public ReturnEventDescriptor(ReturnEvent target) : base(target)
        {

        }

        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("return_event", CommunityEditorPath.Events);
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("return_event", CommunityEditorPath.Events);
        }
    }
}