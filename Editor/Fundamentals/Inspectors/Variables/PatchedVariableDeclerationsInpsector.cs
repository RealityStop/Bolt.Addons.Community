using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.ReorderableList;
using Unity.VisualScripting.ReorderableList.Internal;
using UnityEngine.SceneManagement;

namespace Unity.VisualScripting.Community
{
    public class PatchedVariableDeclarationsInspector : VariableDeclarationsInspector
    {
        private VariableDeclarationsAdaptor adaptor;
        private string newName;

        internal Dictionary<VariableDeclaration, VariableFoldout> foldouts;

        public PatchedVariableDeclarationsInspector(Metadata metadata) : base(metadata)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            var variableDecls = metadata.value as VariableDeclarations;

            if (variableDecls == null)
                return;

            var collection = metadata["collection"];

            var fresh = new Dictionary<VariableDeclaration, VariableFoldout>();

            foldouts ??= new Dictionary<VariableDeclaration, VariableFoldout>();

            foreach (var declaration in variableDecls)
            {
                if (foldouts.TryGetValue(declaration, out var existing))
                {
                    existing.name = declaration.name;
                    fresh[declaration] = existing;
                }
                else
                {
                    fresh[declaration] = new VariableFoldout(declaration.name, false);
                }
            }
            foldouts = fresh;

#pragma warning disable 618
            kind = metadata.GetAttribute<VariableKindAttribute>()?.kind;
#pragma warning restore 618
#if VISUAL_SCRIPTING_1_7
            kind ??= variableDecls.Kind;
#endif
            adaptor = new VariableDeclarationsAdaptor(collection, this);
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            if (metadata.value == null)
                return;

            position.x = 0;
            if (metadata.parent.definedType == typeof(VisualScripting.Variables))
            {
                position.width = LudiqGUIUtility.currentInspectorWidthWithoutScrollbar;
            }

            if (EditorPrefs.GetBool(ProjectSettingsProviderView.ShowVariablesQuickbarKey, false) && !metadata.HasAttribute<HideVariablesQuickbarAttribute>())
            {
                DrawQuickAddToolbar(position);

                position.y += (ButtonHeight * 2) + ButtonSpacingY + (ToolbarPadding * 2);
                position.height -= (ButtonHeight * 2) + ButtonSpacingY + (ToolbarPadding * 2);
            }

            var normal = GUI.backgroundColor;
            adaptor.Field(position, label);
            // Restore color after tinting add button 
            GUI.backgroundColor = normal;

            if (metadata.parent.definedType != typeof(VisualScripting.Variables))
            {
                position.width -= 1;
            }

            var newNamePosition = new Rect(position.x, position.yMax - 20, position.width - Styles.addButtonWidth, 18);

            if (adaptor.Count == 0)
            {
                newNamePosition.y = position.yMax - 20;
            }

            newNamePosition.height += 1;
            OnNewNameGUI(newNamePosition);
        }

        private static readonly GUIContent[] quickTypesLabels =
        {
            new GUIContent("Float", typeof(float).Icon()[IconSize.Small]), new GUIContent("Int", typeof(int).Icon()[IconSize.Small]), new GUIContent("Bool", typeof(bool).Icon()[IconSize.Small]),
            new GUIContent("String", typeof(string).Icon()[IconSize.Small]),
            new GUIContent("Vector", typeof(Vector4).Icon()[IconSize.Small]), new GUIContent("Color", typeof(Color).Icon()[IconSize.Small]), new GUIContent("Object", typeof(GameObject).Icon()[IconSize.Small]),
            new GUIContent("Other", typeof(Generic).Icon()[IconSize.Small])
        };

        private static readonly string[] quickTypes = { "Float", "Int", "Bool", "String", "Vector", "Color", "Object", "Other" };

        private const float Spacing = 4f;
        private const float ButtonHeight = 27f;
        private const float ButtonSpacingX = 4f;
        private const float ButtonSpacingY = 4f;
        private const float CornerRadius = 10f;
        private const float ToolbarPadding = 6f;

        private void DrawQuickAddToolbar(Rect position)
        {
            int columns = 4;
            int rows = 2;
            float totalHeight = (ButtonHeight * rows) + ButtonSpacingY + (ToolbarPadding * 2);
            Rect toolbarRect = new Rect(position.x, position.y, position.width, totalHeight);
#if DARKER_UI
            EditorGUI.DrawRect(toolbarRect, CommunityStyles.backgroundColor);
#else
            EditorGUI.DrawRect(toolbarRect, ColorPalette.unityBackgroundLight);
#endif
            float availableWidth = toolbarRect.width - ToolbarPadding * 2;

            float totalSpacingX = ButtonSpacingX * (columns - 1);
            float buttonWidthAdjusted = (availableWidth - totalSpacingX) / columns;

            float xStart = toolbarRect.x + ToolbarPadding;
            float yStart = toolbarRect.y + ToolbarPadding;

            for (int i = 0; i < quickTypesLabels.Length; i++)
            {
                int row = i / columns;
                int col = i % columns;

                float x = xStart + col * (buttonWidthAdjusted + ButtonSpacingX);
                float y = yStart + row * (ButtonHeight + ButtonSpacingY);

                Rect buttonRect = new Rect(x, y, buttonWidthAdjusted, ButtonHeight);

                if (DrawRoundedButton(buttonRect, quickTypesLabels[i], CornerRadius, out var mouse))
                {
                    bool isRight = mouse == MouseButton.Right;
                    switch (quickTypes[i])
                    {
                        case "Float":
                            {
                                var menu = new GenericMenu();
                                menu.AddItem(new GUIContent("float"), false, () => AddQuickVariable(isRight ? typeof(List<float>) : typeof(float), true));
                                menu.AddItem(new GUIContent("double"), false, () => AddQuickVariable(isRight ? typeof(List<double>) : typeof(double), true));
                                menu.AddItem(new GUIContent("decimal"), false, () => AddQuickVariable(isRight ? typeof(List<decimal>) : typeof(decimal), true));
                                menu.DropDown(buttonRect);
                                break;
                            }

                        case "Int":
                            {
                                var menu = new GenericMenu();
                                menu.AddItem(new GUIContent("int"), false, () => AddQuickVariable(isRight ? typeof(List<int>) : typeof(int)));
                                menu.AddItem(new GUIContent("short"), false, () => AddQuickVariable(isRight ? typeof(List<short>) : typeof(short), true));
                                menu.AddItem(new GUIContent("long"), false, () => AddQuickVariable(isRight ? typeof(List<long>) : typeof(long), true));
                                menu.AddItem(new GUIContent("byte"), false, () => AddQuickVariable(isRight ? typeof(List<byte>) : typeof(byte), true));
                                menu.AddItem(new GUIContent("sbyte"), false, () => AddQuickVariable(isRight ? typeof(List<sbyte>) : typeof(sbyte), true));
                                menu.AddSeparator("");
                                menu.AddItem(new GUIContent("uint"), false, () => AddQuickVariable(isRight ? typeof(List<uint>) : typeof(uint), true));
                                menu.AddItem(new GUIContent("ushort"), false, () => AddQuickVariable(isRight ? typeof(List<ushort>) : typeof(ushort), true));
                                menu.AddItem(new GUIContent("ulong"), false, () => AddQuickVariable(isRight ? typeof(List<ulong>) : typeof(ulong), true));
                                menu.DropDown(buttonRect);
                                break;
                            }

                        case "String":
                            AddQuickVariable(isRight ? typeof(List<string>) : typeof(string), true);
                            break;

                        case "Bool":
                            AddQuickVariable(isRight ? typeof(List<bool>) : typeof(bool), true);
                            break;

                        case "Vector":
                            {
                                var menu = new GenericMenu();
                                menu.AddItem(new GUIContent("Vector 2"), false, () => AddQuickVariable(isRight ? typeof(List<Vector2>) : typeof(Vector2), true));
                                menu.AddItem(new GUIContent("Vector 3"), false, () => AddQuickVariable(isRight ? typeof(List<Vector3>) : typeof(Vector3), true));
                                menu.AddItem(new GUIContent("Vector 4"), false, () => AddQuickVariable(isRight ? typeof(List<Vector4>) : typeof(Vector4), true));
                                menu.AddSeparator("");
                                menu.AddItem(new GUIContent("Vector 2 Int"), false, () => AddQuickVariable(isRight ? typeof(List<Vector2Int>) : typeof(Vector2Int), true));
                                menu.AddItem(new GUIContent("Vector 3 Int"), false, () => AddQuickVariable(isRight ? typeof(List<Vector3Int>) : typeof(Vector3Int), true));
                                menu.DropDown(buttonRect);
                                break;
                            }

                        case "Color":
                            {
                                var menu = new GenericMenu();
                                menu.AddItem(new GUIContent("Color"), false, () => AddQuickVariable(isRight ? typeof(List<Color>) : typeof(Color)));
                                menu.AddItem(new GUIContent("HDRColor"), false, () => AddQuickVariable(isRight ? typeof(List<HDRColor>) : typeof(HDRColor), true));
                                menu.AddItem(new GUIContent("Gradient"), false, () => AddQuickVariable(isRight ? typeof(List<Gradient>) : typeof(Gradient), true));
                                menu.DropDown(buttonRect);
                                break;
                            }

                        case "Object":
                            AddQuickVariable(isRight ? typeof(List<GameObject>) : typeof(GameObject), true);
                            break;

                        case "Other":
                            TypeBuilderWindow.ShowWindow(buttonRect, (t) =>
                            {
                                bool ask = false;
                                if (e.shift)
                                {
                                    ask = true;
                                }
                                AddQuickVariable(t, ask);
                            }, typeof(object), true, Array.Empty<Type>());
                            break;
                    }
                }
            }
        }

        private static bool DrawRoundedButton(Rect rect, GUIContent content, float radius, out MouseButton? mouseButton)
        {
            var rectborder = CommunityStyles.OutlineTexture;

            var style = new GUIStyle(GUI.skin.box)
            {
                border = new RectOffset((int)radius, (int)radius, (int)radius, (int)radius),
                padding = new RectOffset(4, 4, 2, 2),
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = !EditorGUIUtility.isProSkin ? Color.black : Color.white }
            };

            bool isClick = false;

            var previous = EditorGUIUtility.GetIconSize();

            var e = Event.current;

            bool isRightClick = e != null && rect.Contains(e.mousePosition) && e.type == EventType.MouseDown && e.button == 1;

            EditorGUIUtility.SetIconSize(new Vector2(16, 16));

            rect.Draw().Image(style, content).TintedButton(ref isClick, e, rectborder, Color.gray.Brighten(0.1f), CommunityStyles.backgroundColor);

            EditorGUIUtility.SetIconSize(previous);

            var texRect = new Rect(rect.xMax - 14, rect.y + 9, EditorGUIUtility.isProSkin ? 8 : 10, 10);
            GUI.DrawTexture(texRect, ReorderableListResources.GetTexture(ReorderableListTexture.Icon_Add_Normal));

            if (isClick)
            {
                mouseButton = isRightClick ? MouseButton.Right : MouseButton.Left;
                e.Use();
                return true;
            }

            mouseButton = null;
            return false;
        }

        private void AddQuickVariable(Type type, bool ask = false)
        {
            string typeKey = Application.dataPath + $"_Community_ShowTypePopup_{type.AssemblyQualifiedName}";

            bool hasSeenPopup = EditorPrefs.GetBool(typeKey, false);

            if (ask && !Codebase.settingsTypes.Contains(type) && !hasSeenPopup)
            {
                int choice = EditorUtility.DisplayDialogComplex(
                    "Add Type to Settings",
                    $"The type '{type.As().CSharpName(false, false, false)}' is not currently in your settings types.\n\nWould you like to add it?",
                    "Add",
                    "No",
                    "Add && Regenerate"
                );

                var coreConfig = BoltCore.Configuration;

                switch (choice)
                {
                    case 0:
                        coreConfig.typeOptions.Add(type);
                        SaveCoreConfig(coreConfig);
                        Codebase.UpdateSettings();
                        break;

                    case 1:
                        break;

                    case 2:
                        coreConfig.typeOptions.Add(type);
                        SaveCoreConfig(coreConfig);
                        Codebase.UpdateSettings();
                        UnitBase.Rebuild();
                        break;
                }

                EditorPrefs.SetBool(typeKey, true);
            }

            var variableDecls = (VariableDeclarations)metadata.value;

            var collection = (VariableDeclarationCollection)metadata["collection"].value;

            string baseName = type.HumanName(false);
            string newVarName = baseName;
            int counter = 1;

            while (variableDecls.IsDefined(newVarName))
            {
                newVarName = $"{baseName} ({counter++})";
            }

            var newVar = new VariableDeclaration(newVarName, Default(type));
#if VISUAL_SCRIPTING_1_7
            newVar.typeHandle = new SerializableType(type.AssemblyQualifiedName);
#endif
            collection.Add(newVar);

            metadata.RecordUndo();

            if (foldouts == null)
                foldouts = new Dictionary<VariableDeclaration, VariableFoldout>();

            foldouts[newVar] = new VariableFoldout(newVarName, true);

            SetHeightDirty();
        }

        private object Default(Type type)
        {
            var value = type.PseudoDefault();

            if (value == null && type == typeof(Gradient)) return new Gradient();
            return value;
        }

        private void SaveCoreConfig(BoltCoreConfiguration coreConfig)
        {
            var metadata = coreConfig.GetMetadata(nameof(coreConfig.typeOptions));

            metadata.Inspector().SetHeightDirty();
#if VISUAL_SCRIPTING_1_9_0_OR_GREATER
            metadata.GetType()
                .GetMethod("SaveImmediately", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(metadata, new object[] { true });
#else
            metadata.Save();
#endif
        }

        private bool highlightPlaceholder;
        private bool highlightNewNameField;
        private const string newNameFieldControl = "newNameField";

        private void OnNewNameGUI(Rect newNamePosition)
        {
            EditorGUI.BeginChangeCheck();

            GUI.SetNextControlName(newNameFieldControl);
            newName = EditorGUI.TextField(newNamePosition, newName, highlightNewNameField ? Styles.newNameFieldHighlighted : Styles.newNameField);

            var e = UnityEngine.Event.current;
            if (GUI.GetNameOfFocusedControl() == newNameFieldControl && e.type == EventType.KeyUp && e.keyCode == KeyCode.Return)
            {
                adaptor.Add();
                GUI.FocusControl(newNameFieldControl);
                GUI.changed = true;
            }

            if (EditorGUI.EndChangeCheck())
            {
                highlightNewNameField = false;
                highlightPlaceholder = false;
            }

            if (string.IsNullOrEmpty(newName))
            {
                GUI.Label(newNamePosition, "(New Variable Name)", highlightPlaceholder ? Styles.placeholderHighlighted : Styles.placeholder);
            }
        }

        protected override float GetHeight(float width, GUIContent label)
        {
            return adaptor.GetHeight(width, label) + (EditorPrefs.GetBool(ProjectSettingsProviderView.ShowVariablesQuickbarKey, false) && !metadata.HasAttribute<HideVariablesQuickbarAttribute>() ? (ButtonHeight * 2) + ButtonSpacingY + (ToolbarPadding * 2) : 0);
        }

        public class VariableDeclarationsAdaptor : MetadataListAdaptor, IReorderableListDropTarget
        {
            private const float FoldoutHeight = 30f;
            private const float FieldHeight = 18f;
            private const float VerticalSpacing = 4f;
            private const float DeleteButtonWidth = 18f;

            public ReorderableListControl listControl;
            private static readonly FieldInfo listControlFieldInfo = typeof(MetadataCollectionAdaptor).GetField("listControl", BindingFlags.NonPublic | BindingFlags.Instance);

            public new readonly PatchedVariableDeclarationsInspector parentInspector;

            public VariableDeclarationsAdaptor(Metadata metadata, PatchedVariableDeclarationsInspector parent) : base(metadata, parent)
            {
                parentInspector = parent;

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

            protected override bool CanDrop(object item)
            {
                var variableDeclaration = (VariableDeclaration)item;

                if (((VariableDeclarations)parentInspector.metadata.value).IsDefined(variableDeclaration.name))
                {
                    EditorUtility.DisplayDialog("Dragged Variable", "A variable with the same name already exists.", "OK");
                    return false;
                }

                return base.CanDrop(item);
            }

#if DARKER_UI
            // I have to do this setup to change the color of the add button
            // It's very hacky but seems to work better than tinting the background Texture.
            private Color _previousBackgroundColor;
            private bool _tintApplied;
            private bool initialized;
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

                if (initialized)
                {
                    return;
                }

                initialized = true;

                for (int i = 0; i < (metadata.value as VariableDeclarationCollection).Count; i++)
                {
                    var element = metadata[i];
                    var valueMetadata = element["value"];

                    var inspector = valueMetadata.Inspector();
                    var valueInspector = typeof(SystemObjectInspector).GetField("inspector", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(inspector);
                    valueInspector.GetType().GetMethod("ResolveType", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(valueInspector, Array.Empty<object>());
                }
            }

            /// <summary>
            /// Called after all list elements are drawn but before drawing the Add Item button.
            /// Tints the Add button only.
            /// </summary>
            public override void EndGUI()
            {
                _previousBackgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = CommunityStyles.backgroundColor.Brighten(0.36f);
                _tintApplied = true;
            }
#endif
            public override float GetItemHeight(float width, int index)
            {
                var element = metadata[index];
                var declaration = (VariableDeclaration)element.value;

                if (parentInspector.foldouts == null)
                    parentInspector.foldouts = new Dictionary<VariableDeclaration, VariableFoldout>();

                if (!parentInspector.foldouts.TryGetValue(declaration, out var foldout))
                {
                    foldout = new VariableFoldout(declaration.name, false);
                    parentInspector.foldouts[declaration] = foldout;
                }

                if (!foldout.isExpanded)
                    return FoldoutHeight + VerticalSpacing;

                var valueHeight = LudiqGUI.GetInspectorHeight(parentInspector, element["value"], width, GUIContent.none);
                float h = FoldoutHeight + valueHeight + 6f;

                return h;
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
            private HashSet<Inspector> updatedInspectors = new HashSet<Inspector>();
            public override void DrawItem(Rect position, int index)
            {
                position.x -= 20;
                position.width += 20;

                var oldHandleRect = new Rect(position.x + 4, position.y + position.height / 2f - 3, 9, 7);
#if DARKER_UI
                EditorGUI.DrawRect(oldHandleRect, CommunityStyles.backgroundColor);
#else
                EditorGUI.DrawRect(oldHandleRect, ColorPalette.unityBackgroundLight);
#endif
                var element = metadata[index];
                var declaration = (VariableDeclaration)element.value;

                if (parentInspector.foldouts == null)
                    parentInspector.foldouts = new Dictionary<VariableDeclaration, VariableFoldout>();

                if (!parentInspector.foldouts.TryGetValue(declaration, out var foldout))
                {
                    foldout = new VariableFoldout(declaration.name, false);
                    parentInspector.foldouts[declaration] = foldout;
                }

                float y = position.y + 2f;
                Rect boxRect = new Rect(position.x, y, position.width, GetItemHeight(index));
                float lineY = boxRect.y + ((FoldoutHeight - 16f) / 2f) + Spacing - 2;

                float handleSize = 12f;
                Rect handleRect = new Rect(position.x + 2, lineY, handleSize, handleSize);
                GUI.DrawTexture(handleRect, CommunityStyles.DragHandleTexture);
                EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.Pan);

                Rect foldoutRect = new Rect(handleRect.x + handleRect.width + Spacing, lineY, 14, 16);
                Texture2D arrow = foldout.isExpanded ? CommunityStyles.ArrowDownTexture : CommunityStyles.ArrowRightTexture;
                if (arrow) GUI.DrawTexture(foldoutRect, arrow, ScaleMode.ScaleToFit);

                if (Event.current.type == EventType.MouseDown && foldoutRect.Contains(Event.current.mousePosition))
                {
                    foldout.isExpanded = !foldout.isExpanded;
                    parentInspector.foldouts[declaration] = foldout;
                    Event.current.Use();
                }

                var e = Event.current;

                bool draggingObjects = (DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0) ||
                DragAndDrop.GetGenericData(VisualScripting.DraggedListItem.TypeName) != null ||
                DragAndDrop.GetGenericData(DraggedDictionaryItem.TypeName) != null;

                if (e != null && draggingObjects && e.type == EventType.MouseDrag && e.button == (int)MouseButton.Left && boxRect.Contains(e.mousePosition))
                {
                    const float expandDelay = 0.35f;
                    if (!foldout.hoverStartTime.HasValue)
                        foldout.hoverStartTime = EditorApplication.timeSinceStartup;

                    if (EditorApplication.timeSinceStartup - foldout.hoverStartTime.Value > expandDelay)
                    {
                        foldout.isExpanded = true;
                        parentInspector.foldouts[declaration] = foldout;
                    }

                    parentInspector.SetHeightDirty();
                    GUI.changed = true;
                }
                else
                {
                    foldout.hoverStartTime = null;
                }

                float spacing = 4f;
                float startX = foldoutRect.xMax + spacing;
                float endX = position.x + position.width - DeleteButtonWidth - spacing;
                float totalAvailable = endX - startX;
                float halfWidth = (totalAvailable - spacing) / 2f;

                Rect nameRect = new Rect(startX, lineY - 2f, halfWidth, FieldHeight);
                Rect typeRect = new Rect(nameRect.xMax + spacing, lineY - 2f, halfWidth, FieldHeight);

                OnNameGUI(nameRect, element["name"]);

                using (adaptiveWidth.Override(true)) // Hide the Type label
                {
                    var typeInspector = element["typeHandle"].Inspector();
                    if (updatedInspectors.Add(typeInspector) && parentInspector.metadata.HasAttribute<TypeFilter>())
                    {
                        typeof(TypeHandleInspector).GetProperty("typeFilter", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(typeInspector, parentInspector.metadata.GetAttribute<TypeFilter>());
                    }
                    typeInspector.Draw(typeRect, GUIContent.none);
                }

                Rect deleteRect = new Rect(position.x + position.width - DeleteButtonWidth - 4, lineY - 2f, DeleteButtonWidth, FieldHeight);
                if (GUI.Button(deleteRect, "", new GUIStyle(EditorStyles.whiteLabel)
                {
                    normal = { background = CommunityStyles.RemoveItemTexture }
                }))
                {
                    Remove(index);
                    return;
                }

                if (foldout.isExpanded)
                {
                    var valueInspector = element["value"].Inspector();
                    float contentY = boxRect.y + FoldoutHeight;
                    EditorGUI.indentLevel++;
                    var width = position.width - DeleteButtonWidth - 8;
                    valueInspector.Draw(
                        new Rect(position.x + 20, contentY, width,
                        LudiqGUI.GetInspectorHeight(parentInspector, element["value"], width, GUIContent.none)),
                        GUIContent.none
                    );
                    EditorGUI.indentLevel--;
                }

                HandleDragAndDrop(position, handleRect, index, declaration, foldout);
            }

            private void HandleDragAndDrop(Rect position, Rect handleRect, int index, VariableDeclaration declaration, VariableFoldout foldout)
            {
                int controlID = GUIUtility.GetControlID(FocusType.Passive);
                var e = Event.current;

                switch (e.GetTypeForControl(controlID))
                {
                    case EventType.MouseDown:
                        if (e.button == (int)MouseButton.Left && position.Contains(e.mousePosition) && !handleRect.Contains(Event.current.mousePosition))
                        {
                            GUIUtility.hotControl = controlID;
                            e.Use();
                        }
                        break;

                    case EventType.MouseDrag:
                        if (GUIUtility.hotControl == controlID)
                        {
                            var item = this[index];
                            var list = typeof(VariableDeclarationsInspector)
                                .GetNestedType("ListAdaptor", BindingFlags.NonPublic)
                                .Instantiate(false, metadata, parentInspector) as MetadataListAdaptor;
                            GUIUtility.hotControl = 0;
                            DragAndDrop.PrepareStartDrag();
                            DragAndDrop.objectReferences = new UnityEngine.Object[0];
                            DragAndDrop.paths = new string[0];
                            DragAndDrop.SetGenericData(
                                VisualScripting.DraggedListItem.TypeName,
                                new DraggedListItem(list, index, item, (declaration, foldout))
                            );
                            DragAndDrop.StartDrag(metadata.path);
                            e.Use();
                        }
                        break;
                }
            }

            public override void Remove(int index)
            {
                var declaration = (VariableDeclaration)metadata[index].value;

                parentInspector.foldouts?.Remove(declaration);

                base.Remove(index);
                GUIUtility.keyboardControl = 0;
                GUIUtility.hotControl = 0;
                EditorGUIUtility.editingTextField = false;
                GUIUtility.ExitGUI();
            }

            public void OnNameGUI(Rect namePosition, Metadata nameMetadata)
            {
                namePosition = BeginLabeledBlock(nameMetadata, namePosition, GUIContent.none);
                var restoreColor = GUI.backgroundColor;
#if DARKER_UI
                GUI.backgroundColor = EditorGUIUtility.isProSkin ? restoreColor.Darken(0.25f) : restoreColor;
#endif
                var oldName = (string)nameMetadata.value;
                var newName = EditorGUI.DelayedTextField(namePosition, (string)nameMetadata.value, new GUIStyle(EditorStyles.textField) { fontStyle = FontStyle.Bold });
                GUI.backgroundColor = restoreColor;

                if (EndBlock(nameMetadata))
                {
                    var variableDeclarations = (VariableDeclarationCollection)metadata.value;
                    var declaration = (VariableDeclaration)nameMetadata.parent.value;

                    if (StringUtility.IsNullOrWhiteSpace(newName))
                    {
                        EditorUtility.DisplayDialog("Edit Variable Name", "Please enter a variable name.", "OK");
                        return;
                    }
                    else if (variableDeclarations.Contains(newName))
                    {
                        EditorUtility.DisplayDialog("Edit Variable Name", "A variable with the same name already exists.", "OK");
                        return;
                    }

                    nameMetadata.RecordUndo();
                    if (parentInspector.kind == VariableKind.Scene)
                    {
                        if (GraphWindow.activeReference.scene != null)
                            Undo.RecordObject(SceneVariables.Instance(GraphWindow.activeReference.scene.Value).variables, "Changed Scene variable name");
                        else
                        {
                            Scene? current = null;
                            for (int i = 0; i < SceneManager.sceneCount; i++)
                            {
                                var scene = SceneManager.GetSceneAt(i);
                                if (!scene.isLoaded) continue;

                                var variables = VisualScripting.Variables.Scene(scene);

                                if (variables == metadata.parent.value)
                                {
                                    current = scene;
                                    break;
                                }
                            }

                            if (current != null)
                            {
                                Undo.RecordObject(SceneVariables.Instance(current.Value).variables, "Changed Scene variable name");
                            }
                        }
                    }
                    variableDeclarations.EditorRename(declaration, newName);
                    nameMetadata.value = newName;

                    switch (parentInspector.kind)
                    {
                        case VariableKind.Graph:
                            if (EditorWindow.focusedWindow == GraphWindow.active)
                                GraphUtility.UpdateAllGraphVariables((FlowGraph)GraphWindow.activeContext.graph, oldName, newName);
                            else if (VariablesWindow.isVariablesWindowContext && VariablesWindow.currentContext != null)
                                GraphUtility.UpdateAllGraphVariables((FlowGraph)VariablesWindow.currentContext.graph, oldName, newName);
                            break;
                        case VariableKind.Object:
                            {
                                var ancestor = metadata.Ancestor(m => m.value is VisualScripting.Variables);
                                if (ancestor != null && ancestor.value != null)
                                {
                                    var gameObject = (ancestor.value as VisualScripting.Variables).gameObject;
                                    GraphUtility.UpdateAllObjectVariables(gameObject, oldName, newName);
                                }
                                else if (EditorWindow.focusedWindow == GraphWindow.active && GraphWindow.activeReference != null)
                                {
                                    if (GraphWindow.activeReference.gameObject != null)
                                        GraphUtility.UpdateAllObjectVariables(GraphWindow.activeReference.gameObject, oldName, newName);
                                }
                            }
                            break;
                        case VariableKind.Scene:
                            {
                                var ancestor = metadata.Ancestor(m => m.value is VisualScripting.Variables);
                                if (ancestor != null && ancestor.value != null)
                                {
                                    var scene = (ancestor.value as VisualScripting.Variables).gameObject.scene;
                                    GraphUtility.UpdateAllSceneVariables(scene, oldName, newName);
                                }
                                else if (EditorWindow.focusedWindow == GraphWindow.active && GraphWindow.activeReference != null)
                                {
                                    if (GraphWindow.activeReference.scene != null)
                                        GraphUtility.UpdateAllSceneVariables(GraphWindow.activeReference.scene.Value, oldName, newName);
                                    else
                                    {
                                        Scene? current = null;
                                        for (int i = 0; i < SceneManager.sceneCount; i++)
                                        {
                                            var scene = SceneManager.GetSceneAt(i);
                                            if (!scene.isLoaded) continue;

                                            var variables = VisualScripting.Variables.Scene(scene);

                                            if (variables == metadata.parent.value)
                                            {
                                                current = scene;
                                                break;
                                            }
                                        }

                                        if (current == null)
                                        {
                                            Debug.LogWarning(
                                                $"[Rename Variables] Could not find the scene that this variable is in please ensure that the scene is valid and loaded."
                                            );
                                            break;
                                        }

                                        GraphUtility.UpdateAllSceneVariables(current.Value, oldName, newName);
                                        var group = Undo.GetCurrentGroup();
                                        foreach (var target in GraphUtility.GetSceneVariablesRenameTargets(GraphWindow.activeReference, null, oldName))
                                        {
                                            if (target.Item1.name.hasValidConnection) continue;

                                            Undo.RecordObject(target.Item2, $"Renamed '{oldName}' variable to '{newName}'");

                                            target.Item1.name.SetDefaultValue(newName);
                                        }
                                        Undo.CollapseUndoOperations(group);
                                    }
                                }
                            }
                            break;
                        case VariableKind.Application:
                            {
                                if (Application.isPlaying)
                                {
                                    Debug.LogWarning($"[Rename Variables] Cannot rename all Application variables while in play mode!");
                                    break;
                                }
                                bool choice = EditorUtility.DisplayDialog(
                                    "Update ALL Application Variables?",
                                    "This will go through ALL scenes and macros to find every Variable Unit "
                                    + $"using {oldName} and update it to {newName}.\n\n"
                                    + "This operation is FINAL and cannot be undone!",
                                    "Update All",
                                    "Rename Only"
                                );

                                if (choice)
                                {
                                    GraphUtility.RenameApplicationVariables(oldName, newName);
                                }
                            }
                            break;
                        case VariableKind.Saved:
                            {
                                if (Application.isPlaying)
                                {
                                    Debug.LogWarning($"[Rename Variables] Cannot rename all Saved variables while in play mode!");
                                    break;
                                }
                                bool choice = EditorUtility.DisplayDialog(
                                    "Update ALL Saved Variables?",
                                    "This will go through ALL scenes and macros to find every Variable Unit "
                                    + $"using {oldName} and update it to {newName}.\n\n"
                                    + "This operation is FINAL and cannot be undone!",
                                    "Update All",
                                    "Rename Only"
                                );

                                if (choice)
                                {
                                    GraphUtility.RenameSavedVariables(oldName, newName);
                                }
                            }
                            break;
                    }
                }
            }

            protected override bool CanAdd()
            {
                if (StringUtility.IsNullOrWhiteSpace(parentInspector.newName))
                {
                    parentInspector.highlightPlaceholder = true;
                    EditorUtility.DisplayDialog("New Variable", "Please enter a variable name.", "OK");
                    return false;
                }
                else if (((VariableDeclarations)parentInspector.metadata.value).IsDefined(parentInspector.newName))
                {
                    parentInspector.highlightNewNameField = true;
                    EditorUtility.DisplayDialog("New Variable", "A variable with the same name already exists.", "OK");
                    return false;
                }

                return true;
            }

            protected override object ConstructItem()
            {
                var newItem = new VariableDeclaration(parentInspector.newName, null);
                parentInspector.newName = null;
                parentInspector.highlightPlaceholder = false;
                parentInspector.highlightNewNameField = false;
                return newItem;
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

                            if (parentInspector.foldouts == null)
                                parentInspector.foldouts = new Dictionary<VariableDeclaration, VariableFoldout>();

                            parentInspector.foldouts[draggedItem.variableState.Item1] = draggedItem.variableState.Item2;
                        }
                    }
                }

                base.ProcessDropInsertion(insertionIndex);
            }
        }
    }
}