using Unity.VisualScripting.Community.Libraries.Humility;
using System.Collections.Generic;
using System.Linq;

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
            yield return new IncrementNodeOption(VariableKind.Flow);
            yield return new IncrementNodeOption(VariableKind.Graph);
            yield return new IncrementNodeOption(VariableKind.Object);
            yield return new IncrementNodeOption(VariableKind.Scene);
            yield return new IncrementNodeOption(VariableKind.Application);
            yield return new IncrementNodeOption(VariableKind.Saved);

            yield return new DecrementNodeOption(VariableKind.Flow);
            yield return new DecrementNodeOption(VariableKind.Graph);
            yield return new DecrementNodeOption(VariableKind.Object);
            yield return new DecrementNodeOption(VariableKind.Scene);
            yield return new DecrementNodeOption(VariableKind.Application);
            yield return new DecrementNodeOption(VariableKind.Saved);

            yield return new PlusEqualNodeOption(VariableKind.Flow);
            yield return new PlusEqualNodeOption(VariableKind.Graph);
            yield return new PlusEqualNodeOption(VariableKind.Object);
            yield return new PlusEqualNodeOption(VariableKind.Scene);
            yield return new PlusEqualNodeOption(VariableKind.Application);
            yield return new PlusEqualNodeOption(VariableKind.Saved);

            yield return new OnVariableChangedOption(VariableKind.Graph);
            yield return new OnVariableChangedOption(VariableKind.Object);
            yield return new OnVariableChangedOption(VariableKind.Scene);
            yield return new OnVariableChangedOption(VariableKind.Application);
            yield return new OnVariableChangedOption(VariableKind.Saved);

            yield return new GetDictionaryVariableItemNodeOption(VariableKind.Graph);
            yield return new GetDictionaryVariableItemNodeOption(VariableKind.Object);
            yield return new GetDictionaryVariableItemNodeOption(VariableKind.Scene);
            yield return new GetDictionaryVariableItemNodeOption(VariableKind.Application);
            yield return new GetDictionaryVariableItemNodeOption(VariableKind.Saved);

            yield return new SetDictionaryVariableItemNodeOption(VariableKind.Graph);
            yield return new SetDictionaryVariableItemNodeOption(VariableKind.Object);
            yield return new SetDictionaryVariableItemNodeOption(VariableKind.Scene);
            yield return new SetDictionaryVariableItemNodeOption(VariableKind.Application);
            yield return new SetDictionaryVariableItemNodeOption(VariableKind.Saved);
        }

        private static IEnumerable<IUnitOption> MachineVariableOptions()
        {
            var assets = HUMAssets.Find().Assets().OfType<ScriptGraphAsset>();

            for (int i = 0; i < assets.Count; i++)
            {
                var variables = assets[i].graph.variables.ToArrayPooled();

                for (int varIndex = 0; varIndex < variables.Length; varIndex++)
                {
                    yield return new SetMachineVariableNodeOption(new SetMachineVariableNode() { asset = assets[i], defaultName = variables[varIndex].name });
                    yield return new GetMachineVariableNodeOption(new GetMachineVariableNode() { asset = assets[i], defaultName = variables[varIndex].name });
                }
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
            var assets = HUMAssets.Find().Assets().OfType<EditorWindowAsset>();

            for (int i = 0; i < assets.Count; i++)
            {
                var variables = assets[i].variables.variables;

                yield return new WindowIsNodeOption(new WindowIsNode() { asset = assets[i] });

                for (int varIndex = 0; varIndex < variables.Count; varIndex++)
                {
                    yield return new GetWindowVariableNodeOption(new GetWindowVariableNode() { asset = assets[i], defaultName = variables[varIndex].name });
                    yield return new SetWindowVariableNodeOption(new SetWindowVariableNode() { asset = assets[i], defaultName = variables[varIndex].name });
                }
            }
        }
    }
}
