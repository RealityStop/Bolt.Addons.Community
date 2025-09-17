using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Utility;
using UnityEditor;
using UnityEngine;
using ParameterModifier = Unity.VisualScripting.Community.Libraries.CSharp.ParameterModifier;

namespace Unity.VisualScripting.Community.CSharp
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

        protected Metadata interfaces;
        protected SerializedProperty interfacesProp;

        private Type[] attributeTypes = new Type[] { };
        private Type[] classAttributeTypes = new Type[] { };
        private Type[] enumAttributeTypes = new Type[] { };
        private Type[] fieldAttributeTypes = new Type[] { };
        private Type[] parameterAttributeTypes = new Type[] { };
        private Type[] interfaceAttributeTypes = new Type[] { };
        private Type[] methodAttributeTypes = new Type[] { };
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
            var context = GraphWindow.active?.context;
            try
            {
                context?.BeginEdit();
                Constructors();
                GUILayout.Space(4);
                Variables();
                GUILayout.Space(4);
                Methods();

                if (typeof(TMemberTypeAsset) == typeof(ClassAsset) && (Target as ClassAsset).inheritsType)
                {
                    GUILayout.Space(4);
                    RequiredInfo();
                    GUILayout.Space(4);
                    OverridableMembersInfo();
                }
            }
            finally
            {
                context?.EndEdit();
            }
        }

        private bool RequiresInfo()
        {
            var classAsset = Target as ClassAsset;
            var inheritedType = classAsset.inherits.type;

            if (inheritedType == null) return false;
            if (inheritedType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(m => !m.Name.Contains("get_") && !m.Name.Contains("set_")).Any(m => m.IsAbstract && !MethodExists(m)))
            {
                return true;
            }
            if (inheritedType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Any(p => (p.GetMethod?.IsAbstract == true || p.SetMethod?.IsAbstract == true) && !PropertyExists(p)))
            {
                return true;
            }
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
                    foreach (var param in constructor.parameters.Where(param => !param.showInitalizer))
                    {
                        param.showInitalizer = true;
                    }
                    if (constructor.scope != c.GetScope()) continue;
                    var inheritedType = (Target as ClassAsset).inherits.type;
                    var constructorParameters = c.GetParameters();
                    bool requiredParametersHaveBase = constructor.parameters.Where(parameter => parameter.useInInitializer).Select(parameter => parameter.type).SequenceEqual(constructorParameters.Select(parameter => parameter.ParameterType));
                    if (!requiredParametersHaveBase)
                    {
                        continue;
                    }

                    if (!constructor.parameters.Where(parameter => parameter.useInInitializer).Select(parameter => parameter.modifier).SequenceEqual(constructorParameters.Select(parameter => parameter.GetModifier()))) continue;

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
                                declaration.parentAsset = Target;
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
                                UpdatePreview();
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();
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
                                declaration.parentAsset = Target;
                                var getter = CreateInstance<PropertyGetterMacro>();
                                var setter = CreateInstance<PropertySetterMacro>();
                                Undo.RegisterCreatedObjectUndo(getter, "Created Property Getter Macro");
                                Undo.RegisterCreatedObjectUndo(setter, "Created Property Setter Macro");
                                getter.parentAsset = Target;
                                setter.parentAsset = Target;
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

                                UpdatePreview();
                                Undo.CollapseUndoOperations(undoGroup);
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();
                            }

                        }, true);

                        GUILayout.Space(4);
                    }

                    //TODO : Add event support

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
                                declaration.parentAsset = Target;
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
                                declaration.initializerType = ConstructorInitializer.Base;
                                declaration.parameters = parameterizedConstructors[index]
                                    .GetParameters()
                                    .Select(param => new TypeParam(param.ParameterType, param.Name)
                                    {
                                        useInInitializer = !inheritedType.IsAbstract && inheritedType.IsClass
                                    })
                                    .ToList();
                                declaration.modifier = parameterizedConstructors[index].GetModifier();

                                UpdatePreview();
                                Undo.CollapseUndoOperations(undoGroup);
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();
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
            var inheritedType = classAsset.GetInheritedType();

            if (inheritedType == null) return false;
            if (inheritedType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(m => m.Overridable()).Any(m => !MethodExists(m)))
            {
                return true;
            }

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
                                declaration.parentAsset = Target;
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

                                UpdatePreview();
                                Undo.CollapseUndoOperations(undoGroup);

                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();
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

                                declaration.parentAsset = Target;

                                var getter = CreateInstance<PropertyGetterMacro>();
                                Undo.RegisterCreatedObjectUndo(getter, "Created Getter Macro");

                                var setter = CreateInstance<PropertySetterMacro>();
                                Undo.RegisterCreatedObjectUndo(setter, "Created Setter Macro");
                                getter.parentAsset = Target;
                                setter.parentAsset = Target;
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

                                UpdatePreview();
                                Undo.CollapseUndoOperations(undoGroup);

                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();
                            }

                        }, true);

                        GUILayout.Space(4);
                    }

                    // TODO: Add event support
                });
            });
        }


        protected virtual Texture2D DefaultIcon() { return null; }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (ValueInspectorType == null)
                ValueInspectorType = typeof(SystemObjectInspector).Assembly.GetType("Unity.VisualScripting.ValueInspector", throwOnError: true);
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

            if (interfaces == null || interfacesProp == null)
            {
                interfaces = Metadata.FromProperty(serializedObject.FindProperty("interfaces"));
                interfacesProp = serializedObject.FindProperty("interfaces");
            }

            if (Target.icon == null) Target.icon = DefaultIcon();

            CacheConstrainedAttributes();

            UpdatePreview();
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
                        UpdatePreview();
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
                DrawAttributes(attributes, Target.attributes, Target is ClassAsset ? AttributeUsageType.Class : AttributeUsageType.Struct, Target, "Changed Asset Attribute Type");
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
                UpdatePreview();
            }
            EditorGUI.BeginChangeCheck();
            var inspectable = EditorGUILayout.ToggleLeft("Inspectable", Target.inspectable);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(Target, "Toggled Asset Option 'Inspectable'");
                Target.inspectable = inspectable;
                EditorUtility.SetDirty(Target);
                UpdatePreview();
            }
            EditorGUI.BeginChangeCheck();
            var includeInSettings = EditorGUILayout.ToggleLeft("Include In Settings", Target.includeInSettings);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(Target, "Toggled Asset Option 'IncludeInSettings'");
                Target.includeInSettings = includeInSettings;
                EditorUtility.SetDirty(Target);
                UpdatePreview();
            }
            EditorGUI.BeginChangeCheck();
            var definedEvent = EditorGUILayout.ToggleLeft("Flag for Defined Event Filtering", Target.definedEvent);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(Target, "Toggled Asset Option 'DefinedEvent'");
                Target.definedEvent = definedEvent;
                EditorUtility.SetDirty(Target);
                UpdatePreview();
            }
            OnExtendedOptionsGUI();

            if (Target is ClassAsset or StructAsset or InterfaceAsset)
            {
                Interfaces();
            }
        }

        private void Interfaces()
        {
            Target.interfacesOpened = HUMEditor.Foldout(Target.interfacesOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2, () =>
            {
                HUMEditor.Image(typeof(IAction).Icon()[IconSize.Small], 16, 16, new RectOffset(), new RectOffset(4, 8, 4, 4));
                GUILayout.Label("Interfaces");
            }, () =>
            {
                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                {
                    var listOfInterfaces = Target.interfaces;

                    for (int i = 0; i < listOfInterfaces.Count; i++)
                    {
                        var index = i;
                        var _interface = listOfInterfaces[i];
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"Interface {i}");
                        if (GUILayout.Button(new GUIContent(_interface.type.DisplayName(), _interface.type.Icon()[IconSize.Small])))
                        {
                            TypeBuilderWindow.ShowWindow(GUILayoutUtility.GetLastRect(), interfaces[i]["type"], false, Codebase.settingsAssembliesTypes.Where(t => t.IsInterface && t.IsPublic).ToArray());
                        }

                        if (GUILayout.Button("...", GUILayout.Width(19)))
                        {
                            GenericMenu menu = new GenericMenu();
                            menu.AddItem(new GUIContent("Delete"), false, (obj) =>
                            {
                                Undo.RegisterCompleteObjectUndo(Target, "Deleted Interface");
                                UpdatePreview();
                                interfaces.Remove(obj as SystemType);
                            }, listOfInterfaces[index]);

                            if (i > 0)
                            {
                                menu.AddItem(new GUIContent("Move Up"), false, (obj) =>
                                {
                                    Undo.RegisterCompleteObjectUndo(Target, "Moved Interface up");
                                    UpdatePreview();
                                    MoveItemUp(Target.interfaces, index);
                                }, listOfInterfaces[index]);
                            }

                            if (i < listOfInterfaces.Count - 1)
                            {
                                menu.AddItem(new GUIContent("Move Down"), false, (obj) =>
                                {
                                    Undo.RegisterCompleteObjectUndo(Target, "Moved Interface down");
                                    UpdatePreview();
                                    MoveItemDown(Target.interfaces, index);
                                }, listOfInterfaces[index]);
                            }
                            menu.ShowAsContext();
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.Space(4);
                    }
                    if (GUILayout.Button("+ Add Interface"))
                    {
                        Undo.RegisterCompleteObjectUndo(Target, "Added Interface");
                        UpdatePreview();
                        Target.interfaces.Add(new SystemType(Codebase.settingsAssembliesTypes.Where(t => t.IsInterface && t.IsPublic).First()));
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                });
            });
        }

        private Type AttributeTypeField(AttributeDeclaration attribute, AttributeUsageType usage)
        {
            GUILayout.Label(" ", GUILayout.Height(20));
            var position = GUILayoutUtility.GetLastRect();
            Type[] types = usage switch
            {
                AttributeUsageType.Struct => structAttributeTypes,
                AttributeUsageType.Enum => enumAttributeTypes,
                AttributeUsageType.Interface => interfaceAttributeTypes,
                AttributeUsageType.Field => fieldAttributeTypes,
                AttributeUsageType.Property => propertyAttributeTypes,
                AttributeUsageType.Method => methodAttributeTypes,
                AttributeUsageType.Parameter => parameterAttributeTypes,
                _ => classAttributeTypes,
            };
            return LudiqGUI.TypeField(position, GUIContent.none, attribute.GetAttributeType(), () =>
            {
                return new TypeOptionTree(types);
            });
        }

        private void CacheConstrainedAttributes()
        {
            var allAttributeTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => (t.IsSubclassOf(typeof(Attribute)) || t == typeof(Attribute)) && t.IsPublic)
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
                        if (listOfConstructors[index].parentAsset == null) listOfConstructors[index].parentAsset = Target;
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
                                    UpdatePreview();
                                    constructors.Remove(obj as TConstructorDeclaration);
                                    AssetDatabase.RemoveObjectFromAsset(obj as TConstructorDeclaration);
                                }, listOfConstructors[index]);

                                if (index > 0)
                                {
                                    menu.AddItem(new GUIContent("Move Up"), false, (obj) =>
                                    {
                                        Undo.RegisterCompleteObjectUndo(listOfConstructors[index], "Moved Constructor up");
                                        UpdatePreview();
                                        MoveItemUp(listOfConstructors, index);
                                    }, listOfConstructors[index]);
                                }

                                if (index < methods.Count - 1)
                                {
                                    menu.AddItem(new GUIContent("Move Down"), false, (obj) =>
                                    {
                                        Undo.RegisterCompleteObjectUndo(listOfConstructors[index], "Moved Constructor down");
                                        UpdatePreview();
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
                                    UpdatePreview();
                                    listOfConstructors[index].scope = scope;
                                }
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.BeginHorizontal();
                                //HUMEditor.Image(PathUtil.Load("scope_32", CommunityEditorPath.Code).Single(), 16, 16);
                                EditorGUI.BeginChangeCheck();
                                var initializerType = (ConstructorInitializer)EditorGUILayout.EnumPopup("Initializer Type", listOfConstructors[index].initializerType);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    Undo.RegisterCompleteObjectUndo(listOfConstructors[index], "Changed Constructor Initializer Type");
                                    UpdatePreview();
                                    listOfConstructors[index].initializerType = initializerType;
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
                        declaration.parentAsset = Target;
                        declaration.hideFlags = HideFlags.HideInHierarchy;
                        AssetDatabase.AddObjectToAsset(declaration, Target);

                        listOfConstructors.Add(declaration);

                        var functionUnit = new FunctionNode(FunctionType.Constructor)
                        {
                            constructorDeclaration = declaration
                        };

                        declaration.graph.units.Add(functionUnit);

                        var listOfVariables = variables.value as List<TFieldDeclaration>;

                        UpdatePreview();

                        Undo.CollapseUndoOperations(undoGroup);

                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
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
                                InitializeAttribute(attribute, attrIndex);
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
                                    UpdatePreview();
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
                                            UpdatePreview();
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
                                            UpdatePreview();
                                        }
                                    }, attribute);
                                }
                                menu.ShowAsContext();
                            }
                        },
                    () =>
                    {
                        HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                        {
                            var attributeParamMeta = attributesMeta[attrIndex]["parameters"];
                            var constructors = attribute?.GetAttributeType()?.GetConstructors();
                            string[] constructorNames = constructors
                            .Where(constructor => constructor.IsPublic && constructor.GetParameters().All(param => param.ParameterType.HasInspector()))
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
                            InitializeAttribute(attribute, attrIndex);
                            var fields = attribute.GetAttributeType()?.GetFields().Cast<MemberInfo>().Concat(attribute.GetAttributeType()?.GetProperties()).ToArray();
                            for (int i = 0; i < attribute.fields?.Count; i++)
                            {
                                var field = attribute.fields.ElementAt(i);
                                var metadata = attributesMeta[attrIndex]["fields"].Indexer(field.Key).Cast(fields.FirstOrDefault(f => f.Name == field.Key)?.GetAccessorType() ?? field.Value?.GetType() ?? typeof(object));
                                GUILayout.BeginHorizontal();
                                if (metadata.value is Type type)
                                {
                                    GUIContent TypebuilderButtonContent = new(
                                    (metadata.value as Type)?.As().CSharpName(false).RemoveHighlights().RemoveMarkdown() ?? "Select Type",
                                    (metadata.value as Type)?.Icon()?[IconSize.Small]
                                    );
                                    GUILayout.Label(field.Key + ":");
                                    var lastRect = GUILayoutUtility.GetLastRect();
                                    if (GUILayout.Button(TypebuilderButtonContent, EditorStyles.popup, GUILayout.MaxHeight(19f)))
                                    {
                                        TypeBuilderWindow.ShowWindow(lastRect, metadata, true, new Type[0], () => shouldUpdate = true);
                                    }
                                }
                                else
                                {
                                    EditorGUILayout.PrefixLabel(field.Key + ":");
                                    LudiqGUI.InspectorLayout(metadata, GUIContent.none);
                                }
                                if (GUILayout.Button("...", GUILayout.Width(19)))
                                {
                                    GenericMenu menu = new GenericMenu();
                                    menu.AddItem(new GUIContent("Delete"), false, () =>
                                    {
                                        Undo.RegisterCompleteObjectUndo(target, "Removed Atribute Field");
                                        attribute.RemoveField(field.Key);
                                        UpdatePreview();
                                    });
                                    menu.ShowAsContext();
                                }
                                GUILayout.EndHorizontal();

                            }
                            if (GUILayout.Button("+ Add Field"))
                            {
                                var selectedConstructor = constructors
                                    .Where(constructor => constructor.IsPublic && constructor.GetParameters().All(param => param.ParameterType.HasInspector()))
                                    .ToList()[attribute.constructor];
                                GenericMenu menu = new GenericMenu();
                                var allFields = attribute.GetAttributeType()?.GetFields(BindingFlags.Public | BindingFlags.Instance).Cast<MemberInfo>()
                                    .Concat(attribute.GetAttributeType()?.GetProperties(BindingFlags.Public | BindingFlags.Instance))?.ToArray();

                                var constructorParams = selectedConstructor?.GetParameters().Select(p => p.Name).ToHashSet() ?? new HashSet<string>();
                                var availableFields = allFields?
                                    .Where(f => !attribute.fields.ContainsKey(f.Name) && !constructorParams.Any(p => f.Name.Equals(p, StringComparison.OrdinalIgnoreCase)) && f.Name != "TypeId")
                                    .ToList();

                                if (availableFields != null && availableFields.Count > 0)
                                {
                                    foreach (var field in availableFields)
                                    {
                                        var fieldType = field.GetAccessorType();
                                        menu.AddItem(new GUIContent(field.Name), false, () =>
                                        {
                                            attribute.SetField(field.Name, GetDefaultValue(fieldType));
                                            Undo.RegisterCompleteObjectUndo(target, "Added Attribute Field");
                                            UpdatePreview();
                                        });
                                    }
                                }
                                else
                                {
                                    menu.AddDisabledItem(new GUIContent("No available fields"));
                                }

                                menu.ShowAsContext();
                            }

                            if (attributeList.Count > 0) GUILayout.Space(4);
                        });
                    });
                }

                if (GUILayout.Button("+ Add Attribute"))
                {
                    var attribute = new AttributeDeclaration();
                    attribute.SetType(GetConstrainedAttributeTypes(attributeUsageType)[0]);
                    Undo.RegisterCompleteObjectUndo(target, "Added Attribute");
                    attributeList.Add(attribute);
                    InitializeAttribute(attribute, attributeList.Count - 1);
                    UpdatePreview();
                }

                void InitializeAttribute(AttributeDeclaration attribute, int attributeIndex)
                {
                    var attributeParamMeta = attributesMeta[attributeIndex]["parameters"];
                    var constructors = attribute?.GetAttributeType()?.GetConstructors();
                    var selectedConstructor = constructors
                   .Where(constructor => constructor.IsPublic && constructor.GetParameters().All(param => param.ParameterType.HasInspector()))
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
                            Param.modifier = ParameterModifier.Params;
                            Inspector.BeginBlock(
                                attributeParamMeta[paramIndex]["defaultValue"],
                                new Rect()
                            );
                            if (!isParamsParameter && Param.defaultValue is not ICollection)
                            {
                                if (attributeParamMeta[paramIndex]["defaultValue"].value is Type type)
                                {
                                    GUIContent TypebuilderButtonContent = new(
                                    (attributeParamMeta[paramIndex]["defaultValue"].value as Type)?.As().CSharpName(false).RemoveHighlights().RemoveMarkdown() ?? "Select Type",
                                    (attributeParamMeta[paramIndex]["defaultValue"].value as Type)?.Icon()?[IconSize.Small]
                                    );
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Label(parameter.Name + ":");
                                    var lastRect = GUILayoutUtility.GetLastRect();
                                    if (GUILayout.Button(TypebuilderButtonContent, EditorStyles.popup, GUILayout.MaxHeight(19f)))
                                    {
                                        TypeBuilderWindow.ShowWindow(lastRect, attributeParamMeta[paramIndex]["defaultValue"], true, new Type[0], () => shouldUpdate = true);
                                    }
                                    GUILayout.EndHorizontal();
                                }
                                else
                                {
                                    if (attributeParamMeta[paramIndex]["defaultValue"].value is bool boolVal)
                                    {
                                        GUILayout.BeginHorizontal();
                                        EditorGUILayout.PrefixLabel(Param.name + ":");
                                        attributeParamMeta[paramIndex]["defaultValue"].value = GUILayout.Toggle(boolVal, GUIContent.none);
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
                                UpdatePreview();
                            }
                            paramIndex++;
                        }
                        paramIndex = 0;
                    }
                }
            });
        }

        public object GetDefaultValue(Type type)
        {
            var defaultValue = type.PseudoDefault();
            if (defaultValue != null) return defaultValue;
            var defaultConstructor = type.GetDefaultConstructor();
            if (defaultConstructor != null) return defaultConstructor.Invoke(new object[0]);
            var pseudoDefaultConstructor = type.GetConstructors().FirstOrDefault(c => c.IsPublic && c.GetParameters().All(p => p.IsOptional));
            if (pseudoDefaultConstructor != null) return pseudoDefaultConstructor.Invoke(new object[pseudoDefaultConstructor.GetParameters().Length]);
            return null;
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
        Dictionary<Metadata, Type> typeChangedLookup = new Dictionary<Metadata, Type>();
        private void DrawParameters(Metadata paramMeta, UnityEngine.Object target, FunctionNode functionUnit = null)
        {
            var parameters = paramMeta.value as List<TypeParam>;
            HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    parameters[i].opened = HUMEditor.Foldout(parameters[i].opened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                    {
                        IGraphContext context = null;
                        if (paramMeta.parent.value is Macro<FlowGraph> parent)
                        {
                            context = parent.GetReference().AsReference().Context();
                        }
                        context?.BeginEdit();
                        EditorGUI.BeginChangeCheck();
                        var paramName = GUILayout.TextField(parameters[i].name);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RegisterCompleteObjectUndo(target, "Changed Parameter Name");
                            parameters[i].name = paramName.LegalMemberName();
                            foreach (var element in context.graph.elements)
                            {
                                if (element is Unit unit)
                                    unit.Define();
                            }
                        }
                        context?.EndEdit();

                        if (GUILayout.Button("...", GUILayout.Width(19)))
                        {
                            GenericMenu menu = new GenericMenu();
                            menu.AddItem(new GUIContent("Delete"), false, (obj) =>
                            {
                                TypeParam paramToRemove = obj as TypeParam;
                                Undo.RegisterCompleteObjectUndo(target, $"Deleted {paramToRemove.name} parameter");
                                parameters.Remove(paramToRemove);
                                UpdatePreview();
                            }, parameters[i]);

                            if (i > 0)
                            {
                                menu.AddItem(new GUIContent("Move Up"), false, (obj) =>
                                {
                                    var paramIndex = parameters.IndexOf(obj as TypeParam);
                                    if (paramIndex > 0)
                                    {
                                        Undo.RegisterCompleteObjectUndo(target, $"Moved {(obj as TypeParam).name} parameter up");
                                        var temp = parameters[paramIndex];
                                        parameters[paramIndex] = parameters[paramIndex - 1];
                                        parameters[paramIndex - 1] = temp;
                                        UpdatePreview();
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
                                        Undo.RegisterCompleteObjectUndo(target, $"Moved {(obj as TypeParam).name} parameter down");
                                        var temp = parameters[paramIndex];
                                        parameters[paramIndex] = parameters[paramIndex + 1];
                                        parameters[paramIndex + 1] = temp;
                                        UpdatePreview();
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
                            var currentParam = paramMeta[i];
                            var param = parameters[i];
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Type");
                            var lastRect = GUILayoutUtility.GetLastRect();
                            List<FakeGenericParameterType> fakeGenericParameterTypes = new List<FakeGenericParameterType>();
                            if (target is MethodDeclaration methodDeclaration)
                            {
                                foreach (var generic in methodDeclaration.genericParameters)
                                {
                                    fakeGenericParameterTypes.Add(new FakeGenericParameterType(generic.name, methodDeclaration.genericParameters.IndexOf(generic), generic.typeParameterConstraints, generic.baseTypeConstraint, generic.constraints?.ToList()));
                                }
                            }
                            if (TypeBuilderWindow.Button(currentParam["type"].value as Type))
                            {
                                TypeBuilderWindow.ShowWindow(lastRect, (type) => { currentParam["type"].value = type; }, parameters[i].type, true, fakeGenericParameterTypes, () =>
                                {
                                    Undo.RegisterCompleteObjectUndo(target, "Changed Parameter Type");
                                    UpdatePreview();
                                });
                            }
                            GraphWindow.active?.context?.BeginEdit();
                            GUILayout.EndHorizontal();
                            EditorGUI.BeginChangeCheck();
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Modifiers");
                            var modifiers = (ParameterModifier)currentParam["modifier"].value;
                            if ((param.modifier & (ParameterModifier.Params)) != 0 && !param.type.IsArray)
                            {
                                param.modifier &= ~ParameterModifier.Params;
                            }
                            if (GUILayout.Button(modifiers.GetEnumString(ParameterModifier.None, "None"), EditorStyles.popup, GUILayout.MaxHeight(19f)))
                            {
                                GenericMenu menu = new GenericMenu();
                                menu.AddItem(new GUIContent("None"), modifiers == ParameterModifier.None, (obj) =>
                                {
                                    var _param = obj as TypeParam;
                                    _param.modifier = ParameterModifier.None;
                                }, param);
                                menu.AddSeparator("");
                                bool canUseIn = (param.modifier & (ParameterModifier.Out | ParameterModifier.Ref)) == 0;
                                bool canUseOut = (param.modifier & (ParameterModifier.In | ParameterModifier.Ref)) == 0;
                                bool canUseRef = (param.modifier & (ParameterModifier.In | ParameterModifier.Out)) == 0;
                                if (canUseIn)
                                {
                                    menu.AddItem(new GUIContent("In"), (param.modifier & ParameterModifier.In) != 0, (obj) =>
                                    {
                                        var _param = obj as TypeParam;
                                        _param.modifier ^= ParameterModifier.In;
                                    }, param);
                                }
                                else
                                {
                                    menu.AddDisabledItem(new GUIContent("In"));
                                }

                                if (canUseOut)
                                {
                                    menu.AddItem(new GUIContent("Out"), (param.modifier & ParameterModifier.Out) != 0, (obj) =>
                                    {
                                        var _param = obj as TypeParam;
                                        _param.modifier ^= ParameterModifier.Out;
                                    }, param);
                                }
                                else
                                {
                                    menu.AddDisabledItem(new GUIContent("Out"));
                                }

                                if (canUseRef)
                                {
                                    menu.AddItem(new GUIContent("Ref"), (param.modifier & ParameterModifier.Ref) != 0, (obj) =>
                                    {
                                        var _param = obj as TypeParam;
                                        _param.modifier ^= ParameterModifier.Ref;
                                    }, param);
                                }
                                else
                                {
                                    menu.AddDisabledItem(new GUIContent("Ref"));
                                }
                                menu.AddSeparator("");
                                if (param.type.IsArray)
                                {
                                    menu.AddItem(new GUIContent("Params"), (modifiers & ParameterModifier.Params) != 0, (obj) =>
                                    {
                                        var _param = obj as TypeParam;
                                        if ((_param.modifier & ParameterModifier.Params) == 0)
                                        {
                                            _param.modifier |= ParameterModifier.Params;
                                        }
                                        else
                                        {
                                            _param.modifier &= ~ParameterModifier.Params;
                                        }
                                    }, param);
                                }
                                else
                                {
                                    menu.AddDisabledItem(new GUIContent("Params"));
                                }

                                if (target is ClassMethodDeclaration classMethodDeclaration && classMethodDeclaration.modifier == MethodModifier.Static && (classMethodDeclaration.parentAsset as ClassAsset).classModifier == ClassModifier.Static)
                                {
                                    menu.AddItem(new GUIContent("This"), (modifiers & ParameterModifier.This) != 0, (obj) =>
                                    {
                                        var _param = obj as TypeParam;
                                        if ((_param.modifier & ParameterModifier.This) == 0)
                                        {
                                            _param.modifier |= ParameterModifier.This;
                                        }
                                        else
                                        {
                                            _param.modifier &= ~ParameterModifier.This;
                                        }
                                    }, param);
                                }
                                else
                                {
                                    menu.AddDisabledItem(new GUIContent("This"));
                                }

                                menu.ShowAsContext();
                            }
                            GUILayout.EndHorizontal();
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RegisterCompleteObjectUndo(target, "Changed Parameter Modifier");
                                UpdatePreview();
                            }
                            GUILayout.Space(4);
                            if (parameters[i].showInitalizer)
                            {
                                Inspector.BeginBlock(currentParam["useInInitializer"], new Rect());
                                LudiqGUI.InspectorLayout(currentParam["useInInitializer"], new GUIContent("Use In Initializer"));
                                if (Inspector.EndBlock(currentParam["useInInitializer"]))
                                {
                                    Undo.RegisterCompleteObjectUndo(target, "Toggled Use In Initializer");
                                    UpdatePreview();
                                }
                                GUILayout.Space(4);
                            }

                            Inspector.BeginBlock(currentParam["hasDefault"], new Rect());
                            LudiqGUI.InspectorLayout(currentParam["hasDefault"], new GUIContent("Has Default Value"));
                            if (Inspector.EndBlock(currentParam["hasDefault"]))
                            {
                                Undo.RegisterCompleteObjectUndo(target, "Toggled Has Default");
                                UpdatePreview();
                            }
                            GraphWindow.active?.context?.BeginEdit();
                            GUILayout.Space(4);

                            if (i >= 0 && i < paramMeta.Count)
                            {
                                if (parameters[i].hasDefault)
                                {
                                    if (typeChangedLookup.TryGetValue(currentParam["type"], out var type))
                                    {
                                        if (type != currentParam["type"].value as Type || parameters[i].defaultValue?.GetType() != type)
                                        {
                                            UpdateDefaultValueType((Type)currentParam["type"].value, currentParam);
                                            typeChangedLookup[currentParam["type"]] = (Type)currentParam["type"].value;
                                            UpdatePreview();
                                        }
                                    }
                                    else
                                    {
                                        typeChangedLookup[currentParam["type"]] = (Type)currentParam["type"].value;
                                    }

                                    if (parameters[i].type.IsBasic())
                                    {
                                        var inspector = currentParam["defaultValue"].Inspector();
                                        typeof(SystemObjectInspector).GetField("inspector", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(inspector, Activator.CreateInstance(ValueInspectorType, inspector));
                                        inspector.DrawLayout(new GUIContent("Value               "));
                                        GUILayout.Space(4);
                                    }
                                }
                            }

                            if (parameters[i].attributes == null) parameters[i].attributes = new List<AttributeDeclaration>();
                            parameters[i].attributesOpened = HUMEditor.Foldout(parameters[i].attributesOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                            {
                                HUMEditor.Image(PathUtil.Load("attributes_16", CommunityEditorPath.Code).Single(), 16, 16);
                                GUILayout.Label("Attributes");
                            }, () =>
                            {
                                DrawAttributes(currentParam["attributes"], parameters[i].attributes, AttributeUsageType.Parameter, target, "Changed Parameter attribute");
                            });
                            if (Inspector.EndBlock(paramMeta))
                            {
                                Undo.RegisterCompleteObjectUndo(target, "Changed Parameter");
                                GraphWindow.active?.context?.DescribeAndAnalyze();
                                UpdatePreview();
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
                    parameters.Add(new TypeParam(typeof(string), name) { defaultValue = "" });
                }
            });
        }

        void UpdateDefaultValueType(Type type, Metadata currentParam)
        {
            currentParam["typeHandle"].value = new SerializableType(type.AssemblyQualifiedName);
            if (currentParam["defaultValue"].value?.GetType() == type)
            {
                currentParam["defaultValue"].value = null;
                return;
            }
            if (type is FakeGenericParameterType)
            {
                currentParam["defaultValue"].value = null;
                return;
            }
            if (type.IsGenericType && RuntimeTypeUtility.GetNestedFakeGenerics(type).Count() > 0)
            {
                currentParam["defaultValue"].value = null;
                return;
            }

            if (type.IsArray && RuntimeTypeUtility.GetNestedFakeGenerics(RuntimeTypeUtility.GetArrayBase(type)).Count() > 0)
            {
                currentParam["defaultValue"].value = null;
                return;
            }
            if (type == null)
            {
                currentParam["defaultValue"].value = null;
            }
            else if (ConversionUtility.CanConvert(currentParam["defaultValue"].value, type, true))
            {
                currentParam["defaultValue"].value = ConversionUtility.Convert(currentParam["defaultValue"].value, type);
            }
            else
            {
                currentParam["defaultValue"].value = type.PseudoDefault();
            }

            currentParam["defaultValue"].InferOwnerFromParent();
            UpdatePreview();
            GraphWindow.active?.context?.DescribeAndAnalyze();
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
                        if (listOfMethods[index].parentAsset == null) listOfMethods[index].parentAsset = Target;
                        listOfMethods[index].opened = HUMEditor.Foldout(listOfMethods[index].opened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                        {
                            var context = listOfMethods[index].GetReference().AsReference().Context();
                            context?.BeginEdit();
                            HUMEditor.Changed(() =>
                            {
                                listOfMethods[index].methodName = GUILayout.TextField(listOfMethods[index].methodName);
                            }, () =>
                            {
                                listOfMethods[index].name = listOfMethods[index].methodName.LegalMemberName();
                                var functionUnit = listOfMethods[index].graph.units[0] as FunctionNode;
                                functionUnit.Define();
                                functionUnit.Describe();
                                UpdatePreview();
                            });
                            context?.EndEdit();
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
                                    UpdatePreview();
                                }, listOfMethods[index]);

                                if (index > 0)
                                {
                                    menu.AddItem(new GUIContent("Move Up"), false, (obj) =>
                                    {
                                        Undo.RegisterCompleteObjectUndo(listOfMethods[index], "Move method Up");
                                        MoveItemUp(listOfMethods, index);
                                        UpdatePreview();
                                    }, listOfMethods[index]);
                                }

                                if (index < methods.Count - 1)
                                {
                                    menu.AddItem(new GUIContent("Move Down"), false, (obj) =>
                                    {
                                        Undo.RegisterCompleteObjectUndo(listOfMethods[index], "Move method Down");
                                        MoveItemDown(listOfMethods, index);
                                        UpdatePreview();
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

                                GUILayout.BeginHorizontal();
                                GUILayout.Label("Returns");
                                var lastRect = GUILayoutUtility.GetLastRect();
                                if (TypeBuilderWindow.Button(methods[index]["returnType"].value as Type))
                                {
                                    var types = new List<FakeGenericParameterType>();
                                    foreach (var generic in listOfMethods[index].genericParameters)
                                    {
                                        types.Add(new FakeGenericParameterType(generic.name, listOfMethods[index].genericParameters.IndexOf(generic), generic.typeParameterConstraints, generic.baseTypeConstraint, generic.constraints?.ToList()));
                                    }
                                    TypeBuilderWindow.ShowWindow(lastRect, methods[index]["returnType"], true, types, () =>
                                    {
                                        Undo.RegisterCompleteObjectUndo(listOfMethods[index], "Changed Method Return Type");
                                        UpdatePreview();
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
                                listOfMethods[index].genericsOpened = HUMEditor.Foldout(listOfMethods[index].genericsOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                                {
                                    HUMEditor.Image(typeof(Type).Icon()[IconSize.Small], 16, 16);

                                    GUILayout.Label(new GUIContent("Generics", "This feature is experimental and may cause issues in your graphs or behave unpredictably."));
                                }, () =>
                                {
                                    HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                                    {
                                        for (int gIndex = 0; gIndex < listOfMethods[index].genericParameterCount; gIndex++)
                                        {
                                            listOfMethods[index].genericParameters[gIndex].isOpen = HUMEditor.Foldout(listOfMethods[index].genericParameters[gIndex].isOpen, HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2, () =>
                                            {
                                                EditorGUI.BeginChangeCheck();
                                                var previousName = listOfMethods[index].genericParameters[gIndex].name;
                                                var name = GUILayout.TextField(listOfMethods[index].genericParameters[gIndex].name).GenericName(gIndex);
                                                listOfMethods[index].genericParameters[gIndex].SetName(name);
                                                if (EditorGUI.EndChangeCheck() && RuntimeTypeUtility.GetInstanceOfGenericFromMethod(listOfMethods[index], previousName) != null)
                                                {
                                                    var param = RuntimeTypeUtility.GetInstanceOfGenericFromMethod(listOfMethods[index], previousName);
                                                    param?.ChangeName(name);
                                                }

                                                if (GUILayout.Button("...", GUILayout.Width(19)))
                                                {
                                                    GenericMenu menu = new GenericMenu();
                                                    menu.AddItem(new GUIContent("Delete"), false, (obj) =>
                                                    {
                                                        GenericParameter paramToRemove = obj as GenericParameter;
                                                        Undo.RegisterCompleteObjectUndo(target, $"Deleted {paramToRemove} generic parameter");
                                                        listOfMethods[index].genericParameters.Remove(paramToRemove);
                                                        UpdatePreview();
                                                    }, listOfMethods[index].genericParameters[gIndex]);

                                                    if (gIndex > 0)
                                                    {
                                                        menu.AddItem(new GUIContent("Move Up"), false, (obj) =>
                                                        {
                                                            var paramIndex = listOfMethods[index].genericParameters.IndexOf(obj as GenericParameter);
                                                            if (paramIndex > 0)
                                                            {
                                                                Undo.RegisterCompleteObjectUndo(target, $"Moved {(obj as GenericParameter).name} generic parameter up");
                                                                var temp = listOfMethods[index].genericParameters[paramIndex];
                                                                listOfMethods[index].genericParameters[paramIndex] = listOfMethods[index].genericParameters[paramIndex - 1];
                                                                listOfMethods[index].genericParameters[paramIndex - 1] = temp;
                                                                UpdatePreview();
                                                            }
                                                        }, listOfMethods[index].genericParameters[gIndex]);
                                                    }

                                                    if (gIndex < listOfMethods[index].genericParameters.Count - 1)
                                                    {
                                                        menu.AddItem(new GUIContent("Move Down"), false, (obj) =>
                                                        {
                                                            var paramIndex = listOfMethods[index].genericParameters.IndexOf(obj as GenericParameter);
                                                            if (paramIndex < listOfMethods[index].genericParameters.Count - 1)
                                                            {
                                                                Undo.RegisterCompleteObjectUndo(target, $"Moved {(obj as GenericParameter).name} generic parameter down");
                                                                var temp = listOfMethods[index].genericParameters[paramIndex];
                                                                listOfMethods[index].genericParameters[paramIndex] = listOfMethods[index].genericParameters[paramIndex + 1];
                                                                listOfMethods[index].genericParameters[paramIndex + 1] = temp;
                                                                UpdatePreview();
                                                            }
                                                        }, listOfMethods[index].genericParameters[gIndex]);
                                                    }
                                                    menu.ShowAsContext();
                                                }
                                            }, () =>
                                            {
                                                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                                                {
                                                    GUILayout.BeginHorizontal();
                                                    GUILayout.Label("Base Type Constraint");
                                                    var lastRect = GUILayoutUtility.GetLastRect();
                                                    if (TypeBuilderWindow.Button(listOfMethods[index].genericParameters[gIndex].baseTypeConstraint))
                                                    {
                                                        var iv = gIndex;
                                                        var settingAssemblyTypesLookup = Codebase.settingsAssembliesTypes.ToArray();
                                                        var baseTypeLookup = settingAssemblyTypesLookup.Where(t => t != null && !NameUtility.TypeHasSpecialName(t) && RuntimeTypeUtility.IsValidGenericConstraint(t) && !t.IsInterface)
                                                            .ToArray();
                                                        TypeBuilderWindow.ShowWindow(lastRect, methods[index]["genericParameters"][gIndex]["baseTypeConstraint"], true, baseTypeLookup, () =>
                                                        {
                                                            Undo.RegisterCompleteObjectUndo(listOfMethods[index], "Changed Generic Base Type Constraint");
                                                            UpdatePreview();
                                                        }, (t) =>
                                                        {
                                                            var fakeGeneric = RuntimeTypeUtility.GetInstanceOfGenericFromMethod(listOfMethods[index], listOfMethods[index].genericParameters[iv].name);
                                                            fakeGeneric.ChangeBaseTypeConstraint(t);
                                                            if (GraphWindow.active != null && GraphWindow.active.context != null)
                                                                GraphWindow.active.context.DescribeAndAnalyze();
                                                        });
                                                    }
                                                    GUILayout.EndHorizontal();
                                                    GUILayout.BeginHorizontal();
                                                    GUILayout.Label("Type Parameter Constraints");
                                                    var constraints = listOfMethods[index].genericParameters[gIndex].typeParameterConstraints;
                                                    listOfMethods[index].genericParameters[gIndex].typeParameterConstraints = (TypeParameterConstraints)EditorGUILayout.EnumFlagsField(constraints);
                                                    GUILayout.EndHorizontal();
                                                    listOfMethods[index].genericParameters[gIndex].interfaceConstraintsOpen = HUMEditor.Foldout(listOfMethods[index].genericParameters[gIndex].interfaceConstraintsOpen, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                                                    {
                                                        HUMEditor.Image(typeof(IAction).Icon()[IconSize.Small], 16, 16);
                                                        GUILayout.Label("Interface Constraints");
                                                    }, () =>
                                                    {
                                                        HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                                                        {
                                                            var interfaceConstraints = listOfMethods[index].genericParameters[gIndex].constraints ?? new Type[0];
                                                            for (int _i = 0; _i < interfaceConstraints.Length; _i++)
                                                            {
                                                                GUILayout.BeginHorizontal();
                                                                GUILayout.Label("Constraint" + _i);
                                                                var lastRect = GUILayoutUtility.GetLastRect();
                                                                if (TypeBuilderWindow.Button(interfaceConstraints[_i]))
                                                                {
                                                                    TypeBuilderWindow.ShowWindow(lastRect, methods[index]["genericParameters"][gIndex]["constraints"][_i], true, AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => t.IsInterface && t.IsPublic).ToArray(), () =>
                                                                    {
                                                                        Undo.RegisterCompleteObjectUndo(listOfMethods[index], "Changed Generic Interface Type Constraint");
                                                                        UpdatePreview();
                                                                    });
                                                                }
                                                                if (GUILayout.Button("...", GUILayout.Width(19)))
                                                                {
                                                                    var iv = gIndex;
                                                                    GenericMenu menu = new GenericMenu();
                                                                    menu.AddItem(new GUIContent("Delete"), false, (obj) =>
                                                                    {
                                                                        Type paramToRemove = obj as Type;
                                                                        Undo.RegisterCompleteObjectUndo(target, $"Deleted {paramToRemove} interface constraint");
                                                                        var list = listOfMethods[index].genericParameters[iv].constraints.ToList();
                                                                        list.Remove(paramToRemove);
                                                                        listOfMethods[index].genericParameters[iv].constraints = list.ToArray();
                                                                        UpdatePreview();
                                                                    }, listOfMethods[index].genericParameters[gIndex].constraints[_i]);

                                                                    if (_i > 0)
                                                                    {
                                                                        menu.AddItem(new GUIContent("Move Up"), false, (obj) =>
                                                                        {
                                                                            var paramIndex = Array.IndexOf(listOfMethods[index].genericParameters[iv].constraints, obj as Type);
                                                                            if (paramIndex > 0)
                                                                            {
                                                                                Undo.RegisterCompleteObjectUndo(target, $"Moved {(obj as Type).As().CSharpName(false, false, false)} interface constraint up");
                                                                                var temp = listOfMethods[index].genericParameters[iv].constraints[paramIndex];
                                                                                var list = listOfMethods[index].genericParameters[iv].constraints.ToList();
                                                                                list[paramIndex] = list[paramIndex - 1];
                                                                                list[paramIndex - 1] = temp;
                                                                                listOfMethods[index].genericParameters[iv].constraints = list.ToArray();
                                                                                UpdatePreview();
                                                                            }
                                                                        }, listOfMethods[index].genericParameters[gIndex].constraints[_i]);
                                                                    }

                                                                    if (_i < listOfMethods[index].genericParameters[gIndex].constraints.Length - 1)
                                                                    {
                                                                        menu.AddItem(new GUIContent("Move Down"), false, (obj) =>
                                                                        {
                                                                            var paramIndex = Array.IndexOf(listOfMethods[index].genericParameters[iv].constraints, obj as Type);
                                                                            if (paramIndex < listOfMethods[index].genericParameters[iv].constraints.Length - 1)
                                                                            {
                                                                                Undo.RegisterCompleteObjectUndo(target, $"Moved {(obj as Type).As().CSharpName(false, false, false)} interface constraint down");
                                                                                var temp = listOfMethods[index].genericParameters[iv].constraints[paramIndex];
                                                                                var list = listOfMethods[index].genericParameters[iv].constraints.ToList();
                                                                                list[paramIndex] = list[paramIndex + 1];
                                                                                list[paramIndex + 1] = temp;
                                                                                listOfMethods[index].genericParameters[iv].constraints = list.ToArray();
                                                                                UpdatePreview();
                                                                            }
                                                                        }, listOfMethods[index].genericParameters[gIndex].constraints[_i]);
                                                                    }
                                                                    menu.ShowAsContext();
                                                                }
                                                                GUILayout.EndHorizontal();
                                                            }
                                                            if (GUILayout.Button("+ Add Constraint"))
                                                            {
                                                                Undo.RegisterCompleteObjectUndo(listOfMethods[index], "Added Interface Constraint");
                                                                if (listOfMethods[index].genericParameters[gIndex].constraints == null)
                                                                {
                                                                    listOfMethods[index].genericParameters[gIndex].constraints = new Type[0];
                                                                }
                                                                var list = listOfMethods[index].genericParameters[gIndex].constraints.ToList();
                                                                list.Add(typeof(ICollection));
                                                                listOfMethods[index].genericParameters[gIndex].constraints = list.ToArray();
                                                                UpdatePreview();
                                                            }
                                                        });
                                                    });
                                                });
                                            });
                                        }
                                        if (GUILayout.Button("+ Add Generic"))
                                        {
                                            Undo.RegisterCompleteObjectUndo(listOfMethods[index], "Added Generic");
                                            var name = $"T{listOfMethods[index].genericParameterCount}";
                                            var parameter = GenericParameter.Create(typeof(object), name);
                                            parameter.baseTypeConstraint = typeof(object);
                                            listOfMethods[index].genericParameters.Add(parameter);
                                            UpdatePreview();
                                        }
                                    });
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
                        Undo.RegisterCompleteObjectUndo(Target, "Added Method");

                        var declaration = CreateInstance<TMethodDeclaration>();

                        declaration.parentAsset = Target;

                        declaration.hideFlags = HideFlags.HideInHierarchy;

                        AssetDatabase.AddObjectToAsset(declaration, Target);
                        Undo.RegisterCreatedObjectUndo(declaration, "Created Method");

                        listOfMethods.Add(declaration);

                        var functionUnit = new FunctionNode(FunctionType.Method);
                        UpdatePreview();
                        functionUnit.methodDeclaration = declaration;
                        declaration.graph.units.Add(functionUnit);

                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
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
                        if (listOfVariables[index].getter.parentAsset == null || listOfVariables[index].setter.parentAsset == null)
                        {
                            listOfVariables[index].getter.parentAsset = Target;
                            listOfVariables[index].setter.parentAsset = Target;
                        }
                        listOfVariables[index].opened = HUMEditor.Foldout(listOfVariables[index].opened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                        {
                            if (GraphWindow.active != null && GraphWindow.active.context != null)
                                GraphWindow.active.context.BeginEdit();

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
                                UpdatePreview();
                            }
                            if (GraphWindow.active != null && GraphWindow.active.context != null)
                                GraphWindow.active.context.EndEdit();


                            if (GUILayout.Button("...", GUILayout.Width(19)))
                            {
                                GenericMenu menu = new GenericMenu();
                                menu.AddItem(new GUIContent("Delete"), false, (obj) =>
                                {
                                    Undo.RegisterCompleteObjectUndo(Target, "Deleted variable");
                                    variables.Remove(obj as TFieldDeclaration);
                                    AssetDatabase.RemoveObjectFromAsset((obj as TFieldDeclaration).setter);
                                    AssetDatabase.RemoveObjectFromAsset((obj as TFieldDeclaration).getter);
                                    AssetDatabase.RemoveObjectFromAsset(obj as TFieldDeclaration);
                                    UpdatePreview();
                                }, listOfVariables[index]);

                                if (index > 0)
                                {
                                    menu.AddItem(new GUIContent("Move Up"), false, (obj) =>
                                    {
                                        Undo.RegisterCompleteObjectUndo(Target, "Moved Variable Up");
                                        MoveItemUp(listOfVariables, index);
                                        UpdatePreview();
                                    }, listOfVariables[index]);
                                }

                                if (index < listOfVariables.Count - 1)
                                {
                                    menu.AddItem(new GUIContent("Move Down"), false, (obj) =>
                                    {
                                        Undo.RegisterCompleteObjectUndo(Target, "Moved Variable Down");
                                        MoveItemDown(listOfVariables, index);
                                        UpdatePreview();
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
                                    UpdatePreview();
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
                                        UpdatePreview();
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
                                        UpdatePreview();
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
                                    TypeBuilderWindow.ShowWindow(lastRect, variables[index]["type"], true, new Type[0], () =>
                                    {
                                        Undo.RegisterCompleteObjectUndo(listOfVariables[index], "Changed Variable Type");
                                        UpdatePreview();
                                    }, (t) => listOfVariables[index].OnChanged?.Invoke());
                                }
                                GUILayout.EndHorizontal();

                                if (target is ClassAsset)
                                {
                                    if (!listOfVariables[index].isProperty || (listOfVariables[index].get && !(listOfVariables[index].getter.graph.units[0] as FunctionNode).invoke.hasValidConnection) ||
                                    (listOfVariables[index].set && !(listOfVariables[index].setter.graph.units[0] as FunctionNode).invoke.hasValidConnection))
                                    {
                                        if (typeChangedLookup.TryGetValue(variables[index]["type"], out var type))
                                        {
                                            Type currentType = variables[index]["type"].value as Type;
                                            if (type != currentType)
                                            {
                                                UpdateDefaultValueType(currentType, variables[index]);
                                                typeChangedLookup[variables[index]["type"]] = currentType;
                                            }
                                        }
                                        else
                                        {
                                            typeChangedLookup[variables[index]["type"]] = (Type)variables[index]["type"].value;
                                        }

                                        var inspector = variables[index]["defaultValue"].Inspector();
                                        typeof(SystemObjectInspector).GetField("inspector", BindingFlags.Instance | BindingFlags.NonPublic).SetValueOptimized(inspector, ValueInspectorType.Instantiate(true, inspector));
                                        Inspector.BeginBlock(variables[index]["defaultValue"], new Rect(10, 10, 10, 10));
                                        inspector.DrawLayout(new GUIContent("Value               "));
                                        if (Inspector.EndBlock(variables[index]["defaultValue"]))
                                        {
                                            Undo.RegisterCompleteObjectUndo(listOfVariables[index], "Changed Variable Default Value");
                                            UpdatePreview();
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
                                                    UpdatePreview();
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
                                                    UpdatePreview();
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

                        declaration.parentAsset = Target;

                        var getter = CreateInstance<PropertyGetterMacro>();
                        Undo.RegisterCreatedObjectUndo(getter, "Created Getter");
                        var setter = CreateInstance<PropertySetterMacro>();
                        Undo.RegisterCreatedObjectUndo(setter, "Created Setter");
                        getter.parentAsset = Target;
                        setter.parentAsset = Target;
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

                        UpdatePreview();
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                });
            });
        }

        private void MoveItemUp<T>(List<T> list, int index)
        {
            if (index <= 0 || index >= list.Count) return;
            T item = list[index];
            list.RemoveAt(index);
            list.Insert(index - 1, item);
        }

        private void MoveItemDown<T>(List<T> list, int index)
        {
            if (index < 0 || index >= list.Count - 1) return;
            T item = list[index];
            list.RemoveAt(index);
            list.Insert(index + 1, item);
        }

    }
}