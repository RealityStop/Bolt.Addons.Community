using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor;
using System.Reflection;
using UnityEngine.SceneManagement;
using Unity.VisualScripting.Community.Libraries.Humility;

#if VISUAL_SCRIPTING_1_7
using SMachine = Unity.VisualScripting.ScriptMachine;
#else
using SMachine = Unity.VisualScripting.FlowMachine;
#endif

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
            typeof(bool),
            typeof(decimal),
            typeof(byte),
            typeof(short),
            typeof(ushort),
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
            UnitBase.contextualUnitsExtensions.Add(MachineVariableOptions);
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

        private static IEnumerable<IUnitOption> MachineVariableOptions(GraphReference reference)
        {
            if (!reference.scene.HasValue || !reference.scene.Value.IsValid())
                yield break;

            var scene = reference.scene.Value;

            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var machine in root.GetComponentsInChildren<SMachine>(true))
                {
                    if (machine == reference.machine as SMachine) continue;
                    var graph = machine.graph;
                    if (graph == null || graph.variables == null)
                        continue;

                    foreach (var variable in graph.variables)
                    {
                        if (string.IsNullOrEmpty(variable.name))
                            continue;

                        yield return new SetMachineVariableNodeOption(
                            new SetMachineVariableNode { defaultName = variable.name, defaultTarget = machine });

                        yield return new GetMachineVariableNodeOption(
                            new GetMachineVariableNode { defaultName = variable.name, defaultTarget = machine });
                    }
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
            if (IsMemberAsset(reference.macro))
            {
                var asset = GetMemberAsset(reference.macro);
                if (asset is ClassAsset classAsset)
                {
                    yield return new AssetTypeOption(new AssetType(classAsset));
                    foreach (var method in classAsset.methods)
                    {
                        yield return new AssetMethodCallUnitOption(new AssetMethodCallUnit(method.methodName, method, MethodType.Invoke));
                        if (!method.returnType.Is().Void())
                        {
                            yield return new AssetMethodCallUnitOption(new AssetMethodCallUnit(method.methodName, method, MethodType.ReturnValue));
                            yield return new AssetFuncUnitOption(new AssetFuncUnit(method));
                        }
                        else
                        {
                            yield return new AssetActionUnitOption(new AssetActionUnit(method));
                        }
                    }

                    foreach (var variable in classAsset.variables)
                    {
                        bool isConst = variable.fieldModifier == Libraries.CSharp.FieldModifier.Constant;

                        if ((variable.isProperty && variable.get) || !variable.isProperty)
                            yield return new AssetFieldUnitOption(new AssetFieldUnit(variable.FieldName, variable, ActionDirection.Get));

                        if ((variable.isProperty && variable.set) || (!variable.isProperty && !isConst))
                            yield return new AssetFieldUnitOption(new AssetFieldUnit(variable.FieldName, variable, ActionDirection.Set));
                    }

                    var inheritedType = classAsset.GetInheritedType();
                    if (inheritedType == null) yield break;

                    foreach (var method in inheritedType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                    {
                        if (method.IsPrivate || method.IsAssembly || method.Name == "Finalize" || method.IsSpecialName || method.IsProperty() || method.IsEvent() || method.IsGenericMethod) continue;

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

                        if (field.CanWrite())
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
                else if (asset is StructAsset structAsset)
                {
                    yield return new AssetTypeOption(new AssetType(structAsset));
                    foreach (var method in structAsset.methods)
                    {
                        yield return new AssetMethodCallUnitOption(new AssetMethodCallUnit(method.methodName, method, MethodType.Invoke));
                        if (!method.returnType.Is().Void())
                        {
                            yield return new AssetMethodCallUnitOption(new AssetMethodCallUnit(method.methodName, method, MethodType.ReturnValue));
                            yield return new AssetFuncUnitOption(new AssetFuncUnit(method));
                        }
                        else
                        {
                            yield return new AssetActionUnitOption(new AssetActionUnit(method));
                        }
                    }

                    foreach (var variable in structAsset.variables)
                    {
                        bool isConst = variable.fieldModifier == Libraries.CSharp.FieldModifier.Constant;

                        if ((variable.isProperty && variable.get) || !variable.isProperty)
                            yield return new AssetFieldUnitOption(new AssetFieldUnit(variable.FieldName, variable, ActionDirection.Get));

                        if ((variable.isProperty && variable.set) || (!variable.isProperty && !isConst))
                            yield return new AssetFieldUnitOption(new AssetFieldUnit(variable.FieldName, variable, ActionDirection.Set));
                    }

                }
            }
        }

        private static bool IsMemberAsset(IMacro macro)
        {
            if (macro is ConstructorDeclaration || macro is PropertyGetterMacro || macro is PropertySetterMacro || macro is MethodDeclaration)
            {
                return GetMemberAsset(macro) != null;
            }
            return false;
        }

        private static CodeAsset GetMemberAsset(IMacro macro)
        {
            if (macro is ConstructorDeclaration constructorDeclaration)
            {
                return constructorDeclaration.parentAsset;
            }
            else if (macro is PropertyGetterMacro propertyGetterMacro)
            {
                return propertyGetterMacro.parentAsset;
            }
            else if (macro is PropertySetterMacro propertySetterMacro)
            {
                return propertySetterMacro.parentAsset;
            }
            else if (macro is MethodDeclaration methodDeclaration)
            {
                return methodDeclaration.parentAsset;
            }
            return null;
        }

        private static bool IsEvent(this MethodInfo method)
        {
            if (method == null || method.DeclaringType == null)
                return false;

            if (!method.IsSpecialName)
                return false;

            var events = method.DeclaringType.GetEvents(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            foreach (var ev in events)
            {
                if (ev.AddMethod == method || ev.RemoveMethod == method)
                    return true;
            }

            return false;
        }

        private static bool IsProperty(this MethodInfo method)
        {
            if (method == null || method.DeclaringType == null)
                return false;

            if (!method.IsSpecialName)
                return false;

            var props = method.DeclaringType.GetProperties(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            foreach (var prop in props)
            {
                if (prop.GetMethod == method || prop.SetMethod == method)
                    return true;
            }

            return false;
        }
    }
}
