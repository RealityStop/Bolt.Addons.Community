using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.ReorderableList;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class DictionaryAdaptor : MetadataDictionaryAdaptor, IReorderableListDropTarget
    {
        public ReorderableListControl listControl;
        public Metadata Metadata;
        public event Action<object> valueChanged;

        private readonly List<bool> foldoutStates = new List<bool>();
        private Metadata newKeyMetadata;
        private Metadata newValueMetadata;
        private bool newItemExpanded = false;

        private const float FoldoutHeight = 20f;
        private const float FieldHeight = 18f;
        private const float Spacing = 4f;
        private const float DeleteButtonWidth = 18f;
        private const float SpaceBetweenKeyAndValue = 5f;
        private const float ItemPadding = 2f;
        private const float FoldoutArrowWidth = 12f;
        private const float AdaptiveWidthPadding = 10f;
        private const float ExtraItemPadding = 8f;

        private static readonly FieldInfo listControlFieldInfo = typeof(MetadataCollectionAdaptor).GetField("listControl", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly PropertyInfo metadataLabelProperty = typeof(Metadata).GetProperty("label", BindingFlags.Instance | BindingFlags.Public);

        public DictionaryAdaptor(Metadata metadata, Inspector parent) : base(metadata, parent)
        {
            Metadata = metadata;

            Action<object> reinitialize = (previousValue) => Initialize();
            valueChanged += reinitialize;
            metadata.valueChanged += reinitialize;

            if (listControlFieldInfo?.GetValue(this) is ReorderableListControl control)
            {
                listControl = control;
                listControl.Flags = ReorderableListFlags.DisableReordering;
            }
        }

        private void Initialize()
        {
            if (!metadata.isDictionary)
                throw new InvalidOperationException($"Metadata for dictionary adaptor is not a dictionary: {metadata}");

            metadata.value ??= ConstructDictionary();

            newKeyMetadata?.Unlink();
            newValueMetadata?.Unlink();

            string guid = GUID.Generate().ToString();
            newKeyMetadata = metadata.Object($"newKey_{guid}", ConstructKey(), metadata.dictionaryKeyType);
            newValueMetadata = metadata.Object($"newValue_{guid}", ConstructValue(), metadata.dictionaryValueType);

            metadataLabelProperty?.SetValue(newKeyMetadata, GUIContent.none);
            metadataLabelProperty?.SetValue(newValueMetadata, GUIContent.none);
        }

        protected override IDictionary ConstructDictionary()
        {
            if (metadata.dictionaryType == typeof(IDictionary))
                return new AotDictionary();

            if (metadata.dictionaryType.IsGenericType && metadata.dictionaryType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                var args = metadata.dictionaryType.GetGenericArguments();
                return (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(args[0], args[1]));
            }

            return base.ConstructDictionary();
        }

        protected override object ConstructValue()
        {
            if (metadata.dictionaryValueType == typeof(object) || typeof(UnityEngine.Object).IsAssignableFrom(metadata.dictionaryValueType))
                return null;

            return metadata.dictionaryValueType.PseudoDefault() ??
                   metadata.dictionaryValueType.TryInstantiate(false) ??
                   base.ConstructKey();
        }

        public override float GetItemHeight(float width, int index)
        {
            EnsureFoldoutStateSynced(index);

            bool isNewItem = index == Count - 1;
            bool expanded = isNewItem ? newItemExpanded : foldoutStates[index];

            if (!expanded) return FoldoutHeight + Spacing;

            float contentHeight = isNewItem
                ? GetItemContentHeight(newKeyMetadata, newValueMetadata, width)
                : GetItemContentHeight(metadata.KeyMetadata(index), metadata.ValueMetadata(index), width);

            return FoldoutHeight + contentHeight + ExtraItemPadding;
        }

        public override void DrawItem(Rect position, int index)
        {
            EnsureFoldoutStateSynced(index);

            bool isNewItem = index == Count - 1;
            Rect foldoutRect = new Rect(position.x + Spacing, position.y + Spacing, position.width, FieldHeight);

            GUIContent label = isNewItem
                ? new GUIContent("New Item")
                : CommunityStyles.GetCollectionDisplayName(metadata.KeyMetadata(index), index, true);

            bool expanded = DrawFoldout(foldoutRect, index, label, isNewItem);

            if (expanded)
            {
                DrawExpandedContent(position, index, isNewItem);
            }

            if (!isNewItem)
            {
                HandleDragAndDrop(position, index);
            }
        }

        private bool DrawFoldout(Rect foldoutRect, int index, GUIContent label, bool isNewItem)
        {
            using (new EditorGUIUtility.IconSizeScope(new Vector2(IconSize.Small, IconSize.Small)))
            {
                var oldHierarchyMode = EditorGUIUtility.hierarchyMode;
                EditorGUIUtility.hierarchyMode = false;

                EditorGUI.BeginChangeCheck();

                bool expanded = isNewItem
                    ? (newItemExpanded = EditorGUI.Foldout(foldoutRect, newItemExpanded, label))
                    : (foldoutStates[index] = EditorGUI.Foldout(foldoutRect, foldoutStates[index], label));


                if (EditorGUI.EndChangeCheck())
                {
                    parentInspector.SetHeightDirty();
                }

                EditorGUIUtility.hierarchyMode = oldHierarchyMode;
                return expanded;
            }
        }

        private void DrawExpandedContent(Rect position, int index, bool isNewItem)
        {
            Rect contentRect = new Rect(
                position.x,
                position.y + FoldoutHeight + Spacing,
                position.width,
                position.height - FoldoutHeight - Spacing);

            if (isNewItem)
            {
                Rect newItemPosition = new Rect(contentRect.x, contentRect.y, contentRect.width, GetItemContentHeight(newKeyMetadata, newValueMetadata, contentRect.width));
                OnItemGUI(newKeyMetadata, newValueMetadata, newItemPosition, editableKey: true);
            }
            else
            {
                OnItemGUI(metadata.KeyMetadata(index), metadata.ValueMetadata(index), contentRect, editableKey: false);
            }
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
                        GUIUtility.hotControl = controlID;
                        e.Use();
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

                        var pair = KeyValuePair.Create(metadata.KeyMetadata(index).value, item);
                        DragAndDrop.SetGenericData(DraggedDictionaryItem.TypeName, new DraggedDictionaryItem(this, index, pair, foldoutStates[index]));

                        DragAndDrop.StartDrag(metadata.path);
                        e.Use();
                    }
                    break;
            }
        }

        private void EnsureFoldoutStateSynced(int index)
        {
            if (index == Count - 1) return;

            while (foldoutStates.Count <= index)
            {
                foldoutStates.Add(false);
                parentInspector.SetHeightDirty();
            }
        }

        public override void Add()
        {
            if (!CanAdd()) return;

            metadata.RecordUndo();
            metadata.Add(newKeyMetadata.value, newValueMetadata.value);

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

            if (metadata.Contains(newKey))
            {
                EditorUtility.DisplayDialog("New Dictionary Item", "An item with the same key already exists.", "OK");
                return false;
            }

            return true;
        }

        public override void Clear() { base.Clear(); InvokeValueChanged(); }
        public override void Duplicate(int index) { base.Duplicate(index); InvokeValueChanged(); }
        public override void Move(int sourceIndex, int destinationIndex) { base.Move(sourceIndex, destinationIndex); InvokeValueChanged(); }
        public override void Insert(int index) { base.Insert(index); InvokeValueChanged(); }

        public override void Remove(int index)
        {
            if (index < foldoutStates.Count) foldoutStates.RemoveAt(index);
            base.Remove(index);
            InvokeValueChanged();
            ClearHotControls();
        }

        private void InvokeValueChanged() => valueChanged?.Invoke(metadata.value);

        private void ClearHotControls()
        {
            GUIUtility.keyboardControl = 0;
            GUIUtility.hotControl = 0;
        }

        private float GetItemContentHeight(Metadata keyMetadata, Metadata valueMetadata, float width)
        {
            float halfWidth = GetHalfWidth(width);
            return Mathf.Max(
                LudiqGUI.GetInspectorHeight(parentInspector, keyMetadata, halfWidth, GUIContent.none),
                LudiqGUI.GetInspectorHeight(parentInspector, valueMetadata, halfWidth, GUIContent.none)) + (ItemPadding * 2);
        }

        private void OnItemGUI(Metadata keyMetadata, Metadata valueMetadata, Rect position, bool editableKey)
        {
            float halfWidth = GetHalfWidth(position.width);

            Rect keyPosition = new Rect(
                position.x + ItemPadding,
                position.y + ItemPadding,
                halfWidth,
                LudiqGUI.GetInspectorHeight(parentInspector, keyMetadata, halfWidth, GUIContent.none));

            Rect valuePosition = new Rect(
                keyPosition.xMax + SpaceBetweenKeyAndValue,
                position.y + ItemPadding,
                halfWidth,
                LudiqGUI.GetInspectorHeight(parentInspector, valueMetadata, halfWidth, GUIContent.none));

            EditorGUI.BeginDisabledGroup(!editableKey);
            LudiqGUI.Inspector(keyMetadata, keyPosition, GUIContent.none);
            EditorGUI.EndDisabledGroup();

            LudiqGUI.Inspector(valueMetadata, valuePosition, GUIContent.none);
        }

        private float GetHalfWidth(float totalWidth) => (totalWidth - SpaceBetweenKeyAndValue) / 2;

        public bool CanDropInsert(int insertionIndex)
        {
            if (insertionIndex != Count - 1 || !ReorderableListControl.CurrentListPosition.Contains(Event.current.mousePosition))
                return false;

            return DragAndDrop.GetGenericData(DraggedDictionaryItem.TypeName) is DraggedDictionaryItem draggedData &&
                   draggedData.item is KeyValuePair<object, object> valuePair &&
                   !metadata.Contains(valuePair.Key) &&
                   metadata.dictionaryKeyType.IsInstanceOfType(valuePair.Key) &&
                   metadata.dictionaryValueType.IsInstanceOfType(valuePair.Value);
        }

        protected virtual bool CanDrop(object item) => true;

        public void ProcessDropInsertion(int insertionIndex)
        {
            if (Event.current.type == EventType.DragPerform)
            {
                if (DragAndDrop.GetGenericData(DraggedDictionaryItem.TypeName) is DraggedDictionaryItem draggedItem &&
                    draggedItem.sourceDictionaryAdaptor != this &&
                    CanDrop(draggedItem.item))
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

        public override float GetItemAdaptiveWidth(int index)
        {
            EnsureFoldoutStateSynced(index);

            bool isNewItem = index == Count - 1;
            bool expanded = isNewItem ? newItemExpanded : foldoutStates[index];

            GUIContent label = isNewItem ? new GUIContent("New Item") : CommunityStyles.GetCollectionDisplayName(metadata.KeyMetadata(index), index, true);
            float baseWidth = FoldoutArrowWidth + GUI.skin.label.CalcSize(label).x + DeleteButtonWidth + AdaptiveWidthPadding;

            float contentWidth = 0f;

            if (expanded)
            {
                try
                {
                    float keyWidth = metadata.KeyMetadata(index).Inspector()?.GetAdaptiveWidth() ?? 0f;
                    float valueWidth = metadata.ValueMetadata(index).Inspector()?.GetAdaptiveWidth() ?? 0f;
                    contentWidth = keyWidth + valueWidth + SpaceBetweenKeyAndValue + (ItemPadding * 2);
                }
                catch (Exception)
                {

                }
            }

            return Mathf.Max(baseWidth, contentWidth + AdaptiveWidthPadding);
        }
    }
}