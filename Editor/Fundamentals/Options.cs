using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor;
using System.Reflection;
using UnityEngine;

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
            UnitBase.contextualUnitsExtensions.Add(InheritedMembersOptions);
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

        private static IEnumerable<IUnitOption> InheritedMembersOptions(GraphReference reference)
        {
            var classAsset = GetClassAsset(reference.macro);
            var inheritedType = classAsset.GetInheritedType();
            if (classAsset != null && inheritedType != null)
            {
                yield return new AssetTypeOption(new AssetType(classAsset));
                foreach (var method in classAsset.methods)
                {
                    yield return new AssetMethodCallUnitOption(new AssetMethodCallUnit(method.methodName, method, MethodType.Invoke));
                    if (method.returnType != typeof(void) && method.returnType != typeof(Libraries.CSharp.Void))
                        yield return new AssetMethodCallUnitOption(new AssetMethodCallUnit(method.methodName, method, MethodType.ReturnValue));
                }

                foreach (var field in classAsset.variables)
                {
                    yield return new AssetFieldUnitOption(new AssetFieldUnit(field.FieldName, field, ActionDirection.Get));
                    yield return new AssetFieldUnitOption(new AssetFieldUnit(field.FieldName, field, ActionDirection.Set));
                }

                foreach (var method in inheritedType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    if (method.IsPrivate || method.IsProperty() || method.IsEvent()) continue;

                    if (method.IsAbstract || method.IsVirtual)
                    {
                        yield return new BaseMethodUnitOption(new BaseMethodCall(new Member(inheritedType, method), MethodType.Invoke));
                        if (method.ReturnType != typeof(void) && method.GetParameters().All(param => !param.HasOutModifier() && !param.ParameterType.IsByRef))
                            yield return new BaseMethodUnitOption(new BaseMethodCall(new Member(inheritedType, method), MethodType.ReturnValue));
                    }
                    else
                    {
                        yield return new InheritedMethodUnitOption(new InheritedMethodCall(new Member(inheritedType, method), MethodType.Invoke));
                        if (method.ReturnType != typeof(void))
                            yield return new InheritedMethodUnitOption(new InheritedMethodCall(new Member(inheritedType, method), MethodType.ReturnValue));
                    }
                }

                foreach (var property in inheritedType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Where(property => property.GetMethod?.IsAbstract == true || property.SetMethod?.IsAbstract == true))
                {
                    if (property.GetMethod != null && !property.GetMethod.IsPrivate)
                    {
                        yield return new BasePropertyGetterUnitOption(new BasePropertyGetterUnit(new Member(inheritedType, property)));
                    }

                    if (property.SetMethod != null && !property.SetMethod.IsPrivate)
                    {
                        yield return new BasePropertySetterUnitOption(new BasePropertySetterUnit(new Member(inheritedType, property)));
                    }
                }

                foreach (var field in inheritedType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    if (field.IsPrivate) continue;
                    yield return new InheritedFieldUnitOption(new InheritedFieldUnit(new Member(inheritedType, field), ActionDirection.Get));
                    yield return new InheritedFieldUnitOption(new InheritedFieldUnit(new Member(inheritedType, field), ActionDirection.Set));
                }

                foreach (var property in inheritedType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    if (property.GetMethod != null && !property.GetMethod.IsPrivate)
                    {
                        yield return new InheritedFieldUnitOption(new InheritedFieldUnit(new Member(inheritedType, property), ActionDirection.Get));
                    }

                    if (property.SetMethod != null && !property.SetMethod.IsPrivate)
                    {
                        yield return new InheritedFieldUnitOption(new InheritedFieldUnit(new Member(inheritedType, property), ActionDirection.Set));
                    }
                }
            }
        }

        private static ClassAsset GetClassAsset(IMacro macro)
        {
            if (macro is ConstructorDeclaration constructorDeclaration)
            {
                return constructorDeclaration.classAsset;
            }
            else if (macro is PropertyGetterMacro propertyGetterMacro)
            {
                return propertyGetterMacro.classAsset;
            }
            else if (macro is PropertySetterMacro propertySetterMacro)
            {
                return propertySetterMacro.classAsset;
            }
            else if (macro is MethodDeclaration methodDeclaration)
            {
                return methodDeclaration.classAsset;
            }
            return null;
        }

        private static bool IsEvent(this MethodInfo methodInfo)
        {
            return methodInfo.Name.Contains("add_") || methodInfo.Name.Contains("remove_");
        }

        private static bool IsProperty(this MethodInfo methodInfo)
        {
            return methodInfo.Name.Contains("get_") || methodInfo.Name.Contains("set_");
        }
    }
}
