using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.ReorderableList.Internal;
using Unity.VisualScripting.ReorderableList;
using System.IO;

namespace Unity.VisualScripting.Community
{
    public class PatchedVariableDeclarationsInspector : VariableDeclarationsInspector
    {
        private static class Layout
        {
            public const float Spacing = 4f;
            public const float ButtonHeight = 27f;
            public const float ButtonSpacingX = 4f;
            public const float ButtonSpacingY = 4f;
            public const float CornerRadius = 10f;
            public const float ToolbarPadding = 6f;
            public const int QuickAddColumns = 4;
            public const int QuickAddRows = 2;
            public const int TitleHeight = 20;
        }

        private const string NewVariableString = "newVariable";
        private const string QuickTypeTooltip = "Left-Click: Add single type\nRight-Click: Add List<T>";
        private const string QuickOtherTypeTooltip = "Left-Click: Add single type\nHold Shift when pressing the Create Type button: Prompt to add to settings & regenerate";

        internal string newName;
        internal bool addedItem;
        internal VariableDeclaration addedDeclaration;
        public ReorderableListControl listControl { get; private set; }

        // Reflection Caching
        private static readonly FieldInfo AdaptorField = typeof(VariableDeclarationsInspector).GetField("adaptor", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo NewNameField = typeof(VariableDeclarationsInspector).GetField("newName", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo AdaptorFieldMethod = typeof(MetadataCollectionAdaptor).GetMethod("Field", BindingFlags.Instance | BindingFlags.Public);
        private static readonly FieldInfo listControlFieldInfo = typeof(MetadataCollectionAdaptor).GetField("listControl", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly IOptimizedInvoker adaptorFieldMethodInvoker;
        internal static readonly IOptimizedAccessor adaptorFieldAccessor;
        private static readonly IOptimizedAccessor newNameFieldAccessor;

        static PatchedVariableDeclarationsInspector()
        {
            adaptorFieldMethodInvoker = AdaptorFieldMethod.Prewarm();
            adaptorFieldAccessor = AdaptorField.Prewarm();
            newNameFieldAccessor = NewNameField.Prewarm();
        }

        public PatchedVariableDeclarationsInspector(Metadata metadata) : base(metadata) { }

        public override void Initialize()
        {
            base.Initialize();

            var adaptor = (MetadataListAdaptor)adaptorFieldAccessor.GetValue(this);

            if (listControlFieldInfo != null)
            {
                listControl = listControlFieldInfo.GetValueOptimized(adaptor) as ReorderableListControl;

                if (listControl != null)
                {
                    listControl.Flags = ReorderableListFlags.HideAddButton | ReorderableListFlags.DisableContextMenu;
                    listControl.AddMenuClicked += (sender, args) =>
                    {
                        LudiqGUI.FuzzyDropdown(args.ButtonPosition, GetOptions(), typeof(object), (option) =>
                        {
                            addedDeclaration = Add(OperateOnString(NewVariableString), option as Type);
                            addedItem = true;
                        });
                    };
                }
            }
        }

        private IFuzzyOptionTree GetOptions() => new TypeOptionTree(Codebase.GetTypeSet(TypeSet.SettingsTypes), TypeFilter.Any);

        public VariableDeclaration Add(string name, Type type)
        {
            var adaptor = (MetadataListAdaptor)adaptorFieldAccessor.GetValue(this);

            newNameFieldAccessor.SetValue(this, name);
            adaptor.Add();
            GUI.changed = true;

            var collection = metadata["collection"];
            var newElement = collection[collection.Count - 1];
            newElement["name"].value = name;
            newElement["value"].value = type.PseudoDefault();

            var declaration = (VariableDeclaration)newElement.value;
#if VISUAL_SCRIPTING_1_7
            newElement["typeHandle"].value = new SerializableType(type.AssemblyQualifiedName);
#endif
            return declaration;
        }

        private string OperateOnString(string requestedName)
        {
            var declarations = metadata.value as VariableDeclarations;
            string baseName = string.IsNullOrEmpty(requestedName) ? "Unnamed Variable" : requestedName;
            string resolvedName = baseName;

            int counter = 1;
            while (declarations.IsDefined(resolvedName))
            {
                resolvedName = $"{baseName} ({counter++})";
            }

            return resolvedName;
        }

        protected override float GetHeight(float width, GUIContent label)
        {
            float height = base.GetHeight(width, label);

            if (EditorPrefs.GetBool(ProjectSettingsProviderView.ShowVariablesQuickbarKey, false) && !metadata.HasAttribute<HideVariablesQuickbarAttribute>())
            {
                height += (Layout.ButtonHeight * 2) + Layout.ButtonSpacingY + (Layout.ToolbarPadding * 2);
            }

            return height + Layout.TitleHeight;
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            if (metadata.value == null) return;

            position.x = 0;

            if (EditorPrefs.GetBool(ProjectSettingsProviderView.ShowVariablesQuickbarKey, false) && !metadata.HasAttribute<HideVariablesQuickbarAttribute>())
            {
                DrawQuickAddToolbar(position);

                float offset = (Layout.ButtonHeight * 2) + Layout.ButtonSpacingY + (Layout.ToolbarPadding * 2);
                position.y += offset;
                position.height -= offset;
            }

            position.height = base.GetHeight(position.width, label) + 20;

            adaptorFieldMethodInvoker.Invoke(adaptorFieldAccessor.GetValue(this), position, new GUIContent("Variables"));
        }

        private static readonly GUIContent[] quickTypesLabels =
        {
            new GUIContent("Float", typeof(float).Icon()[IconSize.Small], QuickTypeTooltip),
            new GUIContent("Int", typeof(int).Icon()[IconSize.Small], QuickTypeTooltip),
            new GUIContent("Bool", typeof(bool).Icon()[IconSize.Small], QuickTypeTooltip),
            new GUIContent("String", typeof(string).Icon()[IconSize.Small], QuickTypeTooltip),
            new GUIContent("Vector", typeof(Vector4).Icon()[IconSize.Small], QuickTypeTooltip),
            new GUIContent("Color", typeof(Color).Icon()[IconSize.Small], QuickTypeTooltip),
            new GUIContent("Object", typeof(GameObject).Icon()[IconSize.Small], QuickTypeTooltip),
            new GUIContent("Other", typeof(Generic).Icon()[IconSize.Small], QuickOtherTypeTooltip)
        };

        private void DrawQuickAddToolbar(Rect position)
        {
            float totalHeight = (Layout.ButtonHeight * Layout.QuickAddRows) + Layout.ButtonSpacingY + (Layout.ToolbarPadding * 2);
            Rect toolbarRect = new Rect(position.x, position.y, position.width, totalHeight);
#if DARKER_UI
            EditorGUI.DrawRect(toolbarRect, CommunityStyles.backgroundColor);
#else
            EditorGUI.DrawRect(toolbarRect, ColorPalette.unityBackgroundLight);
#endif
            float availableWidth = toolbarRect.width - Layout.ToolbarPadding * 2;
            float totalSpacingX = Layout.ButtonSpacingX * (Layout.QuickAddColumns - 1);
            float buttonWidthAdjusted = (availableWidth - totalSpacingX) / Layout.QuickAddColumns;

            float xStart = toolbarRect.x + Layout.ToolbarPadding;
            float yStart = toolbarRect.y + Layout.ToolbarPadding;

            for (int i = 0; i < quickTypesLabels.Length; i++)
            {
                int row = i / Layout.QuickAddColumns;
                int col = i % Layout.QuickAddColumns;

                float x = xStart + col * (buttonWidthAdjusted + Layout.ButtonSpacingX);
                float y = yStart + row * (Layout.ButtonHeight + Layout.ButtonSpacingY);

                Rect buttonRect = new Rect(x, y, buttonWidthAdjusted, Layout.ButtonHeight);

                if (DrawAddButton(buttonRect, quickTypesLabels[i], out var mouse))
                {
                    bool isRight = mouse == MouseButton.Right;
                    string typeLabel = quickTypesLabels[i].text;

                    if (typeLabel == "Bool")
                    {
                        AddQuickVariable(isRight ? typeof(List<bool>) : typeof(bool), true);
                        continue;
                    }
                    if (typeLabel == "Other")
                    {
                        TypeBuilderWindow.ShowWindow(buttonRect, (t) => AddQuickVariable(t, Event.current.shift), typeof(object), true, Array.Empty<Type>());
                        continue;
                    }

                    var menu = new GenericMenu();
                    switch (typeLabel)
                    {
                        case "Float":
                            menu.AddItem(new GUIContent("float"), false, () => AddQuickVariable(isRight ? typeof(List<float>) : typeof(float), true));
                            menu.AddItem(new GUIContent("double"), false, () => AddQuickVariable(isRight ? typeof(List<double>) : typeof(double), true));
                            menu.AddItem(new GUIContent("decimal"), false, () => AddQuickVariable(isRight ? typeof(List<decimal>) : typeof(decimal), true));
                            break;

                        case "Int":
                            menu.AddItem(new GUIContent("int"), false, () => AddQuickVariable(isRight ? typeof(List<int>) : typeof(int)));
                            menu.AddItem(new GUIContent("short"), false, () => AddQuickVariable(isRight ? typeof(List<short>) : typeof(short), true));
                            menu.AddItem(new GUIContent("long"), false, () => AddQuickVariable(isRight ? typeof(List<long>) : typeof(long), true));
                            menu.AddItem(new GUIContent("byte"), false, () => AddQuickVariable(isRight ? typeof(List<byte>) : typeof(byte), true));
                            menu.AddItem(new GUIContent("sbyte"), false, () => AddQuickVariable(isRight ? typeof(List<sbyte>) : typeof(sbyte), true));
                            menu.AddSeparator("");
                            menu.AddItem(new GUIContent("uint"), false, () => AddQuickVariable(isRight ? typeof(List<uint>) : typeof(uint), true));
                            menu.AddItem(new GUIContent("ushort"), false, () => AddQuickVariable(isRight ? typeof(List<ushort>) : typeof(ushort), true));
                            menu.AddItem(new GUIContent("ulong"), false, () => AddQuickVariable(isRight ? typeof(List<ulong>) : typeof(ulong), true));
                            break;

                        case "String":
                            menu.AddItem(new GUIContent("String"), false, () => AddQuickVariable(isRight ? typeof(List<string>) : typeof(string)));
                            menu.AddItem(new GUIContent("Char"), false, () => AddQuickVariable(isRight ? typeof(List<char>) : typeof(char), true));
                            break;

                        case "Vector":
                            menu.AddItem(new GUIContent("Vector 2"), false, () => AddQuickVariable(isRight ? typeof(List<Vector2>) : typeof(Vector2), true));
                            menu.AddItem(new GUIContent("Vector 3"), false, () => AddQuickVariable(isRight ? typeof(List<Vector3>) : typeof(Vector3), true));
                            menu.AddItem(new GUIContent("Vector 4"), false, () => AddQuickVariable(isRight ? typeof(List<Vector4>) : typeof(Vector4), true));
                            menu.AddSeparator("");
                            menu.AddItem(new GUIContent("Vector 2 Int"), false, () => AddQuickVariable(isRight ? typeof(List<Vector2Int>) : typeof(Vector2Int), true));
                            menu.AddItem(new GUIContent("Vector 3 Int"), false, () => AddQuickVariable(isRight ? typeof(List<Vector3Int>) : typeof(Vector3Int), true));
                            break;

                        case "Color":
                            menu.AddItem(new GUIContent("Color"), false, () => AddQuickVariable(isRight ? typeof(List<Color>) : typeof(Color)));
                            menu.AddItem(new GUIContent("HDRColor"), false, () => AddQuickVariable(isRight ? typeof(List<HDRColor>) : typeof(HDRColor), true));
                            menu.AddItem(new GUIContent("Gradient"), false, () => AddQuickVariable(isRight ? typeof(List<Gradient>) : typeof(Gradient), true));
                            break;

                        case "Object":
                            menu.AddItem(new GUIContent("Game Object"), false, () => AddQuickVariable(isRight ? typeof(List<GameObject>) : typeof(GameObject)));
                            menu.AddItem(new GUIContent("Transform"), false, () => AddQuickVariable(isRight ? typeof(List<Transform>) : typeof(Transform)));
                            menu.AddSeparator("");
                            menu.AddItem(new GUIContent("Physics/Rigid Body"), false, () => AddQuickVariable(isRight ? typeof(List<Rigidbody>) : typeof(Rigidbody), true));
                            menu.AddItem(new GUIContent("Physics/Rigid Body 2D"), false, () => AddQuickVariable(isRight ? typeof(List<Rigidbody2D>) : typeof(Rigidbody2D), true));
                            menu.AddItem(new GUIContent("Physics/Collider"), false, () => AddQuickVariable(isRight ? typeof(List<Collider>) : typeof(Collider), true));
                            menu.AddItem(new GUIContent("Physics/Collider 2D"), false, () => AddQuickVariable(isRight ? typeof(List<Collider2D>) : typeof(Collider2D), true));
                            menu.AddItem(new GUIContent("Physics/Box Collider"), false, () => AddQuickVariable(isRight ? typeof(List<BoxCollider>) : typeof(BoxCollider), true));
                            menu.AddItem(new GUIContent("Physics/Box Collider 2D"), false, () => AddQuickVariable(isRight ? typeof(List<BoxCollider2D>) : typeof(BoxCollider2D), true));
                            menu.AddItem(new GUIContent("Physics/Sphere Collider"), false, () => AddQuickVariable(isRight ? typeof(List<SphereCollider>) : typeof(SphereCollider), true));
                            menu.AddItem(new GUIContent("Physics/Circle Collider"), false, () => AddQuickVariable(isRight ? typeof(List<CircleCollider2D>) : typeof(CircleCollider2D), true));
                            menu.AddSeparator("");
                            menu.AddItem(new GUIContent("Rendering/Mesh Renderer"), false, () => AddQuickVariable(isRight ? typeof(List<MeshRenderer>) : typeof(MeshRenderer), true));
                            menu.AddItem(new GUIContent("Rendering/Camera"), false, () => AddQuickVariable(isRight ? typeof(List<Camera>) : typeof(Camera), true));
                            menu.AddSeparator("");
                            menu.AddItem(new GUIContent("Audio Source"), false, () => AddQuickVariable(isRight ? typeof(List<AudioSource>) : typeof(AudioSource), true));
                            menu.AddItem(new GUIContent("Animator"), false, () => AddQuickVariable(isRight ? typeof(List<Animator>) : typeof(Animator), true));
                            break;
                    }
                    menu.DropDown(buttonRect);
                }
            }
        }

        private static GUIStyle _labelStyle;
        private static GUIStyle labelStyle
        {
            get
            {
                if (_labelStyle == null)
                {
                    _labelStyle = new GUIStyle(EditorStyles.label)
                    {
                        border = new RectOffset((int)Layout.CornerRadius, (int)Layout.CornerRadius, (int)Layout.CornerRadius, (int)Layout.CornerRadius),
                        padding = new RectOffset(4, 4, 2, 2),
                        alignment = TextAnchor.MiddleLeft,
                        normal = { textColor = !EditorGUIUtility.isProSkin ? Color.black : Color.white }
                    };
                }
                return _labelStyle;
            }
        }

        private static bool DrawAddButton(Rect rect, GUIContent content, out MouseButton? mouseButton)
        {
            var e = Event.current;
            bool containsMouse = e != null && rect.Contains(e.mousePosition);
            bool isClick = containsMouse && e.type == EventType.MouseDown && e.button == 0;
            bool isRightClick = containsMouse && e.type == EventType.MouseDown && e.button == 1;

            var previous = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(new Vector2(16, 16));

            var color = Color.gray.Brighten(0.1f);
            if (isClick || isRightClick) color = CommunityStyles.backgroundColor;
            else if (containsMouse) color = CommunityStyles.backgroundColor.Brighten(0.3f);

            LudiqGUI.DrawEmptyRect(rect, color);
            EditorGUI.LabelField(rect, content, labelStyle);
            EditorGUIUtility.SetIconSize(previous);

            var texRect = new Rect(rect.xMax - 14, rect.y + 9, EditorGUIUtility.isProSkin ? 8 : 10, 10);
            GUI.DrawTexture(texRect, ReorderableListResources.GetTexture(ReorderableListTexture.Icon_Add_Normal));

            if (isClick)
            {
                mouseButton = MouseButton.Left;
                e.Use();
                return true;
            }
            if (isRightClick)
            {
                mouseButton = MouseButton.Right;
                e.Use();
                return true;
            }

            mouseButton = null;
            return false;
        }

        private static readonly string SettingsFilePath = Path.Combine("ProjectSettings", "VisualScripting_ShownTypePopups.json");

        private static HashSet<string> LoadSeenTypes()
        {
            if (File.Exists(SettingsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    var wrapper = JsonUtility.FromJson<SerializationWrapper>(json);
                    if (wrapper?.items != null)
                    {
                        return new HashSet<string>(wrapper.items);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to load shown type popups settings: {ex.Message}");
                }
            }
            return new HashSet<string>();
        }

        private static void SaveSeenTypes(HashSet<string> seenTypes)
        {
            try
            {
                var wrapper = new SerializationWrapper { items = new List<string>(seenTypes) };
                string json = JsonUtility.ToJson(wrapper, true);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to save shown type popups settings: {ex.Message}");
            }
        }

        [Serializable]
        private class SerializationWrapper
        {
            public List<string> items;
        }

        private void AddQuickVariable(Type type, bool ask = false)
        {
            string typeKey = type.AssemblyQualifiedName;
            HashSet<string> seenTypes = LoadSeenTypes();
            bool hasSeenPopup = seenTypes.Contains(typeKey);

            var coreConfig = BoltCore.Configuration;

            if (ask && !coreConfig.typeOptions.Contains(type) && !hasSeenPopup && !type.IsGenericType)
            {
                int choice = EditorUtility.DisplayDialogComplex(
                    "Add Type to Settings",
                    $"The type '{type.As().CSharpName(false, false, false)}' is not currently in your settings types.\n\nWould you like to add it?",
                    "Add", "No", "Add && Regenerate"
                );

                if (choice == 0 || choice == 2)
                {
                    coreConfig.typeOptions.Add(type);
                    SaveCoreConfig(coreConfig);
                    Codebase.UpdateSettings();
                    if (choice == 2) UnitBase.Rebuild();
                }

                seenTypes.Add(typeKey);
                SaveSeenTypes(seenTypes);
            }

            var variableDecls = (VariableDeclarations)metadata.value;
            var collection = (VariableDeclarationCollection)metadata["collection"].value;

            string baseName = type.HumanName(true);
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
            SetHeightDirty();
        }

        private object Default(Type type)
        {
            if (type == typeof(Gradient)) return new Gradient();

            return type.PseudoDefault();
        }

        private void SaveCoreConfig(BoltCoreConfiguration coreConfig)
        {
            var meta = coreConfig.GetMetadata(nameof(coreConfig.typeOptions));
            meta.Inspector().SetHeightDirty();
#if VISUAL_SCRIPTING_1_9_0_OR_GREATER
            meta.GetType().GetMethod("SaveImmediately", BindingFlags.Instance | BindingFlags.NonPublic).InvokeOptimized(meta, new object[] { true });
#else
            meta.Save();
#endif
        }
    }
}