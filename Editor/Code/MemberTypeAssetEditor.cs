using UnityEngine;
using UnityEditor;
using Unity.VisualScripting.Community.Libraries.Humility;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using System.Linq;

namespace Unity.VisualScripting.Community
{
    public abstract class MemberTypeAssetEditor<TMemberTypeAsset, TMemberTypeGenerator, TFieldDeclaration, TMethodDeclaration, TConstructorDeclaration> : CodeAssetEditor<TMemberTypeAsset, TMemberTypeGenerator>
        where TMemberTypeAsset : MemberTypeAsset<TFieldDeclaration, TMethodDeclaration, TConstructorDeclaration>
        where TMemberTypeGenerator : MemberTypeAssetGenerator<TMemberTypeAsset, TFieldDeclaration, TMethodDeclaration, TConstructorDeclaration>
        where TFieldDeclaration : FieldDeclaration
        where TMethodDeclaration : MethodDeclaration
        where TConstructorDeclaration : ConstructorDeclaration
    {
        protected Metadata constructors;
        protected SerializedProperty constructorsProp;

        protected Metadata variables;
        protected SerializedProperty variablesProp;

        protected Metadata methods;
        protected SerializedProperty methodsProp;

        private int constructorsCount;
        private int fieldsCount;
        private int methodsCount;

        private Color boxBackground => HUMColor.Grey(0.15f);

        protected virtual Texture2D DefaultIcon() { return PathUtil.Load("object_32", CommunityEditorPath.Code).Single(); }

        private Type[] attributeTypes = new Type[] { };
        private Type[] fieldAttributeTypes = new Type[] { };
        private Type[] propertyAttributeTypes = new Type[] { };
        private Type[] methodAttributeTypes = new Type[] { };
        private Type[] structAttributeTypes = new Type[] { };
        private Type[] classAttributeTypes = new Type[] { };
        private Type[] enumAttributeTypes = new Type[] { };
        private Type[] interfaceAttributeTypes = new Type[] { };

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

            if (Target.icon == null) Target.icon = DefaultIcon();

            CacheConstrainedAttributes();

            shouldUpdate = true;
        }

        private void CacheConstrainedAttributes()
        {
            attributeTypes = typeof(Attribute).Get().Derived();
            classAttributeTypes = attributeTypes.Where((attr) => { return attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.Class || attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.All; }).ToArray();
            structAttributeTypes = attributeTypes.Where((attr) => { return attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.Struct || attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.All; }).ToArray();
            enumAttributeTypes = attributeTypes.Where((attr) => { return attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.Enum || attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.All; }).ToArray();
            interfaceAttributeTypes = attributeTypes.Where((attr) => { return attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.Interface || attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.All; }).ToArray();
            fieldAttributeTypes = attributeTypes.Where((attr) => { return attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.Field || attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.All; }).ToArray();
            propertyAttributeTypes = attributeTypes.Where((attr) => { return attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.Property || attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.All; }).ToArray();
            methodAttributeTypes = attributeTypes.Where((attr) => { return attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.Method || attr.GetAttribute<AttributeUsageAttribute>().ValidOn == AttributeTargets.All; }).ToArray();
        }

        protected override void OnTypeHeaderGUI()
        {
            HUMEditor.Horizontal(() =>
            {
                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground, Color.black, new RectOffset(7, 7, 7, 7), new RectOffset(1, 1, 1, 1), () =>
                      {
                          Target.icon = (Texture2D)EditorGUILayout.ObjectField(GUIContent.none, Target.icon, typeof(Texture2D), false, GUILayout.Width(32), GUILayout.Height(32));
                      }, false, false);

                GUILayout.Space(2);

                HUMEditor.Vertical(() =>
                {
                    base.OnTypeHeaderGUI();
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

        protected virtual void OnExtendedOptionsGUI()
        {

        }

        protected override void BeforePreview()
        {
            Constructors();
            GUILayout.Space(4);
            Variables();
            GUILayout.Space(4);
            Methods();
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
                                listOfVariables[index].name = GUILayout.TextField(listOfVariables[index].name);
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
                                            // To Do
                                        }, listOfVariables[index]);
                                }

                                if (index < methods.Count - 1)
                                {
                                    menu.AddItem(new GUIContent("Move Down"), false, (obj) =>
                                    {
                                            // To Do
                                        }, listOfVariables[index]);
                                }
                                menu.ShowAsContext();
                            }
                        }, () =>
                        {
                            HUMEditor.Vertical().Box(HUMColor.Grey(0.15f), Color.black, new RectOffset(6, 6, 6, 6), new RectOffset(1, 1, 0, 1), () =>
                            {
                                listOfVariables[index].scope = (AccessModifier)EditorGUILayout.EnumPopup("Scope", listOfVariables[index].scope);

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
                                                    shouldUpdate = true;
                                                }, false);

                                                if (GUILayout.Button("...", GUILayout.Width(19)))
                                                {
                                                    GenericMenu menu = new GenericMenu();
                                                    menu.AddItem(new GUIContent("Delete"), false, (obj) =>
                                                    {
                                                        listOfVariables[index].attributes.Remove(obj as AttributeDeclaration);
                                                    }, attribute);

                                                    //if (index > 0)
                                                    //{
                                                    //    menu.AddItem(new GUIContent("Move Up"), false, (obj) =>
                                                    //    {
                                                    //        // To Do
                                                    //    }, listOfConstructors[index]);
                                                    //}

                                                    //if (index < methods.Count - 1)
                                                    //{
                                                    //    menu.AddItem(new GUIContent("Move Down"), false, (obj) =>
                                                    //    {
                                                    //        // To Do
                                                    //    }, listOfConstructors[index]);
                                                    //}
                                                    menu.ShowAsContext();
                                                }
                                            }, () =>
                                            {
                                                var parameters = attribute.parameters;

                                                for (int attrParamIndex = 0; attrParamIndex < parameters.Count; attrParamIndex++)
                                                {

                                                }
                                            });
                                        }

                                        if (listOfVariables[index].attributes.Count > 0) GUILayout.Space(4);

                                        if (GUILayout.Button("+ Add Attributes"))
                                        {
                                            listOfVariables[index].attributes.Add(new AttributeDeclaration());
                                        }
                                    });
                                });

                                GUILayout.Space(4);

                                listOfVariables[index].propertyOpened = HUMEditor.Foldout(listOfVariables[index].propertyOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () => 
                                {
                                    listOfVariables[index].isProperty = EditorGUILayout.ToggleLeft("Property", listOfVariables[index].isProperty);
                                }, () =>
                                {
                                    HUMEditor.Disabled(!listOfVariables[index].isProperty, () =>
                                    {
                                        HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground, Color.black, new RectOffset(4,4,4,4), new RectOffset(1,1,0,1), () =>
                                        {
                                            HUMEditor.Horizontal(() =>
                                            {
                                                HUMEditor.Changed(() => { listOfVariables[index].get = EditorGUILayout.ToggleLeft("Get", listOfVariables[index].get); }, () => { if (!listOfVariables[index].set) listOfVariables[index].get = true; });

                                                HUMEditor.Disabled(!listOfVariables[index].get, () =>
                                                {
                                                    if (GUILayout.Button("Edit", GUILayout.Width(60)))
                                                    {
                                                        GraphWindow.OpenActive(listOfVariables[index].getter.GetReference() as GraphReference);
                                                    }
                                                });
                                            });

                                            HUMEditor.Horizontal(() =>
                                            {
                                                HUMEditor.Changed(() => { listOfVariables[index].set = EditorGUILayout.ToggleLeft("Set", listOfVariables[index].set); }, () => { if (!listOfVariables[index].set) listOfVariables[index].get = true; });

                                                HUMEditor.Disabled(!listOfVariables[index].set, () =>
                                                {
                                                    if (GUILayout.Button("Edit", GUILayout.Width(60)))
                                                    {
                                                        GraphWindow.OpenActive(listOfVariables[index].setter.GetReference() as GraphReference);
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
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                });
            });
        }

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
                            GUILayout.Label($"Constructor {i.ToString()}");

                            if (GUILayout.Button("Edit", GUILayout.Width(60)))
                            {
                                GraphWindow.OpenActive(listOfConstructors[index].GetReference() as GraphReference);
                            }

                            if (GUILayout.Button("...", GUILayout.Width(19)))
                            {
                                GenericMenu menu = new GenericMenu();
                                menu.AddItem(new GUIContent("Delete"), false, (obj) =>
                                {
                                    constructors.Remove(obj as TConstructorDeclaration);
                                    AssetDatabase.RemoveObjectFromAsset(obj as TConstructorDeclaration);
                                }, listOfConstructors[index]);

                                //if (index > 0)
                                //{
                                //    menu.AddItem(new GUIContent("Move Up"), false, (obj) =>
                                //    {
                                //        // To Do
                                //    }, listOfConstructors[index]);
                                //}

                                //if (index < methods.Count - 1)
                                //{
                                //    menu.AddItem(new GUIContent("Move Down"), false, (obj) =>
                                //    {
                                //        // To Do
                                //    }, listOfConstructors[index]);
                                //}
                                menu.ShowAsContext();
                            }
                        }, () =>
                        {
                            HUMEditor.Vertical().Box(HUMColor.Grey(0.15f), Color.black, new RectOffset(6, 6, 6, 6), new RectOffset(1, 1, 0, 1), () =>
                            {
                                listOfConstructors[index].scope = (AccessModifier)EditorGUILayout.EnumPopup("Scope", listOfConstructors[index].scope);

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
                                        var funcionUnit = (listOfConstructors[index].graph.units[0] as FunctionNode);
                                        funcionUnit.Define();
                                        funcionUnit.Describe();
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
                                        // To Do
                                    }, listOfMethods[index]);
                                }

                                if (index < methods.Count - 1)
                                {
                                    menu.AddItem(new GUIContent("Move Down"), false, (obj) =>
                                    {
                                        // To Do
                                    }, listOfMethods[index]);
                                }
                                menu.ShowAsContext();
                            }
                        }, () =>
                        {
                            HUMEditor.Vertical().Box(HUMColor.Grey(0.15f), Color.black, new RectOffset(6, 6, 6, 6), new RectOffset(1, 1, 0, 1), () =>
                            {
                                listOfMethods[index].scope = (AccessModifier)EditorGUILayout.EnumPopup("Scope", listOfMethods[index].scope);
                                listOfMethods[index].modifier = (MethodModifier)EditorGUILayout.EnumPopup("Modifier", listOfMethods[index].modifier);
                                Inspector.BeginBlock(methods[index]["returnType"], new Rect());
                                LudiqGUI.InspectorLayout(methods[index]["returnType"], new GUIContent("Returns"));
                                if (Inspector.EndBlock(methods[index]["returnType"]))
                                {
                                    shouldUpdate = true;
                                }

                                GUILayout.Space(4);

                                listOfMethods[index].parametersOpened = HUMEditor.Foldout(listOfMethods[index].parametersOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                                {
                                    GUILayout.Label("Parameters");
                                }, () =>
                                {
                                    var paramMeta = methods[index]["parameters"];
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
    }
}
