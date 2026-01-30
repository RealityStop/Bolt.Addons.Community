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

        protected override IList ConstructList()
        {
            if (metadata.listType == typeof(IList)) return new AotList();
            else if (metadata.listType.IsGenericType && metadata.listType.GetGenericTypeDefinition() == typeof(IList<>))
            {
                var args = metadata.listType.GetGenericArguments();
                return (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(args[0]));
            }
            return base.ConstructList();
        }

#if DARKER_UI
        // I have to do this setup to change the color of the add button
        // It's very hacky but seems to work better than tinting the background Texture.
        private Color _previousBackgroundColor;
        private bool _tintApplied;

        public override void BeginGUI()
        {
            if (_tintApplied)
            {
                GUI.backgroundColor = _previousBackgroundColor;
                _tintApplied = false;
            }
        }

        public override void EndGUI()
        {
            _previousBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = CommunityStyles.backgroundColor.Brighten(0.36f);
            _tintApplied = true;
        }
#endif

        private void EnsureFoldoutCount(int index)
        {
            while (foldoutStates.Count <= index)
            {
                foldoutStates.Add(false);
                foldoutHoverStartTimes.Add(null);
                parentInspector.SetHeightDirty();
            }
        }

        public override void DrawItemBackground(Rect position, int index)
        {
#if DARKER_UI
            EditorGUI.DrawRect(position, CommunityStyles.backgroundColor);
#else
            EditorGUI.DrawRect(position, ColorPalette.unityBackgroundLight);
#endif
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
        private List<double?> foldoutHoverStartTimes = new List<double?>();
        public override void DrawItem(Rect position, int index)
        {
            if (!foldoutHoverStartTimes.Contains(index)) foldoutHoverStartTimes.Add(null);

            position.x -= 20;
            position.width += 20;
            var element = metadata[index];

            var oldHandleRect = new Rect(position.x + 4, position.y + position.height / 2f - 3, 9, 7);
#if DARKER_UI
            EditorGUI.DrawRect(oldHandleRect, CommunityStyles.backgroundColor);
#else
            EditorGUI.DrawRect(oldHandleRect, ColorPalette.unityBackgroundLight);
#endif
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
                parentInspector.SetHeightDirty();
                Event.current.Use();
            }

            var e = Event.current;

            bool draggingObjects = (DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0) ||
            DragAndDrop.GetGenericData(VisualScripting.DraggedListItem.TypeName) != null ||
            DragAndDrop.GetGenericData(DraggedDictionaryItem.TypeName) != null;

            if (e != null && draggingObjects && e.type == EventType.MouseDrag && e.button == (int)MouseButton.Left && boxRect.Contains(e.mousePosition))
            {
                const float expandDelay = 0.35f;
                if (!foldoutHoverStartTimes[index].HasValue)
                    foldoutHoverStartTimes[index] = EditorApplication.timeSinceStartup;

                if (EditorApplication.timeSinceStartup - foldoutHoverStartTimes[index].Value > expandDelay)
                    foldoutStates[index] = true;

                parentInspector.SetHeightDirty();
                GUI.changed = true;
            }
            else
            {
                foldoutHoverStartTimes[index] = null;
            }

            Rect deleteRect = new Rect(position.x + position.width - DeleteButtonWidth - 4, lineY - 2f, DeleteButtonWidth - 2, FieldHeight - 2);
            if (GUI.Button(deleteRect, "", new GUIStyle(EditorStyles.whiteLabel)
            {
                normal = { background = CommunityStyles.RemoveItemTexture }
            }))
            {
                if (CanRemove(index))
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

        public override void Remove(int index)
        {
            foldoutStates.RemoveAt(index);
            base.Remove(index);
            GUIUtility.keyboardControl = 0;
            GUIUtility.hotControl = 0;
        }

        public override void Add()
        {
            foldoutStates.Add(true);
            base.Add();
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

        public override float GetItemAdaptiveWidth(int index)
        {
            var element = metadata[index];
            EnsureFoldoutCount(index);

            const float foldoutArrowWidth = 12f;

            GUIContent label = CommunityStyles.GetCollectionDisplayName(element, index);
            float labelWidth = GUI.skin.label.CalcSize(label).x;

            float baseWidth = DragHandleWidth + foldoutArrowWidth + labelWidth + DeleteButtonWidth;

            float inspectorWidth = 0f;
            if (foldoutStates[index])
            {
                try
                {
                    var inspector = element.Inspector();
                    if (inspector != null)
                    {
                        float contentWidth = inspector.GetAdaptiveWidth();
                        inspectorWidth = contentWidth + 10f;
                    }
                }
                catch (Exception)
                {
                }
            }

            return Mathf.Max(baseWidth, inspectorWidth);
        }
    }
}