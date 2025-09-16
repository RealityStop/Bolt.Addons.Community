using UnityEditor;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Reflection;

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
        private Action onBeforeChanged;
        private Action<Type> onAfterChanged;
        List<FakeGenericParameterType> fakeGenericParameterTypes = new List<FakeGenericParameterType>();
        public static TypeBuilderWindow Window { get; private set; }

        private static Metadata targetMetadata;

        private bool canMakeArrayTypeForBaseType;
        public static bool Button(Type type, string nullType = "Select Type", params GUILayoutOption[] options)
        {
            GUIContent TypebuilderButtonContent = new(type?.As().CSharpName(false, false, false) ?? nullType, type.GetTypeIcon());
            return GUILayout.Button(TypebuilderButtonContent, EditorStyles.popup, options);
        }
        public static void ShowWindow(Rect position, Metadata Meta, bool canMakeArray = true, Type[] types = null, Action onBeforeChanged = null, Action<Type> onAfterChanged = null)
        {
            if (Window == null)
            {
                Window = CreateInstance<TypeBuilderWindow>();
            }
            targetMetadata = Meta;
            var type = Meta?.value as Type;
            ConfigureWindow(Window, position, type, types, canMakeArray, onBeforeChanged, onAfterChanged);
        }

        public static void ShowWindow(Rect position, Metadata Meta, bool canMakeArray = true, List<FakeGenericParameterType> fakeGenericParameterTypes = null, Action onBeforeChanged = null, Action<Type> onAfterChanged = null)
        {
            if (Window == null)
            {
                Window = CreateInstance<TypeBuilderWindow>();
            }
            targetMetadata = Meta;
            var type = Meta?.value as Type;
            Window.fakeGenericParameterTypes = fakeGenericParameterTypes;
            Window.settingAssemblyTypesLookup = Window.settingAssemblyTypesLookup.Concat(fakeGenericParameterTypes).ToArray();
            Window.baseTypeLookup = Window.settingAssemblyTypesLookup.Where(t => !NameUtility.TypeHasSpecialName(t) && t != null)
                .ToArray();
            ConfigureWindow(Window, position, type, new Type[0], canMakeArray, onBeforeChanged, onAfterChanged);
        }

        public static void ShowWindow(Rect position, Action<Type> result, Type currentType, bool canMakeArray = true, Type[] types = null, Action onBeforeChanged = null, Action<Type> onAfterChanged = null)
        {
            if (Window == null)
            {
                Window = CreateInstance<TypeBuilderWindow>();
            }
            Window.result = result;
            targetMetadata = null;
            ConfigureWindow(Window, position, currentType, types, canMakeArray, onBeforeChanged, onAfterChanged);
        }

        public static void ShowWindow(Rect position, Action<Type> result, Type currentType, bool canMakeArray = true, List<FakeGenericParameterType> fakeGenericParameterTypes = null, Action onBeforeChanged = null, Action<Type> onAfterChanged = null)
        {
            if (Window == null)
            {
                Window = CreateInstance<TypeBuilderWindow>();
            }
            Window.result = result;
            targetMetadata = null;
            Window.fakeGenericParameterTypes = fakeGenericParameterTypes;
            Window.settingAssemblyTypesLookup = Window.settingAssemblyTypesLookup.Concat(fakeGenericParameterTypes).Where(t => t.IsPublic).ToArray();
            Window.baseTypeLookup = Window.settingAssemblyTypesLookup.Where(t => t != null && !NameUtility.TypeHasSpecialName(t))
                .ToArray();
            ConfigureWindow(Window, position, currentType, new Type[0], canMakeArray, onBeforeChanged, onAfterChanged);
        }
        const float DefaultHeight = 320f;
        const float MinWidth = 300f;
        const float MaxWidth = 1000f;
        private bool triggerDropdownOnOpen = false;
        private static void ConfigureWindow(TypeBuilderWindow window, Rect position, Type type, Type[] types, bool canMakeArray, Action onBeforeChanged, Action<Type> onAfterChanged)
        {
            window.onBeforeChanged = onBeforeChanged;
            window.onAfterChanged = onAfterChanged;
            window.canMakeArrayTypeForBaseType = canMakeArray;

            if (type != null)
            {
                genericParameter = GenericParameter.Create(type, type.Name);
                genericParameter.AddGenericParameters(type, (generic) => window.GetConstrainedTypes(generic));
                window.baseType = genericParameter.ConstructType();
            }
            else
            {
                genericParameter.Clear();
            }

            position = GUIUtility.GUIToScreenRect(position);
            position.width = Mathf.Clamp(position.width, MinWidth, MaxWidth);
            window.minSize = new Vector2(300, 200);
            window.titleContent = new GUIContent("Type Builder");

            if (types?.Length > 0)
            {
                if (window.baseType == null)
                    window.baseType = types.First();
                window.customTypeLookup = types.Concat(window.fakeGenericParameterTypes).ToArray();
            }

            var initialSize = new Vector2(position.width, DefaultHeight);
            window.ShowAsDropDown(position, initialSize);
        }

        private void OnEnable()
        {
            triggerDropdownOnOpen = true;
            settingAssemblyTypesLookup = Codebase.settingsAssembliesTypes.ToArray();
            baseTypeLookup = settingAssemblyTypesLookup.Where(t => !NameUtility.TypeHasSpecialName(t) && t != null)
                .ToArray();
        }

        private IFuzzyOptionTree GetBaseTypeOptions()
        {
            return new TypeBuilderTypeOptionTree(customTypeLookup ?? baseTypeLookup);
        }

        private IFuzzyOptionTree GetNestedTypeOptions(GenericParameter parameter)
        {
            var constrainedTypes = GetConstrainedTypes(parameter);
            if (parameter != null && parameter.type.type.IsArray && !constrainedTypes.Contains(parameter.type.type)) constrainedTypes.Append(parameter.type.type);
            return new TypeBuilderTypeOptionTree(constrainedTypes);
        }

        private Type[] GetConstrainedTypes(GenericParameter genericParameter)
        {
            if (genericParameter.constraints == null && (genericParameter.type.type.IsGenericParameter || genericParameter.type.type is FakeGenericParameterType))
            {
                var constraints = genericParameter.type.type.GetGenericParameterConstraints();

                if (constraints.Length > 0)
                {
                    var constrainedTypes = settingAssemblyTypesLookup
                        .AsParallel()
                        .Where(candidateType => constraints.All(constraint => constraint.IsAssignableFrom(candidateType)))
                        .ToArray();

                    genericParameter.constraints = constrainedTypes;
                    return constrainedTypes;
                }
                else
                {
                    var attributes = genericParameter.type.type.GenericParameterAttributes;
                    var constrainedTypes = GetConstraintAttributeTypes(attributes,
                        !genericParameter.HasParent && customTypeLookup != null
                            ? customTypeLookup
                            : settingAssemblyTypesLookup);

                    genericParameter.constraints = constrainedTypes;
                    return constrainedTypes;
                }
            }
            return genericParameter.constraints;
        }

        private static readonly Dictionary<(GenericParameterAttributes, Type[]), Type[]> constraintCache = new();
        public Type[] GetConstraintAttributeTypes(GenericParameterAttributes attributes, Type[] typesLookup)
        {
            var key = (attributes, typesLookup);
            if (constraintCache.TryGetValue(key, out var cachedResult))
            {
                return cachedResult;
            }

            var constrainedTypes = typesLookup
                .Where(candidateType =>
                    ((attributes & GenericParameterAttributes.ReferenceTypeConstraint) == 0 || candidateType.IsClass) &&
                    ((attributes & GenericParameterAttributes.NotNullableValueTypeConstraint) == 0 ||
                        (!candidateType.IsNullable() || candidateType.IsStruct())) &&
                    ((attributes & GenericParameterAttributes.DefaultConstructorConstraint) == 0 ||
                        candidateType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic)
                            .Any(constructor => constructor.GetParameters().Length == 0)) &&
                    !NameUtility.TypeHasSpecialName(candidateType))
                .ToArray();

            constraintCache[key] = constrainedTypes;
            return constrainedTypes;
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Escape)
            {
                if (!IsMouseOverWindow())
                {
                    Close();
                }
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
                    baseType?.As().CSharpName(false, false, false) ?? "Select Type",
                    baseType.GetTypeIcon()
                );

                lastRect = GUILayoutUtility.GetLastRect();
                var buttonWidth = GUI.skin.button.CalcSize(inheritButtonContent).x;
                contentWidth = Mathf.Max(contentWidth, buttonWidth);
                var (size, buttonRect) = DrawTypeField(inheritButtonContent, genericParameter, true);
                if (triggerDropdownOnOpen && Event.current.type == EventType.Repaint)
                {
                    triggerDropdownOnOpen = false;
                    TriggerDropdown(buttonRect);
                }
                if (genericParameter != null && genericParameter.type.type.IsGenericType || GetArrayBase(genericParameter.type.type).IsGenericType)
                {
                    var index = 0;
                    foreach (var param in genericParameter.nestedParameters)
                    {
                        contentWidth = Mathf.Max(contentWidth, DrawGenericParameter(param, GetArrayBase(genericParameter.type.type).GetGenericTypeDefinition().GetGenericArguments()[index]));
                        index++;
                    }
                }

                EditorGUILayout.EndScrollView();
                EditorGUI.BeginDisabledGroup(!IsValidType(baseType, false));
                if (!IsValidType(baseType, false))
                {
                    if (baseType != null)
                        EditorGUILayout.HelpBox($"Can not create arrays of Open Generics, e.g {baseType.As().CSharpName(false, false, false).RemoveHighlights().RemoveMarkdown()} is invalid it has to have a types set for {string.Join(", ", GetInvalidParameters(baseType))}", MessageType.Error);
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

                Window.position = new Rect(position.x, position.y, contentWidth + 50, position.height);
            });
        }
        private void TriggerDropdown(Rect buttonRect)
        {
            int selectedIndex = Array.IndexOf(customTypeLookup ?? baseTypeLookup, typeof(object));
            Type _selected = null;
            LudiqGUI.FuzzyDropdown(buttonRect, GetBaseTypeOptions(), selectedIndex, (type) =>
            {
                _selected = (type as TypeBuilderType).Type;
                var genericParams = new GenericParameter(genericParameter, _selected, _selected.Name);
                baseType = _selected;
                genericParameter?.Clear();
                genericParams.AddGenericParameters(_selected);
                genericParameter = genericParams;
            });
        }
        private bool IsMouseOverWindow()
        {
            return position.Contains(GUIUtility.GUIToScreenPoint(Event.current.mousePosition));
        }

        private Type GetArrayBase(Type type)
        {
            if (type.IsArray)
            {
                type = type.GetElementType();
                while (type.IsArray)
                {
                    type = type.GetElementType();
                }
                return type;
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
                    var baseType = GetArrayBase(arg);
                    if (baseType.IsGenericTypeParameter)
                    {
                        yield return baseType.Name;
                    }
                    else if (baseType.IsGenericType)
                    {
                        foreach (var invalidArg in GetInvalidParameters(baseType))
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

        private (float, Rect) DrawTypeField(GUIContent buttonContent, GenericParameter generic, bool isBaseType)
        {
            GUILayout.BeginHorizontal();
            Rect buttonRect = new();
            if (GUILayout.Button(buttonContent, GUILayout.MaxHeight(19f)))
            {
                buttonRect = lastRect;
                int selectedIndex = Array.IndexOf(isBaseType && customTypeLookup != null ? customTypeLookup : baseTypeLookup, generic != null ? generic.type : typeof(object));
                Type _selected = null;
                LudiqGUI.FuzzyDropdown(lastRect, isBaseType ? GetBaseTypeOptions() : GetNestedTypeOptions(generic), selectedIndex, (type) =>
                {
                    _selected = (type as TypeBuilderType).Type;
                    if (isBaseType)
                    {
                        var genericParams = new GenericParameter(generic, _selected, _selected.Name);
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
                else if (GUILayout.Button("-"))
                {
                    if (isBaseType && genericParameter != null)
                    {
                        if (baseType.IsArray || (baseType is FakeGenericParameterType fakeGenericParameterType && fakeGenericParameterType._isArrayType))
                        {
                            baseType = baseType.GetElementType();
                            genericParameter.type.type = baseType;
                        }
                    }
                    else
                    {
                        if (generic.type.type.IsArray || (generic.type.type is FakeGenericParameterType fakeGenericParameterType && fakeGenericParameterType._isArrayType))
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
            return (buttonWidth, buttonRect);
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
                    GUIContent typeButtonContent = new(parameter.type.type?.As().CSharpName(false, false, false) ?? "Select Type", parameter.type.type?.GetTypeIcon());

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
            Window.onBeforeChanged?.Invoke();
            genericParameter ??= GenericParameter.Create(typeof(object), typeof(object).DisplayName());
            Type constructedType;
            if (genericParameter.type.type.IsGenericType && genericParameter.type.type.IsConstructedGenericType)
            {
                var newConstructedType = new GenericParameter(genericParameter, true);
                constructedType = newConstructedType.ConstructType();
                Window.result?.Invoke(constructedType);
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
                    constructedType = newConstructedType.ConstructType();
                    Window.result?.Invoke(constructedType);
                }
                else
                {
                    constructedType = genericParameter.ConstructType();
                    Window.result?.Invoke(constructedType);
                }
            }
            else
            {
                constructedType = genericParameter.ConstructType();
                Window.result?.Invoke(constructedType);
            }
            Window.onAfterChanged?.Invoke(constructedType);
        }

        public static void ConstructType(Metadata metadata)
        {
            metadata.RecordUndo();
            Window.onBeforeChanged?.Invoke();
            genericParameter ??= GenericParameter.Create(typeof(object), typeof(object).DisplayName());
            Type constructedType;
            if (genericParameter.type.type.IsGenericType && genericParameter.type.type.IsConstructedGenericType)
            {
                var newConstructedType = new GenericParameter(genericParameter, true);
                constructedType = newConstructedType.ConstructType();
                metadata.value = constructedType;
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
                    constructedType = newConstructedType.ConstructType();
                    metadata.value = constructedType;
                }
                else
                {
                    constructedType = genericParameter.ConstructType();
                    metadata.value = constructedType;
                }
            }
            else
            {
                constructedType = genericParameter.ConstructType();
                metadata.value = constructedType;
            }
            Window.onAfterChanged?.Invoke(constructedType);
        }
    }
}