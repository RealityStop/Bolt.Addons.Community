using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Unity.VisualScripting.ReorderableList;
using Unity.VisualScripting.Community.Libraries.Humility;
using System.Collections;
using System.Linq;

namespace Unity.VisualScripting.Community
{
    public class ListAdaptor : MetadataListAdaptor, IReorderableListDropTarget
    {
        private readonly List<bool> foldoutStates = new List<bool>();
        public ReorderableListControl listControl;

        private const float FoldoutHeight = 20f;
        private const float FieldHeight = 18f;
        private const float Spacing = 4f;
        private const float DeleteButtonWidth = 18f;
        private const float DragHandleWidth = 16f;

        private static readonly FieldInfo listControlFieldInfo = typeof(MetadataCollectionAdaptor).GetField("listControl", BindingFlags.NonPublic | BindingFlags.Instance);

        public ListAdaptor(Metadata metadata, Inspector parent) : base(metadata, parent)
        {
            if (listControlFieldInfo != null)
            {
                listControl = listControlFieldInfo.GetValue(this) as ReorderableListControl;
                if (listControl != null)
                {
                    listControl.ContainerStyle = GUIStyle.none;
                    listControl.Flags = ReorderableListFlags.HideRemoveButtons;
                    listControl.HorizontalLineColor = EditorGUIUtility.isProSkin ? Color.black : Color.white;
                    listControl.HorizontalLineAtStart = true;
                    listControl.HorizontalLineAtEnd = true;
                }
            }
            alwaysDragAndDrop = true;
        }

        public override float GetItemHeight(float width, int index)
        {
            var element = metadata[index];

            EnsureFoldoutCount(index);
            bool expanded = foldoutStates[index];

            if (!expanded)
                return FoldoutHeight + Spacing;

            float total = FoldoutHeight + Spacing;

            total += element.Inspector().GetCachedHeight(width - 20, GUIContent.none, parentInspector) + 2f;

            return total + 4f;
        }

        // I have to do this setup to change the color of the add button
        // It's very hacky but seems to work better than tinting the background Texture.
        private Color _previousBackgroundColor;
        private bool _tintApplied;

        /// <summary>
        /// Called before list elements are drawn.
        /// Ensures the GUI color is reset properly.
        /// </summary>
        public override void BeginGUI()
        {
            if (_tintApplied)
            {
                GUI.backgroundColor = _previousBackgroundColor;
                _tintApplied = false;
            }
        }

        /// <summary>
        /// Called after all list elements are drawn.
        /// Tints the Add button only.
        /// </summary>
        public override void EndGUI()
        {
            _previousBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = CommunityStyles.backgroundColor.Brighten(0.36f);
            _tintApplied = true;
        }

        private void EnsureFoldoutCount(int index)
        {
            while (foldoutStates.Count <= index)
                foldoutStates.Add(false);
        }

        public override void DrawItemBackground(Rect position, int index)
        {
            EditorGUI.DrawRect(position, CommunityStyles.backgroundColor);

            var restoredColor = Handles.color;
            Handles.color = Color.gray * 0.6f;
            Handles.DrawAAPolyLine(2f, new Vector3[]
            {
                    new Vector3(position.x, position.y + 1),
                    new Vector3(position.xMax, position.y + 1),
                    new Vector3(position.xMax, position.yMax),
                    new Vector3(position.x, position.yMax),
                    new Vector3(position.x, position.y)
            });
            Handles.color = restoredColor;
        }

        public override void DrawItem(Rect position, int index)
        {
            position.x -= 20;
            position.width += 20;
            var element = metadata[index];

            var oldHandleRect = new Rect(position.x + 4, position.y + position.height / 2f - 3, 9, 7);
            EditorGUI.DrawRect(oldHandleRect, CommunityStyles.backgroundColor);
            EnsureFoldoutCount(index);
            bool expanded = foldoutStates[index];

            float y = position.y + 4;

            Rect boxRect = new Rect(position.x, y, position.width, GetItemHeight(index));
            float lineY = boxRect.y + (FoldoutHeight - 16f) / 2f;

            const float handleSize = 12f;
            Rect handleRect = new Rect(position.x + 3, lineY, handleSize, handleSize);

            Texture2D dragTexture = CommunityStyles.DragHandleTexture;

            GUI.DrawTexture(handleRect, dragTexture);

            EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.Pan);

            Rect foldoutRect = new Rect(position.x + 18, lineY, position.width - DragHandleWidth - DeleteButtonWidth - 23, FieldHeight);

            Texture2D arrowOpen = CommunityStyles.ArrowDownTexture;
            Texture2D arrowClosed = CommunityStyles.ArrowRightTexture;

            Texture2D arrowTexture = foldoutStates[index] ? arrowOpen : arrowClosed;
            var arrowRect = new Rect(foldoutRect)
            {
                width = 12f,
                height = 12f,
                y = foldoutRect.y + 1
            };

            if (arrowTexture != null)
            {
                GUI.DrawTexture(arrowRect, arrowTexture, ScaleMode.ScaleToFit, true);
            }

            var labelRect = new Rect(foldoutRect);
            labelRect.x += 15;
            labelRect.y -= 3;
            GUI.Label(labelRect, CommunityStyles.GetCollectionDisplayName(element, index));

            if (Event.current.type == EventType.MouseDown && arrowRect.Contains(Event.current.mousePosition))
            {
                foldoutStates[index] = !foldoutStates[index];
                Event.current.Use();
            }

            Rect deleteRect = new Rect(position.x + position.width - DeleteButtonWidth - 4, lineY - 2f, DeleteButtonWidth - 2, FieldHeight - 2);
            if (GUI.Button(deleteRect, "", new GUIStyle(EditorStyles.whiteLabel)
            {
                normal = { background = CommunityStyles.RemoveItemTexture }
            }))
            {
                Remove(index);
                return;
            }

            if (expanded)
            {
                float contentY = y + FoldoutHeight + Spacing;
                float width = position.width - 12;
                element.Inspector().Draw(new Rect(position.x + 5, contentY, width, LudiqGUI.GetInspectorHeight(parentInspector, element, width, GUIContent.none)), GUIContent.none);
            }

            var controlID = GUIUtility.GetControlID(FocusType.Passive);

            switch (Event.current.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    var draggablePosition = position;

                    if (Event.current.button == (int)MouseButton.Left && draggablePosition.Contains(Event.current.mousePosition) && !handleRect.Contains(Event.current.mousePosition))
                    {
                        if (alwaysDragAndDrop || Event.current.alt)
                        {
                            GUIUtility.hotControl = controlID;
                            Event.current.Use();
                        }
                    }

                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID)
                    {
                        var item = this[index];
                        GUIUtility.hotControl = 0;
                        DragAndDrop.PrepareStartDrag();
                        DragAndDrop.objectReferences = new UnityEngine.Object[0];
                        DragAndDrop.paths = new string[0];
                        DragAndDrop.SetGenericData(VisualScripting.DraggedListItem.TypeName, new DraggedListItem(this, index, item, foldoutStates[index]));
                        DragAndDrop.StartDrag(metadata.path);
                        Event.current.Use();
                    }

                    break;
            }
        }

        protected override bool CanAdd() => true;

        public override void Remove(int index)
        {
            foldoutStates.RemoveAt(index);
            base.Remove(index);
            GUIUtility.keyboardControl = 0;
            GUIUtility.hotControl = 0;
        }

        public override void Add()
        {
            base.Add();

            foldoutStates.Add(true);
        }

        public override void Move(int sourceIndex, int destIndex)
        {
            base.Move(sourceIndex, destIndex);

            if (foldoutStates.Count == 0)
                return;

            if (destIndex > sourceIndex)
                destIndex--;

            var state = foldoutStates[sourceIndex];
            foldoutStates.RemoveAt(sourceIndex);
            foldoutStates.Insert(destIndex, state);
        }

        public new bool CanDropInsert(int insertionIndex)
        {
            if (!ReorderableListControl.CurrentListPosition.Contains(Event.current.mousePosition))
            {
                return false;
            }

            var data = DragAndDrop.GetGenericData(VisualScripting.DraggedListItem.TypeName);

            return data is DraggedListItem && metadata.listElementType.IsInstanceOfType(((DraggedListItem)data).item);
        }

        public new void ProcessDropInsertion(int insertionIndex)
        {
            if (Event.current.type == EventType.DragPerform)
            {
                var draggedItem = DragAndDrop.GetGenericData(VisualScripting.DraggedListItem.TypeName) as DraggedListItem;

                if (draggedItem != null)
                {
                    if (draggedItem.sourceListAdaptor != this)
                    {
                        if (!CanDrop(draggedItem.item))
                            return;

                        var state = draggedItem.foldoutState;
                        foldoutStates.Insert(insertionIndex, state);
                    }
                }
            }

            base.ProcessDropInsertion(insertionIndex);
        }
    }
}