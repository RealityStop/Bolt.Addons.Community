using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Unity.VisualScripting.Community;

namespace Unity.VisualScripting
{
    [GraphContextExtension(typeof(FlowGraphContext))]
    public class GraphContext : GraphContextExtension<FlowGraphContext>
    {
        public GraphContext(FlowGraphContext context) : base(context)
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
                    yield return new GraphContextMenuItem(Addshortcut, "Add Shortcut");
                }

                foreach (var item in base.contextMenuItems) 
                {
                    yield return item;
                }
            }
        }

        private void Addshortcut(Vector2 pos)
        {
            (graph as FlowGraph).units.Add(new Shortcut() { position = pos });
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
