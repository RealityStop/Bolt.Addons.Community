using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Codice.CM.SEIDInfo;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Utility;
using UnityEditor;
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
        private int fieldsCount;
        private Type[] interfaceAttributeTypes = new Type[] { };
        private Type[] methodAttributeTypes = new Type[] { };
        private int methodsCount;
        private Type[] propertyAttributeTypes = new Type[] { };
        private Type[] structAttributeTypes = new Type[] { };

        private enum AttributeUsageType
        {
            Class,
            Struct,
            Enum,
            Interface,
            Field,
            Property,
            Method
        }

        private Color boxBackground => HUMColor.Grey(0.15f);

        protected override void BeforePreview()
        {
            Constructors();
            GUILayout.Space(4);
            Variables();
            GUILayout.Space(4);
            Methods();
            // if (typeof(TMemberTypeAsset) == typeof(UnitAsset))
            // {
            //     GUILayout.Space(4);
            //     RequiredInfo();
            // }
        }

        protected virtual Texture2D DefaultIcon() { return PathUtil.Load("object_32", CommunityEditorPath.Code).Single(); }

        protected override void OnEnable()
        {
            base.OnEnable();

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
                // if (typeof(TMemberTypeAsset) != typeof(UnitAsset))
                // {
                    HUMEditor.Vertical().Box(
                    HUMEditorColor.DefaultEditorBackground, Color.black,
                    new RectOffset(7, 7, 7, 7),
                    new RectOffset(1, 1, 1, 1),
                    () =>
                    {
                        Target.icon = (Texture2D)EditorGUILayout.ObjectField(
                        GUIContent.none,
                        Target.icon,
                        typeof(Texture2D),
                        false,
                        GUILayout.Width(32),
                        GUILayout.Height(32));
                    }, false, false);
                // }
                // else
                // {
                //     typeIcon.Block(() =>
                //     {
                //         float labelHeight = 15f;
                //         var TypeIconLabel = new GUIContent(" TypeIcon ", "This is the icon of the Unit if you want the default icon use the Type \"Void\"");
                //         GUILayout.Label(TypeIconLabel, GUILayout.Height(labelHeight));
                //         var labelPosition = GUILayoutUtility.GetLastRect();

                //         Rect typeFieldPosition = new Rect(
                //         labelPosition.x + 1,
                //         labelPosition.y + 20,
                //         labelPosition.width,
                //         EditorGUIUtility.singleLineHeight
                //         );

                //         Type[] types = Codebase.settingsAssemblies
                //         .SelectMany(a => a.GetTypes().Where(type => !type.IsAbstract && !type.IsGenericTypeDefinition))
                //         .ToArray();

                //         (typeIcon.value as SystemType).type = LudiqGUI.TypeField(
                //         typeFieldPosition,
                //         GUIContent.none,
                //         (typeIcon.value as SystemType).type,
                //         () =>
                //         {
                //             return new TypeOptionTree(types);
                //         },
                //         new GUIContent("null")
                //         );
                //     },
                //     () =>
                //     {
                //         shouldUpdate = true;
                //     }, true);
                // }

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
                        for (int attrIndex = 0; attrIndex < Target.attributes.Count; attrIndex++)
                        {
                            var attributeMeta = attributes[attrIndex]["attributeType"];
                            var attribute = Target.attributes[attrIndex];

                            attribute.opened = HUMEditor.Foldout(
                                attribute.opened,
                                HUMEditorColor.DefaultEditorBackground.Darken(0f),
                                Color.black,
                                1,
                                () =>
                                {
                                    attributeMeta.Block(() =>
                                    {
                                        if (Target is not StructAsset or InterfaceAsset)
                                        {
                                            AttributeTypeField(attribute, AttributeUsageType.Class);
                                        }
                                        else if (Target is StructAsset)
                                        {
                                            AttributeTypeField(attribute, AttributeUsageType.Struct);
                                        }
                                        // else if (Target is InterfaceAsset)
                                        // {
                                        //     AttributeTypeField(attribute, AttributeUsageType.Interface);
                                        // }
                                    },
                                    () =>
                                    {
                                        attribute.parameters.Clear();
                                        attribute.constructor = 0;
                                        shouldUpdate = true;
                                    },
                                    true
                                );

                                    if (GUILayout.Button("...", GUILayout.Width(19)))
                                    {
                                        GenericMenu menu = new GenericMenu();
                                        menu.AddItem(new GUIContent("Delete"), false, (obj) =>
                                        {
                                            AttributeDeclaration attrToRemove = obj as AttributeDeclaration;
                                            Target.attributes.Remove(attrToRemove);

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

                                        List<AttributeDeclaration> attributeList = Target.attributes;

                                        if (attrIndex > 0)
                                        {
                                            menu.AddItem(new GUIContent("Move Up"), false, (obj) =>
                                            {
                                                var attributeIndex = attributeList.IndexOf(obj as AttributeDeclaration);
                                                if (attributeIndex > 0)
                                                {
                                                    var temp = attributeList[attributeIndex];
                                                    attributeList[attributeIndex] = attributeList[attributeIndex - 1];
                                                    attributeList[attributeIndex - 1] = temp;
                                                }
                                            }, attribute);
                                        }

                                        if (attrIndex < Target.attributes.Count - 1)
                                        {
                                            menu.AddItem(new GUIContent("Move Down"), false, (obj) =>
                                            {
                                                var attributeIndex = attributeList.IndexOf(obj as AttributeDeclaration);
                                                if (attributeIndex < attributeList.Count - 1)
                                                {
                                                    var temp = attributeList[attributeIndex];
                                                    attributeList[attributeIndex] = attributeList[attributeIndex + 1];
                                                    attributeList[attributeIndex + 1] = temp;
                                                }
                                            }, attribute);
                                        }
                                        menu.ShowAsContext();
                                    }
                                },
                            () =>
                            {
                                var attributeParamMeta = attributes[attrIndex]["parameters"];

                                string[] constructorNames = attribute.GetAttributeType().GetConstructors()
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
                                .ToArray();

                                if (attribute.constructor != attribute.selectedconstructor)
                                {
                                    attribute.parameters.Clear();
                                }

                                attribute.selectedconstructor = attribute.constructor;

                                attribute.constructor = EditorGUILayout.Popup(
                                    "Select Type : ",
                                    attribute.constructor,
                                    constructorNames
                                );

                                var selectedConstructor = attribute.GetAttributeType().GetConstructors()
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

                                        if (!isParamsParameter && Param.defaultValue is not IList)
                                        {
                                            Inspector.BeginBlock(
                                                attributeParamMeta[paramIndex]["defaultValue"],
                                                new Rect()
                                            );
                                            LudiqGUI.InspectorLayout(
                                                attributeParamMeta[paramIndex]["defaultValue"].Cast(parameter.ParameterType),
                                                new GUIContent(parameter.Name + ":")
                                            );
                                            if (Inspector.EndBlock(attributeParamMeta[paramIndex]["defaultValue"]))
                                            {
                                                shouldUpdate = true;
                                            }
                                        }
                                        else
                                        {
                                            Inspector.BeginBlock(
                                                attributeParamMeta[paramIndex]["defaultValue"],
                                                new Rect()
                                            );
                                            attributeParamMeta[paramIndex]["defaultValue"].value = Param.defaultValue;
                                            LudiqGUI.InspectorLayout(
                                                attributeParamMeta[paramIndex]["defaultValue"].Cast(parameter.ParameterType),
                                                new GUIContent(parameter.Name + ":")
                                            );
                                            if (Inspector.EndBlock(attributeParamMeta[paramIndex]["defaultValue"]))
                                            {
                                                shouldUpdate = true;
                                            }
                                        }
                                        paramIndex++;
                                    }

                                    paramIndex = 0;
                                }
                                if (Target.attributes.Count > 0) GUILayout.Space(4);
                            });
                        }
                        if (GUILayout.Button("+ Add Attributes"))
                        {
                            var attribute = new AttributeDeclaration();
                            attribute.SetType(typeof(SerializeField));
                            Target.attributes.Add(attribute);
                        }
                    });
            });
        }

        protected override void OptionsGUI()
        {
            Target.serialized = EditorGUILayout.ToggleLeft("Serialized", Target.serialized);
            Target.inspectable = EditorGUILayout.ToggleLeft("Inspectable", Target.inspectable);
            Target.includeInSettings = EditorGUILayout.ToggleLeft("Include In Settings", Target.includeInSettings);
            Target.definedEvent = EditorGUILayout.ToggleLeft("Flag for Defined Event Filtering", Target.definedEvent);
            OnExtendedOptionsGUI();
        }

        private void AttributeTypeField(AttributeDeclaration attribute, AttributeUsageType usage)
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
            }

            attribute.SetType(LudiqGUI.TypeField(position, GUIContent.none, attribute.GetAttributeType(), () =>
            {
                return new TypeOptionTree(types);
            }));
        }

        private void CacheConstrainedAttributes()
        {
            attributeTypes = typeof(Attribute).Get().Derived();
            classAttributeTypes = attributeTypes.Where((attr) => { return attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.Class || attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.All; }).ToArray();
            structAttributeTypes = attributeTypes.Where((attr) => { return attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.Struct || attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.All; }).ToArray();
            enumAttributeTypes = attributeTypes.Where((attr) => { return attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.Enum || attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.All; }).ToArray();
            interfaceAttributeTypes = attributeTypes.Where((attr) => { return attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.Interface || attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.All; }).ToArray();
            fieldAttributeTypes = attributeTypes.Where((attr) =>
            {
                var attributeUsage = attr.GetAttribute<AttributeUsageAttribute>();
                return attributeUsage != null &&
                       (attributeUsage.ValidOn == AttributeTargets.Field || attributeUsage.ValidOn == AttributeTargets.Property ||
                        typeof(Attribute).IsAssignableFrom(attr) || attr == typeof(DoNotSerializeAttribute));
            }).ToArray(); propertyAttributeTypes = attributeTypes.Where((attr) => { return attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.Property || attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.All; }).ToArray();
            methodAttributeTypes = attributeTypes.Where((attr) => { return attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.Method || attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.All; }).ToArray();
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

                            if (GUILayout.Button("Edit", GUILayout.Width(60)))
                            {
                                GraphWindow.OpenActive(listOfConstructors[index].GetReference() as GraphReference);
                                var listOfVariables = variables.value as List<TFieldDeclaration>;
                                var listOfGraphVars = listOfConstructors[index].graph.variables;
                                foreach (var variable in listOfVariables)
                                {
                                    listOfConstructors[index].graph.variables.Set(variable.name, null);
                                    var matchingGraphVar = listOfGraphVars.FirstOrDefault(graphvar => graphvar.name == variable.name);

                                    if (matchingGraphVar != null)
                                    {
                                        SerializableType type = matchingGraphVar.typeHandle;
                                        type.Identification = variable.type.AssemblyQualifiedName;
                                        matchingGraphVar.typeHandle = type;
                                    }
                                }
                            }

                            if (GUILayout.Button("...", GUILayout.Width(19)))
                            {
                                GenericMenu menu = new GenericMenu();
                                menu.AddItem(new GUIContent("Delete"), false, (obj) =>
                                {
                                    constructors.Remove(obj as TConstructorDeclaration);
                                    AssetDatabase.RemoveObjectFromAsset(obj as TConstructorDeclaration);
                                }, listOfConstructors[index]);

                                if (index > 0)
                                {
                                    menu.AddItem(new GUIContent("Move Up"), false, (obj) =>
                                    {
                                        MoveItemUp(listOfConstructors, index);
                                    }, listOfConstructors[index]);
                                }

                                if (index < methods.Count - 1)
                                {
                                    menu.AddItem(new GUIContent("Move Down"), false, (obj) =>
                                    {
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
                                //HUMEditor.Image(PathUtil.Load("scope_32", CommunityEditorPath.Code).Single(), 16, 16);
                                listOfConstructors[index].scope = (AccessModifier)EditorGUILayout.EnumPopup("Scope", listOfConstructors[index].scope);
                                EditorGUILayout.EndHorizontal();
                                GUILayout.Space(4);

                                listOfConstructors[index].parametersOpened = HUMEditor.Foldout(listOfConstructors[index].parametersOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                                {
                                    GUILayout.Label("Parameters");
                                }, () =>
                                {
                                    var paramMeta = constructors[index]["parameters"];
                                    Inspector.BeginBlock(paramMeta, new Rect());
                                    LudiqGUI.InspectorLayout(paramMeta, GUIContent.none);
                                    if (Inspector.EndBlock(paramMeta))
                                    {
                                        shouldUpdate = true;
                                        var functionUnit = (listOfConstructors[index].graph.units[0] as FunctionNode);
                                        functionUnit.Define();
                                        functionUnit.Describe();
                                    }
                                });

                            }, true, false);
                        });

                        GUILayout.Space(4);
                    }

                    if (GUILayout.Button("+ Add Constructor"))
                    {
                        var declaration = CreateInstance<TConstructorDeclaration>();
                        if (Target.GetType() == typeof(ClassAsset)) declaration.classAsset = Target as ClassAsset;
                        if (Target.GetType() == typeof(StructAsset)) declaration.structAsset = Target as StructAsset;
                        declaration.hideFlags = HideFlags.HideInHierarchy;
                        AssetDatabase.AddObjectToAsset(declaration, Target);
                        listOfConstructors.Add(declaration);
                        var functionUnit = new FunctionNode(FunctionType.Constructor);
                        functionUnit.constructorDeclaration = declaration;
                        declaration.graph.units.Add(functionUnit);
                        var listOfVariables = variables.value as List<TFieldDeclaration>;
                        var listOfGraphVars = declaration.graph.variables;
                        foreach (var variable in listOfVariables)
                        {
                            declaration.graph.variables.Set(variable.name, null);

                            var matchingGraphVar = listOfGraphVars.FirstOrDefault(graphvar => graphvar.name == variable.name);

                            if (matchingGraphVar != null)
                            {
                                SerializableType type = matchingGraphVar.typeHandle;
                                type.Identification = variable.type.AssemblyQualifiedName;
                                matchingGraphVar.typeHandle = type;
                            }
                        }

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

        private object GetDefaultParameterValue(Type parameterType)
        {
            if (parameterType == typeof(string))
            {
                return " ";
            }
            else if (parameterType == typeof(int))
            {
                return 0;
            }
            else if (parameterType == typeof(bool))
            {
                return false;
            }
            else if (parameterType == typeof(Type))
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
                            HUMEditor.Changed(() =>
                            {
                                listOfMethods[index].methodName = GUILayout.TextField(listOfMethods[index].methodName);
                            }, () =>
                            {
                                listOfMethods[index].name = listOfMethods[index].methodName.LegalMemberName();
                                var funcionUnit = (listOfMethods[index].graph.units[0] as FunctionNode);
                                funcionUnit.Define();
                                funcionUnit.Describe();
                            });

                            if (GUILayout.Button("Edit", GUILayout.Width(60)))
                            {
                                GraphWindow.OpenActive(listOfMethods[index].GetReference() as GraphReference);
                                var listOfVariables = variables.value as List<TFieldDeclaration>;
                                var listOfGraphVars = listOfMethods[index].graph.variables;
                                foreach (var variable in listOfVariables)
                                {
                                    listOfMethods[index].graph.variables.Set(variable.name, variable.value ?? null);
                                    var matchingGraphVar = listOfGraphVars.FirstOrDefault(graphvar => graphvar.name == variable.name);

                                    if (matchingGraphVar != null)
                                    {
                                        SerializableType type = matchingGraphVar.typeHandle;
                                        type.Identification = variable.type.AssemblyQualifiedName;
                                        matchingGraphVar.typeHandle = type;
                                    }
                                }
                            }

                            if (GUILayout.Button("...", GUILayout.Width(19)))
                            {
                                GenericMenu menu = new GenericMenu();
                                menu.AddItem(new GUIContent("Delete"), false, (obj) =>
                                {
                                    methods.Remove(obj as TMethodDeclaration);
                                    AssetDatabase.RemoveObjectFromAsset(obj as TMethodDeclaration);
                                }, listOfMethods[index]);

                                if (index > 0)
                                {
                                    menu.AddItem(new GUIContent("Move Up"), false, (obj) =>
                                    {
                                        MoveItemUp(listOfMethods, index);
                                    }, listOfMethods[index]);
                                }

                                if (index < methods.Count - 1)
                                {
                                    menu.AddItem(new GUIContent("Move Down"), false, (obj) =>
                                    {
                                        MoveItemDown(listOfMethods, index);
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
                                listOfMethods[index].scope = (AccessModifier)EditorGUILayout.EnumPopup("Scope", listOfMethods[index].scope);
                                EditorGUILayout.EndHorizontal();
                                listOfMethods[index].modifier = (MethodModifier)EditorGUILayout.EnumPopup("Modifier", listOfMethods[index].modifier);
                                Inspector.BeginBlock(methods[index]["returnType"], new Rect());
                                LudiqGUI.InspectorLayout(methods[index]["returnType"], new GUIContent("Returns"));
                                if (Inspector.EndBlock(methods[index]["returnType"]))
                                {
                                    shouldUpdate = true;
                                }

                                GUILayout.Space(4);

                                listOfMethods[index].attributesOpened = HUMEditor.Foldout(listOfMethods[index].attributesOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                                {
                                    HUMEditor.Image(PathUtil.Load("attributes_16", CommunityEditorPath.Code).Single(), 16, 16);
                                    GUILayout.Label("Attributes");
                                }, () =>
                                {
                                    for (int attrIndex = 0; attrIndex < listOfMethods[index].attributes.Count; attrIndex++)
                                    {
                                        var attributeMeta = methods[index]["attributes"][attrIndex]["attributeType"];
                                        var attribute = listOfMethods[index].attributes[attrIndex];

                                        attribute.opened = HUMEditor.Foldout(attribute.opened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                                        {
                                            attributeMeta.Block(() =>
                                            {
                                                AttributeTypeField(attribute, AttributeUsageType.Method);
                                            }, () =>
                                            {
                                                attribute.parameters.Clear();
                                                attribute.constructor = 0;
                                                shouldUpdate = true;
                                            }, true);

                                            if (GUILayout.Button("...", GUILayout.Width(19)))
                                            {
                                                GenericMenu menu = new GenericMenu();
                                                menu.AddItem(new GUIContent("Delete"), false, (obj) =>
                                                {
                                                    AttributeDeclaration attrToRemove = obj as AttributeDeclaration;
                                                    listOfMethods[index].attributes.Remove(attrToRemove);

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

                                                List<AttributeDeclaration> attributeList = listOfMethods[index].attributes;

                                                if (attrIndex > 0)
                                                {
                                                    menu.AddItem(new GUIContent("Move Up"), false, (obj) =>
                                                    {
                                                        var attributeIndex = attributeList.IndexOf(obj as AttributeDeclaration);
                                                        if (attributeIndex > 0)
                                                        {
                                                            var temp = attributeList[attributeIndex];
                                                            attributeList[attributeIndex] = attributeList[attributeIndex - 1];
                                                            attributeList[attributeIndex - 1] = temp;
                                                        }
                                                    }, attribute);
                                                }

                                                if (attrIndex < listOfMethods[index].attributes.Count - 1)
                                                {
                                                    menu.AddItem(new GUIContent("Move Down"), false, (obj) =>
                                                    {
                                                        var attributeIndex = attributeList.IndexOf(obj as AttributeDeclaration);
                                                        if (attributeIndex < attributeList.Count - 1)
                                                        {
                                                            var temp = attributeList[attributeIndex];
                                                            attributeList[attributeIndex] = attributeList[attributeIndex + 1];
                                                            attributeList[attributeIndex + 1] = temp;
                                                        }
                                                    }, attribute);
                                                }
                                                menu.ShowAsContext();
                                            }
                                        }, () =>
                                        {
                                            var attributeParamMeta = methods[index]["attributes"][attrIndex]["parameters"];

                                            string[] constructorNames = attribute.GetAttributeType().GetConstructors()
                                                .Select(constructor =>
                                                {
                                                    string paramInfo = string.Join(", ", constructor.GetParameters()
                                                        .Select(param => $"{param.ParameterType.As().CSharpName(false, false, false)}"));
                                                    return $"{attribute.GetAttributeType().DisplayName()}({paramInfo})".RemoveHighlights().RemoveMarkdown();
                                                })
                                                .ToArray();

                                            if (attribute.constructor != attribute.selectedconstructor)
                                            {
                                                attribute.parameters.Clear();
                                            }

                                            attribute.selectedconstructor = attribute.constructor;

                                            attribute.constructor = EditorGUILayout.Popup("Select Type : ", attribute.constructor, constructorNames);

                                            var selectedConstructor = attribute.GetAttributeType().GetConstructors()[attribute.constructor];

                                            if (selectedConstructor != null)
                                            {
                                                var paramIndex = 0;

                                                foreach (var parameter in selectedConstructor.GetParameters())
                                                {
                                                    TypeParam Param = attribute.parameters.FirstOrDefault(param => param.name == parameter.Name);

                                                    if (Param == null)
                                                    {
                                                        attribute.AddParameter(parameter.Name, parameter.ParameterType, GetDefaultParameterValue(parameter.ParameterType));
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

                                                    if (!isParamsParameter && Param.defaultValue is not IList)
                                                    {
                                                        Inspector.BeginBlock(attributeParamMeta[paramIndex]["defaultValue"], new Rect());
                                                        LudiqGUI.InspectorLayout(attributeParamMeta[paramIndex]["defaultValue"].Cast(parameter.ParameterType), new GUIContent(parameter.Name + ":"));
                                                        if (Inspector.EndBlock(attributeParamMeta[paramIndex]["defaultValue"]))
                                                        {
                                                            shouldUpdate = true;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Inspector.BeginBlock(attributeParamMeta[paramIndex]["defaultValue"], new Rect());

                                                        attributeParamMeta[paramIndex]["defaultValue"].value = Param.defaultValue;

                                                        LudiqGUI.InspectorLayout(attributeParamMeta[paramIndex]["defaultValue"].Cast(parameter.ParameterType), new GUIContent(parameter.Name + ":"));
                                                        if (Inspector.EndBlock(attributeParamMeta[paramIndex]["defaultValue"]))
                                                        {
                                                            shouldUpdate = true;
                                                        }
                                                    }
                                                    paramIndex++;
                                                }

                                                paramIndex = 0;
                                            }
                                            if (listOfMethods[index].attributes.Count > 0) GUILayout.Space(4);
                                        });
                                    }
                                    if (GUILayout.Button("+ Add Attributes"))
                                    {
                                        var attribute = new AttributeDeclaration();
                                        attribute.SetType(typeof(SerializeField));
                                        listOfMethods[index].attributes.Add(attribute);
                                    }
                                });

                                GUILayout.Space(4);

                                listOfMethods[index].parametersOpened = HUMEditor.Foldout(listOfMethods[index].parametersOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                                {
                                    HUMEditor.Image(PathUtil.Load("parameters_16", CommunityEditorPath.Code).Single(), 16, 16, new RectOffset(), new RectOffset(4, 8, 4, 4));
                                    GUILayout.Label("Parameters");
                                }, () =>
                                {
                                    var paramMeta = methods[index]["parameters"];
                                    foreach (var param in paramMeta.value as List<TypeParam>)
                                    {
                                        if (param.defaultValue == null)
                                        {
                                            var value = param.GetDefaultValue();

                                            if (value != null)
                                            {
                                                param.defaultValue = value;
                                            }
                                        }
                                    }
                                    Inspector.BeginBlock(paramMeta, new Rect());
                                    LudiqGUI.InspectorLayout(paramMeta, GUIContent.none);
                                    if (Inspector.EndBlock(paramMeta))
                                    {
                                        shouldUpdate = true;
                                        var funcionUnit = (listOfMethods[index].graph.units[0] as FunctionNode);
                                        funcionUnit.Define();
                                        funcionUnit.Describe();
                                    }
                                });

                            }, true, false);
                        });

                        GUILayout.Space(4);
                    }

                    if (GUILayout.Button("+ Add Method"))
                    {
                        var declaration = CreateInstance<TMethodDeclaration>();
                        if (Target.GetType() == typeof(ClassAsset)) declaration.classAsset = Target as ClassAsset;
                        if (Target.GetType() == typeof(StructAsset)) declaration.structAsset = Target as StructAsset;
                        declaration.hideFlags = HideFlags.HideInHierarchy;
                        AssetDatabase.AddObjectToAsset(declaration, Target);
                        listOfMethods.Add(declaration);
                        var functionUnit = new FunctionNode(FunctionType.Method);
                        
                        functionUnit.methodDeclaration = declaration;
                        declaration.graph.units.Add(functionUnit);
                        
                        declaration.graph.units.Add(functionUnit);
                        var listOfVariables = variables.value as List<TFieldDeclaration>;
                        var listOfGraphVars = declaration.graph.variables;
                        foreach (var variable in listOfVariables)
                        {
                            declaration.graph.variables.Set(variable.name, (variable.value != null ? variable.value : null));
                            var matchingGraphVar = listOfGraphVars.FirstOrDefault(graphvar => graphvar.name == variable.name);

                            if (matchingGraphVar != null)
                            {
                                SerializableType type = matchingGraphVar.typeHandle;
                                type.Identification = variable.type.AssemblyQualifiedName;
                                matchingGraphVar.typeHandle = type;
                            }
                        }

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

                            HUMEditor.Changed(() =>
                            {
                                listOfVariables[index].FieldName = GUILayout.TextField(listOfVariables[index].name);
                                listOfVariables[index].name = listOfVariables[index].FieldName;
                            }, () =>
                            {
                                listOfVariables[index].name = listOfVariables[index].name.LegalMemberName();
                                var getterFunctionUnit = (listOfVariables[index].getter.graph.units[0] as FunctionNode);
                                var setterFunctionUnit = (listOfVariables[index].setter.graph.units[0] as FunctionNode);
                                listOfVariables[index].getter.name = listOfVariables[index].name + " Getter";
                                listOfVariables[index].setter.name = listOfVariables[index].name + " Setter";
                                getterFunctionUnit.Define();
                                getterFunctionUnit.Describe();
                                setterFunctionUnit.Define();
                                setterFunctionUnit.Describe();
                            });

                            if (GUILayout.Button("...", GUILayout.Width(19)))
                            {
                                GenericMenu menu = new GenericMenu();
                                menu.AddItem(new GUIContent("Delete"), false, (obj) =>
                                {
                                    variables.Remove(obj as TFieldDeclaration);
                                }, listOfVariables[index]);

                                if (index > 0)
                                {
                                    menu.AddItem(new GUIContent("Move Up"), false, (obj) =>
                                    {
                                        MoveItemUp(listOfVariables, index);
                                    }, listOfVariables[index]);
                                }

                                if (index < listOfVariables.Count - 1)
                                {
                                    menu.AddItem(new GUIContent("Move Down"), false, (obj) =>
                                    {
                                        MoveItemDown(listOfVariables, index);
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
                                listOfVariables[index].scope = (AccessModifier)EditorGUILayout.EnumPopup("Scope", listOfVariables[index].scope);
                                EditorGUILayout.EndHorizontal();
                                if (!listOfVariables[index].isProperty)
                                {
                                    listOfVariables[index].fieldModifier = (FieldModifier)EditorGUILayout.EnumPopup("Modifier", listOfVariables[index].fieldModifier);
                                }
                                else
                                {
                                    listOfVariables[index].propertyModifier = (PropertyModifier)EditorGUILayout.EnumPopup("Modifier", listOfVariables[index].propertyModifier);
                                }

                                Inspector.BeginBlock(variables[index]["type"], new Rect());
                                LudiqGUI.InspectorLayout(variables[index]["type"], new GUIContent("Type"));
                                if (Inspector.EndBlock(variables[index]["type"]))
                                {
                                    shouldUpdate = true;
                                }

                                GUILayout.Space(4);

                                listOfVariables[index].attributesOpened = HUMEditor.Foldout(listOfVariables[index].attributesOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                                {
                                    HUMEditor.Image(PathUtil.Load("attributes_16", CommunityEditorPath.Code).Single(), 16, 16);
                                    GUILayout.Label("Attributes");
                                }, () =>
                                {
                                    HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground, Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(1, 1, 0, 1), () =>
                                    {
                                        for (int attrIndex = 0; attrIndex < listOfVariables[index].attributes.Count; attrIndex++)
                                        {
                                            var attributeMeta = variables[index]["attributes"][attrIndex]["attributeType"];
                                            var attribute = listOfVariables[index].attributes[attrIndex];

                                            attribute.opened = HUMEditor.Foldout(attribute.opened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                                            {
                                                attributeMeta.Block(() =>
                                                {
                                                    AttributeTypeField(attribute, AttributeUsageType.Field);
                                                }, () =>
                                                {
                                                    attribute.parameters.Clear();
                                                    attribute.constructor = 0;
                                                    shouldUpdate = true;
                                                }, true);

                                                if (GUILayout.Button("...", GUILayout.Width(19)))
                                                {
                                                    GenericMenu menu = new GenericMenu();
                                                    menu.AddItem(new GUIContent("Delete"), false, (obj) =>
                                                    {
                                                        AttributeDeclaration attrToRemove = obj as AttributeDeclaration;
                                                        listOfVariables[index].attributes.Remove(attrToRemove);

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

                                                    List<AttributeDeclaration> attributeList = listOfVariables[index].attributes;

                                                    if (attrIndex > 0)
                                                    {
                                                        menu.AddItem(new GUIContent("Move Up"), false, (obj) =>
                                                        {
                                                            var attributeIndex = attributeList.IndexOf(obj as AttributeDeclaration);
                                                            if (attributeIndex > 0)
                                                            {
                                                                var temp = attributeList[attributeIndex];
                                                                attributeList[attributeIndex] = attributeList[attributeIndex - 1];
                                                                attributeList[attributeIndex - 1] = temp;
                                                            }
                                                        }, attribute);
                                                    }

                                                    if (attrIndex < listOfVariables[index].attributes.Count - 1)
                                                    {
                                                        menu.AddItem(new GUIContent("Move Down"), false, (obj) =>
                                                        {
                                                            var attributeIndex = attributeList.IndexOf(obj as AttributeDeclaration);
                                                            if (attributeIndex < attributeList.Count - 1)
                                                            {
                                                                var temp = attributeList[attributeIndex];
                                                                attributeList[attributeIndex] = attributeList[attributeIndex + 1];
                                                                attributeList[attributeIndex + 1] = temp;
                                                            }
                                                        }, attribute);
                                                    }
                                                    menu.ShowAsContext();
                                                }
                                            }, () =>
                                            {
                                                var attributeParamMeta = variables[index]["attributes"][attrIndex]["parameters"];

                                                string[] constructorNames = attribute.GetAttributeType().GetConstructors()
                                                .Where(constructor => constructor.GetParameters().All(param => param.ParameterType.HasInspector()))
                                                .Select(constructor =>
                                                {
                                                    string paramInfo = string.Join(", ", constructor.GetParameters()
                                                        .Select(param => $"{param.ParameterType.Name} {param.Name}"));
                                                    return $"{attribute.GetAttributeType().Name}({paramInfo})";
                                                })
                                                .ToArray();

                                                if (attribute.constructor != attribute.selectedconstructor)
                                                {
                                                    attribute.parameters.Clear();
                                                }

                                                attribute.selectedconstructor = attribute.constructor;

                                                attribute.constructor = EditorGUILayout.Popup("Select Type : ", attribute.constructor, constructorNames);

                                                var selectedConstructor = attribute.GetAttributeType().GetConstructors().Where(constructor => constructor.GetParameters().All(param => param.ParameterType.HasInspector())).ToList()[attribute.constructor];

                                                if (selectedConstructor != null)
                                                {
                                                    var paramIndex = 0;

                                                    foreach (var parameter in selectedConstructor.GetParameters())
                                                    {
                                                        TypeParam Param = attribute.parameters.FirstOrDefault(param => param.name == parameter.Name);

                                                        if (Param == null)
                                                        {
                                                            attribute.AddParameter(parameter.Name, parameter.ParameterType, GetDefaultParameterValue(parameter.ParameterType));
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

                                                        if (!isParamsParameter && Param.defaultValue is not IList)
                                                        {
                                                            Inspector.BeginBlock(attributeParamMeta[paramIndex]["defaultValue"], new Rect());
                                                            LudiqGUI.InspectorLayout(attributeParamMeta[paramIndex]["defaultValue"].Cast(parameter.ParameterType), new GUIContent(parameter.Name + ":"));
                                                            if (Inspector.EndBlock(attributeParamMeta[paramIndex]["defaultValue"]))
                                                            {
                                                                shouldUpdate = true;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Inspector.BeginBlock(attributeParamMeta[paramIndex]["defaultValue"], new Rect());
                                                            attributeParamMeta[paramIndex]["defaultValue"].value = Param.defaultValue;
                                                            LudiqGUI.InspectorLayout(attributeParamMeta[paramIndex]["defaultValue"].Cast(parameter.ParameterType), new GUIContent(parameter.Name + ":"));
                                                            if (Inspector.EndBlock(attributeParamMeta[paramIndex]["defaultValue"]))
                                                            {
                                                                shouldUpdate = true;
                                                            }
                                                        }
                                                        paramIndex++;
                                                    }

                                                    paramIndex = 0;
                                                }
                                                if (listOfVariables[index].attributes.Count > 0) GUILayout.Space(4);
                                            });
                                        }
                                        if (GUILayout.Button("+ Add Attributes"))
                                        {
                                            var attribute = new AttributeDeclaration();
                                            attribute.SetType(typeof(SerializeField));
                                            listOfVariables[index].attributes.Add(attribute);
                                        }
                                    });
                                });

                                GUILayout.Space(4);

                                listOfVariables[index].propertyOpened = HUMEditor.Foldout(listOfVariables[index].propertyOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                                {
                                    HUMEditor.Image(PathUtil.Load("property_16", CommunityEditorPath.Code).Single(), 16, 16);
                                    listOfVariables[index].isProperty = EditorGUILayout.ToggleLeft("Property", listOfVariables[index].isProperty);
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
                                                    listOfVariables[index].get = EditorGUILayout.ToggleLeft("Get", listOfVariables[index].get);
                                                },
                                                () =>
                                                {
                                                    if (!listOfVariables[index].set) listOfVariables[index].get = true;
                                                });

                                                HUMEditor.Disabled(!listOfVariables[index].get, () =>
                                                {
                                                    if (GUILayout.Button("Edit", GUILayout.Width(60)))
                                                    {
                                                        GraphWindow.OpenActive(listOfVariables[index].getter.GetReference() as GraphReference);
                                                        var listOfgetterGraphVars = listOfVariables[index].getter.graph.variables;
                                                        foreach (var variable in listOfVariables)
                                                        {

                                                            listOfVariables[index].getter.graph.variables.Set(variable.name, null);
                                                            var matchingGetterGraphVar = listOfgetterGraphVars.FirstOrDefault(graphvar => graphvar.name == variable.name);

                                                            if (matchingGetterGraphVar != null)
                                                            {
                                                                SerializableType type = matchingGetterGraphVar.typeHandle;
                                                                type.Identification = variable.type.AssemblyQualifiedName;
                                                                matchingGetterGraphVar.typeHandle = type;
                                                            }
                                                        }
                                                    }
                                                });
                                            });

                                            HUMEditor.Horizontal(() =>
                                            {
                                                HUMEditor.Changed(() =>
                                                {
                                                    HUMEditor.Image(PathUtil.Load("setter_32", CommunityEditorPath.Code).Single(), 16, 16);
                                                    listOfVariables[index].set = EditorGUILayout.ToggleLeft("Set", listOfVariables[index].set);
                                                },
                                                () =>
                                                {
                                                    if (!listOfVariables[index].set) listOfVariables[index].get = true;
                                                });

                                                HUMEditor.Disabled(!listOfVariables[index].set, () =>
                                                {
                                                    if (GUILayout.Button("Edit", GUILayout.Width(60)))
                                                    {
                                                        GraphWindow.OpenActive(listOfVariables[index].setter.GetReference() as GraphReference);
                                                        var listOfsetterGraphVars = listOfVariables[index].setter.graph.variables;
                                                        foreach (var variable in listOfVariables)
                                                        {
                                                            listOfVariables[index].setter.graph.variables.Set(variable.name, null);
                                                            var matchingSetterGraphVar = listOfsetterGraphVars.FirstOrDefault(graphvar => graphvar.name == variable.name);

                                                            if (matchingSetterGraphVar != null)
                                                            {
                                                                SerializableType type = matchingSetterGraphVar.typeHandle;
                                                                type.Identification = variable.type.AssemblyQualifiedName;
                                                                matchingSetterGraphVar.typeHandle = type;
                                                            }
                                                        }
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
                        var declaration = CreateInstance<TFieldDeclaration>();
                        if (Target.GetType() == typeof(ClassAsset)) declaration.classAsset = Target as ClassAsset;
                        if (Target.GetType() == typeof(StructAsset)) declaration.structAsset = Target as StructAsset;
                        var getter = CreateInstance<PropertyGetterMacro>();
                        var setter = CreateInstance<PropertySetterMacro>();
                        AssetDatabase.AddObjectToAsset(declaration, Target);
                        AssetDatabase.AddObjectToAsset(getter, Target);
                        AssetDatabase.AddObjectToAsset(setter, Target);
                        listOfVariables.Add(declaration);
                        var functionGetterUnit = new FunctionNode(FunctionType.Getter);
                        var functionSetterUnit = new FunctionNode(FunctionType.Setter);
                        functionGetterUnit.fieldDeclaration = declaration;
                        functionSetterUnit.fieldDeclaration = declaration;
                        declaration.getter = getter;
                        declaration.setter = setter;
                        declaration.getter.graph.units.Add(functionGetterUnit);
                        declaration.setter.graph.units.Add(functionSetterUnit);
                        declaration.hideFlags = HideFlags.HideInHierarchy;
                        getter.hideFlags = HideFlags.HideInHierarchy;
                        setter.hideFlags = HideFlags.HideInHierarchy;
                        var listOfgetterGraphVars = declaration.getter.graph.variables;
                        var listOfsetterGraphVars = declaration.setter.graph.variables;
                        foreach (var variable in listOfVariables)
                        {
                            declaration.getter.graph.variables.Set(variable.name, null);
                            var matchingGetterGraphVar = listOfgetterGraphVars.FirstOrDefault(graphvar => graphvar.name == variable.name);

                            if (matchingGetterGraphVar != null)
                            {
                                SerializableType type = matchingGetterGraphVar.typeHandle;
                                type.Identification = variable.type.AssemblyQualifiedName;
                                matchingGetterGraphVar.typeHandle = type;
                            }
                            declaration.setter.graph.variables.Set(variable.name, null);
                            var matchingSetterGraphVar = listOfsetterGraphVars.FirstOrDefault(graphvar => graphvar.name == variable.name);

                            if (matchingSetterGraphVar != null)
                            {
                                SerializableType type = matchingSetterGraphVar.typeHandle;
                                type.Identification = variable.type.AssemblyQualifiedName;
                                matchingSetterGraphVar.typeHandle = type;
                            }
                        }
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