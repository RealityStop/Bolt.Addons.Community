using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.ReorderableList;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class ListAdaptor : MetadataListAdaptor, IReorderableListDropTarget
    {
        public ReorderableListControl listControl;

        private readonly List<bool> foldoutStates = new List<bool>();

        private const float FoldoutHeight = 20f;
        private const float FieldHeight = 18f;
        private const float Spacing = 4f;
        private const float DeleteButtonWidth = 18f;
        private const float DragHandleWidth = 16f;
        private const float IndentWidth = 20f;
        private const float ContentIndent = 22f;
        private const float VerticalOffset = 4f;
        private const float HoverExpandDelay = 0.35f;

        public ListAdaptor(Metadata metadata, Inspector parent) : base(metadata, parent) { }

        public override float GetItemHeight(float width, int index)
        {
            var element = metadata[index];
            EnsureFoldoutStateSynced(index);

            float totalHeight = FoldoutHeight + Spacing;

            if (foldoutStates[index])
            {
                totalHeight += element.Inspector().GetCachedHeight(width - IndentWidth, GUIContent.none, parentInspector) + 2f;
                totalHeight += 4f;
            }

            return totalHeight;
        }

        protected override IList ConstructList()
        {
            if (metadata.listType == typeof(IList))
                return new AotList();

            if (metadata.listType.IsGenericType && metadata.listType.GetGenericTypeDefinition() == typeof(IList<>))
            {
                var args = metadata.listType.GetGenericArguments();
                return (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(args[0]));
            }

            return base.ConstructList();
        }

        public override void DrawItem(Rect position, int index)
        {
            EnsureFoldoutStateSynced(index);

            position.x -= IndentWidth;
            position.width += IndentWidth;

            var element = metadata[index];
            float yPos = position.y + VerticalOffset;

            Rect foldoutRect = new Rect(
                position.x + FieldHeight,
                yPos,
                position.width - DragHandleWidth - DeleteButtonWidth - 23f,
                FieldHeight);

            DrawFoldout(foldoutRect, index, element);
            // HandleHoverExpansion(index);

            if (foldoutStates[index])
            {
                DrawExpandedContent(position, yPos, index, element);
            }

            HandleDragAndDrop(position, index);
        }

        private void DrawFoldout(Rect foldoutRect, int index, Metadata element)
        {
            using (new EditorGUIUtility.IconSizeScope(new Vector2(IconSize.Small, IconSize.Small)))
            {
                var oldHierarchyMode = EditorGUIUtility.hierarchyMode;
                EditorGUIUtility.hierarchyMode = false;
                EditorGUI.BeginChangeCheck();

                foldoutStates[index] = EditorGUI.Foldout(foldoutRect, foldoutStates[index], CommunityStyles.GetCollectionDisplayName(element, index));

                if (EditorGUI.EndChangeCheck())
                {
                    parentInspector.SetHeightDirty();
                }

                EditorGUIUtility.hierarchyMode = oldHierarchyMode;
            }
        }

        private void DrawExpandedContent(Rect position, float yPos, int index, Metadata element)
        {
            float contentY = yPos + FoldoutHeight + Spacing;
            float contentWidth = position.width - ContentIndent;

            Rect contentRect = new Rect(
                position.x + ContentIndent,
                contentY,
                contentWidth,
                LudiqGUI.GetInspectorHeight(parentInspector, element, contentWidth, GUIContent.none));

            element.Inspector().Draw(contentRect, GUIContent.none);
        }

        private void HandleDragAndDrop(Rect position, int index)
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            var e = Event.current;

            switch (e.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (e.button == (int)MouseButton.Left && position.Contains(e.mousePosition))
                    {
                        if (alwaysDragAndDrop || e.alt)
                        {
                            GUIUtility.hotControl = controlID;
                            e.Use();
                        }
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID)
                    {
                        var item = this[index];
                        GUIUtility.hotControl = 0;

                        DragAndDrop.PrepareStartDrag();
                        DragAndDrop.objectReferences = Array.Empty<UnityEngine.Object>();
                        DragAndDrop.paths = Array.Empty<string>();
                        DragAndDrop.SetGenericData(DraggedListItem.TypeName, new DraggedListItem(this, index, item, foldoutStates[index]));
                        DragAndDrop.StartDrag(metadata.path);
                        e.Use();
                    }
                    break;
            }
        }

        private void EnsureFoldoutStateSynced(int index)
        {
            while (foldoutStates.Count <= index)
            {
                foldoutStates.Add(false);
                parentInspector.SetHeightDirty();
            }
        }

        protected override bool CanAdd()
        {
            if (metadata.HasAttribute<InspectorRangeAttribute>())
                return metadata.Count < metadata.GetAttribute<InspectorRangeAttribute>().max;

            return true;
        }

        public override bool CanRemove(int index)
        {
            if (metadata.HasAttribute<InspectorRangeAttribute>())
                return metadata.Count > metadata.GetAttribute<InspectorRangeAttribute>().min;

            return base.CanRemove(index);
        }

        public override void Add()
        {
            foldoutStates.Add(true);
            base.Add();
        }

        public override void Clear()
        {
            metadata.RecordUndo();

            for (int i = 0; i < metadata.Count; i++)
            {
                Remove(i);
            }

            parentInspector.SetHeightDirty();
        }

        public override void Remove(int index)
        {
            if (!CanRemove(index))
                return;

            foldoutStates.RemoveAt(index);
            base.Remove(index);
            ClearHotControls();
        }

        public override void Move(int sourceIndex, int destIndex)
        {
            base.Move(sourceIndex, destIndex);

            if (foldoutStates.Count == 0) return;

            if (destIndex > sourceIndex) destIndex--;

            bool state = foldoutStates[sourceIndex];
            foldoutStates.RemoveAt(sourceIndex);
            foldoutStates.Insert(destIndex, state);
        }

        public new bool CanDropInsert(int insertionIndex)
        {
            if (!ReorderableListControl.CurrentListPosition.Contains(Event.current.mousePosition))
                return false;

            return DragAndDrop.GetGenericData(DraggedListItem.TypeName) is DraggedListItem draggedData &&
                   metadata.listElementType.IsInstanceOfType(draggedData.item);
        }

        public new void ProcessDropInsertion(int insertionIndex)
        {
            if (Event.current.type == EventType.DragPerform)
            {
                if (DragAndDrop.GetGenericData(DraggedListItem.TypeName) is DraggedListItem draggedItem)
                {
                    if (draggedItem.sourceListAdaptor != this && CanDrop(draggedItem.item))
                    {
                        foldoutStates.Insert(insertionIndex, draggedItem.foldoutState);
                    }
                }
            }

            base.ProcessDropInsertion(insertionIndex);
        }

        public override float GetItemAdaptiveWidth(int index)
        {
            var element = metadata[index];
            EnsureFoldoutStateSynced(index);

            GUIContent label = CommunityStyles.GetCollectionDisplayName(element, index);
            float baseWidth = GUI.skin.label.CalcSize(label).x;
            float inspectorWidth = 0f;

            if (foldoutStates[index])
            {
                try
                {
                    var inspector = element.Inspector();
                    if (inspector != null)
                    {
                        inspectorWidth = inspector.GetAdaptiveWidth() + 10f;
                    }
                }
                catch (Exception)
                {
                }
            }

            return Mathf.Max(baseWidth, inspectorWidth);
        }

        private void ClearHotControls()
        {
            GUIUtility.keyboardControl = 0;
            GUIUtility.hotControl = 0;
        }
    }
}