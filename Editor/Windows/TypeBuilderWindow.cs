using UnityEditor;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityObject = UnityEngine.Object;
using System.Reflection;
using System.Threading;
using System.Runtime.CompilerServices;

namespace Unity.VisualScripting.Community
{
    public class TypeBuilderWindow : EditorWindow
    {
        private Type _baseType = typeof(object);
        private static GenericParameter _genericParameters = new GenericParameter(typeof(object), typeof(object).Name);
        private Vector2 _scrollPosition;
        private Rect _lastRect;
        private Type[] baseTypeLookup;
        private Type[] allPossibleTypeLookup;
        private Type[] customTypeLookup;

        public static TypeBuilderWindow _window { get; private set; }

        private static Metadata targetMetadata;

        private bool canMakeArrayTypeForBaseType;

        public static void ShowWindow(Rect position, Metadata Meta, bool canMakeArray = true, Type[] types = null)
        {
            if (_window == null)
            {
                _window = CreateInstance<TypeBuilderWindow>();
            }
            _window.canMakeArrayTypeForBaseType = canMakeArray;
            targetMetadata = Meta;
            if (Meta != null)
            {
                _window.OnEnable();
                var type = Meta.value as Type;
                if (type != null)
                {
                    if (type.IsGenericType)
                    {
                        _genericParameters = new GenericParameter(type, type.Name);
                        _genericParameters.AddGenericParameters(type, (generic) => _window.GetConstrainedTypes(generic));
                        _window._baseType = _genericParameters.ConstructType();
                    }
                    else if (type.IsArray)
                    {
                        var tempType = type.GetElementType();
                        while (tempType.IsArray)
                        {
                            tempType = tempType.GetElementType();
                        }

                        _genericParameters = new GenericParameter(tempType, tempType.Name);
                        _genericParameters.AddGenericParameters(tempType, (generic) => _window.GetConstrainedTypes(generic));
                        _window._baseType = _genericParameters.ConstructType();
                    }
                    else
                    {
                        _genericParameters = new GenericParameter(type, type.Name);
                        _genericParameters.AddGenericParameters(type, (generic) => _window.GetConstrainedTypes(generic));
                        _window._baseType = _genericParameters.ConstructType();
                    }
                }
                else
                {
                    _genericParameters.Clear();
                }
            }
            position = GUIUtility.GUIToScreenRect(position);
            position.width = Mathf.Clamp(position.width, 300, 600);
            _window.minSize = new Vector2(300, 200);
            _window.titleContent = new GUIContent("Type Builder");
            if (types != null)
            {
                _window.customTypeLookup = types;
            }
            var initialSize = new Vector2(position.width, 320);

            _window.ShowAsDropDown(position, initialSize);
        }

        private void OnEnable()
        {
            allPossibleTypeLookup = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).ToArray();
            var allTypeLookup = Codebase.settingsAssembliesTypes.ToArray();
            baseTypeLookup = allTypeLookup
                .Except(allTypeLookup.Where(t => TypeHasSpecialName(t) || t == null))
                .ToArray();
        }

        private bool TypeHasSpecialName(Type t)
        {
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
                    var constrainedTypes = allPossibleTypeLookup.Where(t =>
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
                    var constrainedTypes = GetConstraintAttributeTypes(attributes, !param.HasParent && customTypeLookup != null ? customTypeLookup : allPossibleTypeLookup);
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
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandWidth(false));
                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground, Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                {
                    EditorGUILayout.LabelField(new GUIContent("Type Builder", typeof(Type).Icon()?[IconSize.Small], "A tool to create and customize types beyond the standard Type Field capabilities"), LudiqStyles.centeredLabel);
                    var labelWidth = GUI.skin.label.CalcSize(new GUIContent("Select Type")).x;
                    contentWidth = Mathf.Max(contentWidth, labelWidth);
                });

                GUIContent inheritButtonContent = new GUIContent(
                    _baseType?.As().CSharpName(false).RemoveHighlights().RemoveMarkdown() ?? "Select Type",
                    _baseType?.Icon()?[IconSize.Small]
                );

                _lastRect = GUILayoutUtility.GetLastRect();
                var buttonWidth = GUI.skin.button.CalcSize(inheritButtonContent).x;
                contentWidth = Mathf.Max(contentWidth, buttonWidth);
                DrawTypeField(inheritButtonContent, _genericParameters, true);

                if (_genericParameters != null)
                {
                    var index = 0;
                    foreach (var param in _genericParameters.nestedParameters)
                    {
                        contentWidth = Mathf.Max(contentWidth, DrawGenericParameter(param, _genericParameters.type.type.GetGenericTypeDefinition().GetGenericArguments()[index]));
                        index++;
                    }
                }

                EditorGUILayout.EndScrollView();
                EditorGUI.BeginDisabledGroup(!IsValidType(_baseType, false));
                if (!IsValidType(_baseType, false))
                {
                    EditorGUILayout.HelpBox($"Can not create arrays of Open Generics, e.g {_baseType.As().CSharpName(false, false, false).RemoveHighlights().RemoveMarkdown()} is invalid it has to have a types set for {string.Join(", ", GetInvalidParameters(_baseType))}", MessageType.Warning);
                }
                if (GUILayout.Button("Create Type"))
                {
                    ConstructType(targetMetadata);
                    _genericParameters.Clear();
                    Close();
                }
                EditorGUI.EndDisabledGroup();

                // Adjust the window width to fit the content
                _window.position = new Rect(position.x, position.y, contentWidth + 50, position.height);
            });
        }

        private IEnumerable<string> GetInvalidParameters(Type type)
        {
            if (type.IsArray)
            {
                type = type.GetElementType();
                while (type.IsArray)
                {
                    type = type.GetElementType();
                }
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
                var elementType = type.GetElementType();
                while (elementType.IsArray)
                {
                    elementType = elementType.GetElementType();
                }

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
                    var elementType = type.GetElementType();
                    while (elementType.IsArray)
                    {
                        elementType = elementType.GetElementType();
                    }

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

        private float DrawTypeField(GUIContent buttonContent, GenericParameter generic, bool isBase)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(buttonContent, GUILayout.MaxHeight(19f)))
            {
                int selectedIndex = Array.IndexOf(isBase && customTypeLookup != null ? customTypeLookup : baseTypeLookup, generic != null ? generic.type : typeof(object));
                Type _selected = null;
                LudiqGUI.FuzzyDropdown(_lastRect, isBase ? GetBaseTypeOptions() : GetNestedTypeOptions(generic), selectedIndex, (type) =>
                {
                    _selected = (type as TypeBuilderType).Type;

                    if (isBase)
                    {
                        var genericParams = new GenericParameter(_selected, _selected.Name);
                        _baseType = _selected;
                        _genericParameters?.Clear();
                        genericParams.AddGenericParameters(_selected);
                        _genericParameters = genericParams;
                    }
                    else
                    {
                        generic.Clear();
                        generic.AddGenericParameters(_selected);
                        generic.selectedType.type = _selected;
                        generic.type.type = _selected;
                        generic.parent.type.type = generic.parent.ConstructType();
                        _baseType = _genericParameters.ConstructType();
                    }
                });
            }
            if ((isBase && canMakeArrayTypeForBaseType) || (!isBase && !generic.type.type.IsGenericParameter))
            {
                if (GUILayout.Button("As Array"))
                {
                    if (isBase && _genericParameters != null)
                    {
                        _baseType = _baseType.MakeArrayType();
                        _genericParameters.type.type = _baseType;
                    }
                    else
                    {
                        generic.type.type = generic.type.type.MakeArrayType();
                        generic.parent.type.type = generic.parent.ConstructType();
                        _baseType = _genericParameters.ConstructType();
                    }
                }
            }
            GUILayout.EndHorizontal();
            var buttonWidth = GUI.skin.button.CalcSize(buttonContent).x;
            return buttonWidth;
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

        public static Type ConstructType()
        {
            return _genericParameters.ConstructType();
        }

        public static void ConstructType(Metadata metadata)
        {
            if (_genericParameters == null) _genericParameters = new GenericParameter(typeof(object), typeof(object).DisplayName());
            if (_genericParameters.type.type.IsGenericType && _genericParameters.type.type.IsConstructedGenericType)
            {
                var newConstructedType = new GenericParameter(_genericParameters, true);
                metadata.value = newConstructedType.ConstructType();
            }
            else if (_genericParameters.type.type.IsArray)
            {
                var tempType = _genericParameters.type.type.GetElementType();
                while (tempType.IsArray)
                {
                    tempType = tempType.GetElementType();
                }

                if (tempType.IsGenericType && tempType.IsConstructedGenericType)
                {
                    var newConstructedType = new GenericParameter(_genericParameters, true);
                    metadata.value = newConstructedType.ConstructType();
                }
                else
                {
                    metadata.value = _genericParameters.ConstructType();
                }
            }
            else
            {
                metadata.value = _genericParameters.ConstructType();
            }
        }
    }
}

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(TypeBuilderType))]
    public class TypeBuilderTypeOption : DocumentedOption<TypeBuilderType>
    {
        public TypeBuilderTypeOption(Type type)
        {
            value = new TypeBuilderType() { Type = type };
            label = type.DisplayName();
            UnityAPI.Async(() => icon = type.Icon());
            documentation = type.Documentation();
            zoom = true;
        }

        public TypeBuilderTypeOption(Type type, bool parentOnly) : this(type)
        {
            this.parentOnly = parentOnly;
        }

        public static string Haystack(Type type)
        {
            return type.DisplayName();
        }

        public static string SearchResultLabel(Type type, string query)
        {
            return $"{SearchUtility.HighlightQuery(type.DisplayName(), query)} <color=#{ColorPalette.unityForegroundDim.ToHexString()}>(in {type.Namespace().DisplayName()})</color>";
        }
    }

    public class TypeBuilderType
    {
        public Type Type;
    }

    public class TypeBuilderTypeOptionTree : FuzzyOptionTree
    {
        public enum RootMode
        {
            Types,
            Namespaces
        }

        private TypeBuilderTypeOptionTree() : base(new GUIContent("Type")) { }

        public override IFuzzyOption Option(object item)
        {
            if (item is Namespace)
            {
                return new NamespaceOption((Namespace)item, true);
            }
            else if (item is Type)
            {
                return new TypeBuilderTypeOption((Type)item);
            }

            return base.Option(item);
        }

        public TypeBuilderTypeOptionTree(IEnumerable<Type> types) : this()
        {
            Ensure.That(nameof(types)).IsNotNull(types);
            this.types = types.ToHashSet();
        }

        public TypeBuilderTypeOptionTree(IEnumerable<Type> typeSet, TypeFilter filter) : this()
        {
            Ensure.That(nameof(typeSet)).IsNotNull(typeSet);
            Ensure.That(nameof(filter)).IsNotNull(filter);
            this.typeSet = typeSet;
            this.filter = filter;
        }

        private readonly IEnumerable<Type> typeSet;

        private readonly TypeFilter filter;

        private HashSet<Type> types;

        public RootMode rootMode { get; set; } = RootMode.Namespaces;

        public override void Prewarm()
        {
            base.Prewarm();

            types ??= typeSet.Concat(typeSet.Where(t => !t.IsStatic()).Select(t => typeof(List<>).MakeGenericType(t))) // Add lists
                .Where(filter.Configured().ValidateType) // Filter
                .ToHashSet();

            groupEnums = !types.All(t => t.IsEnum);
        }

        #region Configuration

        public bool groupEnums { get; set; } = true;

        public bool surfaceCommonTypes { get; set; } = true;

        #endregion

        #region Hierarchy

        private readonly FuzzyGroup enumsGroup = new FuzzyGroup("(Enums)", typeof(Enum).Icon());

        public override IEnumerable<object> Root()
        {
            if (rootMode == RootMode.Namespaces)
            {
                if (surfaceCommonTypes)
                {
                    foreach (var type in EditorTypeUtility.commonTypes)
                    {
                        if (types.Contains(type))
                        {
                            yield return type;
                        }
                    }
                }

                foreach (var @namespace in types.Where(t => !(groupEnums && t.IsEnum))
                         .Select(t => t.Namespace().Root)
                         .Distinct()
                         .OrderBy(ns => ns.DisplayName(false))
                         .Cast<object>())
                {
                    yield return @namespace;
                }

                if (groupEnums && types.Any(t => t.IsEnum))
                {
                    yield return enumsGroup;
                }
            }
            else if (rootMode == RootMode.Types)
            {
                foreach (var type in types)
                {
                    yield return type;
                }
            }
            else
            {
                throw new UnexpectedEnumValueException<RootMode>(rootMode);
            }
        }

        public override IEnumerable<object> Children(object parent)
        {
            if (parent is Namespace)
            {
                var @namespace = (Namespace)parent;

                if (!@namespace.IsGlobal)
                {
                    foreach (var childNamespace in types.Where(t => !(groupEnums && t.IsEnum))
                             .SelectMany(t => t.Namespace().AndAncestors())
                             .Distinct()
                             .Where(ns => ns.Parent == @namespace)
                             .OrderBy(ns => ns.DisplayName(false)))
                    {
                        yield return childNamespace;
                    }
                }

                foreach (var type in types.Where(t => t.Namespace() == @namespace)
                         .Where(t => !(groupEnums && t.IsEnum))
                         .OrderBy(t => t.DisplayName()))
                {
                    yield return type;
                }
            }
            else if (parent == enumsGroup)
            {
                foreach (var type in types.Where(t => t.IsEnum)
                         .OrderBy(t => t.DisplayName()))
                {
                    yield return type;
                }
            }
        }

        #endregion

        #region Search

        public override bool searchable { get; } = true;

        public override IEnumerable<ISearchResult> SearchResults(string query, CancellationToken cancellation)
        {
            return types.OrderableSearchFilter(query, TypeBuilderTypeOption.Haystack).Cast<ISearchResult>();
        }

        public override string SearchResultLabel(object item, string query)
        {
            return TypeBuilderTypeOption.SearchResultLabel((Type)item, query);
        }

        #endregion
    }

    public static class NameUtility
    {
        private static readonly Dictionary<Type, string> humanPrimitiveNames = new Dictionary<Type, string>
        {
            { typeof(byte), "Byte" },
            { typeof(sbyte), "Signed Byte" },
            { typeof(short), "Short" },
            { typeof(ushort), "Unsigned Short" },
            { typeof(int), "Integer" },
            { typeof(uint), "Unsigned Integer" },
            { typeof(long), "Long" },
            { typeof(ulong), "Unsigned Long" },
            { typeof(float), "Float" },
            { typeof(double), "Double" },
            { typeof(decimal), "Decimal" },
            { typeof(string), "String" },
            { typeof(char), "Character" },
            { typeof(bool), "Boolean" },
            { typeof(void), "Void" },
            { typeof(object), "Object" },
        };

        public static readonly Dictionary<string, string> humanOperatorNames = new Dictionary<string, string>
        {
            { "op_Addition", "Add" },
            { "op_Subtraction", "Subtract" },
            { "op_Multiply", "Multiply" },
            { "op_Division", "Divide" },
            { "op_Modulus", "Modulo" },
            { "op_ExclusiveOr", "Exclusive Or" },
            { "op_BitwiseAnd", "Bitwise And" },
            { "op_BitwiseOr", "Bitwise Or" },
            { "op_LogicalAnd", "Logical And" },
            { "op_LogicalOr", "Logical Or" },
            { "op_Assign", "Assign" },
            { "op_LeftShift", "Left Shift" },
            { "op_RightShift", "Right Shift" },
            { "op_Equality", "Equals" },
            { "op_GreaterThan", "Greater Than" },
            { "op_LessThan", "Less Than" },
            { "op_Inequality", "Not Equals" },
            { "op_GreaterThanOrEqual", "Greater Than Or Equals" },
            { "op_LessThanOrEqual", "Less Than Or Equals" },
            { "op_MultiplicationAssignment", "Multiplication Assignment" },
            { "op_SubtractionAssignment", "Subtraction Assignment" },
            { "op_ExclusiveOrAssignment", "Exclusive Or Assignment" },
            { "op_LeftShiftAssignment", "Left Shift Assignment" },
            { "op_ModulusAssignment", "Modulus Assignment" },
            { "op_AdditionAssignment", "Addition Assignment" },
            { "op_BitwiseAndAssignment", "Bitwise And Assignment" },
            { "op_BitwiseOrAssignment", "Bitwise Or Assignment" },
            { "op_Comma", "Comma" },
            { "op_DivisionAssignment", "Division Assignment" },
            { "op_Decrement", "Decrement" },
            { "op_Increment", "Increment" },
            { "op_UnaryNegation", "Negate" },
            { "op_UnaryPlus", "Positive" },
            { "op_OnesComplement", "One's Complement" },
        };

        private static readonly HashSet<string> booleanVerbs = new HashSet<string>
        {
            "Is",
            "Can",
            "Has",
            "Are",
            "Will",
            "Was",
            "Had",
            "Were"
        };

        public static string SelectedName(this Type type, bool human, bool includeGenericParameters = true)
        {
            return human ? type.HumanName(includeGenericParameters) : type.CSharpName(includeGenericParameters);
        }

        public static string SelectedName(this MemberInfo member, bool human, ActionDirection direction = ActionDirection.Any, bool expectingBoolean = false)
        {
            return human ? member.HumanName(direction) : member.CSharpName(direction);
        }

        public static string SelectedName(this ParameterInfo parameter, bool human)
        {
            return human ? parameter.HumanName() : parameter.Name;
        }

        public static string SelectedName(this Exception exception, bool human)
        {
            return human ? exception.HumanName() : exception.GetType().CSharpName(false);
        }

        public static string SelectedName(this Enum @enum, bool human)
        {
            return human ? HumanName(@enum) : @enum.ToString();
        }

        public static string SelectedName(this Namespace @namespace, bool human, bool full = true)
        {
            return human ? @namespace.HumanName(full) : @namespace.CSharpName(full);
        }

        public static string SelectedParameterString(this MethodBase methodBase, Type targetType, bool human)
        {
            return string.Join(", ", methodBase.GetInvocationParameters(targetType).Select(p => p.SelectedName(human)).ToArray());
        }

        public static string DisplayName(this Type type, bool includeGenericParameters = true)
        {
            return SelectedName(type, BoltCore.Configuration.humanNaming, includeGenericParameters);
        }

        public static string DisplayName(this MemberInfo member, ActionDirection direction = ActionDirection.Any, bool expectingBoolean = false)
        {
            return SelectedName(member, BoltCore.Configuration.humanNaming, direction, expectingBoolean);
        }

        public static string DisplayName(this ParameterInfo parameter)
        {
            return SelectedName(parameter, BoltCore.Configuration.humanNaming);
        }

        public static string DisplayName(this Exception exception)
        {
            return SelectedName(exception, BoltCore.Configuration.humanNaming);
        }

        public static string DisplayName(this Enum @enum)
        {
            return SelectedName(@enum, BoltCore.Configuration.humanNaming);
        }

        public static string DisplayName(this Namespace @namespace, bool full = true)
        {
            return SelectedName(@namespace, BoltCore.Configuration.humanNaming, full);
        }

        public static string DisplayParameterString(this MethodBase methodBase, Type targetType)
        {
            return SelectedParameterString(methodBase, targetType, BoltCore.Configuration.humanNaming);
        }

        public static string HumanName(this Type type, bool includeGenericParameters = true)
        {
            if(type == null)
            {
                return "";
            }

            if (type == typeof(UnityObject))
            {
                return "Unity Object";
            }

            if (humanPrimitiveNames.ContainsKey(type))
            {
                return humanPrimitiveNames[type];
            }
            else if (type.IsGenericParameter)
            {
                var genericParameterName = type.Name.Prettify();

                if (genericParameterName == "T")
                {
                    return "Generic";
                }
                else if (genericParameterName.StartsWith("T "))
                {
                    return genericParameterName.Substring(2).Prettify() + " Generic";
                }
                else
                {
                    return genericParameterName.Prettify();
                }
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var nonNullable = Nullable.GetUnderlyingType(type);

                var underlyingName = nonNullable.HumanName(includeGenericParameters);

                return "Nullable " + underlyingName;
            }
            else if (type.IsArray)
            {
                var tempType = type.GetElementType();
                var arrayString = "[]";
                while (tempType.IsArray)
                {
                    tempType = tempType.GetElementType();
                    arrayString += "[]";
                }

                var tempTypeName = tempType.HumanName(includeGenericParameters);
                return tempTypeName + arrayString;
            }
            else
            {
                string name;

                if (Attribute.GetCustomAttribute(type, typeof(System.ComponentModel.DisplayNameAttribute)) is System.ComponentModel.DisplayNameAttribute displayNameAttribute)
                    name = displayNameAttribute.DisplayName;
                else
                    name = type.Name.Prettify();

                if (type.IsInterface && name.StartsWith("I "))
                {
                    name = name.Substring(2) + " Interface";
                }

                if (type.IsArray && name.Contains("[]"))
                {
                    name = name.Replace("[]", " Array");
                }

                if (type.IsGenericType && name.Contains('`'))
                {
                    name = name.Substring(0, name.IndexOf('`'));
                }

                var genericArguments = (IEnumerable<Type>)type.GetGenericArguments();

                if (type.IsNested)
                {
                    name += " of " + type.DeclaringType.HumanName(includeGenericParameters);

                    if (type.DeclaringType.IsGenericType)
                    {
                        genericArguments.Skip(type.DeclaringType.GetGenericArguments().Length);
                    }
                }

                if (genericArguments.Any())
                {
                    if (type.ContainsGenericParameters)
                    {
                        name = "Generic " + name;

                        var count = genericArguments.Count();

                        if (count > 1)
                        {
                            name += " (" + genericArguments.Count() + " parameters)";
                        }
                    }
                    else
                    {
                        name += " of ";
                        name += string.Join(" and ", genericArguments.Select(t => t.HumanName(includeGenericParameters)).ToArray());
                    }
                }

                return name;
            }
        }

        public static string HumanName(this MemberInfo member, ActionDirection direction = ActionDirection.Any, bool expectingBoolean = false)
        {
            var words = member.Name.Prettify();

            if (member is MethodInfo)
            {
                if (((MethodInfo)member).IsOperator())
                {
                    return humanOperatorNames[member.Name];
                }
                else
                {
                    return words;
                }
            }
            else if (member is FieldInfo || member is PropertyInfo)
            {
                if (direction == ActionDirection.Any)
                {
                    return words;
                }

                var type = member is FieldInfo ? ((FieldInfo)member).FieldType : ((PropertyInfo)member).PropertyType;

                // Fix for Unity's object-to-boolean implicit null-check operators
                if (direction == ActionDirection.Get && typeof(UnityObject).IsAssignableFrom(type) && expectingBoolean)
                {
                    return words + " Is Not Null";
                }

                string verb;

                switch (direction)
                {
                    case ActionDirection.Get:
                        verb = "Get";
                        break;

                    case ActionDirection.Set:
                        verb = "Set";
                        break;

                    default:
                        throw new UnexpectedEnumValueException<ActionDirection>(direction);
                }

                if (type == typeof(bool))
                {
                    // Check for boolean verbs like IsReady, HasChildren, etc.
                    if (words.Contains(' ') && booleanVerbs.Contains(words.Split(' ')[0]))
                    {
                        // Return them as-is for gets
                        if (direction == ActionDirection.Get)
                        {
                            return words;
                        }
                        // Skip them for sets
                        else if (direction == ActionDirection.Set)
                        {
                            return verb + " " + words.Substring(words.IndexOf(' ') + 1);
                        }
                        else
                        {
                            throw new UnexpectedEnumValueException<ActionDirection>(direction);
                        }
                    }
                    else
                    {
                        return verb + " " + words;
                    }
                }
                // Otherwise, add get/set the verb prefix
                else
                {
                    return verb + " " + words;
                }
            }
            else if (member is ConstructorInfo)
            {
                return "Create " + member.DeclaringType.HumanName();
            }
            else
            {
                throw new UnexpectedEnumValueException<ActionDirection>(direction);
            }
        }

        public static string HumanName(this ParameterInfo parameter)
        {
            return parameter.Name.Prettify();
        }

        public static string HumanName(this Exception exception)
        {
            return exception.GetType().CSharpName(false).Prettify().Replace(" Exception", "");
        }

        public static string HumanName(this Enum @enum)
        {
            return @enum.ToString().Prettify();
        }

        public static string CSharpName(this Namespace @namespace, bool full = true)
        {
            return @namespace.IsGlobal ? "(global)" : (full ? @namespace.FullName : @namespace.Name);
        }

        public static string HumanName(this Namespace @namespace, bool full = true)
        {
            return @namespace.IsGlobal ? "(Global Namespace)" : (full ? @namespace.FullName.Replace(".", "/").Prettify() : @namespace.Name.Prettify());
        }

        public static string ToSummaryString(this Exception ex)
        {
            return $"{ex.GetType().DisplayName()}: {ex.Message}";
        }
    }
}
