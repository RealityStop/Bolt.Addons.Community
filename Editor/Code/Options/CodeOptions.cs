using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Bolt.Addons.Libraries.Humility;
using System.Reflection;
using Bolt.Addons.Community.Fundamentals.Editor.UnitOptions;
using Bolt.Addons.Community.Utility;
using Bolt.Addons.Community.Fundamentals;
using Bolt.Addons.Libraries.CSharp;

namespace Bolt.Addons.Community.Code.Editor
{
    [InitializeAfterPlugins]
    public static class CodeOptions
    {
        static CodeOptions()
        {
            UnitBase.dynamicUnitsExtensions.Add(DelegateOptions);
        }

        private static IEnumerable<IUnitOption> DelegateMemberOptions()
        {
            var typeAsset = AssetDatabase.LoadAssetAtPath<DictionaryAsset>("Assets/Unity.VisualScripting.Generated/VisualScripting.Core");
            var types = typeAsset?["typeOptions"] as List<Type>;
            if (types == null) yield break;

            for (int i = 0; i < types.Count; i++)
            {
                var delegateFields = types[i].GetFields();
                var delegateProperties = types[i].GetProperties();
                for (int d = 0; d < delegateFields.Length; d++)
                {
                    var del = delegateFields[d];
                    if (del.IsPublic && del.FieldType.Inherits<Delegate>())
                    {
                        yield return new GetMemberOption(new GetMember(new Member(types[i], del)));
                        yield return new SetMemberOption(new SetMember(new Member(types[i], del)));
                    }
                }

                for (int d = 0; d < delegateProperties.Length; d++)
                {
                    var del = delegateProperties[d];
                    if (del.PropertyType.Inherits<Delegate>())
                    {
                        if (del.CanRead) yield return new GetMemberOption(new GetMember(new Member(types[i], del)));
                        if (del.CanWrite) yield return new SetMemberOption(new SetMember(new Member(types[i], del)));
                    }
                }

                var events = types[i].GetEvents(BindingFlags.Public);

                for (int ev = 0; ev < events.Length; ev++)
                {
                    yield return new GetMemberOption(new GetMember(new Member(types[i], events[ev].Name)));
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
