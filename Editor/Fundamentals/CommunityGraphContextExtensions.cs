using System;
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

                yield return new GraphContextMenuItem(OpenNodeFinder, "Open NodeFinder Window");
                yield return new GraphContextMenuItem(OpenUtilityWindow, "Open Utility Window");
                yield return new GraphContextMenuItem(OpenKeyboardControlsWindow, "Open Keyboard Controls Window");
                yield return new GraphContextMenuItem(OpenGraphSnippetPopup, "Open Graph Snippets Window");
            }
        }

        private void OpenKeyboardControlsWindow(Vector2 _)
        {
            Rect rect = new Rect(e.mousePosition.x, e.mousePosition.y, 0, 0);

            GraphKeyboardControlsPopup.Show(rect);
        }

        private void OpenGraphSnippetPopup(Vector2 _)
        {
            Rect rect = new Rect(e.mousePosition.x, e.mousePosition.y, 0, 0);

            GraphSnippetsPopup.Show(rect);
        }

        private void ConvertToEmbed(Vector2 _)
        {
            NodeSelection.Convert(GraphSource.Embed);
        }

        private void ConvertToMacro(Vector2 _)
        {
            NodeSelection.Convert(GraphSource.Macro);
        }

        private void OpenUtilityWindow(Vector2 _)
        {
            var window = UtilityWindow.Open();
            window.graphContext = context;
        }

        private void OpenNodeFinder(Vector2 _)
        {
            NodeFinderWindow.Open();
        }
    }
}