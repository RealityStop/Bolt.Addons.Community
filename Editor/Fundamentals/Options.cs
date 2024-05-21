using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor;

namespace Unity.VisualScripting.Community.Variables.Editor
{
    [InitializeAfterPlugins]
    public static class Options
    {
        static Options()
        {
            UnitBase.staticUnitsExtensions.Add(GetStaticOptions);
            UnitBase.staticUnitsExtensions.Add(StaticEditorOptions);
            UnitBase.dynamicUnitsExtensions.Add(DynamicEditorOptions);
            UnitBase.dynamicUnitsExtensions.Add(MachineVariableOptions);
        }

        private static IEnumerable<IUnitOption> GetStaticOptions()
        {
            foreach (var variableKind in Enum.GetValues(typeof(VariableKind)).Cast<VariableKind>())
            {
                yield return new IncrementNodeOption(variableKind);
                yield return new DecrementNodeOption(variableKind);
                yield return new PlusEqualNodeOption(variableKind);
                yield return new OnVariableChangedOption(variableKind);
                yield return new GetDictionaryVariableItemNodeOption(variableKind);
                yield return new SetDictionaryVariableItemNodeOption(variableKind);
            }
        }

        private static IEnumerable<IUnitOption> MachineVariableOptions()
        {
            List<IUnitOption> options = new List<IUnitOption>();

            string[] scriptGraphAssetGuids = AssetDatabase.FindAssets($"t:{typeof(ScriptGraphAsset)}");
            foreach (var scriptGraphAssetGuid in scriptGraphAssetGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(scriptGraphAssetGuid);
                ScriptGraphAsset scriptGraphAsset = AssetDatabase.LoadAssetAtPath<ScriptGraphAsset>(assetPath);

                var variables = scriptGraphAsset.graph.variables;

                foreach (var variable in variables)
                {
                    options.Add(new SetMachineVariableNodeOption(new SetMachineVariableNode() { asset = scriptGraphAsset, defaultName = variable.name }));
                    options.Add(new GetMachineVariableNodeOption(new GetMachineVariableNode() { asset = scriptGraphAsset, defaultName = variable.name }));
                }
            }

            foreach (var option in options)
            {
                yield return option;
            }
        }

        private static IEnumerable<IUnitOption> StaticEditorOptions()
        {
            yield return new EditorWindowOnDestroyEventOption(new EditorWindowOnDestroy());
            yield return new EditorWindowOnDisableEventOption(new EditorWindowOnDisable());
            yield return new EditorWindowOnEnableEventOption(new EditorWindowOnEnable());
            yield return new EditorWindowOnFocusEventOption(new EditorWindowOnFocus());
            yield return new EditorWindowOnLostFocusEventOption(new EditorWindowOnLostFocus());
            yield return new EditorWindowOnGUIEventOption(new EditorWindowOnGUI());
        }

        private static IEnumerable<IUnitOption> DynamicEditorOptions()
        {
            List<IUnitOption> options = new List<IUnitOption>();

            string[] editorWindowAssetGuids = AssetDatabase.FindAssets($"t:{typeof(EditorWindowAsset)}");
            foreach (var editorWindowAssetGuid in editorWindowAssetGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(editorWindowAssetGuid);
                EditorWindowAsset editorWindowAsset = AssetDatabase.LoadAssetAtPath<EditorWindowAsset>(assetPath);

                var variables = editorWindowAsset.variables.variables;

                options.Add(new WindowIsNodeOption(new WindowIsNode() { asset = editorWindowAsset }));

                foreach (var variable in variables)
                {
                    options.Add(new GetWindowVariableNodeOption(new GetWindowVariableNode() { asset = editorWindowAsset, defaultName = variable.name }));
                    options.Add(new SetWindowVariableNodeOption(new SetWindowVariableNode() { asset = editorWindowAsset, defaultName = variable.name }));
                }
            }

            foreach (var option in options)
            {
                yield return option;
            }
        }
    }
}
