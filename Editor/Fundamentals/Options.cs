using Bolt.Addons.Community.Fundamentals;
using Bolt.Addons.Community.Fundamentals.Editor.UnitOptions;
using Bolt.Addons.Community.Utility;
using Bolt.Addons.Community.Utility.Editor;
using Bolt.Addons.Community.Utility.Editor.UnitOptions;
using Bolt.Addons.Community.Variables.Editor.UnitOptions;
using Bolt.Addons.Libraries.Humility;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Variables.Editor
{
    [InitializeAfterPlugins]
    public static class Options
    {
        static Options()
        {
            UnitBase.staticUnitsExtensions.Add(GetStaticOptions);
            UnitBase.staticUnitsExtensions.Add(StaticEditorOptions);
            UnitBase.dynamicUnitsExtensions.Add(DynamicEditorOptions);
            UnitBase.dynamicUnitsExtensions.Add(DelegateOptions);
            UnitBase.dynamicUnitsExtensions.Add(MachineVariableOptions);
        }

        private static IEnumerable<IUnitOption> GetStaticOptions()
        {
            yield return new IncrementUnitOption(VariableKind.Flow);
            yield return new IncrementUnitOption(VariableKind.Graph);
            yield return new IncrementUnitOption(VariableKind.Object);
            yield return new IncrementUnitOption(VariableKind.Scene);
            yield return new IncrementUnitOption(VariableKind.Application);
            yield return new IncrementUnitOption(VariableKind.Saved);

            yield return new DecrementUnitOption(VariableKind.Flow);
            yield return new DecrementUnitOption(VariableKind.Graph);
            yield return new DecrementUnitOption(VariableKind.Object);
            yield return new DecrementUnitOption(VariableKind.Scene);
            yield return new DecrementUnitOption(VariableKind.Application);
            yield return new DecrementUnitOption(VariableKind.Saved);

            yield return new PlusEqualUnitOption(VariableKind.Flow);
            yield return new PlusEqualUnitOption(VariableKind.Graph);
            yield return new PlusEqualUnitOption(VariableKind.Object);
            yield return new PlusEqualUnitOption(VariableKind.Scene);
            yield return new PlusEqualUnitOption(VariableKind.Application);
            yield return new PlusEqualUnitOption(VariableKind.Saved);

            yield return new OnVariableChangedOption(VariableKind.Graph);
            yield return new OnVariableChangedOption(VariableKind.Object);
            yield return new OnVariableChangedOption(VariableKind.Scene);
            yield return new OnVariableChangedOption(VariableKind.Application);
            yield return new OnVariableChangedOption(VariableKind.Saved);

            yield return new GetDictionaryVariableItemUnitOption(VariableKind.Graph);
            yield return new GetDictionaryVariableItemUnitOption(VariableKind.Object);
            yield return new GetDictionaryVariableItemUnitOption(VariableKind.Scene);
            yield return new GetDictionaryVariableItemUnitOption(VariableKind.Application);
            yield return new GetDictionaryVariableItemUnitOption(VariableKind.Saved);

            yield return new SetDictionaryVariableItemUnitOption(VariableKind.Graph);
            yield return new SetDictionaryVariableItemUnitOption(VariableKind.Object);
            yield return new SetDictionaryVariableItemUnitOption(VariableKind.Scene);
            yield return new SetDictionaryVariableItemUnitOption(VariableKind.Application);
            yield return new SetDictionaryVariableItemUnitOption(VariableKind.Saved);
        }

        private static IEnumerable<IUnitOption> MachineVariableOptions()
        {
            var assets = HUMAssets.Find().Assets().OfType<ScriptGraphAsset>();

            for (int i = 0; i < assets.Count; i++)
            {
                var variables = assets[i].graph.variables.ToArrayPooled();

                for (int varIndex = 0; varIndex < variables.Length; varIndex++)
                {
                    yield return new SetMachineVariableUnitOption(new SetMachineVariableUnit() { asset = assets[i], defaultName = variables[varIndex].name });
                    yield return new GetMachineVariableUnitOption(new GetMachineVariableUnit() { asset = assets[i], defaultName = variables[varIndex].name });
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

                yield return new WindowIsUnitOption(new WindowIsUnit() { asset = assets[i] });

                for (int varIndex = 0; varIndex < variables.Count; varIndex++)
                {
                    yield return new GetWindowVariableUnitOption(new GetWindowVariableUnit() { asset = assets[i], defaultName = variables[varIndex].name });
                    yield return new SetWindowVariableUnitOption(new SetWindowVariableUnit() { asset = assets[i], defaultName = variables[varIndex].name });
                }
            }
        }

        private static IEnumerable<IUnitOption> DelegateOptions()
        {
            List<Type> result = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int assembly = 0; assembly < assemblies.Length; assembly++)
            {
                Type[] types = assemblies[assembly].GetTypes();

                for (int type = 0; type < types.Length; type++)
                {
                    if (!types[type].IsAbstract)
                    {
                        if (typeof(IAction).IsAssignableFrom(types[type]))
                        {
                            yield return new ActionUnitOption(new ActionUnit(Activator.CreateInstance(types[type] as System.Type) as IAction));
                            yield return new ActionInvokeUnitOption(new ActionInvokeUnit() { _delegate = Activator.CreateInstance(types[type] as System.Type) as IAction });
                            yield return new BindActionUnitOption(new BindActionUnit() { _delegate = Activator.CreateInstance(types[type] as System.Type) as IAction });
                            yield return new UnbindActionUnitOption(new UnbindActionUnit() { _delegate = Activator.CreateInstance(types[type] as System.Type) as IAction });
                        }

                        if (typeof(IFunc).IsAssignableFrom(types[type]))
                        {
                            yield return new FuncUnitOption(new FuncUnit(Activator.CreateInstance(types[type] as System.Type) as IFunc));
                            yield return new FuncInvokeUnitOption(new FuncInvokeUnit() { _delegate = Activator.CreateInstance(types[type] as System.Type) as IFunc });
                            yield return new BindFuncUnitOption(new BindFuncUnit() { _delegate = Activator.CreateInstance(types[type] as System.Type) as IFunc });
                            yield return new UnbindFuncUnitOption(new UnbindFuncUnit() { _delegate = Activator.CreateInstance(types[type] as System.Type) as IFunc });
                        }
                    }
                }
            }
        }
    }
}
