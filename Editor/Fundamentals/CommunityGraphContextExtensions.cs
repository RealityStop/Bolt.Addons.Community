using System.Collections.Generic;
using UnityEngine;

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
                if (selection.Count > 0)
                {
                    yield return new GraphContextMenuItem(ConvertToEmbed, "To Embed Subgraph");
                    yield return new GraphContextMenuItem(ConvertToMacro, "To Macro Subgraph");
                }

                foreach (var item in base.contextMenuItems)
                {
                    yield return item;
                }

                yield return new GraphContextMenuItem(OpenCSharpPreview, "Open Utility Window");
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
            var window = UtilityWindow.Open();
            window.graphContext = context;
        }
    }
}