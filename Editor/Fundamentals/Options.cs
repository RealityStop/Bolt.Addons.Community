
using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;
using Bolt.Addons.Community.Variables.Editor.UnitOptions;
using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bolt.Addons.Community.Variables.Editor
{
    [InitializeAfterPlugins]
    public static class Options
    {
        static Options()
        {
            UnitBase.staticUnitsExtensions.Add(GetStaticOptions);
            UnitBase.dynamicUnitsExtensions.Add(DelegateOptions);
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
                            yield return new ActionInvokeUnitOption(new ActionInvokeUnit(Activator.CreateInstance(types[type] as System.Type) as IAction));
                            yield return new BindActionDelegateUnitOption(new BindDelegateUnit(Activator.CreateInstance(types[type] as System.Type) as IAction));
                        }

                        if (typeof(IFunc).IsAssignableFrom(types[type]))
                        {
                            yield return new FuncUnitOption(new FuncUnit(Activator.CreateInstance(types[type] as System.Type) as IFunc));
                            yield return new FuncInvokeUnitOption(new FuncInvokeUnit(Activator.CreateInstance(types[type] as System.Type) as IFunc));
                            yield return new BindFuncDelegateUnitOption(new BindDelegateUnit(Activator.CreateInstance(types[type] as System.Type) as IFunc));
                        }
                    }
                }
            }
        }
    }
}
