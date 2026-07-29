using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting.ReorderableList;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.VisualScripting.Community
{
#if NEW_VARIABLES_UI
    public sealed class PatchedVariableDeclarationInspector : Inspector
    {
        private static readonly GUIContent Rename = new GUIContent("Rename");
        private static readonly GUIContent CommandMoveToTop = new GUIContent("Move to Top");
        private static readonly GUIContent CommandMoveToBottom = new GUIContent("Move to Bottom");
        private static readonly GUIContent CommandDuplicate = new GUIContent("Duplicate");
        private static readonly GUIContent CommandRemove = new GUIContent("Remove");
        private static readonly GUIContent CommandClearAll = new GUIContent("Clear All");

        private static readonly FieldInfo s_ContextCommandNameFieldInfo;
        private readonly Metadata nameMetadata;
        private readonly Metadata valueMetadata;
        private readonly Metadata typeMetadata;

        private Type cachedType;
        private Texture cachedIcon;
        private InlineTypeInfo cachedInlineInfo;
        private bool cachedIsInline;
        private SerializableType cachedTypeHandle;
        private bool isOpen;
        private bool hasCachedType;
        private bool initializedType;
        private bool changed;
        private bool isRenaming;
        private string renameControl;

        private SystemObjectInspector systemObjectInspector;
        private Inspector valueInspector;
        private Inspector typedValueInspector;

        public readonly static EditorTexture NullIcon = typeof(Null).Icon();
        private static readonly GUIContent Temp = new GUIContent();
        private const int IconSize = 16;
        private const int InlineValueSpacing = 4;
        private const string TextControl = "VariableDeclerationsControl";

        private static readonly Assembly EditorCoreAssembly;
        private static readonly MethodInfo ResolveSerializableTypeMethod;
        private static readonly FieldInfo SystemObjectInspectorField;
        private static readonly MethodInfo GetWidthMethod;
        private static readonly FieldInfo ParentInspectorField;

        PatchedVariableDeclarationsInspector parent;

        private static readonly HashSet<Type> ConstructTypes = new HashSet<Type>()
        {
            typeof(Vector2), typeof(Vector3), typeof(Vector4), typeof(Vector2Int), typeof(Vector3Int),
            typeof(Quaternion), typeof(Rect), typeof(Color), typeof(HDRColor), typeof(Gradient),
        };

        static PatchedVariableDeclarationInspector()
        {
            EditorCoreAssembly = typeof(GraphGUI).Assembly;
            ResolveSerializableTypeMethod = EditorCoreAssembly.GetTypes().FirstOrDefault(t => t.Name == "SerializableTypeExtensions")?.GetMethod("Resolve", BindingFlags.Static | BindingFlags.Public);
            SystemObjectInspectorField = typeof(SystemObjectInspector).GetField("inspector", BindingFlags.Instance | BindingFlags.NonPublic);
            GetWidthMethod = EditorCoreAssembly.GetTypes().FirstOrDefault(t => t.Name == "ValueInspector")?.GetMethod("GetWidth", BindingFlags.Instance | BindingFlags.Public);
            ParentInspectorField = typeof(Inspector).GetField("parentInspector", BindingFlags.NonPublic | BindingFlags.Instance);

            s_ContextCommandNameFieldInfo = typeof(ReorderableListControl).GetField("s_ContextCommandName", BindingFlags.Static | BindingFlags.NonPublic);
        }

        public PatchedVariableDeclarationInspector(Metadata metadata) : base(metadata)
        {
            VSUsageUtility.isVisualScriptingUsed = true;
            nameMetadata = metadata[nameof(VariableDeclaration.name)];
            valueMetadata = metadata[nameof(VariableDeclaration.value)];
            typeMetadata = metadata[nameof(VariableDeclaration.typeHandle)];
        }

        GraphReference reference = null;
        UnityEngine.Object root = null;
        Guid[] parentGuids = null;

        public override void Initialize()
        {
            base.Initialize();

            valueInspector = valueMetadata.Inspector();

            if (valueInspector is SystemObjectInspector sysObjInspector)
            {
                systemObjectInspector = sysObjInspector;
            }

            RefreshCachedTypeInfo();
        }

        private void RefreshCachedTypeInfo()
        {
            var declaration = (VariableDeclaration)metadata.value;

            if (hasCachedType && cachedTypeHandle == declaration.typeHandle) return;

            hasCachedType = true;
            cachedTypeHandle = declaration.typeHandle;
            cachedType = (Type)ResolveSerializableTypeMethod.InvokeOptimized(null, cachedTypeHandle);

            EditorTexture icon = (cachedType == null || cachedType == typeof(Unknown)) ? NullIcon : cachedType.Icon();
            cachedIcon = icon[IconSize];

            cachedInlineInfo = new InlineTypeInfo(cachedType);
            cachedIsInline = cachedType != null && (cachedType.IsBasic() || cachedInlineInfo.isConstruct || cachedInlineInfo.isUnityObject);

            typedValueInspector = valueMetadata.Cast(cachedType).Inspector();
        }

        protected override float GetHeight(float width, GUIContent label)
        {
            float height = 0f;

            using (LudiqGUIUtility.labelWidth.Override(Styles.labelWidth))
            {
                height += Styles.padding + GetNameHeight(width);

                if (isOpen)
                {
                    height += Styles.spacing + GetTypeHeight(width);
                    height += Styles.spacing + GetValueHeight(width);
                }

                height += Styles.padding;
            }

            return height;
        }

        private float GetNameHeight(float width) => EditorGUIUtility.singleLineHeight;
        private float GetValueHeight(float width) => LudiqGUI.GetInspectorHeight(this, valueMetadata, width);
        private float GetTypeHeight(float width) => LudiqGUI.GetInspectorHeight(this, typeMetadata, width);

        private bool LoadedState;

        private void LoadState()
        {
            if (LoadedState) return;

            LoadedState = true;

            parent = ParentInspectorField.GetValueOptimized(this) as PatchedVariableDeclarationsInspector;

            var declaration = metadata.value as VariableDeclaration;

            if (VariablesWindow.isVariablesWindowContext)
            {
                reference = VariablesWindow.currentContext?.reference;
                root = reference?.rootObject;
                parentGuids = reference?.parentElementGuids?.ToArray();
            }
            else
            {
                reference = LudiqGraphsEditorUtility.editedContext?.value?.reference;
                root = reference?.rootObject;
                parentGuids = reference?.parentElementGuids?.ToArray();
            }

            var ancestor = metadata.Ancestor(m => m.value is Variables);
            if (ancestor != null)
            {
                root = ancestor.value as Variables;
                parentGuids = null;
            }

            if (parent.kind == VariableKind.Application)
            {
                root = ApplicationVariables.asset;
                parentGuids = null;
            }
            else if (parent.kind == VariableKind.Saved)
            {
                root = SavedVariables.asset;
                parentGuids = null;
            }

            isOpen = VariableInspectorState.Load(root, parentGuids, declaration.name);
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            LoadState();

            if (e.type == EventType.ContextClick && position.Contains(e.mousePosition))
            {
                var menu = new GenericMenu();
                AddItemsToMenu(menu, metadata.parent.IndexOf(metadata.value));
                menu.ShowAsContext();
                e.Use();
            }

            position = BeginLabeledBlock(metadata, position, label);
            RefreshCachedTypeInfo();

            using (LudiqGUIUtility.labelWidth.Override(Styles.labelWidth))
            {
                y += Styles.padding;
                var namePosition = position.VerticalSection(ref y, GetNameHeight(position.width));

                if (!initializedType && systemObjectInspector != null)
                {
                    GetWidthMethod.InvokeOptimized(SystemObjectInspectorField.GetValueOptimized(systemObjectInspector));
                    initializedType = true;
                }

                OnNameGUI(namePosition);

                if (isOpen)
                {
                    y += Styles.spacing;
                    var typePosition = position.VerticalSection(ref y, GetTypeHeight(position.width));

                    y += Styles.spacing;
                    var valuePosition = position.VerticalSection(ref y, GetValueHeight(position.width));

                    LudiqGUI.Inspector(typeMetadata, typePosition, GUIContent.none);
                    LudiqGUI.Inspector(valueMetadata, valuePosition, GUIContent.none);
                }

                y += Styles.padding;
            }

            if (!changed) EndBlock(metadata);
        }

        public void OnNameGUI(Rect namePosition)
        {
            var declaration = (VariableDeclaration)metadata.value;

            var foldoutRect = new Rect(namePosition.x, namePosition.y, 16, namePosition.height);
            var textRect = new Rect(foldoutRect.xMax + IconSize, namePosition.y, namePosition.width - foldoutRect.width - IconSize, namePosition.height);
            var valueRect = Rect.zero;

            var oldMode = EditorGUIUtility.hierarchyMode;
            EditorGUIUtility.hierarchyMode = false;
            Temp.image = cachedIcon;

            var oldIsOpen = isOpen;
            isOpen = EditorGUI.Foldout(foldoutRect, isOpen, Temp, true);

            if (oldIsOpen != isOpen)
                VariableInspectorState.Save(root, parentGuids, declaration.name, isOpen);

            EditorGUIUtility.hierarchyMode = oldMode;

            bool drawInlineValue = !isOpen && cachedIsInline;
            if (drawInlineValue)
            {
                if (cachedInlineInfo.isEnum)
                {
                    if (typedValueInspector is EnumInspector enumInspector)
                    {
                        var adaptiveWidth = enumInspector.GetAdaptiveWidth();
                        textRect.width -= adaptiveWidth;

                        valueRect = new Rect(textRect.xMax + InlineValueSpacing, textRect.y, adaptiveWidth, textRect.height);
                    }
                    else
                    {
                        textRect.width -= cachedInlineInfo.width + InlineValueSpacing;
                        valueRect = new Rect(textRect.xMax + InlineValueSpacing, textRect.y, cachedInlineInfo.width, textRect.height);
                    }
                }
                else
                {
                    textRect.width -= cachedInlineInfo.width + InlineValueSpacing;
                    valueRect = new Rect(textRect.xMax + InlineValueSpacing, textRect.y, cachedInlineInfo.width, textRect.height);
                }
            }

            var oldName = (string)nameMetadata.value;
            string controlName = TextControl + oldName;
            GUI.SetNextControlName(controlName);

            BeginBlock(nameMetadata, namePosition);
            string newName = EditorGUI.DelayedTextField(textRect, oldName);

            if (renameControl != null && renameControl == controlName)
            {
                GUI.FocusControl(controlName);
                renameControl = null;
            }

            bool endBlock = EndBlock(nameMetadata);

            if (drawInlineValue)
            {
                if (cachedInlineInfo.isUnityObject)
                {
                    EditorGUI.BeginChangeCheck();
                    var updatedObj = EditorGUI.ObjectField(valueRect, valueMetadata.value as UnityEngine.Object, cachedType, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        valueMetadata.RecordUndo();
                        valueMetadata.value = updatedObj;
                    }
                }
                else
                {
                    valueInspector.Draw(valueRect, GUIContent.none);
                }
            }

            if (endBlock && ProcessRename(oldName, newName, declaration, parent?.kind))
            {
                GUI.FocusControl(null);
            }

            if (parent != null && parent.addedItem && declaration == parent.addedDeclaration)
            {
                parent.addedItem = false;
                parent.addedDeclaration = null;
                GUI.FocusControl(controlName);
            }
        }

        private bool ProcessRename(string oldName, string newName, VariableDeclaration declaration, VariableKind? kind)
        {
            var variableDeclarations = (VariableDeclarationCollection)metadata.parent.value;

            if (StringUtility.IsNullOrWhiteSpace(newName))
            {
                EditorUtility.DisplayDialog("Edit Variable Name", "Please enter a variable name.", "OK");
                return false;
            }
            if (variableDeclarations.Contains(newName) && newName != oldName)
            {
                EditorUtility.DisplayDialog("Edit Variable Name", "A variable with the same name already exists.", "OK");
                return false;
            }
            if (oldName == newName) return false;

            nameMetadata.RecordUndo();
            RecordSceneUndo(kind);

            variableDeclarations.EditorRename(declaration, newName);
            nameMetadata.value = newName;

            UpdateVariableReferences(kind, oldName, newName);

            if (isRenaming)
            {
                GraphUtility.RenameVariables(parent.kind.Value, oldName, newName, metadata);
            }
            return true;
        }

        private void RecordSceneUndo(VariableKind? kind)
        {
            if (kind != VariableKind.Scene) return;

            if (GraphWindow.active != null && GraphWindow.activeReference?.scene != null)
            {
                Undo.RecordObject(SceneVariables.Instance(GraphWindow.activeReference.scene.Value).variables, "Changed Scene variable name");
                return;
            }

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && VisualScripting.Variables.Scene(scene) == metadata.parent.value)
                {
                    Undo.RecordObject(SceneVariables.Instance(scene).variables, "Changed Scene variable name");
                    break;
                }
            }
        }

        private void UpdateVariableReferences(VariableKind? kind, string oldName, string newName)
        {
            switch (kind)
            {
                case VariableKind.Flow:
                case VariableKind.Graph:
                    if (EditorWindow.focusedWindow == GraphWindow.active)
                        GraphUtility.UpdateAllGraphVariables((FlowGraph)GraphWindow.activeContext.graph, oldName, newName);
                    else if (VariablesWindow.isVariablesWindowContext && VariablesWindow.currentContext != null)
                        GraphUtility.UpdateAllGraphVariables((FlowGraph)VariablesWindow.currentContext.graph, oldName, newName);
                    break;

                case VariableKind.Object:
                    var objAncestor = metadata.Ancestor(m => m.value is VisualScripting.Variables);
                    if (objAncestor?.value != null)
                        GraphUtility.UpdateAllObjectVariables((objAncestor.value as VisualScripting.Variables).gameObject, oldName, newName);
                    else if (EditorWindow.focusedWindow == GraphWindow.active && GraphWindow.activeReference?.gameObject != null)
                        GraphUtility.UpdateAllObjectVariables(GraphWindow.activeReference.gameObject, oldName, newName);
                    break;

                case VariableKind.Scene:
                    var sceneAncestor = metadata.Ancestor(m => m.value is VisualScripting.Variables);
                    if (sceneAncestor?.value != null)
                        GraphUtility.UpdateAllSceneVariables((sceneAncestor.value as VisualScripting.Variables).gameObject.scene, oldName, newName);
                    else if (EditorWindow.focusedWindow == GraphWindow.active && GraphWindow.activeReference?.scene != null)
                        GraphUtility.UpdateAllSceneVariables(GraphWindow.activeReference.scene.Value, oldName, newName);
                    else
                        Debug.LogWarning("[Rename Variables] Could not find valid scene to update variables.");
                    break;

                case VariableKind.Application:
                case VariableKind.Saved:
                    if (Application.isPlaying)
                    {
                        Debug.LogWarning($"[Rename Variables] Cannot rename {kind} variables while in play mode!");
                        break;
                    }
                    bool choice = changed = EditorUtility.DisplayDialog(
                        $"Update ALL {kind} Variables?",
                        $"This will go through ALL scenes and macros to find every Variable Unit using '{oldName}' and update it to '{newName}'.\n\nThis operation is FINAL and cannot be undone!",
                        "Update All", "Rename Only");

                    if (choice && kind == VariableKind.Application) GraphUtility.RenameApplicationVariables(oldName, newName);
                    else if (choice && kind == VariableKind.Saved) GraphUtility.RenameSavedVariables(oldName, newName);
                    break;
            }
        }

        private void DoCommand(GUIContent command, int itemIndex)
        {
            if (command == Rename)
            {
                isRenaming = true;
                renameControl = TextControl + (metadata.parent[itemIndex].value as VariableDeclaration).name;
                return;
            }
            s_ContextCommandNameFieldInfo.SetValueOptimized(null, command.text);
            parent.listControl.DoCommand(command.text, itemIndex, (IReorderableListAdaptor)PatchedVariableDeclarationsInspector.adaptorFieldAccessor.GetValue(parent));
        }

        private void AddItemsToMenu(GenericMenu menu, int itemIndex)
        {
            void Action(object v) => DoCommand(v as GUIContent, itemIndex);

            menu.AddItem(Rename, false, Action, Rename);
            menu.AddSeparator("");

            if (itemIndex > 0) menu.AddItem(CommandMoveToTop, false, Action, CommandMoveToTop);
            else menu.AddDisabledItem(CommandMoveToTop);

            if (itemIndex + 1 < metadata.parent.Count) menu.AddItem(CommandMoveToBottom, false, Action, CommandMoveToBottom);
            else menu.AddDisabledItem(CommandMoveToBottom);

            menu.AddSeparator("");
            menu.AddItem(CommandDuplicate, false, Action, CommandDuplicate);

            if (menu.GetItemCount() > 0) menu.AddSeparator("");

            menu.AddItem(CommandRemove, false, Action, CommandRemove);
            menu.AddSeparator("");
            menu.AddItem(CommandClearAll, false, Action, CommandClearAll);
        }

        private readonly struct InlineTypeInfo
        {
            public readonly bool isUnityObject;
            public readonly bool isConstruct;
            public readonly bool isEnum;
            public readonly float width;

            public InlineTypeInfo(Type type)
            {
                isUnityObject = type != null && typeof(UnityEngine.Object).IsAssignableFrom(type);
                isConstruct = type != null && ConstructTypes.Contains(type);
                isEnum = type != null && type.IsEnum;

                if (isUnityObject || type == typeof(bool)) width = 20f;
                else if (isEnum) width = 65f;
                else if (type == typeof(Vector2) || type == typeof(Vector2Int) || type == typeof(Color) || type == typeof(HDRColor) || type == typeof(Gradient)) width = 50f;
                else if (type == typeof(Vector3) || type == typeof(Vector3Int)) width = 65f;
                else if (type == typeof(Vector4)) width = 80f;
                else width = 35f;
            }
        }

        public static class Styles
        {
            public static readonly float labelWidth = SystemObjectInspector.Styles.labelWidth;
            public static readonly float padding = 2;
            public static readonly float spacing = EditorGUIUtility.standardVerticalSpacing;
        }
    }
#else
    public sealed class PatchedVariableDeclarationInspector : Inspector
    {
        private static readonly FieldInfo s_ContextCommandNameFieldInfo;
        private static readonly FieldInfo AdaptorField = typeof(VariableDeclarationsInspector).GetField("adaptor", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo listControlFieldInfo = typeof(MetadataCollectionAdaptor).GetField("listControl", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly GUIContent Rename = new GUIContent("Rename");
        private static readonly GUIContent CommandMoveToTop = new GUIContent("Move to Top");
        private static readonly GUIContent CommandMoveToBottom = new GUIContent("Move to Bottom");
        private static readonly GUIContent CommandDuplicate = new GUIContent("Duplicate");
        private static readonly GUIContent CommandRemove = new GUIContent("Remove");
        private static readonly GUIContent CommandClearAll = new GUIContent("Clear All");

        private MetadataListAdaptor adaptor;
        private ReorderableListControl listControl;

        private const string TextControl = "VariableDeclerationsControl";
        private string renameControl;
        private bool isRenaming;

        private Metadata nameMetadata => metadata[nameof(VariableDeclaration.name)];
        private Metadata valueMetadata => metadata[nameof(VariableDeclaration.value)];
#if VISUAL_SCRIPTING_1_7
        private Metadata typeMetadata => metadata[nameof(VariableDeclaration.typeHandle)];
#endif
        private bool changed;
        private VariableDeclarationsInspector parent;
        private static readonly FieldInfo ParentInspectorField = typeof(Inspector).GetField("parentInspector", BindingFlags.NonPublic | BindingFlags.Instance);

        static PatchedVariableDeclarationInspector()
        {
            s_ContextCommandNameFieldInfo = typeof(ReorderableListControl).GetField("s_ContextCommandName", BindingFlags.Static | BindingFlags.NonPublic);
        }

        public PatchedVariableDeclarationInspector(Metadata metadata)
            : base(metadata)
        {
#if VISUAL_SCRIPTING_1_7
            VSUsageUtility.isVisualScriptingUsed = true;
#endif
        }

        protected override float GetHeight(float width, GUIContent label)
        {
            var height = 0f;

            using (LudiqGUIUtility.labelWidth.Override(Styles.labelWidth))
            {
                height += Styles.padding;
                height += GetNameHeight(width);
#if VISUAL_SCRIPTING_1_7
                height += Styles.spacing;
                height += GetTypeHeight(width);
#endif
                height += Styles.spacing;
                height += GetValueHeight(width);
                height += Styles.padding;
            }

            return height;
        }

        private float GetNameHeight(float width)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        private float GetValueHeight(float width)
        {
            return LudiqGUI.GetInspectorHeight(this, valueMetadata, width);
        }
#if VISUAL_SCRIPTING_1_7
        float GetTypeHeight(float width)
        {
            return LudiqGUI.GetInspectorHeight(this, typeMetadata, width);
        }
#endif

        private bool LoadedState;

        private bool addedItem;

        private void LoadState()
        {
            if (LoadedState) return;

            LoadedState = true;

            parent = ParentInspectorField.GetValueOptimized(this) as VariableDeclarationsInspector;

            adaptor = (MetadataListAdaptor)AdaptorField.GetValueOptimized(parent);

            listControl = (ReorderableListControl)listControlFieldInfo.GetValueOptimized(adaptor);

            adaptor.itemAdded += (v) =>
            {
                addedItem = metadata.value == v;
            };
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            LoadState();

            if (e.type == EventType.ContextClick && position.Contains(e.mousePosition))
            {
                var menu = new GenericMenu();
                AddItemsToMenu(menu, metadata.parent.IndexOf(metadata.value));
                menu.ShowAsContext();
                e.Use();
            }

            position = BeginLabeledBlock(metadata, position, label);

            using (LudiqGUIUtility.labelWidth.Override(Styles.labelWidth))
            {
                y += Styles.padding;
                var namePosition = position.VerticalSection(ref y, GetNameHeight(position.width));
#if VISUAL_SCRIPTING_1_7
                y += Styles.spacing;
                var typePosition = position.VerticalSection(ref y, GetTypeHeight(position.width));
#endif
                y += Styles.spacing;
                var valuePosition = position.VerticalSection(ref y, GetValueHeight(position.width));
                y += Styles.padding;

                OnNameGUI(namePosition);
#if VISUAL_SCRIPTING_1_7
                OnTypeGUI(typePosition);
#endif
                OnValueGUI(valuePosition);
            }

            if (!changed) EndBlock(metadata);
        }

        public void OnNameGUI(Rect namePosition)
        {
            var declaration = (VariableDeclaration)metadata.value;

            var oldName = (string)nameMetadata.value;
            string controlName = TextControl + oldName;

            namePosition = BeginLabeledBlock(nameMetadata, namePosition);
            
            GUI.SetNextControlName(controlName);

            var newName = EditorGUI.DelayedTextField(namePosition, oldName);

            if (renameControl != null && renameControl == controlName)
            {
                GUI.FocusControl(controlName);
                renameControl = null;
            }

            if (EndBlock(nameMetadata) && ProcessRename(oldName, newName, declaration, parent?.kind))
            {
                GUI.FocusControl(null);
            }

            if (addedItem)
            {
                addedItem = false;
                GUI.FocusControl(controlName);
            }
        }

        private bool ProcessRename(string oldName, string newName, VariableDeclaration declaration, VariableKind? kind)
        {
            var variableDeclarations = (VariableDeclarationCollection)metadata.parent.value;

            if (StringUtility.IsNullOrWhiteSpace(newName))
            {
                EditorUtility.DisplayDialog("Edit Variable Name", "Please enter a variable name.", "OK");
                return false;
            }
            if (variableDeclarations.Contains(newName) && newName != oldName)
            {
                EditorUtility.DisplayDialog("Edit Variable Name", "A variable with the same name already exists.", "OK");
                return false;
            }
            if (oldName == newName) return false;

            nameMetadata.RecordUndo();
            RecordSceneUndo(kind);

            variableDeclarations.EditorRename(declaration, newName);
            nameMetadata.value = newName;

            UpdateVariableReferences(kind, oldName, newName);

            if (isRenaming)
            {
                GraphUtility.RenameVariables(parent.kind.Value, oldName, newName, metadata);
            }
            return true;
        }

        private void RecordSceneUndo(VariableKind? kind)
        {
            if (kind != VariableKind.Scene) return;

            if (GraphWindow.active != null && GraphWindow.activeReference?.scene != null)
            {
                Undo.RecordObject(SceneVariables.Instance(GraphWindow.activeReference.scene.Value).variables, "Changed Scene variable name");
                return;
            }

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && VisualScripting.Variables.Scene(scene) == metadata.parent.value)
                {
                    Undo.RecordObject(SceneVariables.Instance(scene).variables, "Changed Scene variable name");
                    break;
                }
            }
        }

        private void UpdateVariableReferences(VariableKind? kind, string oldName, string newName)
        {
            switch (kind)
            {
                case VariableKind.Flow:
                case VariableKind.Graph:
                    if (EditorWindow.focusedWindow == GraphWindow.active)
                        GraphUtility.UpdateAllGraphVariables((FlowGraph)GraphWindow.activeContext.graph, oldName, newName);
                    else if (VariablesWindow.isVariablesWindowContext && VariablesWindow.currentContext != null)
                        GraphUtility.UpdateAllGraphVariables((FlowGraph)VariablesWindow.currentContext.graph, oldName, newName);
                    break;

                case VariableKind.Object:
                    var objAncestor = metadata.Ancestor(m => m.value is VisualScripting.Variables);
                    if (objAncestor?.value != null)
                        GraphUtility.UpdateAllObjectVariables((objAncestor.value as VisualScripting.Variables).gameObject, oldName, newName);
                    else if (EditorWindow.focusedWindow == GraphWindow.active && GraphWindow.activeReference?.gameObject != null)
                        GraphUtility.UpdateAllObjectVariables(GraphWindow.activeReference.gameObject, oldName, newName);
                    break;

                case VariableKind.Scene:
                    var sceneAncestor = metadata.Ancestor(m => m.value is VisualScripting.Variables);
                    if (sceneAncestor?.value != null)
                        GraphUtility.UpdateAllSceneVariables((sceneAncestor.value as VisualScripting.Variables).gameObject.scene, oldName, newName);
                    else if (EditorWindow.focusedWindow == GraphWindow.active && GraphWindow.activeReference?.scene != null)
                        GraphUtility.UpdateAllSceneVariables(GraphWindow.activeReference.scene.Value, oldName, newName);
                    else
                        Debug.LogWarning("[Rename Variables] Could not find valid scene to update variables.");
                    break;

                case VariableKind.Application:
                case VariableKind.Saved:
                    if (Application.isPlaying)
                    {
                        Debug.LogWarning($"[Rename Variables] Cannot rename {kind} variables while in play mode!");
                        break;
                    }
                    bool choice = changed = EditorUtility.DisplayDialog(
                        $"Update ALL {kind} Variables?",
                        $"This will go through ALL scenes and macros to find every Variable Unit using '{oldName}' and update it to '{newName}'.\n\nThis operation is FINAL and cannot be undone!",
                        "Update All", "Rename Only");

                    if (choice && kind == VariableKind.Application) GraphUtility.RenameApplicationVariables(oldName, newName);
                    else if (choice && kind == VariableKind.Saved) GraphUtility.RenameSavedVariables(oldName, newName);
                    break;
            }
        }

        private void DoCommand(GUIContent command, int itemIndex)
        {
            if (command == Rename)
            {
                isRenaming = true;
                renameControl = TextControl + (metadata.parent[itemIndex].value as VariableDeclaration).name;
                return;
            }
            s_ContextCommandNameFieldInfo.SetValueOptimized(null, command.text);
            listControl.DoCommand(command.text, itemIndex, adaptor);
        }

        private void AddItemsToMenu(GenericMenu menu, int itemIndex)
        {
            void Action(object v) => DoCommand(v as GUIContent, itemIndex);

            menu.AddItem(Rename, false, Action, Rename);
            menu.AddSeparator("");

            if (itemIndex > 0) menu.AddItem(CommandMoveToTop, false, Action, CommandMoveToTop);
            else menu.AddDisabledItem(CommandMoveToTop);

            if (itemIndex + 1 < metadata.parent.Count) menu.AddItem(CommandMoveToBottom, false, Action, CommandMoveToBottom);
            else menu.AddDisabledItem(CommandMoveToBottom);

            menu.AddSeparator("");
            menu.AddItem(CommandDuplicate, false, Action, CommandDuplicate);

            if (menu.GetItemCount() > 0) menu.AddSeparator("");

            menu.AddItem(CommandRemove, false, Action, CommandRemove);
            menu.AddSeparator("");
            menu.AddItem(CommandClearAll, false, Action, CommandClearAll);
        }

        public void OnValueGUI(Rect valuePosition)
        {
            LudiqGUI.Inspector(valueMetadata, valuePosition, GUIContent.none);
        }
#if VISUAL_SCRIPTING_1_7
        public void OnTypeGUI(Rect position)
        {
            LudiqGUI.Inspector(typeMetadata, position, GUIContent.none);
        }
#endif
        public static class Styles
        {
            public static readonly float labelWidth = SystemObjectInspector.Styles.labelWidth;
            public static readonly float padding = 2;
            public static readonly float spacing = EditorGUIUtility.standardVerticalSpacing;
        }
    }
#endif
}