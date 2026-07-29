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
        private List<FakeGenericParameterType> fakeGenericParameterTypes = new List<FakeGenericParameterType>();

        public static TypeBuilderWindow Window { get; private set; }

        private static Metadata targetMetadata;
        private bool canMakeArrayTypeForBaseType;
        private bool triggerDropdownOnOpen;

        private static GUIStyle popupStyle;
        private static readonly GUIContent sharedContent = new GUIContent();

        private const float DefaultHeight = 320f;
        private const float MinWidth = 500f;
        private const float MaxWidth = 1000f;

        private static readonly Dictionary<(GenericParameterAttributes, Type[]), Type[]> constraintCache = new Dictionary<(GenericParameterAttributes, Type[]), Type[]>();

        public static bool Button(Type type, string nullType = "Select Type", TextAnchor textAnchor = TextAnchor.MiddleLeft, params GUILayoutOption[] options)
        {
            if (popupStyle == null)
            {
                popupStyle = new GUIStyle(EditorStyles.popup);
            }

            popupStyle.alignment = textAnchor;

            sharedContent.text = type != null ? type.As().CSharpName(false, false, false) : nullType;
            sharedContent.image = type?.GetTypeIcon();

            return GUILayout.Button(sharedContent, popupStyle, options);
        }

        public static void ShowWindow(
            Rect position,
            Metadata meta,
            bool canMakeArray = true,
            Type[] types = null,
            Action onBeforeChanged = null,
            Action<Type> onAfterChanged = null)
        {
            ShowWindowInternal(position, meta, null, meta?.value as Type, canMakeArray, types, null, onBeforeChanged, onAfterChanged);
        }

        public static void ShowWindow(
            Rect position,
            Metadata meta,
            bool canMakeArray = true,
            List<FakeGenericParameterType> fakeGenericParameterTypes = null,
            Action onBeforeChanged = null,
            Action<Type> onAfterChanged = null)
        {
            ShowWindowInternal(position, meta, null, meta?.value as Type, canMakeArray, Array.Empty<Type>(), fakeGenericParameterTypes, onBeforeChanged, onAfterChanged);
        }

        public static void ShowWindow(Rect position, Action<Type> result, Type currentType, bool canMakeArray = true, Type[] types = null, Action onBeforeChanged = null, Action<Type> onAfterChanged = null)
        {
            ShowWindowInternal(position, null, result, currentType, canMakeArray, types, null, onBeforeChanged, onAfterChanged);
        }

        public static void ShowWindow(Rect position, Action<Type> result, Type currentType, bool canMakeArray = true,
            List<FakeGenericParameterType> fakeGenericParameterTypes = null, Action onBeforeChanged = null, Action<Type> onAfterChanged = null)
        {
            ShowWindowInternal(position, null, result, currentType, canMakeArray, Array.Empty<Type>(), fakeGenericParameterTypes, onBeforeChanged, onAfterChanged);
        }

        private static void ShowWindowInternal(Rect position, Metadata meta, Action<Type> result, Type currentType, bool canMakeArray, Type[] types, List<FakeGenericParameterType> fakeGenerics, Action onBeforeChanged, Action<Type> onAfterChanged)
        {
            var window = GetWindow();

            targetMetadata = meta;
            window.result = result;

            if (fakeGenerics != null)
            {
                window.fakeGenericParameterTypes = fakeGenerics;
                window.settingAssemblyTypesLookup = MergeAssemblyTypes(fakeGenerics);
                window.baseTypeLookup = FilterBaseTypes(window.settingAssemblyTypesLookup);
            }

            ConfigureWindow(window, position, currentType, types, canMakeArray, onBeforeChanged, onAfterChanged);
        }

        private static TypeBuilderWindow GetWindow()
        {
            if (Window == null)
            {
                Window = CreateInstance<TypeBuilderWindow>();
            }
            return Window;
        }

        private static Type[] MergeAssemblyTypes(IEnumerable<Type> extra)
        {
            var baseTypes = Codebase.settingsAssembliesTypes;
            var result = new List<Type>(baseTypes.Count + 4)
            {
                typeof(void),
                typeof(Libraries.CSharp.Void)
            };

            result.AddRange(baseTypes);

            if (extra != null)
                result.AddRange(extra);

            return result.ToArray();
        }

        private static Type[] FilterBaseTypes(Type[] source)
        {
            var list = new List<Type>(source.Length);
            for (int i = 0; i < source.Length; i++)
            {
                var t = source[i];
                if (t != null && !NameUtility.TypeHasSpecialName(t))
                    list.Add(t);
            }
            return list.ToArray();
        }

        private static void ConfigureWindow(TypeBuilderWindow window, Rect position, Type type, Type[] types, bool canMakeArray, Action onBeforeChanged, Action<Type> onAfterChanged)
        {
            window.onBeforeChanged = onBeforeChanged;
            window.onAfterChanged = onAfterChanged;
            window.canMakeArrayTypeForBaseType = canMakeArray;

            if (type != null)
            {
                genericParameter = GenericParameter.Create(type, type.Name);
                genericParameter.AddGenericParameters(type, g => window.GetConstrainedTypes(g));
                window.baseType = genericParameter.ConstructType();
            }
            else
            {
                genericParameter.Clear();
                window.baseType = null;
            }

            position = GUIUtility.GUIToScreenRect(position);
            position.width = Mathf.Clamp(position.width, MinWidth, MaxWidth);

            if (types != null && types.Length > 0)
            {
                if (window.baseType == null)
                    window.baseType = types[0];

                window.customTypeLookup = types;
            }

            window.ShowAsDropDown(position, new Vector2(position.width, DefaultHeight));
        }

        private void OnEnable()
        {
            triggerDropdownOnOpen = true;
            minSize = new Vector2(MinWidth, MaxWidth);
            titleContent = new GUIContent("Type Builder");

            settingAssemblyTypesLookup = MergeAssemblyTypes(null);
            baseTypeLookup = FilterBaseTypes(settingAssemblyTypesLookup);
        }

        private IFuzzyOptionTree GetBaseTypeOptions() => new TypeBuilderTypeOptionTree(customTypeLookup ?? baseTypeLookup);

        private IFuzzyOptionTree GetNestedTypeOptions(GenericParameter parameter)
        {
            var constrainedTypes = GetConstrainedTypes(parameter);
            if (parameter?.type.type != null && parameter.type.type.IsArray && !constrainedTypes.Contains(parameter.type.type))
            {
                constrainedTypes = constrainedTypes.Append(parameter.type.type).ToArray();
            }
            return new TypeBuilderTypeOptionTree(constrainedTypes);
        }

        private Type[] GetConstrainedTypes(GenericParameter parameter)
        {
            if (parameter.constraints == null && (parameter.type.type.IsGenericParameter || parameter.type.type is FakeGenericParameterType))
            {
                var constraints = parameter.type.type.GetGenericParameterConstraints();

                if (constraints.Length > 0)
                {
                    parameter.constraints = settingAssemblyTypesLookup
                        .Where(candidateType => constraints.All(constraint => constraint.IsAssignableFrom(candidateType)))
                        .ToArray();
                }
                else
                {
                    var attributes = parameter.type.type.GenericParameterAttributes;
                    var typesSource = !parameter.HasParent && customTypeLookup != null ? customTypeLookup : settingAssemblyTypesLookup;
                    parameter.constraints = GetConstraintAttributeTypes(attributes, typesSource);
                }
            }
            return parameter.constraints;
        }

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
                    ((attributes & GenericParameterAttributes.NotNullableValueTypeConstraint) == 0 || (!candidateType.IsNullable() || candidateType.IsStruct())) &&
                    ((attributes & GenericParameterAttributes.DefaultConstructorConstraint) == 0 || candidateType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic).Any(c => c.GetParameters().Length == 0)) &&
                    !NameUtility.TypeHasSpecialName(candidateType))
                .ToArray();

            constraintCache[key] = constrainedTypes;
            return constrainedTypes;
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Escape && !IsMouseOverWindow())
            {
                Close();
                return;
            }

            HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 2, 2), () =>
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(false));

                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground, Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                {
                    EditorGUILayout.LabelField(new GUIContent("Type Builder", typeof(Type).Icon()?[IconSize.Small], "A tool to create and customize types beyond the standard Type Field capabilities"), LudiqStyles.centeredLabel);
                });

                var inheritButtonContent = new GUIContent(
                    baseType?.As().CSharpName(false, false, false) ?? "Select Type",
                    baseType.GetTypeIcon()
                );

                lastRect = GUILayoutUtility.GetLastRect();
                var buttonRect = DrawTypeField(inheritButtonContent, genericParameter, true);

                if (triggerDropdownOnOpen && Event.current.type == EventType.Repaint)
                {
                    triggerDropdownOnOpen = false;
                    TriggerDropdown(buttonRect);
                }

                if (genericParameter != null && (genericParameter.type.type.IsGenericType || GetArrayBase(genericParameter.type.type).IsGenericType))
                {
                    var index = 0;
                    foreach (var param in genericParameter.nestedParameters)
                    {
                        DrawGenericParameter(param, GetArrayBase(genericParameter.type.type).GetGenericTypeDefinition().GetGenericArguments()[index]);
                        index++;
                    }
                }

                EditorGUILayout.EndScrollView();

                var isValid = IsValidType(baseType);
                EditorGUI.BeginDisabledGroup(!isValid);

                if (!isValid && baseType != null)
                {
                    EditorGUILayout.HelpBox($"Cannot create partially constructed types, e.g. {baseType.As().CSharpName(false, false, false)} is invalid. Types must be set for: {string.Join(", ", GetInvalidParameters(baseType))}", MessageType.Error);
                }

                var e = Event.current;
                if (GUILayout.Button("Create Type") || (isValid && e != null && focusedWindow == this && e.type == EventType.KeyDown && e.keyCode == KeyCode.Return))
                {
                    if (targetMetadata != null)
                        ConstructType(targetMetadata);
                    else
                        ConstructType();

                    genericParameter.Clear();
                    Close();
                }
                EditorGUI.EndDisabledGroup();
            });
        }

        private void TriggerDropdown(Rect buttonRect)
        {
            int selectedIndex = Array.IndexOf(customTypeLookup ?? baseTypeLookup, typeof(object));
            LudiqGUI.FuzzyDropdown(buttonRect, GetBaseTypeOptions(), selectedIndex, (type) =>
            {
                var selectedType = (type as TypeBuilderType).Type;
                var genericParams = new GenericParameter(genericParameter, selectedType, selectedType.Name);
                baseType = selectedType;
                genericParameter?.Clear();
                genericParams.AddGenericParameters(selectedType);
                genericParameter = genericParams;
            });
        }

        private bool IsMouseOverWindow() => position.Contains(GUIUtility.GUIToScreenPoint(Event.current.mousePosition));

        private static Type GetArrayBase(Type type)
        {
            while (type != null && type.IsArray)
            {
                type = type.GetElementType();
            }
            return type;
        }
        private IEnumerable<string> GetInvalidParameters(Type type)
        {
            if (type == null || !type.ContainsGenericParameters)
                yield break;

            var current = GetArrayBase(type);

            if (current.IsGenericParameter)
            {
                yield return current.Name;
                yield break;
            }

            if (current.IsGenericType)
            {
                foreach (var arg in current.GetGenericArguments())
                {
                    foreach (var invalid in GetInvalidParameters(arg))
                    {
                        yield return invalid;
                    }
                }
            }
        }

        private bool IsValidType(Type type)
        {
            var elementType = GetArrayBase(type);
            return elementType != null && !elementType.ContainsGenericParameters;
        }

        private Rect DrawTypeField(GUIContent buttonContent, GenericParameter generic, bool isBaseType)
        {
            GUILayout.BeginHorizontal();
            var buttonRect = new Rect();

            if (GUILayout.Button(buttonContent, GUILayout.MaxHeight(19f)))
            {
                buttonRect = lastRect;
                var lookupSource = isBaseType && customTypeLookup != null ? customTypeLookup : baseTypeLookup;
                int selectedIndex = Array.IndexOf(lookupSource, generic?.type.type ?? typeof(object));

                LudiqGUI.FuzzyDropdown(lastRect, isBaseType ? GetBaseTypeOptions() : GetNestedTypeOptions(generic), selectedIndex, (type) =>
                {
                    var selectedType = (type as TypeBuilderType).Type;
                    if (isBaseType)
                    {
                        var genericParams = new GenericParameter(generic, selectedType, selectedType.Name);
                        baseType = selectedType;
                        genericParameter?.Clear();
                        genericParams.AddGenericParameters(selectedType);
                        genericParameter = genericParams;
                    }
                    else
                    {
                        generic.Clear();
                        generic.AddGenericParameters(selectedType);
                        generic.selectedType.type = selectedType;
                        generic.type.type = selectedType;
                        generic.parent.type.type = generic.parent.ConstructType();
                        baseType = genericParameter.ConstructType();
                    }
                });
            }

            bool canMakeArray = generic?.type.type != null && !generic.type.type.IsGenericParameter && CanTypeSupportArray(isBaseType ? genericParameter : generic);

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
                    if (isBaseType && genericParameter != null && (baseType.IsArray || baseType is FakeGenericParameterType { IsArray: true }))
                    {
                        baseType = baseType.GetElementType();
                        genericParameter.type.type = baseType;
                    }
                    else if (!isBaseType && (generic.type.type.IsArray || generic.type.type is FakeGenericParameterType { IsArray: true }))
                    {
                        generic.type.type = generic.type.type.GetElementType();
                        generic.parent.type.type = generic.parent.ConstructType();
                        baseType = genericParameter.ConstructType();
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();

            return buttonRect;
        }

        private bool CanTypeSupportArray(GenericParameter param)
        {
            if (param?.type.type == null || param.type.type == typeof(void)) return false;

            if (param.constraints != null && param.constraints.Length > 0)
            {
                return param.constraints.Any(constraint => constraint.IsAssignableFrom(typeof(Array)) || constraint.IsAssignableFrom(param.type.type.MakeArrayType()));
            }

            return true;
        }

        private void DrawGenericParameter(GenericParameter parameter, Type genericParam)
        {
            parameter.isOpen = HUMEditor.Foldout(parameter.isOpen, HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2, () =>
            {
                GUILayout.Label(parameter.type.type.As().CSharpName(false, false, false), LudiqStyles.centeredLabel);
            }, () =>
            {
                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(genericParam.As().CSharpName(false, false, false), GUILayout.Width(150));
                    var typeButtonContent = new GUIContent(parameter.type.type?.As().CSharpName(false, false, false) ?? "Select Type", parameter.type.type?.GetTypeIcon());

                    DrawTypeField(typeButtonContent, parameter, false);

                    GUILayout.EndHorizontal();

                    var type = GetArrayBase(parameter.type.type);

                    if (!type.IsGenericType)
                        return;

                    var genericArguments = type.GetGenericTypeDefinition().GetGenericArguments();

                    for (int i = 0; i < parameter.nestedParameters.Count; i++)
                    {
                        var nested = parameter.nestedParameters[i];

                        DrawGenericParameter(nested, genericArguments[i]);
                    }
                });
            });
        }

        public static void ConstructType() => ConstructTypeInternal(null);

        public static void ConstructType(Metadata metadata) => ConstructTypeInternal(metadata);

        private static void ConstructTypeInternal(Metadata metadata)
        {
            if (metadata != null)
            {
                metadata.RecordUndo();
            }
            else
            {
                UndoUtility.RecordEditedObject("TypeBuilder Constructed Type");
            }

            Window.onBeforeChanged?.Invoke();
            genericParameter ??= GenericParameter.Create(typeof(object), typeof(object).DisplayName());

            Type constructedType;
            var currentType = genericParameter.type.type;

            if ((currentType.IsGenericType && currentType.IsConstructedGenericType) || currentType.IsArray)
            {
                var tempType = GetArrayBase(currentType);
                if ((tempType != null && tempType.IsGenericType && tempType.IsConstructedGenericType) || currentType.IsGenericType)
                {
                    var newConstructedType = new GenericParameter(genericParameter, true);
                    constructedType = newConstructedType.ConstructType();
                }
                else
                {
                    constructedType = genericParameter.ConstructType();
                }
            }
            else
            {
                constructedType = genericParameter.ConstructType();
            }

            if (metadata != null)
            {
                metadata.value = constructedType;
            }
            else
            {
                Window.result?.Invoke(constructedType);
            }

            Window.onAfterChanged?.Invoke(constructedType);
        }
    }
}