using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [GraphContextExtension(typeof(FlowGraphContext))]
    public class CommunityFlowGraphContextExtensions : GraphContextExtension<FlowGraphContext>
    {
        public CommunityFlowGraphContextExtensions(FlowGraphContext context) : base(context)
        {
        }

        public override bool AcceptsDragAndDrop()
        {
            return DragAndDrop.GetGenericData("Graphs.NesterElementCopy") is IUnit;
        }

        public override void DrawDragAndDropPreview()
        {
            var element = DragAndDrop.GetGenericData("Graphs.NesterElementCopy") as IGraphNesterElement;
            var name = GraphTraversal.GetNesterName(element);
            GraphGUI.DrawDragAndDropPreviewLabel(new Vector2(e.mousePosition.x, e.mousePosition.y), "Add: " + name, element.GetType().Icon());
        }

        public override void PerformDragAndDrop()
        {
            var element = DragAndDrop.GetGenericData("Graphs.NesterElementCopy") as IGraphNesterElement;
            element.guid = Guid.NewGuid();
            graph.elements.Add(element);
            if (element is IUnit unit) unit.position = e.mousePosition;
        }

        public override DragAndDropVisualMode dragAndDropVisualMode => DragAndDropVisualMode.Copy;

        public override IEnumerable<GraphContextMenuItem> contextMenuItems
        {
            get
            {
                if (selection.Count > 0)
                {
                    yield return new GraphContextMenuItem(ConvertToEmbed, "Selection/To Embed Subgraph");
                    yield return new GraphContextMenuItem(ConvertToMacro, "Selection/To Macro Subgraph");
                }

                foreach (var item in base.contextMenuItems)
                {
                    yield return item;
                }

                yield return new GraphContextMenuItem(OpenNodeFinder, "Windows/Open NodeFinder Window");
                yield return new GraphContextMenuItem(OpenUtilityWindow, "Windows/Open Utility Window");
                yield return new GraphContextMenuItem(OpenKeyboardControlsWindow, "Windows/Open Keyboard Controls Window");
                yield return new GraphContextMenuItem(OpenGraphSnippetPopup, "Windows/Open Graph Snippets Window");
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