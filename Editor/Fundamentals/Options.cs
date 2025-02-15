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
        public static Dictionary<Type, FuzzyLiteralOption> dynamicLiteralOptions = new Dictionary<Type, FuzzyLiteralOption>();
        public static FuzzyExpressionOption fuzzyExpressionOption;

        private static List<Type> dynamicOptionsOrder = new List<Type>
        {
            typeof(string),
            typeof(int),
            typeof(float),
            typeof(long),
            typeof(double),
            typeof(ulong),
            typeof(short),
            typeof(ushort),
            typeof(bool),
            typeof(byte),
            typeof(decimal),
            typeof(DateTime),
            typeof(TimeSpan)
        };
        static Options()
        {
            InitializeFuzzyLiteralOptions();
            fuzzyExpressionOption = new FuzzyExpressionOption(new FuzzyExpression());
            UnitBase.staticUnitsExtensions.Add(GetStaticOptions);
            UnitBase.staticUnitsExtensions.Add(StaticEditorOptions);
            UnitBase.dynamicUnitsExtensions.Add(DynamicEditorOptions);
            UnitBase.dynamicUnitsExtensions.Add(MachineVariableOptions);
            UnitBase.contextualUnitsExtensions.Add(InheritedMembersOptions);
            UnitBase.contextualUnitsExtensions.Add(GenericOptions);
            UnitBase.dynamicUnitsExtensions.Add(GetDynamicOptions);
            UnitBase.contextualUnitsExtensions.Add(SnippetInputNodeOption);
        }

        private static IEnumerable<IUnitOption> GenericOptions(GraphReference reference)
        {
            if (reference.macro is MethodDeclaration method)
            {
                foreach (var generic in method.genericParameters)
                {
                    yield return new GenericNodeOption(new GenericNode(method, method.genericParameters.IndexOf(generic)));
                }
            }
        }

        private static IEnumerable<IUnitOption> SnippetInputNodeOption(GraphReference reference)
        {
            if (reference.macro is GraphSnippet graphSnippet)
            {
                yield return new SnippetInputNodeOption(new SnippetInputNode());

                foreach (var arg in graphSnippet.snippetArguments)
                {
                    var node = new SnippetInputNode();
                    node.argumentName = arg.argumentName;
                    yield return new SnippetInputNodeOption(node);
                }
            }
        }

        private static void InitializeFuzzyLiteralOptions()
        {
            foreach (var optionType in dynamicOptionsOrder)
            {
                if (!dynamicLiteralOptions.ContainsKey(optionType))
                {
                    dynamicLiteralOptions[optionType] = new FuzzyLiteralOption(new FuzzyLiteral(optionType, optionType.PseudoDefault()));
                }
            }
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

        private static IEnumerable<IUnitOption> GetDynamicOptions()
        {
            InitializeFuzzyLiteralOptions();

            yield return fuzzyExpressionOption;

            foreach (var optionType in dynamicOptionsOrder)
            {
                if (dynamicLiteralOptions.ContainsKey(optionType))
                {
                    yield return dynamicLiteralOptions[optionType];
                }
            }
        }


        private static IEnumerable<IUnitOption> InheritedMembersOptions(GraphReference reference)
        {
            if (IsClassAsset(reference.macro))
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
                        {
                            yield return new AssetMethodCallUnitOption(new AssetMethodCallUnit(method.methodName, method, MethodType.ReturnValue));
                            yield return new AssetFuncUnitOption(new AssetFuncUnit(method));
                        }
                        else if (method.returnType == typeof(void) || method.returnType == typeof(Libraries.CSharp.Void))
                        {
                            yield return new AssetActionUnitOption(new AssetActionUnit(method));
                        }
                    }

                    foreach (var field in classAsset.variables)
                    {
                        yield return new AssetFieldUnitOption(new AssetFieldUnit(field.FieldName, field, ActionDirection.Get));
                        yield return new AssetFieldUnitOption(new AssetFieldUnit(field.FieldName, field, ActionDirection.Set));
                    }

                    foreach (var method in inheritedType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                    {
                        if (method.IsPrivate || method.IsAssembly || method.IsProperty() || method.IsEvent() || method.IsGenericMethod) continue;

                        if (method.IsAbstract || method.IsVirtual)
                        {
                            yield return new BaseMethodUnitOption(new BaseMethodCall(new Member(inheritedType, method), MethodType.Invoke));
                            if (method.ReturnType != typeof(void) && method.GetParameters().All(param => !param.HasOutModifier() && !param.ParameterType.IsByRef))
                                yield return new BaseMethodUnitOption(new BaseMethodCall(new Member(inheritedType, method), MethodType.ReturnValue));
                        }

                        if (!method.IsAbstract)
                        {
                            yield return new InheritedMethodUnitOption(new InheritedMethodCall(new Member(inheritedType, method), MethodType.Invoke));
                            if (method.ReturnType != typeof(void))
                                yield return new InheritedMethodUnitOption(new InheritedMethodCall(new Member(inheritedType, method), MethodType.ReturnValue));
                        }
                    }

                    foreach (var property in inheritedType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Where(property => property.GetMethod?.IsAbstract == true || property.SetMethod?.IsAbstract == true))
                    {
                        if (property.GetMethod != null && !property.GetMethod.IsPrivate && !property.GetMethod.IsAssembly && !property.GetMethod.IsGenericMethod)
                        {
                            yield return new BasePropertyGetterUnitOption(new BasePropertyGetterUnit(new Member(inheritedType, property)));
                        }

                        if (property.SetMethod != null && !property.SetMethod.IsPrivate && !property.SetMethod.IsAssembly && !property.SetMethod.IsGenericMethod)
                        {
                            yield return new BasePropertySetterUnitOption(new BasePropertySetterUnit(new Member(inheritedType, property)));
                        }
                    }

                    foreach (var field in inheritedType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                    {
                        if (field.IsPrivate || field.IsAssembly) continue;
                        yield return new InheritedFieldUnitOption(new InheritedFieldUnit(new Member(inheritedType, field), ActionDirection.Get));
                        yield return new InheritedFieldUnitOption(new InheritedFieldUnit(new Member(inheritedType, field), ActionDirection.Set));
                    }

                    foreach (var property in inheritedType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                    {
                        if (property.GetMethod != null && !property.GetMethod.IsPrivate && !property.GetMethod.IsAssembly && !property.GetMethod.IsGenericMethod)
                        {
                            yield return new InheritedFieldUnitOption(new InheritedFieldUnit(new Member(inheritedType, property), ActionDirection.Get));
                        }

                        if (property.SetMethod != null && !property.SetMethod.IsPrivate && !property.SetMethod.IsAssembly && !property.SetMethod.IsGenericMethod)
                        {
                            yield return new InheritedFieldUnitOption(new InheritedFieldUnit(new Member(inheritedType, property), ActionDirection.Set));
                        }
                    }
                }
            }
        }

        private static bool IsClassAsset(IMacro macro)
        {
            if (macro is ConstructorDeclaration or PropertyGetterMacro or PropertySetterMacro or MethodDeclaration)
            {
                return GetClassAsset(macro) != null;
            }
            return false;
        }

        private static ClassAsset GetClassAsset(IMacro macro)
        {
            if (macro is ConstructorDeclaration constructorDeclaration)
            {
                return constructorDeclaration.parentAsset as ClassAsset;
            }
            else if (macro is PropertyGetterMacro propertyGetterMacro)
            {
                return propertyGetterMacro.parentAsset as ClassAsset;
            }
            else if (macro is PropertySetterMacro propertySetterMacro)
            {
                return propertySetterMacro.parentAsset as ClassAsset;
            }
            else if (macro is MethodDeclaration methodDeclaration)
            {
                return methodDeclaration.parentAsset as ClassAsset;
            }
            return null;
        }

        private static bool IsEvent(this MethodInfo methodInfo)
        {
            return (methodInfo.Name.Contains("add_") && methodInfo.DeclaringType.GetEvents()
                    .Any(e => e.GetAddMethod() == methodInfo))
                || (methodInfo.Name.Contains("remove_") && methodInfo.DeclaringType.GetEvents()
                    .Any(e => e.GetRemoveMethod() == methodInfo));
        }


        private static bool IsProperty(this MethodInfo methodInfo)
        {
            var property = methodInfo.DeclaringType
                .GetProperties()
                .FirstOrDefault(p => p.GetGetMethod() == methodInfo || p.GetSetMethod() == methodInfo);

            return property != null;
        }
    }
}
