using UnityEngine;
using Unity.VisualScripting;
using UnityEditor;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Community.Utility;
using System.Reflection;
using ParameterModifier = Unity.VisualScripting.Community.Libraries.CSharp.ParameterModifier;
using System;
using System.Collections;

namespace Unity.VisualScripting.Community.CSharp
{
    [CustomEditor(typeof(InterfaceAsset))]
    public class InterfaceAssetEditor : CodeAssetEditor<InterfaceAsset, InterfaceAssetGenerator>
    {
        private Metadata variables, methods, interfaces;
        private Type[] allTypes;
        private Type ValueInspectorType;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (ValueInspectorType == null)
                ValueInspectorType = typeof(SystemObjectInspector).Assembly.GetType("Unity.VisualScripting.ValueInspector", throwOnError: true);

            if (variables == null)
            {
                variables = Metadata.FromProperty(serializedObject.FindProperty("variables"));
            }

            if (methods == null)
            {
                methods = Metadata.FromProperty(serializedObject.FindProperty("methods"));
            }

            if (interfaces == null)
            {
                interfaces = Metadata.FromProperty(serializedObject.FindProperty("interfaces"));
            }
            shouldUpdate = true;

            allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).ToArray();

            CacheConstrainedAttributes();
        }

        private void CacheConstrainedAttributes()
        {
            var allAttributeTypes = allTypes
                .Where(t => (t.IsSubclassOf(typeof(Attribute)) || t == typeof(Attribute)) && t.IsPublic)
                .ToArray();

            interfaceAttributeTypes = allAttributeTypes
                .Where(attr => GetAttributeUsage(attr).ValidOn.HasFlag(AttributeTargets.Interface) || GetAttributeUsage(attr).ValidOn == AttributeTargets.All)
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
            Interfaces();
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
                                shouldUpdate = true;
                                interfaces.Remove(obj as SystemType);
                            }, listOfInterfaces[index]);

                            if (i > 0)
                            {
                                menu.AddItem(new GUIContent("Move Up"), false, (obj) =>
                                {
                                    Undo.RegisterCompleteObjectUndo(Target, "Moved Interface up");
                                    shouldUpdate = true;
                                    MoveItemUp(Target.interfaces, index);
                                }, listOfInterfaces[index]);
                            }

                            if (i < listOfInterfaces.Count - 1)
                            {
                                menu.AddItem(new GUIContent("Move Down"), false, (obj) =>
                                {
                                    Undo.RegisterCompleteObjectUndo(Target, "Moved Interface down");
                                    shouldUpdate = true;
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
                        shouldUpdate = true;
                        Target.interfaces.Add(new SystemType(Codebase.settingsAssembliesTypes.Where(t => t.IsInterface && t.IsPublic).First()));
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                });
            });
        }

        protected override void BeforePreview()
        {
            Variables();
            GUILayout.Space(4);
            Methods();
        }

        Dictionary<Metadata, Type> typeChangedLookup = new Dictionary<Metadata, Type>();
        private void DrawParameters(Metadata paramMeta, UnityEngine.Object target)
        {
            var parameters = paramMeta.value as List<TypeParam>;
            HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    parameters[i].opened = HUMEditor.Foldout(parameters[i].opened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                    {
                        EditorGUI.BeginChangeCheck();
                        var paramName = GUILayout.TextField(parameters[i].name);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RegisterCompleteObjectUndo(target, "Changed Parameter Name");
                            parameters[i].name = paramName.LegalMemberName();
                        }

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
                            if (TypeBuilderWindow.Button(currentParam["Paramtype"]["type"].value as Type))
                            {
                                TypeBuilderWindow.ShowWindow(lastRect, (type) => { currentParam["Paramtype"]["type"].value = type; }, parameters[i].Paramtype.type, true, new Type[0], null, (t) =>
                                {
                                    Undo.RegisterCompleteObjectUndo(target, "Changed Parameter Type");
                                    UpdatePreview();
                                });
                            }
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
                                GenericMenu menu = new GenericMenu();//
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

                            Inspector.BeginBlock(currentParam["hasDefault"], new Rect());
                            LudiqGUI.InspectorLayout(currentParam["hasDefault"], new GUIContent("Has Default Value"));
                            if (Inspector.EndBlock(currentParam["hasDefault"]))
                            {
                                Undo.RegisterCompleteObjectUndo(target, "Toggled Has Default");
                                UpdatePreview();
                            }

                            GUILayout.Space(4);

                            if (i >= 0 && i < paramMeta.Count)
                            {
                                if (parameters[i].hasDefault)
                                {
                                    if (typeChangedLookup.TryGetValue(currentParam["Paramtype"]["type"], out var type))
                                    {
                                        if (type != currentParam["Paramtype"]["type"].value as Type || parameters[i].defaultValue?.GetType() != type)
                                        {
                                            UpdateDefaultValueType((Type)currentParam["Paramtype"]["type"].value, currentParam);
                                            typeChangedLookup[currentParam["Paramtype"]["type"]] = (Type)currentParam["Paramtype"]["type"].value;
                                            UpdatePreview();
                                        }
                                    }
                                    else
                                    {
                                        typeChangedLookup[currentParam["Paramtype"]["type"]] = (Type)currentParam["Paramtype"]["type"].value;
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
                                UpdatePreview();
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
#if VISUAL_SCRIPTING_1_7
            currentParam["typeHandle"].value = new SerializableType(type.AssemblyQualifiedName);
#endif
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
                            if (constructors == null)
                            {
                                GUILayout.Label("No Constructors");
                                return;
                            }
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
                                "Select Constructor : ",
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
                                    GUIContent TypebuilderButtonContent = new GUIContent(
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
                    attribute.SetType(GetConstrainedAttributeTypes(attributeUsageType).FirstOrDefault());
                    Undo.RegisterCompleteObjectUndo(target, "Added Attribute");
                    attributeList.Add(attribute);
                    InitializeAttribute(attribute, attributeList.Count - 1);
                    UpdatePreview();
                }

                void InitializeAttribute(AttributeDeclaration attribute, int attributeIndex)
                {
                    var attributeParamMeta = attributesMeta[attributeIndex]["parameters"];
                    var constructors = attribute?.GetAttributeType()?.GetConstructors();
                    var validConstructors = constructors?.Where(c => c.IsPublic && c.GetParameters().All(param => param.ParameterType.HasInspector())).ToList();

                    ConstructorInfo selectedConstructor;

                    if (validConstructors != null && attribute.constructor >= 0 && attribute.constructor < validConstructors.Count)
                    {
                        selectedConstructor = validConstructors[attribute.constructor];
                    }
                    else
                    {
                        selectedConstructor = constructors?.FirstOrDefault();
                    }
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
                            if (!isParamsParameter && !(Param.defaultValue is ICollection))
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
        private Type[] methodAttributeTypes = new Type[] { };
        private Type[] propertyAttributeTypes = new Type[] { };
        private Type[] interfaceAttributeTypes = new Type[] { };
        private Type[] parameterAttributeTypes = new Type[] { };
        Type[] GetConstrainedAttributeTypes(AttributeUsageType usage)
        {
            return usage switch
            {
                AttributeUsageType.Property => propertyAttributeTypes,
                AttributeUsageType.Method => methodAttributeTypes,
                AttributeUsageType.Interface => interfaceAttributeTypes,
                _ => new Type[0],
            };
        }

        private Type AttributeTypeField(AttributeDeclaration attribute, AttributeUsageType usage)
        {
            GUILayout.Label(" ", GUILayout.Height(20));
            var position = GUILayoutUtility.GetLastRect();
            Type[] types = usage switch
            {
                AttributeUsageType.Interface => interfaceAttributeTypes,
                AttributeUsageType.Property => propertyAttributeTypes,
                AttributeUsageType.Method => methodAttributeTypes,
                AttributeUsageType.Parameter => parameterAttributeTypes,
                _ => new Type[0],
            };
            return LudiqGUI.TypeField(position, GUIContent.none, attribute.GetAttributeType(), () =>
            {
                return new TypeOptionTree(types);
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

        private void Variables()
        {
            Target.propertiesOpen = HUMEditor.Foldout(Target.propertiesOpen, HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2, () =>
            {
                HUMEditor.Image(PathUtil.Load("variables_16", CommunityEditorPath.Code).Single(), 16, 16, new RectOffset(), new RectOffset(4, 8, 4, 4));
                GUILayout.Label("Variables");
            }, () =>
            {
                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                {
                    for (int i = 0; i < Target.variables.Count; i++)
                    {
                        Target.variables[i].isOpen = HUMEditor.Foldout(Target.variables[i].isOpen, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                        {
                            GUILayout.BeginHorizontal();
                            variables[i]["name"].value = EditorGUILayout.TextField((string)variables[i]["name"].value);
                            if (GUILayout.Button("...", GUILayout.Width(19)))
                            {
                                var index = i;
                                GenericMenu menu = new GenericMenu();
                                menu.AddItem(new GUIContent("Delete"), false, (obj) =>
                                {
                                    Undo.RegisterCompleteObjectUndo(Target, "Deleted Variable");
                                    UpdatePreview();
                                    Target.variables.Remove(obj as InterfacePropertyItem);
                                }, variables[i].value);

                                if (i > 0)
                                {
                                    menu.AddItem(new GUIContent("Move Up"), false, (obj) =>
                                    {
                                        Undo.RegisterCompleteObjectUndo(Target, "Moved Variable up");
                                        UpdatePreview();
                                        MoveItemUp(Target.variables, index);
                                    }, variables[i].value);
                                }

                                if (i < Target.variables.Count - 1)
                                {
                                    menu.AddItem(new GUIContent("Move Down"), false, (obj) =>
                                    {
                                        Undo.RegisterCompleteObjectUndo(Target, "Moved Variable down");
                                        UpdatePreview();
                                        MoveItemDown(Target.variables, index);
                                    }, variables[i].value);
                                }
                                menu.ShowAsContext();
                            }
                            GUILayout.EndHorizontal();
                        }, () =>
                        {
                            HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Label("Type");
                                if (TypeBuilderWindow.Button(variables[i]["type"]["type"].value as Type))
                                {
                                    TypeBuilderWindow.ShowWindow(GUILayoutUtility.GetLastRect(), variables[i]["type"]["type"], true, new Type[0], () =>
                                    {
                                        Undo.RegisterCompleteObjectUndo(Target, "Changed Variable Type");
                                        UpdatePreview();
                                    });
                                }

                                GUILayout.EndHorizontal();

                                EditorGUI.BeginChangeCheck();
                                GUILayout.BeginHorizontal();
                                GUILayout.Label("Get");
                                variables[i]["get"].value = EditorGUILayout.Toggle((bool)variables[i]["get"].value);
                                GUILayout.EndHorizontal();
                                if (EditorGUI.EndChangeCheck())
                                {
                                    Undo.RegisterCompleteObjectUndo(Target, "Toggled Property Get");
                                }

                                EditorGUI.BeginChangeCheck();
                                GUILayout.BeginHorizontal();
                                GUILayout.Label("Set");
                                variables[i]["set"].value = EditorGUILayout.Toggle((bool)variables[i]["set"].value);
                                GUILayout.EndHorizontal();
                                if (EditorGUI.EndChangeCheck())
                                {
                                    Undo.RegisterCompleteObjectUndo(Target, "Toggled Property Set");
                                }
                            });
                        });
                    }

                    if (GUILayout.Button("+ Add Variable"))
                    {
                        Target.variables.Add(new InterfacePropertyItem());
                    }
                });
            });
        }

        private void Methods()
        {
            Target.methodsOpen = HUMEditor.Foldout(Target.methodsOpen, HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2, () =>
            {
                HUMEditor.Image(PathUtil.Load("method_16", CommunityEditorPath.Code).Single(), 16, 16, new RectOffset(), new RectOffset(4, 8, 4, 4));
                GUILayout.Label("Methods");
            }, () =>
            {
                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                {
                    var listOfMethods = methods.value as List<InterfaceMethodItem>;

                    for (int i = 0; i < listOfMethods.Count; i++)
                    {
                        var index = i;
                        listOfMethods[index].opened = HUMEditor.Foldout(listOfMethods[index].opened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                        {
                            HUMEditor.Changed(() =>
                            {
                                listOfMethods[index].name = GUILayout.TextField(listOfMethods[index].name);
                            }, () =>
                            {
                                listOfMethods[index].name = listOfMethods[index].name.LegalMemberName();
                                shouldUpdate = true;
                            });

                            if (GUILayout.Button("...", GUILayout.Width(19)))
                            {
                                GenericMenu menu = new GenericMenu();
                                menu.AddItem(new GUIContent("Delete"), false, (obj) =>
                                {
                                    methods.Remove(obj as InterfaceMethodItem);
                                    shouldUpdate = true;
                                }, listOfMethods[index]);

                                if (index > 0)
                                {
                                    menu.AddItem(new GUIContent("Move Up"), false, (obj) =>
                                    {
                                        MoveItemUp(methods.value as List<InterfaceMethodItem>, index);
                                    }, listOfMethods[index]);
                                }

                                if (index < methods.Count - 1)
                                {
                                    menu.AddItem(new GUIContent("Move Down"), false, (obj) =>
                                    {
                                        MoveItemDown(methods.value as List<InterfaceMethodItem>, index);
                                    }, listOfMethods[index]);
                                }
                                menu.ShowAsContext();
                            }
                        }, () =>
                        {
                            HUMEditor.Vertical().Box(HUMColor.Grey(0.15f), Color.black, new RectOffset(6, 6, 6, 6), new RectOffset(1, 1, 0, 1), () =>
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Label("Return Type");
                                if (TypeBuilderWindow.Button(methods[index]["returnType"]["type"].value as Type))
                                {
                                    TypeBuilderWindow.ShowWindow(GUILayoutUtility.GetLastRect(), methods[index]["returnType"]["type"], true, new Type[0], () =>
                                    {
                                        Undo.RegisterCompleteObjectUndo(Target, "Changed Return Type");
                                        UpdatePreview();
                                    });
                                }

                                GUILayout.EndHorizontal();
                                GUILayout.Space(4);

                                listOfMethods[index].parametersOpened = HUMEditor.Foldout(listOfMethods[index].parametersOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                                {
                                    GUILayout.Label("Parameters");
                                }, () =>
                                {
                                    DrawParameters(methods[index]["parameters"], target);
                                });

                            }, true, false);
                        });

                        GUILayout.Space(4);
                    }

                    if (GUILayout.Button("+ Add Methods"))
                    {
                        listOfMethods.Add(new InterfaceMethodItem());
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
