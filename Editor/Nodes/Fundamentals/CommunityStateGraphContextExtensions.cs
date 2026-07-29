using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [GraphContextExtension(typeof(StateGraphContext))]
    public class CommunityStateGraphContextExtensions : GraphContextExtension<StateGraphContext>
    {
        public CommunityStateGraphContextExtensions(StateGraphContext context) : base(context)
        {
        }

        public override bool AcceptsDragAndDrop()
        {
            return DragAndDrop.GetGenericData("Graphs.NesterElementCopy") is IState;
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
            if (element is IState state) state.position = e.mousePosition;
        }
        
        public override DragAndDropVisualMode dragAndDropVisualMode => DragAndDropVisualMode.Copy;

        public override IEnumerable<GraphContextMenuItem> contextMenuItems
        {
            get
            {
                foreach (var item in base.contextMenuItems)
                {
                    yield return item;
                }

                yield return new GraphContextMenuItem(OpenNodeFinder, "Windows/Open NodeFinder Window");
                yield return new GraphContextMenuItem(OpenUtilityWindow, "Windows/Open Utility Window");
                yield return new GraphContextMenuItem(OpenGraphSnippetPopup, "Windows/Open Graph Snippets Window");
            }
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