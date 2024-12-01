using UnityEditor;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Reflection;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Collections;

namespace Unity.VisualScripting.Community
{
    public class TypeBuilderWindow : EditorWindow
    {
        private Type baseType = typeof(object);
        private static GenericParameter genericParameter = new GenericParameter(typeof(object), typeof(object).Name);
        private Vector2 scrollPosition;
        private Rect lastRect;
        private Type[] baseTypeLookup;
        private Type[] settingAssemblyTypesLookup;
        private Type[] customTypeLookup;
        private Action<Type> result;
        private Action onChanged;

        public static TypeBuilderWindow window { get; private set; }

        private static Metadata targetMetadata;

        private bool canMakeArrayTypeForBaseType;

        public static void ShowWindow(Rect position, Metadata Meta, bool canMakeArray = true, Type[] types = null, Action onChanged = null)
        {
            if (window == null)
            {
                window = CreateInstance<TypeBuilderWindow>();
            }
            window.onChanged = onChanged; 
            window.canMakeArrayTypeForBaseType = canMakeArray;
            targetMetadata = Meta;
            if (Meta != null)
            {
                window.OnEnable();
                var type = Meta.value as Type;
                if (type != null)
                {
                    if (type.IsGenericType)
                    {
                        genericParameter = new GenericParameter(type, type.Name);
                        genericParameter.AddGenericParameters(type, (generic) => window.GetConstrainedTypes(generic));
                        window.baseType = genericParameter.ConstructType();
                    }
                    else if (type.IsArray)
                    {
                        var tempType = type.GetElementType();
                        while (tempType.IsArray)
                        {
                            tempType = tempType.GetElementType();
                        }

                        genericParameter = new GenericParameter(tempType, tempType.Name);
                        genericParameter.AddGenericParameters(tempType, (generic) => window.GetConstrainedTypes(generic));
                        window.baseType = genericParameter.ConstructType();
                    }
                    else
                    {
                        genericParameter = new GenericParameter(type, type.Name);
                        genericParameter.AddGenericParameters(type, (generic) => window.GetConstrainedTypes(generic));
                        window.baseType = genericParameter.ConstructType();
                    }
                }
                else
                {
                    genericParameter.Clear();
                }
            }
            position = GUIUtility.GUIToScreenRect(position);
            position.width = Mathf.Clamp(position.width, 300, 600);
            window.minSize = new Vector2(300, 200);
            window.titleContent = new GUIContent("Type Builder");
            if (types != null)
            {
                window.customTypeLookup = types;
            }
            var initialSize = new Vector2(position.width, 320);

            window.ShowAsDropDown(position, initialSize);
        }

        public static void ShowWindow(Rect position, Action<Type> result, Type currentType, bool canMakeArray = true, Type[] types = null, Action onChanged = null)
        {
            if (window == null)
            {
                window = CreateInstance<TypeBuilderWindow>();
            }
            window.onChanged = onChanged; 
            window.canMakeArrayTypeForBaseType = canMakeArray;
            window.result = result;
            targetMetadata = null;
            window.OnEnable();
            var type = currentType;
            if (type != null)
            {
                if (type.IsGenericType)
                {
                    genericParameter = new GenericParameter(type, type.Name);
                    genericParameter.AddGenericParameters(type, (generic) => window.GetConstrainedTypes(generic));
                    window.baseType = genericParameter.ConstructType();
                }
                else if (type.IsArray)
                {
                    var tempType = type.GetElementType();
                    while (tempType.IsArray)
                    {
                        tempType = tempType.GetElementType();
                    }

                    genericParameter = new GenericParameter(tempType, tempType.Name);
                    genericParameter.AddGenericParameters(tempType, (generic) => window.GetConstrainedTypes(generic));
                    window.baseType = genericParameter.ConstructType();
                }
                else
                {
                    genericParameter = new GenericParameter(type, type.Name);
                    genericParameter.AddGenericParameters(type, (generic) => window.GetConstrainedTypes(generic));
                    window.baseType = genericParameter.ConstructType();
                }
            }
            else
            {
                genericParameter.Clear();
            }
            position = GUIUtility.GUIToScreenRect(position);
            position.width = Mathf.Clamp(position.width, 300, 600);
            window.minSize = new Vector2(300, 200);
            window.titleContent = new GUIContent("Type Builder");
            if (types != null)
            {
                window.customTypeLookup = types;
            }
            var initialSize = new Vector2(position.width, 320);

            window.ShowAsDropDown(position, initialSize);
        }

        private void OnEnable()
        {
            settingAssemblyTypesLookup = Codebase.settingsAssembliesTypes.ToArray();
            baseTypeLookup = settingAssemblyTypesLookup
                .Except(settingAssemblyTypesLookup.Where(t => TypeHasSpecialName(t) || t == null))
                .ToArray();
        }

        private bool TypeHasSpecialName(Type t)
        {
            if (t.IsArray || typeof(IList).IsAssignableFrom(t) || typeof(IDictionary).IsAssignableFrom(t))
                return true;
            return t.IsSpecialName || t.IsDefined(typeof(CompilerGeneratedAttribute));
        }

        private IFuzzyOptionTree GetBaseTypeOptions()
        {
            return new TypeBuilderTypeOptionTree(customTypeLookup != null ? customTypeLookup : baseTypeLookup);
        }

        private IFuzzyOptionTree GetNestedTypeOptions(GenericParameter parameter)
        {
            var constrainedTypes = GetConstrainedTypes(parameter);
            if (parameter != null && parameter.type.type.IsArray && !constrainedTypes.Contains(parameter.type.type)) constrainedTypes.Append(parameter.type.type);
            return new TypeBuilderTypeOptionTree(constrainedTypes);
        }

        private Type[] GetConstrainedTypes(GenericParameter param)
        {
            if (param.constraints == null && param.type.type.IsGenericParameter)
            {
                var constraints = param.type.type.GetGenericParameterConstraints();

                if (constraints.Length > 0)
                {
                    var constrainedTypes = settingAssemblyTypesLookup.Where(t =>
                    {
                        foreach (var constraint in constraints)
                        {
                            if (!constraint.IsAssignableFrom(t))
                            {
                                return false;
                            }
                        }
                        return true;
                    }).ToArray();

                    param.constraints = constrainedTypes;
                    return constrainedTypes;
                }
                else
                {
                    var attributes = param.type.type.GenericParameterAttributes;
                    var constrainedTypes = GetConstraintAttributeTypes(attributes, !param.HasParent && customTypeLookup != null ? customTypeLookup : settingAssemblyTypesLookup);
                    param.constraints = constrainedTypes;
                    return constrainedTypes;
                }
            }
            return param.constraints;
        }

        public Type[] GetConstraintAttributeTypes(GenericParameterAttributes attributes, Type[] typesLookup)
        {
            var constrainedTypes = typesLookup.AsEnumerable();

            if ((attributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
            {
                constrainedTypes = constrainedTypes.Where(type => type.IsClass);
            }
            if ((attributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
            {
                constrainedTypes = constrainedTypes.Where(type => !type.IsNullable() || type.IsStruct());
            }
            if ((attributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
            {
                constrainedTypes = constrainedTypes.Where(type => type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic).Any(constructor => constructor.GetParameters().Length == 0));
            }

            return constrainedTypes.Where(t => !TypeHasSpecialName(t)).ToArray();
        }

        private void OnGUI()
        {
            if (Event.current.keyCode == KeyCode.Escape || Event.current.rawType == EventType.KeyUp && !mouseOverWindow)
            {
                Close();
            }
            HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 2, 2), () =>
            {
                float contentWidth = 0;
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(false));
                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground, Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                {
                    EditorGUILayout.LabelField(new GUIContent("Type Builder", typeof(Type).Icon()?[IconSize.Small], "A tool to create and customize types beyond the standard Type Field capabilities"), LudiqStyles.centeredLabel);
                    var labelWidth = GUI.skin.label.CalcSize(new GUIContent("Select Type")).x;
                    contentWidth = Mathf.Max(contentWidth, labelWidth);
                });

                GUIContent inheritButtonContent = new GUIContent(
                    baseType?.As().CSharpName(false).RemoveHighlights().RemoveMarkdown() ?? "Select Type",
                    baseType?.Icon()?[IconSize.Small]
                );

                lastRect = GUILayoutUtility.GetLastRect();
                var buttonWidth = GUI.skin.button.CalcSize(inheritButtonContent).x;
                contentWidth = Mathf.Max(contentWidth, buttonWidth);
                DrawTypeField(inheritButtonContent, genericParameter, true);

                if (genericParameter != null)
                {
                    var index = 0;
                    foreach (var param in genericParameter.nestedParameters)
                    {
                        contentWidth = Mathf.Max(contentWidth, DrawGenericParameter(param, genericParameter.type.type.GetGenericTypeDefinition().GetGenericArguments()[index]));
                        index++;
                    }
                }

                EditorGUILayout.EndScrollView();
                EditorGUI.BeginDisabledGroup(!IsValidType(baseType, false));
                if (!IsValidType(baseType, false))
                {
                    EditorGUILayout.HelpBox($"Can not create arrays of Open Generics, e.g {baseType.As().CSharpName(false, false, false).RemoveHighlights().RemoveMarkdown()} is invalid it has to have a types set for {string.Join(", ", GetInvalidParameters(baseType))}", MessageType.Warning);
                }
                if (GUILayout.Button("Create Type"))
                {
                    if (targetMetadata != null)
                        ConstructType(targetMetadata);
                    else
                        ConstructType();
                    genericParameter.Clear();
                    Close();
                }
                EditorGUI.EndDisabledGroup();

                window.position = new Rect(position.x, position.y, contentWidth + 50, position.height);
            });
        }
        private Type GetArrayBase(Type type)
        {
            type = type.GetElementType();
            while (type.IsArray)
            {
                type = type.GetElementType();
            }
            return type;
        }

        private IEnumerable<string> GetInvalidParameters(Type type)
        {
            if (type.IsArray)
            {
                type = GetArrayBase(type);
            }
            foreach (var arg in type.GetGenericArguments())
            {
                if (arg.IsGenericTypeParameter)
                {
                    yield return arg.Name;
                }
                else if (arg.IsGenericType)
                {
                    foreach (var invalidArg in GetInvalidParameters(arg))
                    {
                        yield return invalidArg;
                    }
                }
                else if (arg.IsArray)
                {
                    var tempType = arg.GetElementType();
                    while (tempType.IsArray)
                    {
                        tempType = tempType.GetElementType();
                    }
                    if (tempType.IsGenericType)
                    {
                        foreach (var invalidArg in GetInvalidParameters(tempType))
                        {
                            yield return invalidArg;
                        }
                    }
                }
            }
        }

        private bool IsValidType(Type type, bool checkingNested)
        {
            if (type == null)
            {
                return false;
            }

            if (type.IsArray)
            {
                var elementType = GetArrayBase(type);

                if (elementType.IsGenericType)
                {
                    var genericArguments = elementType.GetGenericArguments();
                    foreach (var arg in genericArguments)
                    {
                        if (arg.IsGenericParameter)
                        {
                            return false;
                        }
                        if (!IsValidType(arg, true))
                        {
                            return false;
                        }
                    }
                }
                return IsValidType(elementType, true);
            }
            else if (checkingNested)
            {
                if (type.IsArray)
                {
                    var elementType = GetArrayBase(type);
                    if (elementType.IsGenericType)
                    {
                        var genericArguments = elementType.GetGenericArguments();
                        foreach (var arg in genericArguments)
                        {
                            if (arg.IsGenericParameter)
                            {
                                return false;
                            }
                            if (!IsValidType(arg, true))
                            {
                                return false;
                            }
                        }
                    }
                    return IsValidType(elementType, true);
                }
                else if (type.IsGenericType)
                {
                    var genericArguments = type.GetGenericArguments();
                    foreach (var arg in genericArguments)
                    {
                        if (arg.IsGenericParameter)
                        {
                            return false;
                        }
                        if (!IsValidType(arg, true))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private float DrawTypeField(GUIContent buttonContent, GenericParameter generic, bool isBaseType)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(buttonContent, GUILayout.MaxHeight(19f)))
            {
                int selectedIndex = Array.IndexOf(isBaseType && customTypeLookup != null ? customTypeLookup : baseTypeLookup, generic != null ? generic.type : typeof(object));
                Type _selected = null;
                LudiqGUI.FuzzyDropdown(lastRect, isBaseType ? GetBaseTypeOptions() : GetNestedTypeOptions(generic), selectedIndex, (type) =>
                {
                    _selected = (type as TypeBuilderType).Type;
                    if (isBaseType)
                    {
                        var genericParams = new GenericParameter(_selected, _selected.Name);
                        baseType = _selected;
                        genericParameter?.Clear();
                        genericParams.AddGenericParameters(_selected);
                        genericParameter = genericParams;
                    }
                    else
                    {
                        generic.Clear();
                        generic.AddGenericParameters(_selected);
                        generic.selectedType.type = _selected;
                        generic.type.type = _selected;
                        generic.parent.type.type = generic.parent.ConstructType();
                        baseType = genericParameter.ConstructType();
                    }
                });
            }
            bool canMakeArray = generic != null
            && generic.type.type != null
            && !generic.type.type.IsGenericParameter
            && CanTypeSupportArray(isBaseType ? genericParameter : generic);

            if (canMakeArray && ((isBaseType && canMakeArrayTypeForBaseType) || (!isBaseType && !generic.type.type.IsGenericParameter)))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Array: ");

                if (GUILayout.Button("+"))
                {
                    if (isBaseType && genericParameter != null)
                    {
                        baseType = baseType.MakeArrayType();
                        genericParameter.type.type = baseType;
                    }
                    else
                    {
                        generic.type.type = generic.type.type.MakeArrayType();
                        generic.parent.type.type = generic.parent.ConstructType();
                        baseType = genericParameter.ConstructType();
                    }
                }

                // Remove array level button
                else if (GUILayout.Button("-"))
                {
                    if (isBaseType && genericParameter != null)
                    {
                        if (baseType.IsArray)
                        {
                            baseType = baseType.GetElementType();
                            genericParameter.type.type = baseType;
                        }
                    }
                    else
                    {
                        if (generic.type.type.IsArray)
                        {
                            generic.type.type = generic.type.type.GetElementType();
                            generic.parent.type.type = generic.parent.ConstructType();
                            baseType = genericParameter.ConstructType();
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();
            var buttonWidth = GUI.skin.button.CalcSize(buttonContent).x;
            return buttonWidth;
        }

        private bool CanTypeSupportArray(GenericParameter param)
        {
            if (param == null || param.type.type == null) return false;

            if (param.type.type == typeof(void)) return false;

            if (param.constraints != null && param.constraints.Length > 0)
            {
                foreach (var constraint in param.constraints)
                {
                    if (constraint.IsAssignableFrom(typeof(Array)) || constraint.IsAssignableFrom(param.type.type.MakeArrayType()))
                        return true;
                }

                return false;
            }

            return true;
        }

        private float DrawGenericParameter(GenericParameter parameter, Type genericParam)
        {
            float maxWidth = 0;
            parameter.isOpen = HUMEditor.Foldout(parameter.isOpen, HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2, () =>
            {
                GUILayout.Label(parameter.type.type.As().CSharpName(false, false, false), LudiqStyles.centeredLabel);
                maxWidth = Mathf.Max(maxWidth, GUI.skin.box.CalcSize(new GUIContent(parameter.type.type.DisplayName())).x);
            }, () =>
            {
                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(genericParam.As().CSharpName(false, false, false), GUILayout.Width(150));
                    GUIContent typeButtonContent = new GUIContent(
                        parameter.type.type?.As().CSharpName(false, false, false) ?? "Select Type",
                        parameter.type.type?.Icon()?[IconSize.Small]
                    );

                    var buttonWidth = DrawTypeField(typeButtonContent, parameter, false);

                    maxWidth = Mathf.Max(maxWidth, GUI.skin.box.CalcSize(typeButtonContent).x);
                    GUILayout.EndHorizontal();

                    var index = 0;
                    foreach (var nested in parameter.nestedParameters)
                    {
                        maxWidth = Mathf.Max(maxWidth, DrawGenericParameter(nested, parameter.type.type.GetGenericTypeDefinition().GetGenericArguments()[index]));
                        index++;
                    }
                });
            });
            return maxWidth;
        }

        public static void ConstructType()
        {
            UndoUtility.RecordEditedObject("TypeBuilder Constructed Type");
            window.onChanged?.Invoke();
            genericParameter ??= new GenericParameter(typeof(object), typeof(object).DisplayName());
            if (genericParameter.type.type.IsGenericType && genericParameter.type.type.IsConstructedGenericType)
            {
                var newConstructedType = new GenericParameter(genericParameter, true);
                window.result?.Invoke(newConstructedType.ConstructType());
            }
            else if (genericParameter.type.type.IsArray)
            {
                var tempType = genericParameter.type.type.GetElementType();
                while (tempType.IsArray)
                {
                    tempType = tempType.GetElementType();
                }

                if (tempType.IsGenericType && tempType.IsConstructedGenericType)
                {
                    var newConstructedType = new GenericParameter(genericParameter, true);
                    window.result?.Invoke(newConstructedType.ConstructType());
                }
                else
                {
                    window.result?.Invoke(genericParameter.ConstructType());
                }
            }
            else
            {
                window.result?.Invoke(genericParameter.ConstructType());
            }
        }

        public static void ConstructType(Metadata metadata)
        {
            metadata.RecordUndo();
            window.onChanged?.Invoke();
            genericParameter ??= new GenericParameter(typeof(object), typeof(object).DisplayName());
            if (genericParameter.type.type.IsGenericType && genericParameter.type.type.IsConstructedGenericType)
            {
                var newConstructedType = new GenericParameter(genericParameter, true);
                metadata.value = newConstructedType.ConstructType();
            }
            else if (genericParameter.type.type.IsArray)
            {
                var tempType = genericParameter.type.type.GetElementType();
                while (tempType.IsArray)
                {
                    tempType = tempType.GetElementType();
                }

                if (tempType.IsGenericType && tempType.IsConstructedGenericType)
                {
                    var newConstructedType = new GenericParameter(genericParameter, true);
                    metadata.value = newConstructedType.ConstructType();
                }
                else
                {
                    metadata.value = genericParameter.ConstructType();
                }
            }
            else
            {
                metadata.value = genericParameter.ConstructType();
            }
        }
    }
}