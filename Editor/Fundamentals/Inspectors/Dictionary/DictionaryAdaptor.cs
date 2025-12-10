using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Unity.VisualScripting.ReorderableList;
using System;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    public class DictionaryAdaptor : MetadataDictionaryAdaptor, IReorderableListDropTarget
    {
        private readonly List<bool> foldoutStates = new List<bool>();

        private const float FoldoutHeight = 20f;
        private const float FieldHeight = 18f;
        private const float Spacing = 4f;
        private const float DeleteButtonWidth = 18f;
        private const float spaceBetweenKeyAndValue = 5;
        private const float itemPadding = 2;

        private Metadata newKeyMetadata;
        private Metadata newValueMetadata;

        private static readonly FieldInfo listControlFieldInfo = typeof(MetadataCollectionAdaptor).GetField("listControl", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly PropertyInfo metadataLabelProperty = typeof(Metadata).GetProperty("label", BindingFlags.Instance | BindingFlags.Public);

        public ReorderableListControl listControl;

        public DictionaryAdaptor(Metadata metadata, Inspector parent) : base(metadata, parent)
        {
            Metadata = metadata;
            valueChanged += (previousValue) =>
            {
                Initialize();
            };

            metadata.valueChanged += (previousValue) =>
            {
                Initialize();
            };

            if (listControlFieldInfo != null)
            {
                listControl = listControlFieldInfo.GetValue(this) as ReorderableListControl;
                if (listControl != null)
                {
                    listControl.ContainerStyle = GUIStyle.none;
                    listControl.Flags = ReorderableListFlags.HideRemoveButtons | ReorderableListFlags.DisableReordering;
                    listControl.HorizontalLineColor = EditorGUIUtility.isProSkin ? Color.black : Color.white;
                    listControl.HorizontalLineAtStart = true;
                    listControl.HorizontalLineAtEnd = true;
                }
            }
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
        private void Initialize()
        {
            if (!metadata.isDictionary)
            {
                throw new InvalidOperationException("Metadata for dictionary adaptor is not a dictionary: " + metadata);
            }

            if (metadata.value == null)
            {
                metadata.value = ConstructDictionary();
            }

            newKeyMetadata?.Unlink();
            newValueMetadata?.Unlink();

            // It seems like Unlink is not enough.
            // so we make sure to give it a new name
            var guid = GUID.Generate().ToString();

            // Todo: Find a way to overwrite the key and value
            // instead of using a new key. 
            newKeyMetadata = metadata.Object($"newKey_{guid}", ConstructKey(), metadata.dictionaryKeyType);
            newValueMetadata = metadata.Object($"newValue_{guid}", ConstructValue(), metadata.dictionaryValueType);

            // Some Metadata types use this, so we insure that its not null.
            metadataLabelProperty.SetValue(newKeyMetadata, GUIContent.none);
            metadataLabelProperty.SetValue(newValueMetadata, GUIContent.none);
        }

        protected override object ConstructKey()
        {
            return base.ConstructKey();
        }

        protected override object ConstructValue()
        {
            if (metadata.dictionaryValueType == typeof(object)) return null;
            if (typeof(UnityEngine.Object).IsAssignableFrom(metadata.dictionaryValueType)) return null;
            return metadata.dictionaryValueType.PseudoDefault() ?? metadata.dictionaryValueType.TryInstantiate(false) ?? base.ConstructKey();
        }

        public Metadata Metadata;

        private void EnsureFoldoutCount(int index)
        {
            if (index == Count - 1) return;

            while (foldoutStates.Count <= index)
            {
                foldoutStates.Add(false);
                parentInspector.SetHeightDirty();
            }
        }

        public override float GetItemHeight(float width, int index)
        {
            EnsureFoldoutCount(index);

            bool expanded = (index == Count - 1) ? newItemExpanded : foldoutStates[index];

            if (!expanded)
                return FoldoutHeight + Spacing;

            if (index == Count - 1)
                return FoldoutHeight + GetItemHeight(newKeyMetadata, newValueMetadata, index) + 8f;

            return FoldoutHeight + GetItemHeight(metadata.KeyMetadata(index), metadata.ValueMetadata(index), index) + 8f;
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

        private bool newItemExpanded = false;

        public override void DrawItem(Rect position, int index)
        {
            EnsureFoldoutCount(index);

            bool isNewItem = index == Count - 1;

            bool expanded = isNewItem ? newItemExpanded : foldoutStates[index];

            float lineY = position.y + (FoldoutHeight - 8) / 2f;

            Rect foldoutRect = new Rect(position.x + Spacing, position.y + 4, position.width - DeleteButtonWidth - 35, FieldHeight);
            Rect arrowRect = new Rect(foldoutRect.x, lineY, 12, 12);

            Texture2D arrowTex = expanded ? CommunityStyles.ArrowDownTexture : CommunityStyles.ArrowRightTexture;
            if (arrowTex != null)
                GUI.DrawTexture(arrowRect, arrowTex, ScaleMode.ScaleToFit, true);

            GUIContent label = isNewItem ? new GUIContent("New Item") : CommunityStyles.GetCollectionDisplayName(metadata.KeyMetadata(index), index, true);
            Rect labelRect = new Rect(foldoutRect.x + 16, lineY - 3, foldoutRect.width - 16, FieldHeight);
            GUI.Label(labelRect, label, EditorStyles.label);

            if (!isNewItem)
            {
                Rect deleteRect = new Rect(position.x + position.width - DeleteButtonWidth - 4, lineY - 2, DeleteButtonWidth - 2, FieldHeight - 2);
                if (GUI.Button(deleteRect, GUIContent.none, new GUIStyle(EditorStyles.whiteLabel)
                {
                    normal = { background = CommunityStyles.RemoveItemTexture }
                }))
                {
                    Remove(index);
                    return;
                }
            }

            if (Event.current.type == EventType.MouseDown && arrowRect.Contains(Event.current.mousePosition))
            {
                if (isNewItem)
                    newItemExpanded = !newItemExpanded;
                else
                    foldoutStates[index] = !expanded;

                parentInspector.SetHeightDirty();

                Event.current.Use();
            }

            if (expanded)
            {
                Rect contentRect = new Rect(position.x, position.y + FoldoutHeight + Spacing, position.width - Spacing, position.height - FoldoutHeight - Spacing);

                contentRect.x -= (arrowRect.width / 2) - Spacing;
                contentRect.width += (arrowRect.width / 2) - Spacing;

                if (isNewItem)
                {
                    DrawNewItem(contentRect);
                }
                else
                {
                    var keyMeta = metadata.KeyMetadata(index);
                    var valMeta = metadata.ValueMetadata(index);
                    OnItemGUI(keyMeta, valMeta, contentRect, false);
                }
            }

            if (index == Count - 1)
                return;

            var controlID = GUIUtility.GetControlID(FocusType.Passive);

            switch (Event.current.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    var draggablePosition = position;

                    if (Event.current.button == (int)MouseButton.Left && draggablePosition.Contains(Event.current.mousePosition))
                    {
                        GUIUtility.hotControl = controlID;
                        Event.current.Use();
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
                        DragAndDrop.SetGenericData(DraggedDictionaryItem.TypeName, new DraggedDictionaryItem(this, index, KeyValuePair.Create(metadata.KeyMetadata(index).value, item), foldoutStates[index]));
                        DragAndDrop.StartDrag(metadata.path);
                        Event.current.Use();
                    }

                    break;
            }
        }

        public new event Action<object, object> itemAdded;
        public event Action<object> valueChanged;

        public override void Add()
        {
            var newKey = newKeyMetadata.value;
            var newValue = newValueMetadata.value;

            if (!CanAdd())
            {
                return;
            }

            metadata.RecordUndo();
            metadata.Add(newKey, newValue);

            itemAdded?.Invoke(newKey, newValue);

            parentInspector.SetHeightDirty();
            foldoutStates.Add(true);
            valueChanged?.Invoke(metadata.value);
        }

        protected new bool CanAdd()
        {
            var newKey = newKeyMetadata.value;

            if (newKey == null)
            {
                EditorUtility.DisplayDialog("New Dictionary Item", "A dictionary key cannot be null.", "OK");
                return false;
            }

            if (metadata.Contains(newKeyMetadata.value))
            {
                EditorUtility.DisplayDialog("New Dictionary Item", "An item with the same key already exists.", "OK");
                return false;
            }

            return true;
        }

        public override void Clear()
        {
            base.Clear();
            valueChanged?.Invoke(metadata.value);
        }

        public override void Duplicate(int index)
        {
            base.Duplicate(index);
            valueChanged?.Invoke(metadata.value);
        }

        public override void Move(int sourceIndex, int destinationIndex)
        {
            base.Move(sourceIndex, destinationIndex);
            valueChanged?.Invoke(metadata.value);
        }

        public override void Insert(int index)
        {
            base.Insert(index);
            valueChanged?.Invoke(metadata.value);
        }

        public override void Remove(int index)
        {
            if (index < foldoutStates.Count)
                foldoutStates.RemoveAt(index);
            base.Remove(index);
            valueChanged?.Invoke(metadata.value);
            GUIUtility.keyboardControl = 0;
            GUIUtility.hotControl = 0;
        }

        private void DrawNewItem(Rect position)
        {
            var newItemPosition = new Rect
                (
                position.x,
                position.y,
                position.width,
                GetItemHeight(newKeyMetadata, newValueMetadata, position.width)
                );

            OnItemGUI(newKeyMetadata, newValueMetadata, newItemPosition, true);
        }

        private float GetItemHeight(Metadata keyMetadata, Metadata valueMetadata, float width)
        {
            return Mathf.Max(GetKeyHeight(keyMetadata, GetKeyWidth(width)), GetValueHeight(valueMetadata, GetValueWidth(width))) + (itemPadding * 2);
        }

        private void OnItemGUI(Metadata keyMetadata, Metadata valueMetadata, Rect position, bool editableKey)
        {
            var keyPosition = new Rect
                (
                position.x + itemPadding,
                position.y + itemPadding,
                GetKeyWidth(position.width),
                GetKeyHeight(keyMetadata, GetKeyWidth(position.width))
                );

            var valuePosition = new Rect
                (
                keyPosition.xMax + spaceBetweenKeyAndValue,
                position.y + itemPadding,
                GetValueWidth(position.width),
                GetValueHeight(valueMetadata, GetValueWidth(position.width))
                );

            EditorGUI.BeginDisabledGroup(!editableKey);
            OnKeyGUI(keyMetadata, keyPosition);
            EditorGUI.EndDisabledGroup();

            OnValueGUI(valueMetadata, valuePosition);
        }

        private void OnKeyGUI(Metadata keyMetadata, Rect keyPosition)
        {
            LudiqGUI.Inspector(keyMetadata, keyPosition, GUIContent.none);
        }

        private void OnValueGUI(Metadata valueMetadata, Rect valuePosition)
        {
            LudiqGUI.Inspector(valueMetadata, valuePosition, GUIContent.none);
        }

        private float GetKeyHeight(Metadata keyMetadata, float keyWidth)
        {
            return LudiqGUI.GetInspectorHeight(parentInspector, keyMetadata, keyWidth, GUIContent.none);
        }

        private float GetValueHeight(Metadata valueMetadata, float valueWidth)
        {
            return LudiqGUI.GetInspectorHeight(parentInspector, valueMetadata, valueWidth, GUIContent.none);
        }

        private float GetKeyWidth(float width)
        {
            return (width - spaceBetweenKeyAndValue) / 2;
        }

        private float GetValueWidth(float width)
        {
            return (width - spaceBetweenKeyAndValue) / 2;
        }

        public bool CanDropInsert(int insertionIndex)
        {
            if (insertionIndex != Count - 1)
                return false;
            if (!ReorderableListControl.CurrentListPosition.Contains(Event.current.mousePosition))
            {
                return false;
            }

            var data = DragAndDrop.GetGenericData(DraggedDictionaryItem.TypeName);

            if (data is DraggedDictionaryItem draggedDictionaryItem && draggedDictionaryItem.item is KeyValuePair<object, object> valuePair)
            {
                return !metadata.Contains(valuePair.Key) && metadata.dictionaryKeyType.IsInstanceOfType(valuePair.Key) && metadata.dictionaryValueType.IsInstanceOfType(valuePair.Value);
            }

            return false;
        }

        protected virtual bool CanDrop(object item)
        {
            return true;
        }

        public void ProcessDropInsertion(int insertionIndex)
        {
            if (Event.current.type == EventType.DragPerform)
            {
                var draggedItem = (DraggedDictionaryItem)DragAndDrop.GetGenericData(DraggedDictionaryItem.TypeName);

                if (draggedItem.sourceDictionaryAdaptor != this)
                {
                    if (CanDrop(draggedItem.item))
                    {
                        var pair = (KeyValuePair<object, object>)draggedItem.item;
                        metadata.Add(pair.Key, pair.Value);

                        draggedItem.sourceDictionaryAdaptor.Remove(draggedItem.index);
                        draggedItem.sourceDictionaryAdaptor.parentInspector.SetHeightDirty();
                        parentInspector.SetHeightDirty();
                        GUI.changed = true;
                        Event.current.Use();
                    }
                }
            }
        }

        public override float GetItemAdaptiveWidth(int index)
        {
            EnsureFoldoutCount(index);

            bool isNewItem = index == Count - 1;
            bool expanded = isNewItem ? newItemExpanded : foldoutStates[index];

            const float foldoutArrowWidth = 12f;
            const float padding = 10f;

            GUIContent label = isNewItem
                ? new GUIContent("New Item")
                : CommunityStyles.GetCollectionDisplayName(metadata.KeyMetadata(index), index, true);

            float labelWidth = GUI.skin.label.CalcSize(label).x;

            float baseWidth = foldoutArrowWidth + labelWidth + DeleteButtonWidth + padding;

            float keyWidth = 0f;
            float valueWidth = 0f;

            if (expanded)
            {
                try
                {
                    var keyInspector = metadata.KeyMetadata(index).Inspector();
                    var valueInspector = metadata.ValueMetadata(index).Inspector();

                    if (keyInspector != null)
                    {
                        keyWidth = keyInspector.GetAdaptiveWidth();
                    }

                    if (valueInspector != null)
                    {
                        valueWidth = valueInspector.GetAdaptiveWidth();
                    }
                }
                catch (Exception)
                {
                }
            }

            float contentWidth = keyWidth + valueWidth + spaceBetweenKeyAndValue + itemPadding * 2;

            return Mathf.Max(baseWidth, contentWidth + padding);
        }
    }
}