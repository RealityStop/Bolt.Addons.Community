using System.Reflection;
using Unity.VisualScripting.ReorderableList;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class NesterAdaptor : MetadataListAdaptor
    {
        public ReorderableListControl listControl;
        private static readonly FieldInfo listControlFieldInfo = typeof(MetadataCollectionAdaptor).GetField("listControl", BindingFlags.NonPublic | BindingFlags.Instance);

        public NesterAdaptor(Metadata metadata, Inspector parentInspector) : base(metadata, parentInspector)
        {
            isExpanded = EditorPrefs.GetBool(ExpandedKey, true);

            if (listControlFieldInfo != null)
            {
                listControl = listControlFieldInfo.GetValueOptimized(this) as ReorderableListControl;
                if (listControl != null)
                {
                    listControl.Flags = ReorderableListFlags.HideAddButton | ReorderableListFlags.DisableContextMenu | ReorderableListFlags.DisableReordering;
                }
            }
        }

        public override float GetItemHeight(float width, int index)
        {
            if (!isExpanded) return -4;
            return EditorGUIUtility.singleLineHeight;
        }

        protected override bool CanAdd()
        {
            return false;
        }

        public override bool CanRemove(int index)
        {
            return false;
        }

        private bool isExpanded = false;

        private const string ExpandedKey = "Community_FlowGraphEditor_NesterGraphs_Expanded";

        protected override void OnTitleGUI(Rect position, GUIContent title)
        {
            EditorGUI.BeginChangeCheck();
            isExpanded = CommunityStyles.TitleFoldout(position, isExpanded, title);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(ExpandedKey, isExpanded);
            }
        }

        private static readonly RectOffset MissingSizeOffset = new RectOffset(0, 25, 0, 0);
        public override void DrawItem(Rect position, int index)
        {
            if (!isExpanded || metadata[index] == null || metadata[index].value == null) return;

            var nesterElement = (IGraphNesterElement)metadata[index].value;
            var nester = metadata[index];

            var nestMetadata = nester[nameof(IGraphNesterElement.nest)];

            var graphMetadata = nestMetadata?[nameof(IGraphNest.graph)];

            try
            {
                if (graphMetadata?.value == null)
                {
                    var helpBoxRect = position.ExpandByX(MissingSizeOffset);
                    EditorGUI.HelpBox(helpBoxRect, "Missing Graph Reference", MessageType.Warning);

                    if (Event.current.type == EventType.MouseDown && helpBoxRect.Contains(Event.current.mousePosition))
                    {
                        Event.current.Use();

                        var context = LudiqGraphsEditorUtility.editedContext.value;
                        if (context == null) context = GraphWindow.activeContext;

                        if (context != null && context.reference != null && context.canvas != null)
                        {
                            context.canvas.ViewElements(new IGraphElement[] { (IGraphElement)nesterElement });
                        }
                    }
                    return;
                }
            }
            catch
            {
                return;
            }

            var titleMetadata = graphMetadata[nameof(IGraph.title)];

            const float buttonWidth = 50f;
            const float iconSize = 16f;
            const float spacing = 4f;

            position.width += buttonWidth / 2;

            var textureRect = new Rect(position.x, position.y + (position.height - iconSize) / 2, iconSize, iconSize);
            GUI.DrawTexture(textureRect, nesterElement.GetType().Icon()?[IconSize.Small]);

            float titleWidth = position.width - iconSize - buttonWidth - (spacing * 2);
            Rect titleRect = new Rect(position.x + iconSize + spacing, position.y, titleWidth, position.height);

            Rect buttonRect = new Rect(position.xMax - buttonWidth, position.y, buttonWidth, position.height - 4);

            string actualTitle = (string)titleMetadata.value;
            string displayName = actualTitle;

            if (string.IsNullOrEmpty(actualTitle))
            {
                displayName = GraphTraversal.GetNesterName(nesterElement);
            }

            EditorGUI.BeginChangeCheck();

            var userInput = EditorGUI.TextField(titleRect, displayName, EditorStyles.textField);

            if (EditorGUI.EndChangeCheck())
            {
                if (userInput == GraphTraversal.GetNesterName(nesterElement))
                {
                    titleMetadata.value = string.Empty;
                }
                else
                {
                    titleMetadata.value = userInput;
                }
            }

            if (GUI.Button(buttonRect, new GUIContent($"Open", $"Open {nesterElement.nest.source} graph"), EditorStyles.miniButton))
            {
                var context = LudiqGraphsEditorUtility.editedContext.value;

                if (context == null) context = GraphWindow.activeContext;

                if (context == null || context.reference == null || context.canvas == null) return;

                context.canvas.window.reference = context.reference.ChildReference(nesterElement, false);
            }

            HandleDragAndDrop(position, nesterElement);
        }

        private void HandleDragAndDrop(Rect position, IGraphNesterElement nesterElement)
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            var e = Event.current;

            switch (e.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (e.button == (int)MouseButton.Left && position.Contains(e.mousePosition))
                    {
                        GUIUtility.hotControl = controlID;
                        e.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID)
                    {
                        GUIUtility.hotControl = 0;
                        DragAndDrop.PrepareStartDrag();
                        DragAndDrop.objectReferences = new UnityEngine.Object[0];
                        DragAndDrop.paths = new string[0];
                        DragAndDrop.SetGenericData("Graphs.NesterElementCopy", nesterElement.CloneViaFakeSerialization());
                        DragAndDrop.StartDrag(metadata.path);
                        e.Use();
                    }
                    break;
            }
        }
    }
}