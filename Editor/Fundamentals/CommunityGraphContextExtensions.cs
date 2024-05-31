using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Unity.VisualScripting.Community;

namespace Unity.VisualScripting.Community
{
    [GraphContextExtension(typeof(FlowGraphContext))]
    public class CommunityGraphContextExtensions : GraphContextExtension<FlowGraphContext>
    {
        public CommunityGraphContextExtensions(FlowGraphContext context) : base(context)
        {
        }

        public override IEnumerable<GraphContextMenuItem> contextMenuItems
        {
            get
            {
                if (selection.Count >= 2)
                {
                    yield return new GraphContextMenuItem(ConvertToEmbed, "To Embed Subgraph");
                    yield return new GraphContextMenuItem(ConvertToMacro, "To Macro Subgraph");
                }

                if (selection.Count == 0)
                {
                    yield return new GraphContextMenuItem(OpenCSharpPreview, "Open Utility Window");
                }

                foreach (var item in base.contextMenuItems)
                {
                    yield return item;
                }
            }
        }

        private void ConvertToEmbed(Vector2 pos)
        {
            NodeSelection.Convert(GraphSource.Embed);
        }

        private void ConvertToMacro(Vector2 pos)
        {
            NodeSelection.Convert(GraphSource.Macro);
        }

        private void OpenCSharpPreview(Vector2 pos)
        {
            UtilityWindow.Open();
        }
    }
}