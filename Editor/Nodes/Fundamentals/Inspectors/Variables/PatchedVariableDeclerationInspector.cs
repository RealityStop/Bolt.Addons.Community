using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.VisualScripting.Community
{
    public sealed class PatchedVariableDeclarationInspector : Inspector
    {
        private Metadata nameMetadata => metadata[nameof(VariableDeclaration.name)];
        private Metadata valueMetadata => metadata[nameof(VariableDeclaration.value)];
#if VISUAL_SCRIPTING_1_7
        private Metadata typeMetadata => metadata[nameof(VariableDeclaration.typeHandle)];
#endif
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
        protected override void OnGUI(Rect position, GUIContent label)
        {
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

            if (!changed)
                EndBlock(metadata);
        }

        private bool changed;

        public void OnNameGUI(Rect namePosition)
        {
            namePosition = BeginLabeledBlock(nameMetadata, namePosition);

            var oldName = (string)nameMetadata.value;
            var newName = EditorGUI.DelayedTextField(namePosition, (string)nameMetadata.value);

            if (EndBlock(nameMetadata))
            {
                var variableDeclarations = (VariableDeclarationCollection)metadata.parent.value;
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
                var kind =
                (typeof(Inspector).GetField("parentInspector", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this) as VariableDeclarationsInspector).kind;
                nameMetadata.RecordUndo();
                if (kind == VariableKind.Scene)
                {
                    if (GraphWindow.active != null && GraphWindow.activeReference?.scene != null)
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
                            bool choice = changed = EditorUtility.DisplayDialog(
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
                            bool choice = changed = EditorUtility.DisplayDialog(
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
}
