using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using NUnit.Framework.Internal;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Utility;
using Unity.VisualScripting.ReorderableList;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public abstract class MemberTypeAssetEditor<TMemberTypeAsset, TMemberTypeGenerator, TFieldDeclaration, TMethodDeclaration, TConstructorDeclaration> : CodeAssetEditor<TMemberTypeAsset, TMemberTypeGenerator>
        where TMemberTypeAsset : MemberTypeAsset<TFieldDeclaration, TMethodDeclaration, TConstructorDeclaration>
        where TMemberTypeGenerator : MemberTypeAssetGenerator<TMemberTypeAsset, TFieldDeclaration, TMethodDeclaration, TConstructorDeclaration>
        where TFieldDeclaration : FieldDeclaration
        where TMethodDeclaration : MethodDeclaration
        where TConstructorDeclaration : ConstructorDeclaration
    {

        public Dictionary<string, object> AttributeParameters;

        protected Metadata attributes;
        protected SerializedProperty attributesProp;
        protected Metadata constructors;
        protected SerializedProperty constructorsProp;

        protected Metadata methods;
        protected SerializedProperty methodsProp;

        protected Metadata typeIcon;
        protected SerializedProperty typeIconProp;

        protected Metadata variables;
        protected SerializedProperty variablesProp;

        private Type[] attributeTypes = new Type[] { };
        private Type[] classAttributeTypes = new Type[] { };

        private int constructorsCount;
        private Type[] enumAttributeTypes = new Type[] { };
        private Type[] fieldAttributeTypes = new Type[] { };
        private Type[] parameterAttributeTypes = new Type[] { };
        private int fieldsCount;
        private Type[] interfaceAttributeTypes = new Type[] { };
        private Type[] methodAttributeTypes = new Type[] { };
        private int methodsCount;
        private Type[] propertyAttributeTypes = new Type[] { };
        private Type[] structAttributeTypes = new Type[] { };

        private Type ValueInspectorType;

        private enum AttributeUsageType
        {
            Class,
            Struct,
            Enum,
            Interface,
            Field,
            Property,
            Method,
            Parameter
        }

        private Color boxBackground => HUMColor.Grey(0.15f);

        protected override void BeforePreview()
        {
            GraphWindow.active?.context?.BeginEdit();
            Constructors();
            GUILayout.Space(4);
            Variables();
            GUILayout.Space(4);
            Methods();

            // Check if the target is a ClassAsset and if it has required information
            if (typeof(TMemberTypeAsset) == typeof(ClassAsset) && (Target as ClassAsset).inheritsType)
            {
                GUILayout.Space(4);
                RequiredInfo();
                GUILayout.Space(4);
                OverridableMembersInfo();
            }
            GraphWindow.active?.context?.EndEdit();
        }

        private bool RequiresInfo()
        {
            var classAsset = Target as ClassAsset;
            var inheritedType = classAsset.inherits.type;

            // Check for abstract methods
            if (inheritedType == null) return false;
            if (inheritedType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(m => !m.Name.Contains("get_") && !m.Name.Contains("set_")).Any(m => m.IsAbstract && !MethodExists(m)))
            {
                return true;
            }

            // Check for abstract properties
            if (inheritedType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Any(p => (p.GetMethod?.IsAbstract == true || p.SetMethod?.IsAbstract == true) && !PropertyExists(p)))
            {
                return true;
            }

            // // Check for abstract events
            // if (inheritedType.GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Any(e => e.GetAddMethod(true)?.IsAbstract == true || e.GetRemoveMethod(true)?.IsAbstract == true))
            // {
            //     return true;
            // }

            // Check if the inherited type is not abstract and is a class with parameterized constructors
            if (!inheritedType.IsAbstract && inheritedType.IsClass)
            {
                if (inheritedType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .Any(constructor => constructor.GetParameters().Length > 0 && !ConstructorExists(constructor)))
                {
                    return true;
                }
            }

            return false;
        }

        private bool ConstructorExists(ConstructorInfo c)
        {
            var listOfConstructors = constructors.value as List<TConstructorDeclaration>;
            if (typeof(TMemberTypeAsset) == typeof(ClassAsset))
            {
                foreach (var constructor in listOfConstructors)
                {
                    foreach (var param in constructor.parameters.Where(param => !param.showCall))
                    {
                        param.showCall = true;
                    }
                    if (constructor.scope != c.GetScope()) continue;
                    var inheritedType = (Target as ClassAsset).inherits.type;
                    var constructorParameters = c.GetParameters();
                    bool requiredParametersHaveBase = constructor.parameters.Where(parameter => parameter.useInCall).Select(parameter => parameter.type).SequenceEqual(constructorParameters.Select(parameter => parameter.ParameterType));
                    if (!requiredParametersHaveBase)
                    {
                        continue;
                    }

                    if (!constructor.parameters.Where(parameter => parameter.useInCall).Select(parameter => parameter.modifier).SequenceEqual(constructorParameters.Select(parameter => parameter.GetModifier()))) continue;

                    return true;
                }
            }
            return false;
        }

        private bool MethodExists(MethodInfo m)
        {
            var listOfMethods = methods.value as List<TMethodDeclaration>;
            if (!listOfMethods.Any(method => method.methodName == m.Name)) return false;
            foreach (var method in listOfMethods.Where(method => method.methodName == m.Name))
            {
                if (method.scope != m.GetScope()) continue;
                if (method.modifier != MethodModifier.Override) continue;
                if (method.parameters.Count != m.GetParameters().Length) continue;
                if (method.returnType != m.ReturnType) continue;
                if (!method.parameters.Select(parameter => parameter.type).SequenceEqual(m.GetParameters().Select(parameter => parameter.ParameterType))) continue;
                if (!method.parameters.Select(parameter => parameter.modifier).SequenceEqual(m.GetParameters().Select(parameter => parameter.GetModifier()))) continue;
                return true;
            }
            return false;
        }

        private bool PropertyExists(PropertyInfo p)
        {
            var listOfFields = variables.value as List<TFieldDeclaration>;
            if (!listOfFields.Any(field => field.name == p.Name)) return false;
            foreach (var property in listOfFields.Where(fields => fields.FieldName == p.Name && fields.isProperty))
            {
                if (property.scope != p.GetScope()) continue;
                if (property.propertyModifier != PropertyModifier.Override) continue;
                if (property.type != p.PropertyType) continue;
                /* If a getter is not expected but present || If a getter is expected but not present */
                if ((property.get && !p.CanRead) || (!property.get && p.CanRead)) continue;
                /* If a setter is not expected but present || If a setter is expected but not present */
                if ((property.set && !p.CanWrite) || (!property.set && p.CanWrite)) continue;
                if (p.CanRead)
                {
                    if (property.getterScope != p.GetGetMethod(true).GetScope()) continue;
                }

                if (p.CanWrite)
                {
                    if (property.setterScope != p.GetSetMethod(true).GetScope()) continue;
                }

                return true;
            }
            return false;
        }

        private void RequiredInfo()
        {
            Target.requiredInfoOpened = HUMEditor.Foldout(Target.requiredInfoOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2, () =>
            {
                if (RequiresInfo())
                {
                    HUMEditor.Image(PathUtil.Load("warning_32", CommunityEditorPath.Code).Single(), 16, 16, new RectOffset(), new RectOffset(4, 8, 4, 4));
                }
                else
                {
                    HUMEditor.Image(PathUtil.Load("okay_32", CommunityEditorPath.Code).Single(), 16, 16, new RectOffset(), new RectOffset(4, 8, 4, 4));
                }
                GUILayout.Label("Required Info");
            }, () =>
            {
                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                {
                    if (!RequiresInfo())
                    {
                        EditorGUILayout.HelpBox("This class does not require anything", MessageType.Info);
                        return;
                    }

                    var listOfConstructors = constructors.value as List<TConstructorDeclaration>;
                    var listOfMethods = methods.value as List<TMethodDeclaration>;
                    var listOfVariables = variables.value as List<TFieldDeclaration>;

                    var classAsset = Target as ClassAsset;
                    var inheritedType = classAsset.inherits.type;

                    var abstractMethods = inheritedType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                        .Where(m => m.IsAbstract && !m.Name.Contains("get_") && !m.Name.Contains("set_"))
                        .ToArray();

                    for (int i = 0; i < abstractMethods.Length; i++)
                    {
                        var index = i;
                        HUMEditor.Horizontal().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                        {
                            GUILayout.Label(abstractMethods[index].Name);

                            if (GUILayout.Button("Add", GUILayout.Width(60)))
                            {
                                int undoGroup = Undo.GetCurrentGroup();
                                Undo.SetCurrentGroupName("Added Required Method");

                                Undo.RegisterCompleteObjectUndo(Target, "Added Required Method");

                                var declaration = CreateInstance<TMethodDeclaration>();
                                Undo.RegisterCreatedObjectUndo(declaration, "Created Method Declaration");

                                if (Target is ClassAsset classAsset)
                                    declaration.classAsset = classAsset;
                                else if (Target is StructAsset structAsset)
                                    declaration.structAsset = structAsset;

                                declaration.modifier = MethodModifier.Override;
                                declaration.methodName = abstractMethods[i].Name;
                                declaration.name = declaration.methodName;
                                declaration.scope = abstractMethods[i].GetScope();
                                declaration.returnType = abstractMethods[i].ReturnType;
                                declaration.parameters = abstractMethods[i]
                                    .GetParameters()
                                    .Select(param => new TypeParam(param.ParameterType, param.Name))
                                    .ToList();

                                declaration.hideFlags = HideFlags.HideInHierarchy;
                                AssetDatabase.AddObjectToAsset(declaration, Target);
                                listOfMethods.Add(declaration);
                                var functionUnit = new FunctionNode(FunctionType.Method)
                                {
                                    methodDeclaration = declaration
                                };
                                declaration.graph.units.Add(functionUnit);

                                Undo.CollapseUndoOperations(undoGroup);
                                shouldUpdate = true;
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();
                            }

                            if (methods.Count != methodsCount)
                            {
                                if (Target is ClassAsset)
                                {
                                    for (int i = 0; i < methods.Count; i++)
                                    {
                                        ((TMethodDeclaration)methods[i].value).classAsset = Target as ClassAsset;
                                    }
                                }
                                else
                                {
                                    if (Target is StructAsset)
                                    {
                                        for (int i = 0; i < methods.Count; i++)
                                        {
                                            ((TMethodDeclaration)methods[i].value).structAsset = Target as StructAsset;
                                        }
                                    }
                                }

                                methodsCount = methods.Count;
                            }
                        }, true);

                        GUILayout.Space(4);
                    }

                    var abstractProperties = inheritedType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .Where(p => p.GetGetMethod(true)?.IsAbstract == true || p.GetSetMethod(true)?.IsAbstract == true)
                    .ToArray();

                    for (int i = 0; i < abstractProperties.Length; i++)
                    {
                        var index = i;
                        HUMEditor.Horizontal().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                        {
                            GUILayout.Label(abstractProperties[index].Name);
                            var property = abstractProperties[index];
                            if (GUILayout.Button("Add", GUILayout.Width(60)))
                            {
                                int undoGroup = Undo.GetCurrentGroup();
                                Undo.SetCurrentGroupName("Added Required Property");

                                Undo.RegisterCompleteObjectUndo(Target, "Added Required Property");

                                var declaration = CreateInstance<TFieldDeclaration>();
                                Undo.RegisterCreatedObjectUndo(declaration, "Created Field Declaration");

                                if (Target is ClassAsset classAsset)
                                    declaration.classAsset = classAsset;
                                else if (Target is StructAsset structAsset)
                                    declaration.structAsset = structAsset;

                                var getter = CreateInstance<PropertyGetterMacro>();
                                var setter = CreateInstance<PropertySetterMacro>();
                                Undo.RegisterCreatedObjectUndo(getter, "Created Property Getter Macro");
                                Undo.RegisterCreatedObjectUndo(setter, "Created Property Setter Macro");

                                if (typeof(TMemberTypeAsset) == typeof(ClassAsset))
                                {
                                    getter.classAsset = Target as ClassAsset;
                                    setter.classAsset = Target as ClassAsset;
                                }
                                else
                                {
                                    getter.structAsset = Target as StructAsset;
                                    setter.structAsset = Target as StructAsset;
                                }

                                AssetDatabase.AddObjectToAsset(declaration, Target);
                                AssetDatabase.AddObjectToAsset(getter, Target);
                                AssetDatabase.AddObjectToAsset(setter, Target);

                                declaration.FieldName = property.Name;
                                declaration.name = property.Name;
                                declaration.isProperty = true;

                                listOfVariables.Add(declaration);

                                var functionGetterUnit = new FunctionNode(FunctionType.Getter)
                                {
                                    fieldDeclaration = declaration
                                };

                                var functionSetterUnit = new FunctionNode(FunctionType.Setter)
                                {
                                    fieldDeclaration = declaration
                                };

                                declaration.getter = getter;
                                declaration.setter = setter;
                                declaration.getter.graph.units.Add(functionGetterUnit);
                                declaration.setter.graph.units.Add(functionSetterUnit);

                                declaration.hideFlags = HideFlags.HideInHierarchy;
                                getter.hideFlags = HideFlags.HideInHierarchy;
                                setter.hideFlags = HideFlags.HideInHierarchy;

                                declaration.scope = property.GetScope();
                                declaration.type = property.PropertyType;
                                declaration.propertyModifier = PropertyModifier.Override;

                                if (property.CanRead)
                                {
                                    declaration.get = true;
                                    declaration.getterScope = property.GetGetMethod(true).GetScope();
                                }

                                if (property.CanWrite)
                                {
                                    declaration.set = true;
                                    declaration.setterScope = property.GetSetMethod(true).GetScope();
                                }

                                shouldUpdate = true;
                                Undo.CollapseUndoOperations(undoGroup);
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();
                            }

                        }, true);

                        GUILayout.Space(4);
                    }

                    //TODO : Add event support

                    // var abstractEvents = inheritedType.GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    // .Where(e => e.GetAddMethod(true)?.IsAbstract == true || e.GetRemoveMethod(true)?.IsAbstract == true)
                    // .ToArray();

                    // for (int i = 0; i < abstractEvents.Length; i++)
                    // {
                    //     var index = i;
                    //     HUMEditor.Horizontal().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                    //     {
                    //         GUILayout.Label(abstractEvents[index].Name);

                    //         if (GUILayout.Button("Add", GUILayout.Width(60)))
                    //         {

                    //         }
                    //     }, true);

                    //     GUILayout.Space(4);
                    // }

                    var parameterizedConstructors = inheritedType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .Where(constructor => constructor.GetParameters().Length > 0)
                    .ToArray();

                    for (int i = 0; i < parameterizedConstructors.Length; i++)
                    {
                        var index = i;
                        HUMEditor.Horizontal().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                        {
                            var parameters = parameterizedConstructors[index].GetParameters();
                            var parameterDescriptions = string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"));

                            string typeName = inheritedType.Name;
                            int backtickIndex = typeName.IndexOf('`');
                            if (backtickIndex > 0)
                            {
                                typeName = typeName[..backtickIndex];
                            }
                            GUILayout.Label($"{typeName}({parameterDescriptions})");

                            if (GUILayout.Button("Add", GUILayout.Width(60)))
                            {
                                int undoGroup = Undo.GetCurrentGroup();
                                Undo.SetCurrentGroupName("Added Required Constructor");

                                Undo.RegisterCompleteObjectUndo(Target, "Added Required Constructor");

                                var declaration = CreateInstance<TConstructorDeclaration>();
                                Undo.RegisterCreatedObjectUndo(declaration, "Created Constructor Declaration");

                                if (Target is ClassAsset classAsset)
                                    declaration.classAsset = classAsset;
                                else if (Target is StructAsset structAsset)
                                    declaration.structAsset = structAsset;

                                declaration.hideFlags = HideFlags.HideInHierarchy;
                                AssetDatabase.AddObjectToAsset(declaration, Target);

                                listOfConstructors.Add(declaration);
                                declaration.name = $"Constructor {listOfConstructors.IndexOf(declaration)}";

                                var functionUnit = new FunctionNode(FunctionType.Constructor)
                                {
                                    constructorDeclaration = declaration
                                };

                                declaration.graph.units.Add(functionUnit);

                                var listOfVariables = variables.value as List<TFieldDeclaration>;
                                declaration.scope = parameterizedConstructors[index].GetScope();
                                declaration.CallType = ConstructorCallType.Base;
                                declaration.parameters = parameterizedConstructors[index]
                                    .GetParameters()
                                    .Select(param => new TypeParam(param.ParameterType, param.Name)
                                    {
                                        useInCall = !inheritedType.IsAbstract && inheritedType.IsClass
                                    })
                                    .ToList();
                                declaration.modifier = parameterizedConstructors[index].GetModifier();

                                shouldUpdate = true;
                                Undo.CollapseUndoOperations(undoGroup);
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();

                                if (methods.Count != methodsCount)
                                {
                                    if (Target is ClassAsset)
                                    {
                                        for (int i = 0; i < methods.Count; i++)
                                        {
                                            ((TMethodDeclaration)methods[i].value).classAsset = Target as ClassAsset;
                                        }
                                    }
                                    else
                                    {
                                        if (Target is StructAsset)
                                        {
                                            for (int i = 0; i < methods.Count; i++)
                                            {
                                                ((TMethodDeclaration)methods[i].value).structAsset = Target as StructAsset;
                                            }
                                        }
                                    }

                                    methodsCount = methods.Count;
                                }
                            }
                        }, true);

                        GUILayout.Space(4);
                    }
                });
            });
        }

        private bool OverridableMembers()
        {
            var classAsset = Target as ClassAsset;
            var inheritedType = classAsset.inherits.type;

            // Check for methods that can be overridden
            if (inheritedType == null) return false;
            if (inheritedType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(m => m.Overridable()).Any(m => !MethodExists(m)))
            {
                return true;
            }

            // Check for properties that can be overridden
            if (inheritedType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Any(p => (p.GetMethod?.IsFinal == false || p.SetMethod?.IsFinal == false) && !PropertyExists(p)))
            {
                return true;
            }

            return false;
        }

        private void OverridableMembersInfo()
        {
            Target.overridableMembersInfoOpened = HUMEditor.Foldout(Target.overridableMembersInfoOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2, () =>
            {
                HUMEditor.Image(PathUtil.Load("okay_32", CommunityEditorPath.Code).Single(), 16, 16, new RectOffset(), new RectOffset(4, 8, 4, 4));

                GUILayout.Label("Overridable Members");
            }, () =>
            {
                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                {
                    if (!OverridableMembers())
                    {
                        EditorGUILayout.HelpBox("No overridable members detected", MessageType.Info);
                        return;
                    }

                    var listOfMethods = methods.value as List<TMethodDeclaration>;
                    var listOfVariables = variables.value as List<TFieldDeclaration>;

                    var classAsset = Target as ClassAsset;
                    var inheritedType = classAsset.inherits.type;

                    var nonFinalMethods = inheritedType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(m => m.Overridable() && !MethodExists(m) && !m.IsPrivate)
                        .ToArray();

                    for (int i = 0; i < nonFinalMethods.Length; i++)
                    {
                        var index = i;
                        HUMEditor.Horizontal().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                        {
                            var labelStyle = new GUIStyle(GUI.skin.label)
                            {
                                wordWrap = true
                            };

                            GUILayout.Label(
                                nonFinalMethods[index].Name + $" ({string.Join(", ", nonFinalMethods[index].GetParameters().Select(param => param.ParameterType.As().CSharpName(false, false, false)))})",
                                labelStyle,
                                GUILayout.ExpandWidth(true)
                            );

                            if (GUILayout.Button("Add", GUILayout.Width(60)))
                            {
                                int undoGroup = Undo.GetCurrentGroup();
                                Undo.SetCurrentGroupName("Added Overridable Method");

                                Undo.RegisterCompleteObjectUndo(Target, "Added Overridable Method");

                                var declaration = CreateInstance<TMethodDeclaration>();
                                Undo.RegisterCreatedObjectUndo(declaration, "Created Overridable Method");

                                if (Target is ClassAsset classAsset)
                                    declaration.classAsset = classAsset;
                                else if (Target is StructAsset structAsset)
                                    declaration.structAsset = structAsset;

                                declaration.modifier = MethodModifier.Override;
                                declaration.methodName = nonFinalMethods[i].Name;
                                declaration.name = declaration.methodName;
                                declaration.scope = nonFinalMethods[i].GetScope();
                                declaration.returnType = nonFinalMethods[i].ReturnType;
                                declaration.parameters = nonFinalMethods[i].GetParameters()
                                    .Select(param => new TypeParam(param.ParameterType, param.Name))
                                    .ToList();

                                declaration.hideFlags = HideFlags.HideInHierarchy;
                                AssetDatabase.AddObjectToAsset(declaration, Target);
                                listOfMethods.Add(declaration);

                                var functionUnit = new FunctionNode(FunctionType.Method)
                                {
                                    methodDeclaration = declaration
                                };
                                declaration.graph.units.Add(functionUnit);

                                if (!nonFinalMethods[i].ReturnType.Is().NullOrVoid())
                                {
                                    var baseUnit = new BaseMethodCall(new Member(inheritedType, nonFinalMethods[i]), MethodType.ReturnValue);
                                    var returnUnit = new EventReturn();

                                    declaration.graph.units.Add(baseUnit);
                                    declaration.graph.units.Add(returnUnit);

                                    functionUnit.invoke.ConnectToValid(returnUnit.enter);
                                    returnUnit.value.ConnectToValid(baseUnit.result);

                                    returnUnit.position = new Vector2(functionUnit.position.x + 400, functionUnit.position.y);
                                    baseUnit.position = new Vector2(functionUnit.position.x + 200, functionUnit.position.y + 150);

                                    foreach (var output in functionUnit.parameterPorts)
                                    {
                                        output.ConnectToValid(
                                            baseUnit.InputParameters.Values.First(input =>
                                                input.key.Replace("%", "").Replace("&", "").Equals(output.key, StringComparison.OrdinalIgnoreCase)
                                            )
                                        );
                                    }

                                }

                                shouldUpdate = true;
                                Undo.CollapseUndoOperations(undoGroup);

                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();
                            }


                            if (methods.Count != methodsCount)
                            {
                                if (Target is ClassAsset)
                                {
                                    for (int i = 0; i < methods.Count; i++)
                                    {
                                        ((TMethodDeclaration)methods[i].value).classAsset = Target as ClassAsset;
                                    }
                                }
                                else
                                {
                                    if (Target is StructAsset)
                                    {
                                        for (int i = 0; i < methods.Count; i++)
                                        {
                                            ((TMethodDeclaration)methods[i].value).structAsset = Target as StructAsset;
                                        }
                                    }
                                }

                                methodsCount = methods.Count;
                            }
                        }, true);

                        GUILayout.Space(4);
                    }

                    var nonFinalProperties = inheritedType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(p => p.Overridable() && !PropertyExists(p) && p.GetScope() != AccessModifier.Private)
                        .ToArray();

                    for (int i = 0; i < nonFinalProperties.Length; i++)
                    {
                        var index = i;
                        HUMEditor.Horizontal().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                        {
                            GUILayout.Label(nonFinalProperties[index].Name);
                            var property = nonFinalProperties[index];
                            if (GUILayout.Button("Add", GUILayout.Width(60)))
                            {
                                int undoGroup = Undo.GetCurrentGroup();
                                Undo.SetCurrentGroupName("Added Overridable Property");

                                Undo.RegisterCompleteObjectUndo(Target, "Added Overridable Property");

                                var declaration = CreateInstance<TFieldDeclaration>();
                                Undo.RegisterCreatedObjectUndo(declaration, "Created Overridable Property");

                                if (Target is ClassAsset classAsset)
                                    declaration.classAsset = classAsset;
                                else if (Target is StructAsset structAsset)
                                    declaration.structAsset = structAsset;

                                var getter = CreateInstance<PropertyGetterMacro>();
                                Undo.RegisterCreatedObjectUndo(getter, "Created Getter Macro");

                                var setter = CreateInstance<PropertySetterMacro>();
                                Undo.RegisterCreatedObjectUndo(setter, "Created Setter Macro");

                                if (Target is ClassAsset)
                                {
                                    getter.classAsset = Target as ClassAsset;
                                    setter.classAsset = Target as ClassAsset;
                                }
                                else
                                {
                                    getter.structAsset = Target as StructAsset;
                                    setter.structAsset = Target as StructAsset;
                                }

                                AssetDatabase.AddObjectToAsset(declaration, Target);
                                AssetDatabase.AddObjectToAsset(getter, Target);
                                AssetDatabase.AddObjectToAsset(setter, Target);

                                declaration.FieldName = property.Name;
                                declaration.name = property.Name;
                                declaration.isProperty = true;

                                listOfVariables.Add(declaration);

                                var functionGetterUnit = new FunctionNode(FunctionType.Getter)
                                {
                                    fieldDeclaration = declaration
                                };

                                var functionSetterUnit = new FunctionNode(FunctionType.Setter)
                                {
                                    fieldDeclaration = declaration
                                };

                                declaration.getter = getter;
                                declaration.setter = setter;
                                declaration.getter.graph.units.Add(functionGetterUnit);
                                declaration.setter.graph.units.Add(functionSetterUnit);

                                declaration.hideFlags = HideFlags.HideInHierarchy;
                                getter.hideFlags = HideFlags.HideInHierarchy;
                                setter.hideFlags = HideFlags.HideInHierarchy;

                                declaration.scope = property.GetScope();
                                declaration.type = property.PropertyType;
                                declaration.propertyModifier = PropertyModifier.Override;

                                if (property.CanRead)
                                {
                                    declaration.get = true;
                                    declaration.getterScope = property.GetGetMethod(true).GetScope();
                                }

                                if (property.CanWrite)
                                {
                                    declaration.set = true;
                                    declaration.setterScope = property.GetSetMethod(true).GetScope();
                                }

                                shouldUpdate = true;
                                Undo.CollapseUndoOperations(undoGroup);

                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();
                            }

                        }, true);

                        GUILayout.Space(4);
                    }

                    // TODO: Add event support
                    // var nonFinalEvents = inheritedType.GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    // .Where(e => e.GetAddMethod(true)?.IsFinal == false || e.GetRemoveMethod(true)?.IsFinal == false)
                    // .ToArray();

                    // for (int i = 0; i < nonFinalEvents.Length; i++)
                    // {
                    //     var index = i;
                    //     HUMEditor.Horizontal().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                    //     {
                    //         GUILayout.Label(nonFinalEvents[index].Name);

                    //         if (GUILayout.Button("Add", GUILayout.Width(60)))
                    //         {
                    //             // Add event handling code here
                    //         }
                    //     }, true);

                    //     GUILayout.Space(4);
                    // }
                });
            });
        }


        protected virtual Texture2D DefaultIcon() { return PathUtil.Load("object_32", CommunityEditorPath.Code).Single(); }

        protected override void OnEnable()
        {
            base.OnEnable();
            ValueInspectorType = typeof(SystemObjectInspector).Assembly.GetTypes().First(type => type.Namespace == "Unity.VisualScripting" && type.Name == "ValueInspector");
            if (constructors == null || constructorsProp == null)
            {
                constructors = Metadata.FromProperty(serializedObject.FindProperty("constructors"));
                constructorsProp = serializedObject.FindProperty("constructors");
            }

            if (variables == null || variablesProp == null)
            {
                variables = Metadata.FromProperty(serializedObject.FindProperty("variables"));
                variablesProp = serializedObject.FindProperty("variables");
            }

            if (methods == null || methodsProp == null)
            {
                methods = Metadata.FromProperty(serializedObject.FindProperty("methods"));
                methodsProp = serializedObject.FindProperty("methods");
            }

            if (attributes == null || attributesProp == null)
            {
                attributes = Metadata.FromProperty(serializedObject.FindProperty("attributes"));
                attributesProp = serializedObject.FindProperty("attributes");
            }

            //TODO: add Custom Unit Generator
            // if (typeof(TMemberTypeAsset) != typeof(UnitAsset))
            // {
            if (Target.icon == null) Target.icon = DefaultIcon();
            // }
            // else
            // {
            //     if (typeIcon == null || typeIconProp == null)
            //     {
            //         typeIcon = Metadata.FromProperty(serializedObject.FindProperty("TypeIcon"));
            //         typeIconProp = serializedObject.FindProperty("TypeIcon");
            //     }
            // }

            CacheConstrainedAttributes();

            shouldUpdate = true;
        }

        protected virtual void OnExtendedHorizontalHeaderGUI()
        {

        }

        protected virtual void OnExtendedOptionsGUI()
        {

        }

        protected virtual void OnExtendedVerticalHeaderGUI()
        {

        }

        protected override void OnTypeHeaderGUI()
        {
            HUMEditor.Horizontal(GUIStyle.none, () =>
            {
                HUMEditor.Vertical().Box(
                HUMEditorColor.DefaultEditorBackground, Color.black,
                new RectOffset(7, 7, 7, 7),
                new RectOffset(1, 1, 1, 1),
                () =>
                {
                    EditorGUI.BeginChangeCheck();
                    var icon = (Texture2D)EditorGUILayout.ObjectField(
                    GUIContent.none,
                    Target.icon,
                    typeof(Texture2D),
                    false,
                    GUILayout.Width(32),
                    GUILayout.Height(32));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RegisterCompleteObjectUndo(Target, "Changed Asset Icon");
                        Target.icon = icon;
                        EditorUtility.SetDirty(Target);
                        shouldUpdate = true;
                    }

                }, false, false);

                GUILayout.Space(2);

                HUMEditor.Vertical(() =>
                {
                    base.OnTypeHeaderGUI();
                    OnExtendedHorizontalHeaderGUI();
                });
            });

            OnExtendedVerticalHeaderGUI();

            Target.attributesOpened = HUMEditor.Foldout(
            Target.attributesOpened,
            HUMEditorColor.DefaultEditorBackground.Darken(0f),
            Color.black,
            1,
            () =>
            {
                GUILayout.Label("Attributes");
            },
            () =>
            {
                HUMEditor.Vertical().Box(
                    HUMEditorColor.DefaultEditorBackground,
                    Color.black,
                    new RectOffset(4, 4, 4, 4),
                    new RectOffset(1, 1, 0, 1),
                    () =>
                    {
                        DrawAttributes(attributes, Target.attributes, Target is ClassAsset ? AttributeUsageType.Class : AttributeUsageType.Struct, Target, "Changed Asset Attribute Type");
                    });
            });
        }

        protected override void OptionsGUI()
        {
            EditorGUI.BeginChangeCheck();
            var serialized = EditorGUILayout.ToggleLeft("Serialized", Target.serialized);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(Target, "Toggled Asset Option 'Serialized'");
                Target.serialized = serialized;
                EditorUtility.SetDirty(Target);
                shouldUpdate = true;
            }
            EditorGUI.BeginChangeCheck();
            var inspectable = EditorGUILayout.ToggleLeft("Inspectable", Target.inspectable);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(Target, "Toggled Asset Option 'Inspectable'");
                Target.inspectable = inspectable;
                EditorUtility.SetDirty(Target);
                shouldUpdate = true;
            }
            EditorGUI.BeginChangeCheck();
            var includeInSettings = EditorGUILayout.ToggleLeft("Include In Settings", Target.includeInSettings);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(Target, "Toggled Asset Option 'IncludeInSettings'");
                Target.includeInSettings = includeInSettings;
                EditorUtility.SetDirty(Target);
                shouldUpdate = true;
            }
            EditorGUI.BeginChangeCheck();
            var definedEvent = EditorGUILayout.ToggleLeft("Flag for Defined Event Filtering", Target.definedEvent);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(Target, "Toggled Asset Option 'DefinedEvent'");
                Target.definedEvent = definedEvent;
                EditorUtility.SetDirty(Target);
                shouldUpdate = true;
            }
            OnExtendedOptionsGUI();
        }

        private Type AttributeTypeField(AttributeDeclaration attribute, AttributeUsageType usage)
        {
            GUILayout.Label(" ", GUILayout.Height(20));
            var position = GUILayoutUtility.GetLastRect();

            Type[] types = new Type[] { };

            switch (usage)
            {
                case AttributeUsageType.Class:
                    types = classAttributeTypes;
                    break;
                case AttributeUsageType.Struct:
                    types = structAttributeTypes;
                    break;
                case AttributeUsageType.Enum:
                    types = enumAttributeTypes;
                    break;
                case AttributeUsageType.Interface:
                    types = interfaceAttributeTypes;
                    break;
                case AttributeUsageType.Field:
                    types = fieldAttributeTypes;
                    break;
                case AttributeUsageType.Property:
                    types = propertyAttributeTypes;
                    break;
                case AttributeUsageType.Method:
                    types = methodAttributeTypes;
                    break;
                case AttributeUsageType.Parameter:
                    types = parameterAttributeTypes;
                    break;
            }

            return LudiqGUI.TypeField(position, GUIContent.none, attribute.GetAttributeType(), () =>
            {
                return new TypeOptionTree(types);
            });
        }

        private void CacheConstrainedAttributes()
        {
            var allAttributeTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(Attribute)) || t == typeof(Attribute))
                .ToArray();

            // Filter attributes based on valid targets
            classAttributeTypes = allAttributeTypes
                .Where(attr => GetAttributeUsage(attr).ValidOn.HasFlag(AttributeTargets.Class) || GetAttributeUsage(attr).ValidOn == AttributeTargets.All)
                .ToArray();

            structAttributeTypes = allAttributeTypes
                .Where(attr => GetAttributeUsage(attr).ValidOn.HasFlag(AttributeTargets.Struct) || GetAttributeUsage(attr).ValidOn == AttributeTargets.All)
                .ToArray();

            enumAttributeTypes = allAttributeTypes
                .Where(attr => GetAttributeUsage(attr).ValidOn.HasFlag(AttributeTargets.Enum) || GetAttributeUsage(attr).ValidOn == AttributeTargets.All)
                .ToArray();

            interfaceAttributeTypes = allAttributeTypes
                .Where(attr => GetAttributeUsage(attr).ValidOn.HasFlag(AttributeTargets.Interface) || GetAttributeUsage(attr).ValidOn == AttributeTargets.All)
                .ToArray();

            fieldAttributeTypes = allAttributeTypes
                .Where(attr => GetAttributeUsage(attr).ValidOn.HasFlag(AttributeTargets.Field) || GetAttributeUsage(attr).ValidOn.HasFlag(AttributeTargets.All))
                .ToArray();

            propertyAttributeTypes = allAttributeTypes
                .Where(attr => GetAttributeUsage(attr).ValidOn.HasFlag(AttributeTargets.Property) || GetAttributeUsage(attr).ValidOn == AttributeTargets.All)
                .ToArray();

            methodAttributeTypes = allAttributeTypes
                .Where(attr => GetAttributeUsage(attr).ValidOn.HasFlag(AttributeTargets.Method) || GetAttributeUsage(attr).ValidOn == AttributeTargets.All)
                .ToArray();

            parameterAttributeTypes = allAttributeTypes
                .Where(attr => GetAttributeUsage(attr).ValidOn.HasFlag(AttributeTargets.Parameter) || GetAttributeUsage(attr).ValidOn == AttributeTargets.All)
                .ToArray();
        }

        // Helper method to get AttributeUsageAttribute from a type
        private AttributeUsageAttribute GetAttributeUsage(Type attrType)
        {
            return attrType.GetCustomAttributes(typeof(AttributeUsageAttribute), false)
                .Cast<AttributeUsageAttribute>()
                .FirstOrDefault() ?? new AttributeUsageAttribute(AttributeTargets.All);
        }

        private void Constructors()
        {
            Target.constructorsOpened = HUMEditor.Foldout(Target.constructorsOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2, () =>
            {
                HUMEditor.Image(PathUtil.Load("constructor_32", CommunityEditorPath.Code).Single(), 16, 16, new RectOffset(), new RectOffset(4, 8, 4, 4));
                GUILayout.Label("Constructors");
            }, () =>
            {
                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                {
                    var listOfConstructors = constructors.value as List<TConstructorDeclaration>;

                    for (int i = 0; i < listOfConstructors.Count; i++)
                    {
                        var index = i;
                        listOfConstructors[index].opened = HUMEditor.Foldout(listOfConstructors[index].opened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                        {
                            GUILayout.Label($"Constructor {i}");
                            if (string.IsNullOrEmpty(listOfConstructors[i].name)) listOfConstructors[i].name = $"Constructor {i}";
                            if (GUILayout.Button("Edit", GUILayout.Width(60)))
                            {
                                GraphWindow.OpenActive(listOfConstructors[index].GetReference() as GraphReference);
                            }

                            if (GUILayout.Button("...", GUILayout.Width(19)))
                            {
                                GenericMenu menu = new GenericMenu();
                                menu.AddItem(new GUIContent("Delete"), false, (obj) =>
                                {
                                    Undo.RegisterCompleteObjectUndo(listOfConstructors[index], "Deleted Constructor");
                                    shouldUpdate = true;
                                    constructors.Remove(obj as TConstructorDeclaration);
                                    AssetDatabase.RemoveObjectFromAsset(obj as TConstructorDeclaration);
                                }, listOfConstructors[index]);

                                if (index > 0)
                                {
                                    menu.AddItem(new GUIContent("Move Up"), false, (obj) =>
                                    {
                                        Undo.RegisterCompleteObjectUndo(listOfConstructors[index], "Moved Constructor up");
                                        shouldUpdate = true;
                                        MoveItemUp(listOfConstructors, index);
                                    }, listOfConstructors[index]);
                                }

                                if (index < methods.Count - 1)
                                {
                                    menu.AddItem(new GUIContent("Move Down"), false, (obj) =>
                                    {
                                        Undo.RegisterCompleteObjectUndo(listOfConstructors[index], "Moved Constructor down");
                                        shouldUpdate = true;
                                        MoveItemDown(listOfConstructors, index);
                                    }, listOfConstructors[index]);
                                }
                                menu.ShowAsContext();
                            }
                        }, () =>
                        {
                            HUMEditor.Vertical().Box(HUMColor.Grey(0.15f), Color.black, new RectOffset(6, 6, 6, 6), new RectOffset(1, 1, 0, 1), () =>
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUI.BeginChangeCheck();
                                //HUMEditor.Image(PathUtil.Load("scope_32", CommunityEditorPath.Code).Single(), 16, 16);
                                var scope = (AccessModifier)EditorGUILayout.EnumPopup("Scope", listOfConstructors[index].scope);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    Undo.RegisterCompleteObjectUndo(listOfConstructors[index], "Changed Constructor Scope");
                                    shouldUpdate = true;
                                    listOfConstructors[index].scope = scope;
                                }
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.BeginHorizontal();
                                //HUMEditor.Image(PathUtil.Load("scope_32", CommunityEditorPath.Code).Single(), 16, 16);
                                EditorGUI.BeginChangeCheck();
                                var CallType = (ConstructorCallType)EditorGUILayout.EnumPopup("CallType", listOfConstructors[index].CallType);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    Undo.RegisterCompleteObjectUndo(listOfConstructors[index], "Changed Constructor CallType");
                                    shouldUpdate = true;
                                    listOfConstructors[index].CallType = CallType;
                                }
                                EditorGUILayout.EndHorizontal();
                                GUILayout.Space(4);

                                listOfConstructors[index].parametersOpened = HUMEditor.Foldout(listOfConstructors[index].parametersOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                                {
                                    GUILayout.Label("Parameters");
                                }, () =>
                                {
                                    DrawParameters(constructors[index]["parameters"], listOfConstructors[index], listOfConstructors[index].graph.units[0] as FunctionNode);
                                });

                            }, true, false);
                        });

                        GUILayout.Space(4);
                    }
                    if (GUILayout.Button("+ Add Constructor"))
                    {
                        int undoGroup = Undo.GetCurrentGroup();
                        Undo.SetCurrentGroupName("Added Constructor");

                        Undo.RegisterCompleteObjectUndo(Target, "Added Constructor");

                        var declaration = CreateInstance<TConstructorDeclaration>();
                        Undo.RegisterCreatedObjectUndo(declaration, "Created Constructor");

                        if (Target is ClassAsset classAsset)
                            declaration.classAsset = classAsset;
                        else if (Target is StructAsset structAsset)
                            declaration.structAsset = structAsset;

                        declaration.hideFlags = HideFlags.HideInHierarchy;
                        AssetDatabase.AddObjectToAsset(declaration, Target);

                        listOfConstructors.Add(declaration);

                        var functionUnit = new FunctionNode(FunctionType.Constructor)
                        {
                            constructorDeclaration = declaration
                        };

                        declaration.graph.units.Add(functionUnit);

                        var listOfVariables = variables.value as List<TFieldDeclaration>;

                        shouldUpdate = true;

                        Undo.CollapseUndoOperations(undoGroup);

                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }


                    if (methods.Count != methodsCount)
                    {
                        if (Target is ClassAsset)
                        {
                            for (int i = 0; i < methods.Count; i++)
                            {
                                ((TMethodDeclaration)methods[i].value).classAsset = Target as ClassAsset;
                            }
                        }
                        else
                        {
                            if (Target is StructAsset)
                            {
                                for (int i = 0; i < methods.Count; i++)
                                {
                                    ((TMethodDeclaration)methods[i].value).structAsset = Target as StructAsset;
                                }
                            }
                        }

                        methodsCount = methods.Count;
                    }
                });
            });
        }

        private void DrawAttributes(Metadata attributesMeta, List<AttributeDeclaration> attributeList, AttributeUsageType attributeUsageType, UnityEngine.Object target, string undoName)
        {
            HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
            {
                for (int attrIndex = 0; attrIndex < attributeList.Count; attrIndex++)
                {
                    var attributeMeta = attributesMeta[attrIndex]["attributeType"];
                    var attribute = attributeList[attrIndex];

                    attribute.opened = HUMEditor.Foldout(
                        attribute.opened,
                        HUMEditorColor.DefaultEditorBackground.Darken(0f),
                        Color.black,
                        1,
                        () =>
                        {
                            Type type = null;
                            attributeMeta.Block(() =>
                            {
                                type = AttributeTypeField(attribute, attributeUsageType);
                            },
                            () =>
                            {
                                attribute.parameters.Clear();
                                attribute.constructor = 0;
                                Undo.RegisterCompleteObjectUndo(target, undoName);
                                attribute.SetType(type);
                            },
                            false
                        );

                            if (GUILayout.Button("...", GUILayout.Width(19)))
                            {
                                GenericMenu menu = new GenericMenu();
                                menu.AddItem(new GUIContent("Delete"), false, (obj) =>
                                {
                                    Undo.RegisterCompleteObjectUndo(target, "Removed Atribute");
                                    AttributeDeclaration attrToRemove = obj as AttributeDeclaration;
                                    attributeList.Remove(attrToRemove);
                                    shouldUpdate = true;
                                    foreach (var Constructor in attrToRemove.GetAttributeType().GetConstructors())
                                    {
                                        foreach (var parameter in Constructor.GetParameters())
                                        {
                                            if (parameter.ParameterType == typeof(string))
                                            {
                                                TypeParam paramToRemove = attrToRemove.parameters.FirstOrDefault(param => param.name == parameter.Name);
                                                if (paramToRemove != null)
                                                {
                                                    attribute.constructor = 0;
                                                    attrToRemove.parameters.Remove(paramToRemove);
                                                }
                                            }
                                        }
                                    }
                                }, attribute);

                                if (attrIndex > 0)
                                {
                                    menu.AddItem(new GUIContent("Move Up"), false, (obj) =>
                                    {
                                        var attributeIndex = attributeList.IndexOf(obj as AttributeDeclaration);
                                        if (attributeIndex > 0)
                                        {
                                            Undo.RegisterCompleteObjectUndo(target, "Moved Atribute Up");
                                            var temp = attributeList[attributeIndex];
                                            attributeList[attributeIndex] = attributeList[attributeIndex - 1];
                                            attributeList[attributeIndex - 1] = temp;
                                            shouldUpdate = true;
                                        }
                                    }, attribute);
                                }

                                if (attrIndex < attributeList.Count - 1)
                                {
                                    menu.AddItem(new GUIContent("Move Down"), false, (obj) =>
                                    {
                                        var attributeIndex = attributeList.IndexOf(obj as AttributeDeclaration);
                                        if (attributeIndex < attributeList.Count - 1)
                                        {
                                            Undo.RegisterCompleteObjectUndo(target, "Moved Atribute Down");
                                            var temp = attributeList[attributeIndex];
                                            attributeList[attributeIndex] = attributeList[attributeIndex + 1];
                                            attributeList[attributeIndex + 1] = temp;
                                            shouldUpdate = true;
                                        }
                                    }, attribute);
                                }
                                menu.ShowAsContext();
                            }
                        },
                    () =>
                    {
                        var attributeParamMeta = attributesMeta[attrIndex]["parameters"];

                        string[] constructorNames = attribute?.GetAttributeType()?.GetConstructors()
                        .Where(constructor => constructor.GetParameters().All(param => param.ParameterType.HasInspector()))
                        .Select(constructor =>
                        {
                            string paramInfo = string.Join(
                                ", ",
                                constructor.GetParameters()
                                .Select(param => $"{param.ParameterType.Name} {param.Name}")
                            );
                            return $"{attribute.GetAttributeType().Name}({paramInfo})";
                        })
                        .ToArray() ?? new string[0];

                        if (attribute.constructor != attribute.selectedconstructor)
                        {
                            attribute.parameters.Clear();
                        }

                        attribute.selectedconstructor = attribute.constructor;
                        EditorGUI.BeginChangeCheck();
                        attribute.constructor = EditorGUILayout.Popup(
                            "Select Type : ",
                            attribute.constructor,
                            constructorNames
                        );
                        shouldUpdate = EditorGUI.EndChangeCheck();

                        var selectedConstructor = attribute?.GetAttributeType()?.GetConstructors()
                            .Where(constructor => constructor.GetParameters().All(param => param.ParameterType.HasInspector()))
                            .ToList()[attribute.constructor];

                        if (selectedConstructor != null)
                        {
                            var paramIndex = 0;

                            foreach (var parameter in selectedConstructor.GetParameters())
                            {
                                TypeParam Param = attribute.parameters.FirstOrDefault(param => param.name == parameter.Name);
                                if (Param == null)
                                {
                                    attribute.AddParameter(
                                        parameter.Name,
                                        parameter.ParameterType,
                                        GetDefaultParameterValue(parameter.ParameterType)
                                    );
                                    Param = attribute.parameters.First(param => param.name == parameter.Name);
                                }

                                if (Param.defaultValue == null)
                                {
                                    var value = Param.GetDefaultValue();

                                    if (value == null)
                                    {
                                        Param.defaultValue = GetDefaultParameterValue(parameter.ParameterType);
                                    }
                                    else
                                    {
                                        Param.defaultValue = value;
                                    }
                                }

                                var isParamsParameter = parameter.IsDefined(typeof(ParamArrayAttribute));

                                Param.isParamsParameter = isParamsParameter;

                                Inspector.BeginBlock(
                                    attributeParamMeta[paramIndex]["defaultValue"],
                                    new Rect()
                                );
                                if (!isParamsParameter && Param.defaultValue is not IList)
                                {
                                    if (attributeParamMeta[paramIndex]["defaultValue"].value is Type type)
                                    {
                                        GUIContent TypebuilderButtonContent = new GUIContent(
                                        (attributeParamMeta[paramIndex]["defaultValue"].value as Type)?.As().CSharpName(false).RemoveHighlights().RemoveMarkdown() ?? "Select Type",
                                        (attributeParamMeta[paramIndex]["defaultValue"].value as Type)?.Icon()?[IconSize.Small]
                                        );

                                        GUILayout.BeginHorizontal();
                                        GUILayout.Label(parameter.Name + ":");
                                        var lastRect = GUILayoutUtility.GetLastRect();
                                        if (GUILayout.Button(TypebuilderButtonContent, EditorStyles.popup, GUILayout.MaxHeight(19f)))
                                        {
                                            TypeBuilderWindow.ShowWindow(lastRect, attributeParamMeta[paramIndex]["defaultValue"], true, null, () => shouldUpdate = true);
                                        }
                                        GUILayout.EndHorizontal();
                                    }
                                    else
                                    {
                                        LudiqGUI.InspectorLayout(
                                            attributeParamMeta[paramIndex]["defaultValue"].Cast(parameter.ParameterType),
                                            new GUIContent(parameter.Name + ":")
                                        );
                                    }
                                }
                                else
                                {
                                    attributeParamMeta[paramIndex]["defaultValue"].value = Param.defaultValue;
                                    LudiqGUI.InspectorLayout(
                                        attributeParamMeta[paramIndex]["defaultValue"].Cast(parameter.ParameterType),
                                        new GUIContent(parameter.Name + ":")
                                    );
                                }
                                if (Inspector.EndBlock(attributeParamMeta[paramIndex]["defaultValue"]))
                                {
                                    attributeParamMeta[paramIndex]["defaultValue"].RecordUndo();
                                    shouldUpdate = true;
                                }
                                paramIndex++;
                            }

                            paramIndex = 0;
                        }
                        if (attributeList.Count > 0) GUILayout.Space(4);
                    });
                }

                if (GUILayout.Button("+ Add Attribute"))
                {
                    var attribute = new AttributeDeclaration();
                    attribute.SetType(GetConstrainedAttributeTypes(attributeUsageType)[0]);
                    Undo.RegisterCompleteObjectUndo(target, "Added Attribute");
                    attributeList.Add(attribute);
                    shouldUpdate = true;
                }
            });
        }

        Type[] GetConstrainedAttributeTypes(AttributeUsageType usage)
        {
            return usage switch
            {
                AttributeUsageType.Class => classAttributeTypes,
                AttributeUsageType.Struct => structAttributeTypes,
                AttributeUsageType.Enum => enumAttributeTypes,
                AttributeUsageType.Interface => interfaceAttributeTypes,
                AttributeUsageType.Field => fieldAttributeTypes,
                AttributeUsageType.Property => propertyAttributeTypes,
                AttributeUsageType.Method => methodAttributeTypes,
                AttributeUsageType.Parameter => parameterAttributeTypes,
                _ => new Type[0],
            };
        }
        Dictionary<Metadata, HashSet<int>> subscribedEvents = new Dictionary<Metadata, HashSet<int>>();
        private void DrawParameters(Metadata paramMeta, UnityEngine.Object target, FunctionNode functionUnit = null)
        {
            var parameters = paramMeta.value as List<TypeParam>;
            HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    parameters[i].opened = HUMEditor.Foldout(parameters[i].opened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                    {
                        GraphWindow.active?.context?.BeginEdit();
                        EditorGUI.BeginChangeCheck();
                        var paramName = GUILayout.TextField(parameters[i].name);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RegisterCompleteObjectUndo(target, "Changed Parameter Name");
                            parameters[i].name = paramName.LegalMemberName();
                        }
                        GraphWindow.active?.context?.EndEdit();

                        if (GUILayout.Button("...", GUILayout.Width(19)))
                        {
                            GenericMenu menu = new GenericMenu();
                            menu.AddItem(new GUIContent("Delete"), false, (obj) =>
                            {
                                TypeParam paramToRemove = obj as TypeParam;
                                Undo.RegisterCompleteObjectUndo(target, $"Deleted {parameters[i].name} parameter");
                                parameters.Remove(paramToRemove);
                                shouldUpdate = true;
                            }, parameters[i]);

                            if (i > 0)
                            {
                                menu.AddItem(new GUIContent("Move Up"), false, (obj) =>
                                {
                                    var paramIndex = parameters.IndexOf(obj as TypeParam);
                                    if (paramIndex > 0)
                                    {
                                        Undo.RegisterCompleteObjectUndo(target, $"Moved {parameters[i].name} parameter up");
                                        var temp = parameters[paramIndex];
                                        parameters[paramIndex] = parameters[paramIndex - 1];
                                        parameters[paramIndex - 1] = temp;
                                        shouldUpdate = true;
                                    }
                                }, parameters[i]);
                            }

                            if (i < parameters.Count - 1)
                            {
                                menu.AddItem(new GUIContent("Move Down"), false, (obj) =>
                                {
                                    var paramIndex = parameters.IndexOf(obj as TypeParam);
                                    if (paramIndex < parameters.Count - 1)
                                    {
                                        Undo.RegisterCompleteObjectUndo(target, $"Moved {parameters[i].name} parameter down");
                                        var temp = parameters[paramIndex];
                                        parameters[paramIndex] = parameters[paramIndex + 1];
                                        parameters[paramIndex + 1] = temp;
                                        shouldUpdate = true;
                                    }
                                }, parameters[i]);
                            }
                            menu.ShowAsContext();
                        }
                    }, () =>
                    {
                        HUMEditor.Vertical().Box(HUMColor.Grey(0.15f), Color.black, new RectOffset(6, 6, 6, 6), new RectOffset(1, 1, 0, 1), () =>
                        {
                            Inspector.BeginBlock(paramMeta, new Rect());

                            GUIContent TypebuilderButtonContent = new GUIContent(
                            (paramMeta[i]["type"].value as Type)?.As().CSharpName(false).RemoveHighlights().RemoveMarkdown() ?? "Select Type",
                            (paramMeta[i]["type"].value as Type)?.Icon()?[IconSize.Small]
                            );

                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Type");
                            var lastRect = GUILayoutUtility.GetLastRect();
                            if (GUILayout.Button(TypebuilderButtonContent, EditorStyles.popup, GUILayout.MaxHeight(19f)))
                            {
                                TypeBuilderWindow.ShowWindow(lastRect, paramMeta[i]["type"], true, null, () =>
                                {
                                    Undo.RegisterCompleteObjectUndo(target, "Changed Parameter Type");
                                    shouldUpdate = true;
                                });
                            }
                            GraphWindow.active?.context?.BeginEdit();
                            GUILayout.EndHorizontal();
                            Inspector.BeginBlock(paramMeta[i]["modifier"], new Rect());
                            LudiqGUI.InspectorLayout(paramMeta[i]["modifier"], new GUIContent("Modifier"));
                            if (Inspector.EndBlock(paramMeta[i]["modifier"]))
                            {
                                Undo.RegisterCompleteObjectUndo(target, "Changed Parameter Modifier");
                                shouldUpdate = true;
                            }
                            GUILayout.Space(4);
                            if (parameters[i].showCall)
                            {
                                Inspector.BeginBlock(paramMeta[i]["useInCall"], new Rect());
                                LudiqGUI.InspectorLayout(paramMeta[i]["useInCall"], new GUIContent("Use In Call"));
                                if (Inspector.EndBlock(paramMeta[i]["useInCall"]))
                                {
                                    Undo.RegisterCompleteObjectUndo(target, "Changed Parameter Modifier");
                                    shouldUpdate = true;
                                }
                                GUILayout.Space(4);
                            }
                            Inspector.BeginBlock(paramMeta[i]["hasDefault"], new Rect());
                            LudiqGUI.InspectorLayout(paramMeta[i]["hasDefault"], new GUIContent("Has Default Value"));
                            if (Inspector.EndBlock(paramMeta[i]["hasDefault"]))
                            {
                                Undo.RegisterCompleteObjectUndo(target, "Changed Parameter Modifier");
                                shouldUpdate = true;
                            }
                            GraphWindow.active?.context?.BeginEdit();
                            GUILayout.Space(4);

                            if (i >= 0 && i < paramMeta.Count)
                            {
                                if (parameters[i].hasDefault)
                                {
                                    if (!subscribedEvents.ContainsKey(paramMeta))
                                    {
                                        subscribedEvents[paramMeta] = new HashSet<int>();
                                    }

                                    if (!subscribedEvents[paramMeta].Contains(i))
                                    {
                                        paramMeta[i]["type"].valueChanged += (type) =>
                                        {
                                            GraphWindow.active?.context?.DescribeAndAnalyze();
                                            paramMeta[i]["typeHandle"].value = new SerializableType((type as Type).AssemblyQualifiedName);

                                            if (paramMeta[i]["defaultValue"].value?.GetType() == type as Type)
                                            {
                                                return;
                                            }

                                            if (type == null)
                                            {
                                                paramMeta[i]["defaultValue"].value = null;
                                            }
                                            else if (ConversionUtility.CanConvert(paramMeta[i]["defaultValue"].value, type as Type, true))
                                            {
                                                paramMeta[i]["defaultValue"].value = ConversionUtility.Convert(paramMeta[i]["defaultValue"].value, type as Type);
                                            }
                                            else
                                            {
                                                paramMeta[i]["defaultValue"].value = (type as Type).Default();
                                            }

                                            paramMeta[i]["defaultValue"].InferOwnerFromParent();
                                            shouldUpdate = true;
                                        };

                                        subscribedEvents[paramMeta].Add(i);
                                    }

                                    var inspector = paramMeta[i]["defaultValue"].Inspector();
                                    typeof(SystemObjectInspector).GetField("inspector", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(inspector, Activator.CreateInstance(ValueInspectorType, inspector));
                                    inspector.DrawLayout(new GUIContent("Value               "));
                                    GUILayout.Space(4);
                                }
                            }

                            if (parameters[i].attributes == null) parameters[i].attributes = new List<AttributeDeclaration>();
                            parameters[i].attributesOpened = HUMEditor.Foldout(parameters[i].attributesOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                            {
                                HUMEditor.Image(PathUtil.Load("attributes_16", CommunityEditorPath.Code).Single(), 16, 16);
                                GUILayout.Label("Attributes");
                            }, () =>
                            {
                                DrawAttributes(paramMeta[i]["attributes"], parameters[i].attributes, AttributeUsageType.Parameter, target, "Changed Parameter attribute");
                            });
                            if (Inspector.EndBlock(paramMeta))
                            {
                                Undo.RegisterCompleteObjectUndo(target, "Changed Parameter");
                                GraphWindow.active?.context?.DescribeAndAnalyze();
                                shouldUpdate = true;
                                if (functionUnit != null)
                                {
                                    functionUnit.Define();
                                    functionUnit.Describe();
                                }
                            }
                        }, true);
                    });
                    GUILayout.Space(4);
                }
                if (GUILayout.Button("+ Add Parameter"))
                {
                    var name = "New Parameter";
                    var index = 0;
                    while (parameters.Any(param => param.name == name + index))
                    {
                        index++;
                    }
                    name += index;
                    Undo.RegisterCompleteObjectUndo(target, "Added Parameter");
                    parameters.Add(new TypeParam(typeof(int), name) { defaultValue = "" });
                }
            });
        }
        private object GetDefaultParameterValue(Type parameterType)
        {
            if (parameterType == typeof(Type))
            {
                return typeof(float);
            }
            return parameterType.PseudoDefault();
        }

        private void Methods()
        {
            Target.methodsOpened = HUMEditor.Foldout(Target.methodsOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2, () =>
            {
                HUMEditor.Image(PathUtil.Load("method_16", CommunityEditorPath.Code).Single(), 16, 16, new RectOffset(), new RectOffset(4, 8, 4, 4));
                GUILayout.Label("Methods");
            }, () =>
            {
                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                {
                    var listOfMethods = methods.value as List<TMethodDeclaration>;

                    for (int i = 0; i < listOfMethods.Count; i++)
                    {
                        var index = i;

                        listOfMethods[index].opened = HUMEditor.Foldout(listOfMethods[index].opened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                        {
                            GraphWindow.active?.context?.BeginEdit();
                            HUMEditor.Changed(() =>
                            {
                                listOfMethods[index].methodName = GUILayout.TextField(listOfMethods[index].methodName);
                            }, () =>
                            {
                                listOfMethods[index].name = listOfMethods[index].methodName.LegalMemberName();
                                var funcionUnit = (listOfMethods[index].graph.units[0] as FunctionNode);
                                funcionUnit.Define();
                                funcionUnit.Describe();
                                shouldUpdate = true;
                            });
                            GraphWindow.active?.context?.EndEdit();
                            if (GUILayout.Button("Edit", GUILayout.Width(60)))
                            {
                                GraphWindow.OpenActive(listOfMethods[index].GetReference() as GraphReference);
                                var listOfVariables = variables.value as List<TFieldDeclaration>;
                            }

                            if (GUILayout.Button("...", GUILayout.Width(19)))
                            {
                                GenericMenu menu = new GenericMenu();
                                menu.AddItem(new GUIContent("Delete"), false, (obj) =>
                                {
                                    Undo.RegisterCompleteObjectUndo(listOfMethods[index], "Delete Method");
                                    methods.Remove(obj as TMethodDeclaration);
                                    AssetDatabase.RemoveObjectFromAsset(obj as TMethodDeclaration);
                                    shouldUpdate = true;
                                }, listOfMethods[index]);

                                if (index > 0)
                                {
                                    menu.AddItem(new GUIContent("Move Up"), false, (obj) =>
                                    {
                                        Undo.RegisterCompleteObjectUndo(listOfMethods[index], "Move method Up");
                                        MoveItemUp(listOfMethods, index);
                                        shouldUpdate = true;
                                    }, listOfMethods[index]);
                                }

                                if (index < methods.Count - 1)
                                {
                                    menu.AddItem(new GUIContent("Move Down"), false, (obj) =>
                                    {
                                        Undo.RegisterCompleteObjectUndo(listOfMethods[index], "Move method Down");
                                        MoveItemDown(listOfMethods, index);
                                        shouldUpdate = true;
                                    }, listOfMethods[index]);
                                }
                                menu.ShowAsContext();
                            }
                        }, () =>
                        {
                            HUMEditor.Vertical().Box(HUMColor.Grey(0.15f), Color.black, new RectOffset(6, 6, 6, 6), new RectOffset(1, 1, 0, 1), () =>
                            {
                                EditorGUILayout.BeginHorizontal();
                                //HUMEditor.Image(PathUtil.Load("scope_32", CommunityEditorPath.Code).Single(), 16, 16);
                                EditorGUI.BeginChangeCheck();
                                listOfMethods[index].scope = (AccessModifier)EditorGUILayout.EnumPopup("Scope", listOfMethods[index].scope);
                                shouldUpdate = EditorGUI.EndChangeCheck();
                                EditorGUILayout.EndHorizontal();
                                EditorGUI.BeginChangeCheck();
                                listOfMethods[index].modifier = (MethodModifier)EditorGUILayout.EnumPopup("Modifier", listOfMethods[index].modifier);
                                shouldUpdate = EditorGUI.EndChangeCheck();

                                GUIContent TypebuilderButtonContent = new GUIContent(
                                    (methods[index]["returnType"].value as Type)?.As().CSharpName(false).RemoveHighlights().RemoveMarkdown() ?? "Select Type",
                                    (methods[index]["returnType"].value as Type)?.Icon()?[IconSize.Small]
                                    );

                                GUILayout.BeginHorizontal();
                                GUILayout.Label("Returns");
                                var lastRect = GUILayoutUtility.GetLastRect();
                                if (GUILayout.Button(TypebuilderButtonContent, EditorStyles.popup, GUILayout.MaxHeight(19f)))
                                {
                                    TypeBuilderWindow.ShowWindow(lastRect, methods[index]["returnType"], true, null, () =>
                                    {
                                        Undo.RegisterCompleteObjectUndo(listOfMethods[index], "Changed Method Return Type");
                                        shouldUpdate = true;
                                    });
                                    if (!subscribedEvents.ContainsKey(methods[index]["returnType"]))
                                    {
                                        subscribedEvents[methods[index]["returnType"]] = new HashSet<int>();
                                    }

                                    if (!subscribedEvents[methods[index]["returnType"]].Contains(0))
                                    {
                                        methods[index]["returnType"].valueChanged += (val) => GraphWindow.active?.context?.DescribeAndAnalyze();
                                        subscribedEvents[methods[index]["returnType"]].Add(0);
                                    }
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.Space(4);

                                listOfMethods[index].attributesOpened = HUMEditor.Foldout(listOfMethods[index].attributesOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                                {
                                    HUMEditor.Image(PathUtil.Load("attributes_16", CommunityEditorPath.Code).Single(), 16, 16);
                                    GUILayout.Label("Attributes");
                                }, () =>
                                {
                                    DrawAttributes(methods[index]["attributes"], listOfMethods[index].attributes, AttributeUsageType.Method, listOfMethods[index], "Changed Method Attribute Type");
                                });

                                GUILayout.Space(4);

                                listOfMethods[index].parametersOpened = HUMEditor.Foldout(listOfMethods[index].parametersOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                                {
                                    HUMEditor.Image(PathUtil.Load("parameters_16", CommunityEditorPath.Code).Single(), 16, 16, new RectOffset(), new RectOffset(4, 8, 4, 4));
                                    GUILayout.Label("Parameters");
                                }, () =>
                                {
                                    var paramMeta = methods[index]["parameters"];
                                    DrawParameters(paramMeta, listOfMethods[index], listOfMethods[index].graph.units[0] as FunctionNode);
                                });
                            }, true, false);
                        });

                        GUILayout.Space(4);
                    }

                    if (GUILayout.Button("+ Add Method"))
                    {
                        // Register undo for the parent object (Target)
                        Undo.RegisterCompleteObjectUndo(Target, "Added Method");

                        var declaration = CreateInstance<TMethodDeclaration>();

                        if (Target.GetType() == typeof(ClassAsset))
                            declaration.classAsset = Target as ClassAsset;
                        if (Target.GetType() == typeof(StructAsset))
                            declaration.structAsset = Target as StructAsset;

                        declaration.hideFlags = HideFlags.HideInHierarchy;

                        AssetDatabase.AddObjectToAsset(declaration, Target);
                        Undo.RegisterCreatedObjectUndo(declaration, "Created Method");

                        listOfMethods.Add(declaration);

                        var functionUnit = new FunctionNode(FunctionType.Method);
                        shouldUpdate = true;
                        functionUnit.methodDeclaration = declaration;
                        declaration.graph.units.Add(functionUnit);

                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }

                    if (methods.Count != methodsCount)
                    {
                        if (Target is ClassAsset)
                        {
                            for (int i = 0; i < methods.Count; i++)
                            {
                                ((TMethodDeclaration)methods[i].value).classAsset = Target as ClassAsset;
                            }
                        }
                        else
                        {
                            if (Target is StructAsset)
                            {
                                for (int i = 0; i < methods.Count; i++)
                                {
                                    ((TMethodDeclaration)methods[i].value).structAsset = Target as StructAsset;
                                }
                            }
                        }

                        methodsCount = methods.Count;
                    }
                });
            });
        }

        private void Variables()
        {
            Target.fieldsOpened = HUMEditor.Foldout(Target.fieldsOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2, () =>
            {
                HUMEditor.Image(PathUtil.Load("variables_16", CommunityEditorPath.Code).Single(), 16, 16, new RectOffset(), new RectOffset(4, 8, 4, 4));
                GUILayout.Label("Variables");
            }, () =>
            {
                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                {
                    var listOfVariables = variables.value as List<TFieldDeclaration>;

                    for (int i = 0; i < listOfVariables.Count; i++)
                    {
                        var index = i;
                        listOfVariables[index].opened = HUMEditor.Foldout(listOfVariables[index].opened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                        {
                            GraphWindow.active?.context?.BeginEdit();

                            EditorGUI.BeginChangeCheck();
                            var fieldName = GUILayout.TextField(listOfVariables[index].name);
                            if (EditorGUI.EndChangeCheck())
                            {
                                int undoGroup = Undo.GetCurrentGroup();
                                Undo.SetCurrentGroupName("Changed Variable Name");

                                Undo.RegisterCompleteObjectUndo(listOfVariables[index], "Changed Variable Name");
                                listOfVariables[index].FieldName = fieldName.LegalMemberName();
                                listOfVariables[index].name = listOfVariables[index].FieldName;
                                Undo.RegisterCompleteObjectUndo(listOfVariables[index].getter, "Changed Variable Name");
                                listOfVariables[index].getter.name = listOfVariables[index].name + " Getter";
                                Undo.RegisterCompleteObjectUndo(listOfVariables[index].setter, "Changed Variable Name");
                                listOfVariables[index].setter.name = listOfVariables[index].name + " Setter";
                                Undo.CollapseUndoOperations(undoGroup);
                                var getterFunctionUnit = listOfVariables[index].getter.graph.units[0] as FunctionNode;
                                var setterFunctionUnit = listOfVariables[index].setter.graph.units[0] as FunctionNode;
                                getterFunctionUnit.Define();
                                getterFunctionUnit.Describe();
                                setterFunctionUnit.Define();
                                setterFunctionUnit.Describe();
                                shouldUpdate = true;
                            }

                            GraphWindow.active?.context?.EndEdit();

                            if (typeof(TMemberTypeAsset) == typeof(ClassAsset) && (listOfVariables[index].getter.classAsset == null || listOfVariables[index].setter.classAsset == null))
                            {
                                listOfVariables[index].getter.classAsset = Target as ClassAsset;
                                listOfVariables[index].setter.classAsset = Target as ClassAsset;
                            }
                            else if (typeof(TMemberTypeAsset) == typeof(StructAsset) && (listOfVariables[index].getter.structAsset == null || listOfVariables[index].setter.structAsset == null))
                            {
                                listOfVariables[index].getter.structAsset = Target as StructAsset;
                                listOfVariables[index].setter.structAsset = Target as StructAsset;
                            }

                            if (GUILayout.Button("...", GUILayout.Width(19)))
                            {
                                GenericMenu menu = new GenericMenu();
                                menu.AddItem(new GUIContent("Delete"), false, (obj) =>
                                {
                                    Undo.RegisterCompleteObjectUndo(Target, "Deleted variable");
                                    variables.Remove(obj as TFieldDeclaration);
                                    shouldUpdate = true;
                                }, listOfVariables[index]);

                                if (index > 0)
                                {
                                    menu.AddItem(new GUIContent("Move Up"), false, (obj) =>
                                    {
                                        Undo.RegisterCompleteObjectUndo(Target, "Moved Variable Up");
                                        MoveItemUp(listOfVariables, index);
                                        shouldUpdate = true;
                                    }, listOfVariables[index]);
                                }

                                if (index < listOfVariables.Count - 1)
                                {
                                    menu.AddItem(new GUIContent("Move Down"), false, (obj) =>
                                    {
                                        Undo.RegisterCompleteObjectUndo(Target, "Moved Variable Down");
                                        MoveItemDown(listOfVariables, index);
                                        shouldUpdate = true;
                                    }, listOfVariables[index]);
                                }
                                menu.ShowAsContext();
                            }
                        }, () =>
                        {
                            HUMEditor.Vertical().Box(HUMColor.Grey(0.15f), Color.black, new RectOffset(6, 6, 6, 6), new RectOffset(1, 1, 0, 1), () =>
                            {
                                EditorGUILayout.BeginHorizontal();
                                //HUMEditor.Image(PathUtil.Load("scope_32", CommunityEditorPath.Code).Single(), 16, 16);
                                EditorGUI.BeginChangeCheck();
                                var scope = (AccessModifier)EditorGUILayout.EnumPopup("Scope", listOfVariables[index].scope);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    Undo.RegisterCompleteObjectUndo(listOfVariables[index], "Changed Variable Scope");
                                    shouldUpdate = true;
                                    listOfVariables[index].scope = scope;
                                }
                                EditorGUILayout.EndHorizontal();
                                if (!listOfVariables[index].isProperty)
                                {
                                    EditorGUI.BeginChangeCheck();
                                    var fieldModifier = (FieldModifier)EditorGUILayout.EnumPopup("Modifier", listOfVariables[index].fieldModifier);
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        Undo.RegisterCompleteObjectUndo(listOfVariables[index], "Changed Variable Modifier");
                                        shouldUpdate = true;
                                        listOfVariables[index].fieldModifier = fieldModifier;
                                    }
                                }
                                else
                                {
                                    EditorGUI.BeginChangeCheck();
                                    var propertyModifier = (PropertyModifier)EditorGUILayout.EnumPopup("Modifier", listOfVariables[index].propertyModifier);
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        Undo.RegisterCompleteObjectUndo(listOfVariables[index], "Changed Variable Modifier");
                                        shouldUpdate = true;
                                        listOfVariables[index].propertyModifier = propertyModifier;
                                    }
                                }

                                GUIContent TypebuilderButtonContent = new GUIContent(
                                (variables[index]["type"].value as Type)?.As().CSharpName(false).RemoveHighlights().RemoveMarkdown() ?? "Select Type",
                                (variables[index]["type"].value as Type)?.Icon()?[IconSize.Small]
                                );

                                GUILayout.BeginHorizontal();
                                GUILayout.Label("Type");
                                var lastRect = GUILayoutUtility.GetLastRect();
                                if (GUILayout.Button(TypebuilderButtonContent, EditorStyles.popup, GUILayout.MaxHeight(19f)))
                                {
                                    TypeBuilderWindow.ShowWindow(lastRect, variables[index]["type"], true, null, () =>
                                    {
                                        Undo.RegisterCompleteObjectUndo(listOfVariables[index], "Changed Variable Type");
                                        shouldUpdate = true;
                                    });
                                }
                                GUILayout.EndHorizontal();

                                if (target is ClassAsset)
                                {
                                    if (!listOfVariables[index].isProperty || (listOfVariables[index].get && !(listOfVariables[index].getter.graph.units[0] as FunctionNode).invoke.hasValidConnection) ||
                                    (listOfVariables[index].set && !(listOfVariables[index].setter.graph.units[0] as FunctionNode).invoke.hasValidConnection))
                                    {
                                        if (!subscribedEvents.ContainsKey(variables[index]["type"]))
                                        {
                                            subscribedEvents[variables[index]["type"]] = new HashSet<int>();
                                        }

                                        if (!subscribedEvents[variables[index]["type"]].Contains(i))
                                        {
                                            variables[index]["type"].valueChanged += (type) =>
                                            {
                                                GraphWindow.active?.context?.DescribeAndAnalyze();
                                                variables[index]["typeHandle"].value = new SerializableType((type as Type).AssemblyQualifiedName);
                                                if (variables[index]["defaultValue"].value?.GetType() == type as Type)
                                                {
                                                    return;
                                                }

                                                if (type == null)
                                                {
                                                    variables[index]["defaultValue"].value = null;
                                                }
                                                else if (ConversionUtility.CanConvert(variables[index]["defaultValue"].value, type as Type, true))
                                                {
                                                    variables[index]["defaultValue"].value = ConversionUtility.Convert(variables[index]["defaultValue"].value, type as Type);
                                                }
                                                else
                                                {
                                                    variables[index]["defaultValue"].value = (type as Type).Default();
                                                }

                                                variables[index]["defaultValue"].InferOwnerFromParent();
                                                shouldUpdate = true;
                                            };
                                            subscribedEvents[variables[index]["type"]].Add(i);
                                        }
                                        var inspector = variables[index]["defaultValue"].Inspector();
                                        typeof(SystemObjectInspector).GetField("inspector", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(inspector, Activator.CreateInstance(ValueInspectorType, inspector));
                                        Inspector.BeginBlock(variables[index]["defaultValue"], new Rect(10, 10, 10, 10));
                                        inspector.DrawLayout(new GUIContent("Value               "));
                                        if (Inspector.EndBlock(variables[index]["defaultValue"]))
                                        {
                                            Undo.RegisterCompleteObjectUndo(listOfVariables[index], "Changed Variable Default Value");
                                            shouldUpdate = true;
                                        }
                                    }
                                }

                                GUILayout.Space(4);

                                listOfVariables[index].attributesOpened = HUMEditor.Foldout(listOfVariables[index].attributesOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                                {
                                    HUMEditor.Image(PathUtil.Load("attributes_16", CommunityEditorPath.Code).Single(), 16, 16);
                                    GUILayout.Label("Attributes");
                                }, () =>
                                {
                                    DrawAttributes(variables[index]["attributes"], listOfVariables[index].attributes, listOfVariables[index].isProperty ? AttributeUsageType.Property : AttributeUsageType.Field, listOfVariables[index], "Changed Variable Attribute Type");
                                });

                                GUILayout.Space(4);

                                listOfVariables[index].propertyOpened = HUMEditor.Foldout(listOfVariables[index].propertyOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                                {
                                    HUMEditor.Image(PathUtil.Load("property_16", CommunityEditorPath.Code).Single(), 16, 16);
                                    EditorGUI.BeginChangeCheck();
                                    var isProperty = EditorGUILayout.ToggleLeft("Property", listOfVariables[index].isProperty);
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        Undo.RegisterCompleteObjectUndo(listOfVariables[index], "Toggle variable property");
                                        listOfVariables[index].isProperty = isProperty;
                                    }
                                }, () =>
                                {
                                    HUMEditor.Disabled(!listOfVariables[index].isProperty, () =>
                                    {
                                        HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground, Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(1, 1, 0, 1), () =>
                                        {
                                            HUMEditor.Horizontal(() =>
                                            {
                                                HUMEditor.Changed(() =>
                                                {
                                                    HUMEditor.Image(PathUtil.Load("getter_32", CommunityEditorPath.Code).Single(), 16, 16);
                                                    EditorGUI.BeginChangeCheck();
                                                    var get = EditorGUILayout.ToggleLeft("Get", listOfVariables[index].get);
                                                    if (EditorGUI.EndChangeCheck())
                                                    {
                                                        Undo.RegisterCompleteObjectUndo(listOfVariables[index], "Toggled Variable Get");
                                                        listOfVariables[index].get = get;
                                                    }
                                                },
                                                () =>
                                                {
                                                    if (!listOfVariables[index].set) listOfVariables[index].get = true;
                                                    shouldUpdate = true;
                                                });

                                                HUMEditor.Disabled(!listOfVariables[index].get, () =>
                                                {
                                                    if (GUILayout.Button("Edit", GUILayout.Width(60)))
                                                    {
                                                        GraphWindow.OpenActive(listOfVariables[index].getter.GetReference() as GraphReference);
                                                    }
                                                });

                                                HUMEditor.Disabled(!listOfVariables[index].get, () =>
                                                {
                                                    EditorGUI.BeginChangeCheck();
                                                    var value = (AccessModifier)EditorGUILayout.EnumPopup((AccessModifier)variables[index]["getterScope"].value, GUILayout.Width(100));
                                                    if (EditorGUI.EndChangeCheck())
                                                    {
                                                        Undo.RegisterCompleteObjectUndo(listOfVariables[index], "Changed Variable Get Scope");
                                                        variables[index]["getterScope"].value = value;
                                                    }
                                                });
                                            });

                                            HUMEditor.Horizontal(() =>
                                            {
                                                HUMEditor.Changed(() =>
                                                {
                                                    HUMEditor.Image(PathUtil.Load("setter_32", CommunityEditorPath.Code).Single(), 16, 16);
                                                    EditorGUI.BeginChangeCheck();
                                                    var set = EditorGUILayout.ToggleLeft("Set", listOfVariables[index].set);
                                                    if (EditorGUI.EndChangeCheck())
                                                    {
                                                        Undo.RegisterCompleteObjectUndo(listOfVariables[index], "Toggled Variable Set");
                                                        listOfVariables[index].set = set;
                                                    }
                                                },
                                                () =>
                                                {
                                                    if (!listOfVariables[index].set) listOfVariables[index].get = true;
                                                    shouldUpdate = true;
                                                });

                                                HUMEditor.Disabled(!listOfVariables[index].set, () =>
                                                {
                                                    if (GUILayout.Button("Edit", GUILayout.Width(60)))
                                                    {
                                                        GraphWindow.OpenActive(listOfVariables[index].setter.GetReference() as GraphReference);
                                                    }
                                                });

                                                HUMEditor.Disabled(!listOfVariables[index].set, () =>
                                                {
                                                    EditorGUI.BeginChangeCheck();
                                                    var setterScope = (AccessModifier)EditorGUILayout.EnumPopup(listOfVariables[index].setterScope, GUILayout.Width(100));
                                                    if (EditorGUI.EndChangeCheck())
                                                    {
                                                        Undo.RegisterCompleteObjectUndo(listOfVariables[index], "Changed Variable Set Scope");
                                                        listOfVariables[index].setterScope = setterScope;
                                                    }
                                                });
                                            });
                                        }, true, false);
                                    });

                                });
                            }, true, false);
                        });

                        GUILayout.Space(4);
                    }

                    if (GUILayout.Button("+ Add Variable"))
                    {
                        int undoGroup = Undo.GetCurrentGroup();
                        Undo.SetCurrentGroupName("Added Variable");

                        Undo.RegisterCompleteObjectUndo(Target, "Added Variable");

                        var declaration = CreateInstance<TFieldDeclaration>();
                        Undo.RegisterCreatedObjectUndo(declaration, "Created Variable");

                        if (Target is ClassAsset classAsset)
                            declaration.classAsset = classAsset;
                        else if (Target is StructAsset structAsset)
                            declaration.structAsset = structAsset;

                        var getter = CreateInstance<PropertyGetterMacro>();
                        Undo.RegisterCreatedObjectUndo(getter, "Created Getter");
                        var setter = CreateInstance<PropertySetterMacro>();
                        Undo.RegisterCreatedObjectUndo(setter, "Created Setter");

                        if (Target is ClassAsset)
                        {
                            getter.classAsset = Target as ClassAsset;
                            setter.classAsset = Target as ClassAsset;
                        }
                        else
                        {
                            getter.structAsset = Target as StructAsset;
                            setter.structAsset = Target as StructAsset;
                        }

                        AssetDatabase.AddObjectToAsset(declaration, Target);
                        AssetDatabase.AddObjectToAsset(getter, Target);
                        AssetDatabase.AddObjectToAsset(setter, Target);

                        listOfVariables.Add(declaration);

                        var functionGetterUnit = new FunctionNode(FunctionType.Getter) { fieldDeclaration = declaration };
                        var functionSetterUnit = new FunctionNode(FunctionType.Setter) { fieldDeclaration = declaration };

                        declaration.getter = getter;
                        declaration.setter = setter;
                        declaration.getter.graph.units.Add(functionGetterUnit);
                        declaration.setter.graph.units.Add(functionSetterUnit);

                        declaration.hideFlags = HideFlags.HideInHierarchy;
                        getter.hideFlags = HideFlags.HideInHierarchy;
                        setter.hideFlags = HideFlags.HideInHierarchy;

                        Undo.CollapseUndoOperations(undoGroup);

                        shouldUpdate = true;
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                });
            });
        }

        private void MoveItemDown<T>(List<T> list, int index)
        {
            if (index < list.Count - 1)
            {
                T item = list[index];
                list.RemoveAt(index);
                list.Insert(index + 1, item);
            }
        }

        private void MoveItemUp<T>(List<T> list, int index)
        {
            if (index > 0)
            {
                T item = list[index];
                list.RemoveAt(index);
                list.Insert(index - 1, item);
            }
        }
    }
}