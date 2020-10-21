using Bolt;
using Ludiq;
using UnityEditor;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals.Units.Utility.Editor
{
    [Descriptor(typeof(FlowReroute))]
    public sealed class FlowRerouteDescriptor : UnitDescriptor<FlowReroute>
    {
        public FlowRerouteDescriptor(FlowReroute target) : base(target)
        {
        }

        protected override void DefinedPort(IUnitPort port, UnitPortDescription description)
        {
            base.DefinedPort(port, description);

            description.showLabel = false;
        }

        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("flow_reroute", CommunityEditorPath.Fundamentals);
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("flow_reroute", CommunityEditorPath.Fundamentals);
        }
    }
} 