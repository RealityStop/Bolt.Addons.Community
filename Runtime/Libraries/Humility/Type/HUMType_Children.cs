using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Reflection;
using UnityEngine;
using System.Linq;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMType_Children
    {
        /// <summary>
        /// Begins a type filtering operation.
        /// </summary>
        public static HUMType.Data.With With(this HUMType.Data.Types types)
        {
            return new HUMType.Data.With(types);
        }

        public static T New<T>(this HUMType.Data.As @as)
        {
            return (T)HUMType.New(@as.type);
        }

        /// <summary>
        /// Converts a type to its actual declared csharp name. Custom types return the type name. Example 'System.Int32' becomes 'int'.
        /// </summary>
        public static string CSharpName(this HUMType.Data.As @as, bool hideSystemObject = false, bool fullName = false, bool highlight = true)
        {
            if (highlight) return @as.HighlightCSharpName(hideSystemObject, fullName);
            if (@as.type == null) return "null";
            if (@as.type == typeof(CSharp.Void)) return "void";
            if (@as.type.IsConstructedGenericType || @as.type.IsGenericType) return GenericDeclaration(@as.type, fullName, highlight);
            if (@as.type == typeof(int)) return "int";
            if (@as.type == typeof(string)) return "string";
            if (@as.type == typeof(float)) return "float";
            if (@as.type == typeof(void)) return "void";
            if (@as.type == typeof(double)) return "double";
            if (@as.type == typeof(bool)) return "bool";
            if (@as.type == typeof(byte)) return "byte";
            if (@as.type == typeof(void)) return "void";
            if (@as.type == typeof(object) && @as.type.BaseType == null) return hideSystemObject ? string.Empty : "object";
            if (@as.type == typeof(object[])) return "object[]";
            if (@as.type is FakeGenericParameterType fakeGenericParameter) return fakeGenericParameter.Name;
            if (@as.type.Name.Contains("Attribute")) return fullName && !string.IsNullOrEmpty(@as.type.Namespace) ? @as.type.CSharpFullName().Replace("Attribute", "") : @as.type.CSharpName().Replace("Attribute", "");
            if (@as.type.IsArray)
            {
                var tempType = @as.type.GetElementType();
                var arrayString = "[]";
                while (tempType.IsArray)
                {
                    tempType = tempType.GetElementType();
                    arrayString += "[]";
                }

                var tempTypeName = tempType.As().CSharpName(hideSystemObject, fullName, false);
                return tempTypeName + arrayString;
            }
            if (string.IsNullOrEmpty(@as.type.Name))
            {
                return "UnknownType".WarningHighlight();
            }
            else if (string.IsNullOrEmpty(@as.type.Namespace))
            {
                return @as.type.CSharpName();
            }
            return fullName ? @as.type.CSharpFullName() : @as.type.CSharpName();
        }

        private static string HighlightCSharpName(this HUMType.Data.As @as, bool hideSystemObject = false, bool fullName = false)
        {
            if (@as.type == null) return "null".ConstructHighlight();
            if (@as.type == typeof(CSharp.Void)) return "void".ConstructHighlight();
            if (@as.type.IsConstructedGenericType || @as.type.IsGenericType) return GenericDeclaration(@as.type, fullName, true);
            if (@as.type == typeof(int)) return "int".ConstructHighlight();
            if (@as.type == typeof(string)) return "string".ConstructHighlight();
            if (@as.type == typeof(float)) return "float".ConstructHighlight();
            if (@as.type == typeof(void)) return "void".ConstructHighlight();
            if (@as.type == typeof(double)) return "double".ConstructHighlight();
            if (@as.type == typeof(bool)) return "bool".ConstructHighlight();
            if (@as.type == typeof(byte)) return "byte".ConstructHighlight();
            if (@as.type == typeof(void)) return "void".ConstructHighlight();
            if (@as.type.IsEnum) return (!string.IsNullOrEmpty(@as.type.Namespace) && fullName ? @as.type.Namespace.NamespaceHighlight() + "." : "") + @as.type.Name.EnumHighlight();
            if (@as.type.IsInterface) return (!string.IsNullOrEmpty(@as.type.Namespace) && fullName ? @as.type.Namespace.NamespaceHighlight() + "." : "") + @as.type.Name.InterfaceHighlight();
            if (@as.type == typeof(object) && @as.type.BaseType == null) return hideSystemObject ? string.Empty : "object".ConstructHighlight();
            if (@as.type == typeof(object[])) return "object".ConstructHighlight() + "[]";
            if (@as.type is FakeGenericParameterType fakeGenericParameter)
            {
                if (fakeGenericParameter._isArrayType)
                {
                    var tempType = @as.type.GetElementType() as FakeGenericParameterType;
                    while (tempType._isArrayType)
                    {
                        tempType = (FakeGenericParameterType)tempType.GetElementType();
                    }

                    var tempTypeName = tempType.Name.TypeHighlight();
                    return fakeGenericParameter.Name.Replace(tempType.Name, tempTypeName);
                }
                return fakeGenericParameter.Name.TypeHighlight();
            }
            if (@as.type.Name.Contains("Attribute")) return fullName && !string.IsNullOrEmpty(@as.type.Namespace) ? @as.type.CSharpFullName().Replace(@as.type.Name, @as.type.Name.TypeHighlight()).Replace(@as.type.Namespace, @as.type.Namespace.NamespaceHighlight()).Replace("Attribute", "") : @as.type.CSharpName().TypeHighlight().Replace("Attribute", "");
            if (@as.type.IsArray)
            {
                var tempType = @as.type.GetElementType();
                var arrayString = "[]";
                while (tempType.IsArray)
                {
                    tempType = tempType.GetElementType();
                    arrayString += "[]";
                }

                var tempTypeName = tempType.As().CSharpName(hideSystemObject, fullName, true);
                return tempTypeName + arrayString;
            }
            if (string.IsNullOrEmpty(@as.type.Name))
            {
                return "UnknownType".WarningHighlight();
            }
            else if (string.IsNullOrEmpty(@as.type.Namespace))
            {
                return @as.type.CSharpName().TypeHighlight();
            }
            return fullName ? @as.type.CSharpFullName().Replace(@as.type.Name, @as.type.Name.TypeHighlight()).Replace(@as.type.Namespace, @as.type.Namespace.NamespaceHighlight()) : @as.type.CSharpName().TypeHighlight();
        }

        public enum HighlightType
        {
            None,
            Enum,
            Interface,
            Type,
            Construct,
            Comment,
            Variable
        }

        public static string WithHighlight(this string text, HighlightType type)
        {
            switch (type)
            {
                case HighlightType.None:
                    return text;
                case HighlightType.Comment:
                    return text.CommentHighlight();
                case HighlightType.Construct:
                    return text.ConstructHighlight();
                case HighlightType.Interface:
                    return text.InterfaceHighlight();
                case HighlightType.Enum:
                    return text.EnumHighlight();
                case HighlightType.Type:
                    return text.TypeHighlight();
                case HighlightType.Variable:
                    return text.VariableHighlight();
                default:
                    return text;
            }
        }

        /// <summary>
        /// Returns true if a type is equal to another type.
        /// </summary>
        public static bool Type<T>(this HUMType.Data.Is isData)
        {
            return isData.type == typeof(T);
        }

        /// <summary>
        /// Returns true if a type is equal to another type.
        /// </summary>
        public static bool Type(this HUMType.Data.Is isData, Type type)
        {
            return isData.type == type;
        }

        /// <summary>
        /// Returns true if the type is a void.
        /// </summary>
        public static bool Void(this HUMType.Data.Is isData)
        {
            return isData.type == typeof(void);
        }

        public static bool NullOrVoid(this HUMType.Data.Is isData)
        {
            return isData.type == typeof(void) || isData.type == null;
        }

        /// <summary>
        /// Returns true if the type is Inheritable.
        /// </summary>
        public static bool Inheritable(this HUMType.Data.Is isData)
        {
            var t = isData.type;

            if (t == typeof(CSharp.Void) || t == typeof(void))
            {
                return false;
            }

            if (t.IsSealed) return false;

            if (t.IsSpecialName || t.IsCOMObject)
            {
                return false;
            }

            if (t.IsNested)
            {
                return t.IsNestedPublic && (t.IsClass || (t.IsAbstract && !t.IsSealed));
            }

            if (typeof(Delegate).IsAssignableFrom(t))
            {
                return false;
            }

            return (t.IsClass || (t.IsAbstract && !t.IsSealed)) && t.IsPublic;
        }


        /// <summary>
        /// Returns true if the type is a enumerator collection type.
        /// </summary>
        public static bool Enumerator(this HUMType.Data.Is isData)
        {
            if (isData.type == typeof(IEnumerator) || isData.type == typeof(IEnumerable) || isData.type == typeof(IEnumerator<>) || isData.type == typeof(IEnumerable<>))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the type contains a field or method this or returns an Enumerator
        /// </summary>
        public static bool Enumerator(this HUMType.Data.Has hasData, bool fields = true, bool methods = true)
        {
            return hasData.type.HasFieldOrMethodOfType<IEnumerator>(fields, methods);
        }

        /// <summary>
        /// Returns true if the enumerator includes a generic argument.
        /// </summary>
        public static bool Enumerator(this HUMType.Data.Generic genericData)
        {
            var interfaces = genericData.isData.type.GetInterfaces();

            for (int i = 0; i < interfaces.Length; i++)
            {
                if (interfaces[i].IsGenericType && interfaces[i].GetGenericTypeDefinition() == typeof(IEnumerator<>))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if an enumerable includes a generic argument.
        /// </summary>
        public static bool Enumerable(this HUMType.Data.Generic genericData)
        {
            var interfaces = genericData.isData.type.GetInterfaces();

            for (int i = 0; i < interfaces.Length; i++)
            {
                if (interfaces[i].IsGenericType && interfaces[i].GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if the type is an enumerable collection type.
        /// </summary>
        public static bool Enumerable(this HUMType.Data.Is isData)
        {
            return isData.type.Inherits(typeof(IEnumerator)) || isData.type.Inherits(typeof(IEnumerable)) || isData.type.Is().Generic().Enumerator() || isData.Enumerator();
        }

        /// <summary>
        /// Returns true if the type has a field or method of this a type.
        /// </summary>
        public static bool Enumerable(this HUMType.Data.Has has, bool fields = true, bool methods = true)
        {
            return has.type.HasFieldOrMethodOfType<IEnumerable>(fields, methods);
        }

        /// <summary>
        /// Returns true if the type is a Coroutine.
        /// </summary>
        public static bool Coroutine(this HUMType.Data.Is isData)
        {
            if (isData.type == typeof(IEnumerator) && isData.type != typeof(IEnumerable) && isData.type != typeof(IEnumerator<>) && isData.type != typeof(IEnumerable<>))
            {
                return true;
            }

            return isData.type == typeof(Coroutine) || isData.type == typeof(YieldInstruction) || isData.type == typeof(CustomYieldInstruction);
        }

        /// <summary>
        /// Returns true if the type is a GameObject or a Component type.
        /// </summary>
        public static bool UnityObject(this HUMType.Data.Is isData)
        {
            return isData.type == typeof(GameObject) || typeof(Component).IsAssignableFrom(isData);
        }

        /// <summary>
        /// Finds all types with this attribute.
        /// </summary>
        public static Type[] Attribute<TAttribute>(this HUMType.Data.With with, Assembly _assembly = null, Func<TAttribute, bool> predicate = null) where TAttribute : Attribute
        {
            List<Type> result = new List<Type>();
            Assembly[] assemblies = _assembly == null ? AppDomain.CurrentDomain.GetAssemblies() : new Assembly[] { _assembly };

            for (int assembly = 0; assembly < assemblies.Length; assembly++)
            {
                Type[] types = assemblies[assembly].GetTypes();

                for (int type = 0; type < types.Length; type++)
                {
                    if (with.types.get.type.IsAssignableFrom(types[type]))
                    {
                        if (types[type].IsAbstract) continue;
                        var attribs = types[type].GetCustomAttributes(typeof(TAttribute), false);
                        if (attribs == null || attribs.Length == 0) continue;
                        TAttribute attrib = attribs[0] as TAttribute;
                        attrib.Is().NotNull(() =>
                        {
                            if (predicate != null)
                            {
                                if (predicate(attrib))
                                    result.Add(types[type]);
                            }
                            else
                            {
                                result.Add(types[type]);
                            }
                        });
                    }
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Finds all types with this attribute.
        /// </summary>
        public static Type[] Attribute<TAttribute>(this HUMType.Data.With with, Func<TAttribute, bool> predicate) where TAttribute : Attribute
        {
            List<Type> result = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int assembly = 0; assembly < assemblies.Length; assembly++)
            {
                Type[] types = assemblies[assembly].GetTypes();

                for (int type = 0; type < types.Length; type++)
                {
                    if (with.types.get.type.IsAssignableFrom(types[type]))
                    {
                        if (types[type].IsAbstract) continue;
                        var attribs = types[type].GetCustomAttributes(typeof(TAttribute), false);
                        if (attribs == null || attribs.Length == 0) continue;
                        TAttribute attrib = attribs[0] as TAttribute;
                        attrib.Is().NotNull(() =>
                        {
                            if (predicate(attrib))
                                result.Add(types[type]);
                        });
                    }
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Starts an operation where we determing if a type is a generic of some kind.
        /// </summary>
        public static HUMType.Data.Generic Generic(this HUMType.Data.Is isData)
        {
            return new HUMType.Data.Generic(isData);
        }

        /// <summary>
        /// Returns all types that derive or have a base type of a this type.
        /// </summary>
        public static Type[] Derived(this HUMType.Data.Get derived, bool includeSelf = false)
        {
            List<Type> result = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int assembly = 0; assembly < assemblies.Length; assembly++)
            {
                Type[] types = assemblies[assembly].GetTypes();

                for (int type = 0; type < types.Length; type++)
                {
                    if ((!types[type].IsAbstract && !types[type].IsInterface) && derived.type.IsAssignableFrom(types[type]))
                    {
                        result.Add(types[type]);
                    }
                }
            }
            if (includeSelf) result.Add(derived.type);
            return result.ToArray();
        }

        /// <summary>
        /// Returns all types that derive or have a base type of a this type.
        /// </summary>
        public static Type[] All(this HUMType.Data.Get derived, bool includeSelf = false)
        {
            List<Type> result = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int assembly = 0; assembly < assemblies.Length; assembly++)
            {
                Type[] types = assemblies[assembly].GetTypes();

                for (int type = 0; type < types.Length; type++)
                {
                    if (derived.type.IsAssignableFrom(types[type]))
                    {
                        result.Add(types[type]);
                    }
                }
            }
            if (includeSelf) result.Add(derived.type);
            return result.ToArray();
        }

        public static bool PrimitiveOrString(this HUMType.Data.Is @is)
        {
            return @is.type.IsPrimitive || @is.type == typeof(string);
        }

        public static bool PrimitiveStringOrVoid(this HUMType.Data.Is @is)
        {
            if (@is.type == null) return false;
            return @is.type.IsPrimitive || @is.type == typeof(string) || @is.type == typeof(void) || @is.type.BaseType == null && @is.type == typeof(object);
        }

        /// <summary>
        /// Continues the get operation by getting types of some kind.
        /// </summary>
        public static HUMType.Data.Types Types(this HUMType.Data.Get get)
        {
            return new HUMType.Data.Types(get);
        }

        /// <summary>
        /// Converts a value into code form. Example: a float value of '10' would be '10f'. A string would add quotes, etc.
        /// </summary>
        public static string Code(this HUMValue.Data.As @as, bool isNew, bool isLiteral = false, bool highlight = true, string parameters = "", bool newLineLiteral = false, bool fullName = false, bool variableForObject = true)
        {
            if (highlight) return HighlightedCode(@as, isNew, isLiteral, parameters, newLineLiteral, fullName, variableForObject);
            Type type = @as.value?.GetType();
            if (@as.value is Type) return "typeof(" + ((Type)@as.value).As().CSharpName(false, fullName, false) + ")";
            if (type == null) return "null";
            if (type == typeof(void)) return "void";
            if (type == typeof(bool)) return @as.value.ToString().ToLower();
            if (type == typeof(float)) return @as.value.ToString().Replace(",", ".") + "f";
            if (type == typeof(double) || type == typeof(decimal)) return @as.value.ToString().Replace(",", ".");
            if (type == typeof(string)) return @"""" + @as.value.ToString() + @"""";
            if (type == typeof(char)) return string.IsNullOrEmpty(@as.value.ToString()) ? "new Char()" : $"'{@as.value}'";
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                if (variableForObject)
                {
                    var hasVariable = CodeGeneratorValueUtility.TryGetVariable((UnityEngine.Object)@as.value, out string current);
                    var variable = hasVariable ? current : $"ObjectVariable_{(@as.value as UnityEngine.Object).name.LegalMemberName() + "_" + Guid.NewGuid().ToString()[..3]}";

                    if (!hasVariable)
                    {
                        CodeGeneratorValueUtility.AddValue(variable, (UnityEngine.Object)@as.value);
                    }
                    else
                    {
                        CodeGeneratorValueUtility.SetIsUsed(current);
                    }
                    return variable;
                }
                else
                {
                    return "null";
                }
            }

            //Special Cases
            if (type == typeof(Vector2))
            {
                var value = (Vector2)@as.value;
                return Create("Vector2", value.x.As().Code(false, false, false), value.y.As().Code(false, false, false));
            }
            if (type == typeof(Vector3))
            {
                var value = (Vector3)@as.value;
                return Create("Vector3", value.x.As().Code(false, false, false), value.y.As().Code(false, false, false), value.z.As().Code(false, false, false));
            }
            if (type == typeof(Vector4))
            {
                var value = (Vector4)@as.value;
                return Create("Vector4", value.x.As().Code(false, false, false), value.y.As().Code(false, false, false), value.z.As().Code(false, false, false), value.w.As().Code(false, false, false));
            }
            if (type == typeof(AnimationCurve))
            {
                var value = @as.value as AnimationCurve;
                return Create("AnimationCurve", value.keys.Select(k => Create("Keyframe", k.time.As().Code(false, false, false), k.value.As().Code(false, false, false), k.inTangent.As().Code(false, false, false), k.outTangent.As().Code(false, false, false), k.inWeight.As().Code(false, false, false), k.outWeight.As().Code(false, false, false))).ToArray());
            }
<<<<<<< Updated upstream

=======
            if (type == typeof(Color))
            {
                var value = (Color)@as.value;
                return Create("Color", value.r.As().Code(false, false, false), value.g.As().Code(false, false, false), value.b.As().Code(false, false, false), value.a.As().Code(false, false, false));
            }
            if (type == typeof(WaitForFlowLogic))
            {
                var value = (WaitForFlowLogic)@as.value;
                return Create("WaitForFlowLogic", value.InputCount.As().Code(false, false, false), value.ResetOnExit.As().Code(false, false, false));
            }
>>>>>>> Stashed changes
            if (type.IsNumeric()) return @as.value.ToString();
            if (type.IsEnum) return (@as.value as Enum).ToMultipleEnumString(false);

            if (isNew)
            {
                if (type.IsClass || !type.IsClass && !type.IsInterface && !type.IsEnum)
                {
                    if (type.IsGenericType) return isLiteral ? Literal(@as.value, newLineLiteral, fullName, false, variableForObject) : "new " + GenericDeclaration(type, fullName, false) + "()";
                    if (type.IsConstructedGenericType) return isLiteral ? Literal(@as.value, newLineLiteral, fullName, false, variableForObject) : "new " + GenericDeclaration(type, fullName, false) + "(" + parameters + ")";
                    return isLiteral ? Literal(@as.value, newLineLiteral, fullName, false, variableForObject) : "new " + type.As().CSharpName(false, fullName, false) + "(" + parameters + ")";
                }
                else
                {
                    if (type.IsValueType && !type.IsEnum && !type.IsPrimitive) return isLiteral ? Literal(@as.value, newLineLiteral, fullName, false, variableForObject) : "new " + type.As().CSharpName(false, fullName, false) + "(" + parameters + ")";
                }
            }

            return @as.value.ToString();
        }

        public static string Code(this HUMValue.Data.As @as, bool isNew, Unit unit, bool isLiteral = false, bool highlight = true, string parameters = "", bool newLineLiteral = false, bool fullName = false, bool variableForObject = true)
        {
            if (highlight) return HighlightedCode(@as, isNew, unit, isLiteral, parameters, newLineLiteral, fullName, variableForObject);
            Type type = @as.value?.GetType();
            if (@as.value is Type) return CodeUtility.MakeSelectable(unit, "typeof(" + ((Type)@as.value).As().CSharpName(false, fullName, false) + ")");
            if (type == null) return CodeUtility.MakeSelectable(unit, "null");
            if (type == typeof(void)) return CodeUtility.MakeSelectable(unit, "void");
            if (type == typeof(bool)) return CodeUtility.MakeSelectable(unit, @as.value.ToString().ToLower());
            if (type == typeof(float)) return CodeUtility.MakeSelectable(unit, @as.value.ToString().Replace(",", ".") + "f");
            if (type == typeof(double) || type == typeof(decimal)) return CodeUtility.MakeSelectable(unit, @as.value.ToString().Replace(",", "."));
            if (type == typeof(string)) return CodeUtility.MakeSelectable(unit, @"""" + @as.value.ToString() + @"""");
            if (type == typeof(char)) return CodeUtility.MakeSelectable(unit, string.IsNullOrEmpty(@as.value.ToString()) ? "new Char()" : $"'{@as.value}'");
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                if (variableForObject)
                {
                    var hasVariable = CodeGeneratorValueUtility.TryGetVariable((UnityEngine.Object)@as.value, out string current);
                    var variable = hasVariable ? current : $"ObjectVariable_{(@as.value as UnityEngine.Object).name.LegalMemberName() + "_" + Guid.NewGuid().ToString()[..3]}";

                    if (!hasVariable)
                        CodeGeneratorValueUtility.AddValue(variable, (UnityEngine.Object)@as.value);
                    else
                        CodeGeneratorValueUtility.SetIsUsed(current);
                    return CodeUtility.MakeSelectable(unit, variable);
                }
                else
                {
                    return CodeUtility.MakeSelectable(unit, "null");
                }
            }

            //Special Cases
            if (type == typeof(Vector2))
            {
                var value = (Vector2)@as.value;
                return CodeUtility.MakeSelectable(unit, Create("Vector2", value.x.As().Code(false, false, false), value.y.As().Code(false, false, false)));
            }
            if (type == typeof(Vector3))
            {
                var value = (Vector3)@as.value;
                return CodeUtility.MakeSelectable(unit, Create("Vector3", value.x.As().Code(false, false, false), value.y.As().Code(false, false, false), value.z.As().Code(false, false, false)));
            }
            if (type == typeof(Vector4))
            {
                var value = (Vector4)@as.value;
                return CodeUtility.MakeSelectable(unit, Create("Vector4", value.x.As().Code(false, false, false), value.y.As().Code(false, false, false), value.z.As().Code(false, false, false), value.w.As().Code(false, false, false)));
            }
            if (type == typeof(AnimationCurve))
            {
                var value = @as.value as AnimationCurve;
                return CodeUtility.MakeSelectable(unit, Create("AnimationCurve", value.keys.Select(k => Create("Keyframe", k.time.As().Code(false, false, false), k.value.As().Code(false, false, false), k.inTangent.As().Code(false, false, false), k.outTangent.As().Code(false, false, false), k.inWeight.As().Code(false, false, false), k.outWeight.As().Code(false, false, false))).ToArray()));
            }
<<<<<<< Updated upstream
            if (type.IsNumeric()) return CodeUtility.MakeSelectable(unit, @as.value.ToString());
            if (type.IsEnum) return CodeUtility.MakeSelectable(unit, (@as.value as Enum).ToMultipleEnumString(false));
=======
            if (type == typeof(Color))
            {
                var value = (Color)@as.value;
                return CodeUtility.MakeClickable(unit, Create("Color", value.r.As().Code(false, false, false), value.g.As().Code(false, false, false), value.b.As().Code(false, false, false), value.a.As().Code(false, false, false)));
            }
            if (type == typeof(WaitForFlowLogic))
            {
                var value = (WaitForFlowLogic)@as.value;
                return CodeUtility.MakeClickable(unit, Create("WaitForFlowLogic", value.InputCount.As().Code(false, false, false), value.ResetOnExit.As().Code(false, false, false)));
            }
            if (type.IsNumeric()) return CodeUtility.MakeClickable(unit, @as.value.ToString());
            if (type.IsEnum) return CodeUtility.MakeClickable(unit, (@as.value as Enum).ToMultipleEnumString(false));
>>>>>>> Stashed changes

            if (isNew)
            {
                if (type.IsClass || (!type.IsClass && !type.IsInterface && !type.IsEnum))
                {
                    if (type.IsGenericType) return isLiteral ? Literal(@as.value, unit, newLineLiteral, fullName, false, variableForObject) : CodeUtility.MakeSelectable(unit, "new ".ConstructHighlight() + GenericDeclaration(type, fullName, false) + "()");
                    if (type.IsConstructedGenericType) return isLiteral ? Literal(@as.value, unit, newLineLiteral, fullName, false, variableForObject) : CodeUtility.MakeSelectable(unit, "new " + GenericDeclaration(type, fullName, false) + "(" + parameters + ")");
                    return isLiteral ? Literal(@as.value, unit, newLineLiteral, fullName, false, variableForObject) : CodeUtility.MakeSelectable(unit, "new " + type.As().CSharpName(false, fullName, false) + "(" + parameters + ")");
                }
                else
                {
                    if (type.IsValueType && !type.IsEnum && !type.IsPrimitive) return isLiteral ? Literal(@as.value, unit, newLineLiteral, fullName, false, variableForObject) : CodeUtility.MakeSelectable(unit, "new " + (fullName ? type.FullName : type.Name) + "(" + parameters + ")");
                }
            }

            return CodeUtility.MakeSelectable(unit, @as.value.ToString());
        }

        private static string HighlightedCode(this HUMValue.Data.As @as, bool isNew, bool isLiteral = false, string parameters = "", bool newLineLiteral = false, bool fullName = false, bool variableForObject = true)
        {
            Type type = @as.value?.GetType();
            if (@as.value is Type) return "typeof".ConstructHighlight() + "(" + ((Type)@as.value).As().CSharpName(false, fullName) + ")";
            if (type == null) return "null".ConstructHighlight();
            if (type == typeof(void)) return "void".ConstructHighlight();
            if (type == typeof(bool)) return @as.value.ToString().ToLower().ConstructHighlight();
            if (type == typeof(float)) return (@as.value.ToString().Replace(",", ".") + "f").NumericHighlight();
            if (type == typeof(double) || type == typeof(decimal)) return @as.value.ToString().Replace(",", ".").NumericHighlight();
            if (type == typeof(string)) return (@"""" + @as.value.ToString() + @"""").StringHighlight();
            if (type == typeof(char)) return (char)@as.value == char.MinValue ? "/* Cannot have a empty character */".WarningHighlight() : $"'{@as.value}'".StringHighlight();
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                if (variableForObject)
                {
                    var hasVariable = CodeGeneratorValueUtility.TryGetVariable((UnityEngine.Object)@as.value, out string current);
                    var variable = hasVariable ? current : $"ObjectVariable_{(@as.value as UnityEngine.Object).name.LegalMemberName() + "_" + Guid.NewGuid().ToString()[..3]}";

                    if (!hasVariable)
                        CodeGeneratorValueUtility.AddValue(variable, (UnityEngine.Object)@as.value);
                    else
                        CodeGeneratorValueUtility.SetIsUsed(current);
                    return variable.VariableHighlight();
                }
                else
                {
                    return "null".ConstructHighlight();
                }
            }

            //Special Cases
            if (type == typeof(Vector2))
            {
                var value = (Vector2)@as.value;
                return CreateHighlighted("Vector2", value.x.As().Code(false, false, true), value.y.As().Code(false, false, true));
            }
            if (type == typeof(Vector3))
            {
                var value = (Vector3)@as.value;
                return CreateHighlighted("Vector3", value.x.As().Code(false, false, true), value.y.As().Code(false, false, true), value.z.As().Code(false, false, true));
            }
            if (type == typeof(Vector4))
            {
                var value = (Vector4)@as.value;
                return CreateHighlighted("Vector4", value.x.As().Code(false, false, true), value.y.As().Code(false, false, true), value.z.As().Code(false, false, true), value.w.As().Code(false, false, true));
            }
            if (type == typeof(AnimationCurve))
            {
                var value = @as.value as AnimationCurve;
                return CreateHighlighted("AnimationCurve", value.keys.Select(k => CreateHighlighted("Keyframe", k.time.As().Code(false), k.value.As().Code(false), k.inTangent.As().Code(false), k.outTangent.As().Code(false), k.inWeight.As().Code(false), k.outWeight.As().Code(false))).ToArray());
            }
<<<<<<< Updated upstream
=======
            if (type == typeof(Color))
            {
                var value = (Color)@as.value;
                return CreateHighlighted("Color", value.r.As().Code(false), value.g.As().Code(false), value.b.As().Code(false), value.a.As().Code(false));
            }
            if (type == typeof(WaitForFlowLogic))
            {
                var value = (WaitForFlowLogic)@as.value;
                return CreateHighlighted("WaitForFlowLogic", value.InputCount.As().Code(false), value.ResetOnExit.As().Code(false));
            }
>>>>>>> Stashed changes
            if (type.IsNumeric()) return @as.value.ToString().NumericHighlight();
            if (type.IsEnum) return (@as.value as Enum).ToMultipleEnumString(true);
            if (isNew)
            {
                if (type.IsClass || !type.IsClass && !type.IsInterface && !type.IsEnum)
                {
                    if (type.IsGenericType) return isLiteral ? Literal(@as.value, newLineLiteral, fullName, true, variableForObject) : "new ".ConstructHighlight() + GenericDeclaration(type, fullName) + "()";
                    if (type.IsConstructedGenericType) return isLiteral ? Literal(@as.value, newLineLiteral, fullName, true, variableForObject) : "new ".ConstructHighlight() + GenericDeclaration(type, fullName) + "(" + parameters + ")";
                    return isLiteral ? Literal(@as.value, newLineLiteral, fullName, true, variableForObject) : "new ".ConstructHighlight() + type.As().CSharpName(false, fullName, true) + "(" + parameters + ")";
                }
                else
                {
                    if (type.IsValueType && !type.IsEnum && !type.IsPrimitive) return isLiteral ? Literal(@as.value, newLineLiteral, fullName, true, variableForObject) : "new ".ConstructHighlight() + type.As().CSharpName(false, fullName, true) + "(" + parameters + ")";
                }
            }

            return @as.value.ToString();
        }

        private static string HighlightedCode(this HUMValue.Data.As @as, bool isNew, Unit unit, bool isLiteral = false, string parameters = "", bool newLineLiteral = false, bool fullName = false, bool variableForObject = true)
        {
            Type type = @as.value?.GetType();
            if (@as.value is Type) return CodeUtility.MakeSelectable(unit, "typeof".ConstructHighlight() + "(" + ((Type)@as.value).As().CSharpName(false, true) + ")");
            if (type == null) return CodeUtility.MakeSelectable(unit, "null".ConstructHighlight());
            if (type == typeof(void)) return CodeUtility.MakeSelectable(unit, "void".ConstructHighlight());
            if (type == typeof(bool)) return CodeUtility.MakeSelectable(unit, @as.value.ToString().ToLower().ConstructHighlight());
            if (type == typeof(float)) return CodeUtility.MakeSelectable(unit, (@as.value.ToString().Replace(",", ".") + "f").NumericHighlight());
            if (type == typeof(double) || type == typeof(decimal)) return CodeUtility.MakeSelectable(unit, @as.value.ToString().Replace(",", ".").NumericHighlight());
            if (type == typeof(string)) return CodeUtility.MakeSelectable(unit, (@"""" + @as.value.ToString() + @"""").StringHighlight());
            if (type == typeof(char)) return (char)@as.value == char.MinValue ? CodeUtility.MakeSelectable(unit, "/* Cannot have an empty character */".WarningHighlight()) : CodeUtility.MakeSelectable(unit, $"'{@as.value}'".StringHighlight());
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                if (variableForObject)
                {
                    var hasVariable = CodeGeneratorValueUtility.TryGetVariable((UnityEngine.Object)@as.value, out string current);
                    var variable = hasVariable ? current : $"ObjectVariable_{(@as.value as UnityEngine.Object).name.LegalMemberName() + "_" + Guid.NewGuid().ToString()[..3]}";

                    if (!hasVariable)
                        CodeGeneratorValueUtility.AddValue(variable, (UnityEngine.Object)@as.value);
                    else
                        CodeGeneratorValueUtility.SetIsUsed(current);

                    return CodeUtility.MakeSelectable(unit, variable.VariableHighlight());
                }
                else
                {
                    return CodeUtility.MakeSelectable(unit, "null".ConstructHighlight());
                }
            }

            //Special Cases
            if (type == typeof(Vector2))
            {
                var value = (Vector2)@as.value;
                return CodeUtility.MakeSelectable(unit, CreateHighlighted("Vector2", value.x.As().Code(false, false, true), value.y.As().Code(false, false, true)));
            }
            if (type == typeof(Vector3))
            {
                var value = (Vector3)@as.value;
                return CodeUtility.MakeSelectable(unit, CreateHighlighted("Vector3", value.x.As().Code(false, false, true), value.y.As().Code(false, false, true), value.z.As().Code(false, false, true)));
            }
            if (type == typeof(Vector4))
            {
                var value = (Vector4)@as.value;
                return CodeUtility.MakeSelectable(unit, CreateHighlighted("Vector4", value.x.As().Code(false, false, true), value.y.As().Code(false, false, true), value.z.As().Code(false, false, true), value.w.As().Code(false, false, true)));
            }
            if (type == typeof(AnimationCurve))
            {
                var value = @as.value as AnimationCurve;
                return CodeUtility.MakeSelectable(unit, CreateHighlighted("AnimationCurve", value.keys.Select(k => CreateHighlighted("Keyframe", k.time.As().Code(false), k.value.As().Code(false), k.inTangent.As().Code(false), k.outTangent.As().Code(false), k.inWeight.As().Code(false), k.outWeight.As().Code(false))).ToArray()));
            }
<<<<<<< Updated upstream
            if (type.IsNumeric()) return CodeUtility.MakeSelectable(unit, @as.value.ToString().NumericHighlight());
            if (type.IsEnum) return CodeUtility.MakeSelectable(unit, (@as.value as Enum).ToMultipleEnumString(true));
=======
            if (type == typeof(Color))
            {
                var value = (Color)@as.value;
                return CodeUtility.MakeClickable(unit, CreateHighlighted("Color", value.r.As().Code(false), value.g.As().Code(false), value.b.As().Code(false), value.a.As().Code(false)));
            }
            if (type == typeof(WaitForFlowLogic))
            {
                var value = (WaitForFlowLogic)@as.value;
                return CodeUtility.MakeClickable(unit, CreateHighlighted("WaitForFlowLogic", value.InputCount.As().Code(false), value.ResetOnExit.As().Code(false)));
            }

            if (type.IsNumeric()) return CodeUtility.MakeClickable(unit, @as.value.ToString().NumericHighlight());
            if (type.IsEnum) return CodeUtility.MakeClickable(unit, (@as.value as Enum).ToMultipleEnumString(true));
>>>>>>> Stashed changes
            if (isNew)
            {
                if (type.IsClass || !type.IsClass && !type.IsInterface && !type.IsEnum)
                {
                    if (type.IsGenericType) return isLiteral ? Literal(@as.value, unit, newLineLiteral, fullName, true, variableForObject) : CodeUtility.MakeSelectable(unit, "new ".ConstructHighlight() + GenericDeclaration(type, fullName) + "()");
                    if (type.IsConstructedGenericType) return isLiteral ? Literal(@as.value, unit, newLineLiteral, fullName, true, variableForObject) : CodeUtility.MakeSelectable(unit, "new ".ConstructHighlight() + GenericDeclaration(type, fullName) + "(" + parameters + ")");
                    return isLiteral ? Literal(@as.value, unit, newLineLiteral, fullName, true, variableForObject) : CodeUtility.MakeSelectable(unit, "new ".ConstructHighlight() + type.As().CSharpName(false, fullName, true) + "(" + parameters + ")");
                }
                else
                {
                    if (type.IsValueType && !type.IsEnum && !type.IsPrimitive) return isLiteral ? Literal(@as.value, unit, newLineLiteral, fullName, true, variableForObject) : CodeUtility.MakeSelectable(unit, "new ".ConstructHighlight() + type.As().CSharpName(false, fullName, true) + "(" + parameters + ")");
                }
            }

            return CodeUtility.MakeSelectable(unit, @as.value.ToString());
        }

        private static string New(bool highlight = true)
        {
            return highlight ? "new ".ConstructHighlight() : "new ";
        }

        private static string CreateHighlighted(string type, params string[] parameters)
        {
            return New() + Type(type) + "(" + string.Join(", ", parameters) + ")";
        }

        private static string Create(string type, params string[] parameters)
        {
            return New(false) + Type(type, false) + "(" + string.Join(", ", parameters) + ")";
        }

        private static string Type(string type, bool highlight = true)
        {
            return highlight ? type.TypeHighlight() : type;
        }

        private static readonly Dictionary<Type, FieldInfo[]> FieldCache = new();

        private static FieldInfo[] GetCachedFields(Type type)
        {
            if (type == null) return new FieldInfo[0];
            if (!FieldCache.TryGetValue(type, out var fields))
            {
                fields = type.GetFields(BindingFlags.Public);
                FieldCache[type] = fields;
            }
            return fields;
        }

        private static string Literal(object value, bool newLine = false, bool fullName = false, bool highlight = true, bool variableForObject = true)
        {
            if (highlight)
            {
                return HightlightedLiteral(value, newLine, fullName, variableForObject);
            }
            var fields = GetCachedFields(value?.GetType());
            var output = string.Empty;
            var usableFields = new List<FieldInfo>();
            var isMultiLine = fields.Length > 2;

            // Check if the value is a dictionary and if it has more than 0 elements
            if (value is IDictionary dictionary && dictionary.Count > 0)
            {
                isMultiLine = true;
            }

            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].IsPublic && !fields[i].IsStatic && !fields[i].IsInitOnly)
                {
                    usableFields.Add(fields[i]);
                }
            }

            output += (newLine ? "\n" + CodeBuilder.GetCurrentIndent() : string.Empty) + "new " + (!value.GetType().IsGenericType ? value.GetType().As().CSharpName(false, fullName, false) : GenericDeclaration(value.GetType(), fullName), false) + (value.GetType().IsArray ? "" : "()");

            if (isMultiLine)
            {
                output += "\n" + CodeBuilder.GetCurrentIndent() + ((value is ICollection collection && collection.Count > 0) || usableFields.Count > 0 ? "{" : string.Empty) + "\n";
            }
            else
            {
                output += (value is ICollection collection && collection.Count > 0) || usableFields.Count > 0 ? $"\n{CodeBuilder.GetCurrentIndent()}{{" : string.Empty;
            }

            if (value is IDictionary or IList)
                CodeBuilder.Indent(CodeBuilder.currentIndent + 1);

            var indent = CodeBuilder.GetCurrentIndent();

            if (value is IList list)
            {
                int index = 0;
                foreach (var item in list)
                {
                    if (list.Count > 2)
                    {
                        output += "\n";
                        output += indent + item.As().Code(true, true, false, "", false, fullName, variableForObject);
                    }
                    else
                    {
                        output += (index == 0 ? " " : "") + item.As().Code(true, true, false, "", false, fullName, variableForObject);
                    }

                    if (++index != list.Count)
                    {
                        output += ", ";
                    }
                }
            }
            else if (value is IDictionary _dictionary)
            {
                int index = 0;
                foreach (DictionaryEntry entry in _dictionary)
                {
                    string newLinestr = _dictionary.Count > 2 ? "\n" : "";
                    output += indent + "{ ";
                    output += entry.Key.As().Code(true, true, false, "", false, fullName, variableForObject);
                    output += ", ";
                    output += entry.Value.As().Code(true, true, false, "", false, fullName, variableForObject);
                    output += " }";
                    if (++index != _dictionary.Count)
                    {
                        output += $", {newLinestr}";
                    }
                }
            }
            else if (value is Array array)
            {
                int index = 0;
                foreach (var item in array)
                {
                    if (array.Length > 2)
                    {
                        output += "\n";
                        output += indent + item.As().Code(true, true, false, "", false, fullName, variableForObject);
                    }
                    else
                    {
                        output += (index == 0 ? " " : "") + item.As().Code(true, true, false, "", false, fullName, variableForObject);
                    }

                    if (++index != array.Length)
                    {
                        output += ", ";
                    }
                }
            }

            if (isMultiLine)
                CodeBuilder.Indent(CodeBuilder.currentIndent + 1);
            for (int i = 0; i < usableFields.Count; i++)
            {
                output += (isMultiLine ? CodeBuilder.GetCurrentIndent() : string.Empty) + usableFields[i].Name + " = " + usableFields[i].GetValue(value).As().Code(true, true, false, "", false, fullName, variableForObject);
                output += i < usableFields.Count - 1 ? ", " + (isMultiLine ? "\n" : string.Empty) : string.Empty;
            }
            if (isMultiLine)
                CodeBuilder.Indent(CodeBuilder.currentIndent - 1);

            output += isMultiLine ? "\n" + (value is ICollection ? CodeBuilder.Indent(CodeBuilder.currentIndent - 1) : CodeBuilder.Indent(CodeBuilder.currentIndent)) + "}" : (value is ICollection collectionWithItems && collectionWithItems.Count > 0) || usableFields.Count > 0 ? $"\n{CodeBuilder.Indent(CodeBuilder.currentIndent - 1)}}}" : string.Empty;

            return output;
        }

        private static string HightlightedLiteral(object value, bool newLine = false, bool fullName = false, bool variableForObject = true)
        {
            var fields = GetCachedFields(value?.GetType());
            var output = string.Empty;
            var usableFields = new List<FieldInfo>();
            var isMultiLine = fields.Length > 2;

            // Check if the value is a dictionary and if it has more than 0 elements
            if (value is IDictionary dictionary && dictionary.Count > 0)
            {
                isMultiLine = true;
            }

            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].IsPublic && !fields[i].IsStatic && !fields[i].IsInitOnly)
                {
                    usableFields.Add(fields[i]);
                }
            }

            output += (newLine ? "\n" + CodeBuilder.GetCurrentIndent() : string.Empty) + "new ".ConstructHighlight() + (!value.GetType().IsGenericType ? value.GetType().As().CSharpName(false, fullName) : GenericDeclaration(value.GetType(), fullName)) + (value.GetType().IsArray ? "" : "()");

            if (isMultiLine)
            {
                output += "\n" + CodeBuilder.GetCurrentIndent() + ((value is ICollection collection && collection.Count > 0) || usableFields.Count > 0 ? "{" : string.Empty) + "\n";
            }
            else
            {
                output += (value is ICollection collection && collection.Count > 0) || usableFields.Count > 0 ? $"\n{CodeBuilder.GetCurrentIndent()}{{\n{CodeBuilder.GetCurrentIndent(1)}" : string.Empty;
            }

            if (value is IDictionary or IList)
                CodeBuilder.Indent(CodeBuilder.currentIndent + 1);

            var indent = CodeBuilder.GetCurrentIndent();

            if (value is IList list)
            {
                int index = 0;
                foreach (var item in list)
                {
                    if (list.Count > 2)
                    {
                        output += "\n";
                        output += indent + item.As().Code(true, true, true, "", false, fullName, variableForObject);
                    }
                    else
                    {
                        output += (index == 0 ? " " : "") + item.As().Code(true, true, true, "", false, fullName, variableForObject);
                    }

                    if (++index != list.Count)
                    {
                        output += ", ";
                    }
                }
            }
            else if (value is IDictionary _dictionary)
            {
                int index = 0;
                foreach (DictionaryEntry entry in _dictionary)
                {
                    string newLinestr = _dictionary.Count > 2 ? "\n" : "";
                    output += indent + "{ ";
                    output += entry.Key.As().Code(true, true, true, "", false, fullName, variableForObject);
                    output += ", ";
                    output += entry.Value.As().Code(true, true, true, "", false, fullName, variableForObject);
                    output += " }";
                    if (++index != _dictionary.Count)
                    {
                        output += $", {newLinestr}";
                    }
                }
            }
            else if (value is Array array)
            {
                int index = 0;
                foreach (var item in array)
                {
                    if (array.Length > 2)
                    {
                        output += "\n";
                        output += indent + item.As().Code(true, true, true, "", false, fullName, variableForObject);
                    }
                    else
                    {
                        output += (index == 0 ? " " : "") + item.As().Code(true, true, true, "", false, fullName, variableForObject);
                    }

                    if (++index != array.Length)
                    {
                        output += ", ";
                    }
                }
            }

            if (isMultiLine)
                CodeBuilder.Indent(CodeBuilder.currentIndent + 1);
            for (int i = 0; i < usableFields.Count; i++)
            {
                output += (isMultiLine ? CodeBuilder.GetCurrentIndent() : string.Empty) + usableFields[i].Name + " = " + usableFields[i].GetValue(value).As().Code(true, true, true, "", false, fullName, variableForObject);
                output += i < usableFields.Count - 1 ? ", " + (isMultiLine ? "\n" : string.Empty) : string.Empty;
            }
            if (isMultiLine || (value is ICollection _collection && _collection.Count > 0))
                CodeBuilder.Indent(CodeBuilder.currentIndent - 1);

            output += isMultiLine ? "\n" + (value is ICollection ? CodeBuilder.Indent(CodeBuilder.currentIndent - 1) : CodeBuilder.Indent(CodeBuilder.currentIndent)) + "}" : (value is ICollection collectionWithItems && collectionWithItems.Count > 0) || usableFields.Count > 0 ? $"\n{CodeBuilder.Indent(CodeBuilder.currentIndent - 1)}}}" : string.Empty;

            return output;
        }

        private static string Literal(object value, Unit unit, bool newLine = false, bool fullName = false, bool highlight = true, bool variableForObject = true)
        {
            if (highlight)
            {
                return HighlightedLiteral(value, unit, newLine, fullName, variableForObject);
            }
            var fields = GetCachedFields(value?.GetType());
            var output = string.Empty;
            var usableFields = new List<FieldInfo>();
            var isMultiLine = fields.Length > 2;

            // Check if the value is a dictionary and if it has more than 0 elements
            if (value is IDictionary dictionary && dictionary.Count > 0)
            {
                isMultiLine = true;
            }

            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].IsPublic && !fields[i].IsStatic && !fields[i].IsInitOnly)
                {
                    usableFields.Add(fields[i]);
                }
            }

            output += (newLine ? "\n" + CodeBuilder.GetCurrentIndent() : string.Empty) + CodeUtility.MakeSelectable(unit, "new " + (!value.GetType().IsGenericType ? value.GetType().As().CSharpName(false, fullName, false) : GenericDeclaration(value.GetType(), fullName), false) + (value.GetType().IsArray ? "" : "()"));

            if (isMultiLine)
            {
                output += "\n" + CodeBuilder.GetCurrentIndent() + CodeUtility.MakeSelectable(unit, (value is ICollection collection && collection.Count > 0) || usableFields.Count > 0 ? "{" : string.Empty) + "\n";
            }
            else
            {
                output += (value is ICollection collection && collection.Count > 0) || usableFields.Count > 0 ? $"\n{CodeBuilder.GetCurrentIndent()}{CodeUtility.MakeSelectable(unit, "{")}" : string.Empty;
            }

            if (value is IDictionary or IList)
                CodeBuilder.Indent(CodeBuilder.currentIndent + 1);

            var indent = CodeBuilder.GetCurrentIndent();

            if (value is IList list)
            {
                int index = 0;
                foreach (var item in list)
                {
                    if (list.Count > 2)
                    {
                        output += "\n";
                        output += indent + CodeUtility.MakeSelectable(unit, item.As().Code(true, true, false, "", false, fullName, variableForObject));
                    }
                    else
                    {
                        output += (index == 0 ? " " : "") + CodeUtility.MakeSelectable(unit, item.As().Code(true, true, false, "", false, fullName, variableForObject));
                    }

                    if (++index != list.Count)
                    {
                        output += CodeUtility.MakeSelectable(unit, ", ");
                    }
                }
            }
            else if (value is IDictionary _dictionary)
            {
                int index = 0;
                foreach (DictionaryEntry entry in _dictionary)
                {
                    string newLinestr = _dictionary.Count > 2 ? "\n" : "";
                    output += indent + CodeUtility.MakeSelectable(unit, "{ ");
                    output += CodeUtility.MakeSelectable(unit, entry.Key.As().Code(true, true, false, "", false, fullName, variableForObject));
                    output += CodeUtility.MakeSelectable(unit, ", ");
                    output += CodeUtility.MakeSelectable(unit, entry.Value.As().Code(true, true, false, "", false, fullName, variableForObject));
                    output += CodeUtility.MakeSelectable(unit, " }");
                    if (++index != _dictionary.Count)
                    {
                        output += CodeUtility.MakeSelectable(unit, ",") + $" {newLinestr}";
                    }
                }
            }
            else if (value is Array array)
            {
                int index = 0;
                foreach (var item in array)
                {
                    if (array.Length > 2)
                    {
                        output += "\n";
                        output += indent + CodeUtility.MakeSelectable(unit, item.As().Code(true, true, false, "", false, fullName, variableForObject));
                    }
                    else
                    {
                        output += (index == 0 ? " " : "") + CodeUtility.MakeSelectable(unit, item.As().Code(true, true, false, "", false, fullName, variableForObject));
                    }

                    if (++index != array.Length)
                    {
                        output += CodeUtility.MakeSelectable(unit, ", ");
                    }
                }
            }

            if (isMultiLine)
                CodeBuilder.Indent(CodeBuilder.currentIndent + 1);
            for (int i = 0; i < usableFields.Count; i++)
            {
                output += (isMultiLine ? CodeBuilder.GetCurrentIndent() : string.Empty) + CodeUtility.MakeSelectable(unit, usableFields[i].Name + " = " + usableFields[i].GetValue(value).As().Code(true, true, false, "", false, fullName, variableForObject));
                output += i < usableFields.Count - 1 ? CodeUtility.MakeSelectable(unit, ", ") + (isMultiLine ? "\n" : string.Empty) : string.Empty;
            }
            if (isMultiLine)
                CodeBuilder.Indent(CodeBuilder.currentIndent - 1);

            output += isMultiLine ? "\n" + (value is ICollection ? CodeBuilder.Indent(CodeBuilder.currentIndent - 1) : CodeBuilder.Indent(CodeBuilder.currentIndent)) + CodeUtility.MakeSelectable(unit, "}") : (value is ICollection collectionWithItems && collectionWithItems.Count > 0) || usableFields.Count > 0 ? $"\n{CodeBuilder.Indent(CodeBuilder.currentIndent - 1)}" + CodeUtility.MakeSelectable(unit, "}") : string.Empty;

            return output;
        }

        private static string HighlightedLiteral(object value, Unit unit, bool newLine = false, bool fullName = false, bool variableForObject = false)
        {
            var fields = GetCachedFields(value?.GetType());
            var output = string.Empty;
            var usableFields = new List<FieldInfo>();
            var isMultiLine = fields.Length > 2;

            // Check if the value is a dictionary and if it has more than 0 elements
            if (value is IDictionary dictionary && dictionary.Count > 0)
            {
                isMultiLine = true;
            }

            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].IsPublic && !fields[i].IsStatic && !fields[i].IsInitOnly)
                {
                    usableFields.Add(fields[i]);
                }
            }

            output += (newLine ? "\n" + CodeBuilder.GetCurrentIndent() : string.Empty) + CodeUtility.MakeSelectable(unit, "new ".ConstructHighlight() + (!value.GetType().IsGenericType ? value.GetType().As().CSharpName(false, fullName) : GenericDeclaration(value.GetType(), fullName)) + (value.GetType().IsArray ? "" : "()"));

            if (isMultiLine)
            {
                output += "\n" + CodeBuilder.GetCurrentIndent() + CodeUtility.MakeSelectable(unit, (value is ICollection collection && collection.Count > 0) || usableFields.Count > 0 ? "{" : string.Empty) + "\n";
            }
            else
            {
                output += (value is ICollection collection && collection.Count > 0) || usableFields.Count > 0 ? $"\n{CodeBuilder.GetCurrentIndent()}{CodeUtility.MakeSelectable(unit, "{")}" : string.Empty;
            }

            if (value is IDictionary or IList)
                CodeBuilder.Indent(CodeBuilder.currentIndent + 1);

            var indent = CodeBuilder.GetCurrentIndent();

            if (value is IList list)
            {
                int index = 0;
                foreach (var item in list)
                {
                    if (list.Count > 2)
                    {
                        output += "\n";
                        output += indent + CodeUtility.MakeSelectable(unit, item.As().Code(true, true, true, "", false, fullName, variableForObject));
                    }
                    else
                    {
                        output += (index == 0 ? " " : "") + CodeUtility.MakeSelectable(unit, item.As().Code(true, true, true, "", false, fullName, variableForObject));
                    }

                    if (++index != list.Count)
                    {
                        output += CodeUtility.MakeSelectable(unit, ", ");
                    }
                }
            }
            else if (value is IDictionary _dictionary)
            {
                int index = 0;
                foreach (DictionaryEntry entry in _dictionary)
                {
                    string newLinestr = _dictionary.Count > 2 ? "\n" : "";
                    output += indent + CodeUtility.MakeSelectable(unit, "{ ");
                    output += CodeUtility.MakeSelectable(unit, entry.Key.As().Code(true, true, true, "", false, fullName, variableForObject));
                    output += CodeUtility.MakeSelectable(unit, ", ");
                    output += CodeUtility.MakeSelectable(unit, entry.Value.As().Code(true, true, true, "", false, fullName, variableForObject));
                    output += CodeUtility.MakeSelectable(unit, " }");
                    if (++index != _dictionary.Count)
                    {
                        output += CodeUtility.MakeSelectable(unit, ",") + $" {newLinestr}";
                    }
                }
            }
            else if (value is Array array)
            {
                int index = 0;
                foreach (var item in array)
                {
                    if (array.Length > 2)
                    {
                        output += "\n";
                        output += indent + CodeUtility.MakeSelectable(unit, item.As().Code(true, true, true, "", false, fullName, variableForObject));
                    }
                    else
                    {
                        output += (index == 0 ? " " : "") + CodeUtility.MakeSelectable(unit, item.As().Code(true, true, true, "", false, fullName, variableForObject));
                    }

                    if (++index != array.Length)
                    {
                        output += CodeUtility.MakeSelectable(unit, ", ");
                    }
                }
            }

            if (isMultiLine)
                CodeBuilder.Indent(CodeBuilder.currentIndent + 1);
            for (int i = 0; i < usableFields.Count; i++)
            {
                output += (isMultiLine ? CodeBuilder.GetCurrentIndent() : string.Empty) + CodeUtility.MakeSelectable(unit, usableFields[i].Name + " = " + usableFields[i].GetValue(value).As().Code(true, true, true, "", false, fullName, variableForObject));
                output += i < usableFields.Count - 1 ? CodeUtility.MakeSelectable(unit, ", ") + (isMultiLine ? "\n" : string.Empty) : string.Empty;
            }
            if (isMultiLine)
                CodeBuilder.Indent(CodeBuilder.currentIndent - 1);

            output += isMultiLine ? "\n" + (value is ICollection ? CodeBuilder.Indent(CodeBuilder.currentIndent - 1) : CodeBuilder.Indent(CodeBuilder.currentIndent)) + CodeUtility.MakeSelectable(unit, "}") : (value is ICollection collectionWithItems && collectionWithItems.Count > 0) || usableFields.Count > 0 ? $"\n{CodeBuilder.Indent(CodeBuilder.currentIndent - 1)}" + CodeUtility.MakeSelectable(unit, "}") : string.Empty;

            return output;
        }

        public static string GenericDeclaration(Type type, bool fullName = true, bool highlight = true)
        {
            var output = string.Empty;

            if (!type.IsConstructedGenericType && !type.IsGenericType) throw new Exception("Type is not a generic type but you are trying to declare a generic.");
            if (highlight)
            {
                output += (fullName && !string.IsNullOrEmpty(type.Namespace) ? type.Namespace.NamespaceHighlight() + "." : "") + (type.Name.Contains("`") ? type.Name.Remove(type.Name.IndexOf("`"), type.Name.Length - type.Name.IndexOf("`")).WithHighlight(type.IsInterface ? HighlightType.Interface : HighlightType.Type) : type.Name.WithHighlight(type.IsInterface ? HighlightType.Interface : HighlightType.Type));
            }
            else
            {
                output += (fullName && !string.IsNullOrEmpty(type.Namespace) ? type.Namespace + "." : "") + (type.Name.Contains("`") ? type.Name.Remove(type.Name.IndexOf("`"), type.Name.Length - type.Name.IndexOf("`")) : type.Name);
            }
            output += "<";

            var args = type.GetGenericArguments();

            for (int i = 0; i < args.Length; i++)
            {
                output += args[i].As().CSharpName(false, fullName, highlight);
                if (i < args.Length - 1) output += ", ";
            }

            output += ">";

            return output;
        }

        public static string GenericDeclaration(Type type, List<Type> parameters, bool fullName = true, bool highlight = true)
        {
            var output = string.Empty;

            if (!type.IsConstructedGenericType && !type.IsGenericType) return type.As().CSharpName(false, fullName, highlight);
            if (highlight)
            {
                output += type.Name.Contains("`") ? type.Name.Remove(type.Name.IndexOf("`"), type.Name.Length - type.Name.IndexOf("`")).WithHighlight(type.IsInterface ? HighlightType.Interface : HighlightType.Type) : type.Name.WithHighlight(type.IsInterface ? HighlightType.Interface : HighlightType.Type);
            }
            else
            {
                output += type.Name.Contains("`") ? type.Name.Remove(type.Name.IndexOf("`"), type.Name.Length - type.Name.IndexOf("`")) : type.Name;
            }

            if (parameters.Count > 0)
            {
                output += "<";
                for (int i = 0; i < parameters.Count; i++)
                {
                    output += parameters[i].As().CSharpName(false, fullName, highlight);
                    if (i < parameters.Count - 1) output += ", ";
                }
                output += ">";
            }
            else
            {
                output += "<";

                var args = type.GetGenericArguments();

                for (int i = 0; i < args.Length; i++)
                {
                    output += args[i].As().CSharpName(false, fullName, highlight);
                    if (i < args.Length - 1) output += ", ";
                }

                output += ">";
            }

            return output;
        }

        public static string GenericDeclaration(Type type, bool fullName = true, bool highlight = true, params Type[] parameters)
        {
            var output = string.Empty;

            if (!type.IsConstructedGenericType && !type.IsGenericType) return type.As().CSharpName();
            if (highlight)
            {
                output += type.Name.Contains("`") ? type.Name.Remove(type.Name.IndexOf("`"), type.Name.Length - type.Name.IndexOf("`")).WithHighlight(type.IsInterface ? HighlightType.Interface : HighlightType.Type) : type.Name.WithHighlight(type.IsInterface ? HighlightType.Interface : HighlightType.Type);
            }
            else
            {
                output += type.Name.Contains("`") ? type.Name.Remove(type.Name.IndexOf("`"), type.Name.Length - type.Name.IndexOf("`")) : type.Name;
            }

            if (parameters.Length > 0)
            {
                output += "<";
                for (int i = 0; i < parameters.Length; i++)
                {
                    output += parameters[i].As().CSharpName(false, fullName, highlight);
                    if (i < parameters.Length - 1) output += ", ";
                }
                output += ">";
            }
            else
            {
                output += "<";

                var args = type.GetGenericArguments();

                for (int i = 0; i < args.Length; i++)
                {
                    output += args[i].As().CSharpName(false, fullName, highlight);
                    if (i < args.Length - 1) output += ", ";
                }

                output += ">";
            }

            return output;
        }

    }
}

namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// Copied from visual scripting
    /// </summary>
    public static class CSharpNameUtility
    {
        private static readonly Dictionary<Type, string> primitives = new Dictionary<Type, string>
        {
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(decimal), "decimal" },
            { typeof(string), "string" },
            { typeof(char), "char" },
            { typeof(bool), "bool" },
            { typeof(void), "void" },
            { typeof(object), "object" },
        };

        public static readonly Dictionary<string, string> operators = new Dictionary<string, string>
        {
            { "op_Addition", "+" },
            { "op_Subtraction", "-" },
            { "op_Multiply", "*" },
            { "op_Division", "/" },
            { "op_Modulus", "%" },
            { "op_ExclusiveOr", "^" },
            { "op_BitwiseAnd", "&" },
            { "op_BitwiseOr", "|" },
            { "op_LogicalAnd", "&&" },
            { "op_LogicalOr", "||" },
            { "op_Assign", "=" },
            { "op_LeftShift", "<<" },
            { "op_RightShift", ">>" },
            { "op_Equality", "==" },
            { "op_GreaterThan", ">" },
            { "op_LessThan", "<" },
            { "op_Inequality", "!=" },
            { "op_GreaterThanOrEqual", ">=" },
            { "op_LessThanOrEqual", "<=" },
            { "op_MultiplicationAssignment", "*=" },
            { "op_SubtractionAssignment", "-=" },
            { "op_ExclusiveOrAssignment", "^=" },
            { "op_LeftShiftAssignment", "<<=" },
            { "op_ModulusAssignment", "%=" },
            { "op_AdditionAssignment", "+=" },
            { "op_BitwiseAndAssignment", "&=" },
            { "op_BitwiseOrAssignment", "|=" },
            { "op_Comma", "," },
            { "op_DivisionAssignment", "/=" },
            { "op_Decrement", "--" },
            { "op_Increment", "++" },
            { "op_UnaryNegation", "-" },
            { "op_UnaryPlus", "+" },
            { "op_OnesComplement", "~" },
        };

        private static readonly HashSet<char> illegalTypeFileNameCharacters = new HashSet<char>()
        {
            '<',
            '>',
            '?',
            ' ',
            ',',
            ':',
        };

        public static string CSharpName(this MemberInfo member, ActionDirection direction)
        {
            if (member is MethodInfo && ((MethodInfo)member).IsOperator())
            {
                return operators[member.Name] + " operator";
            }

            if (member is ConstructorInfo)
            {
                return "new " + member.DeclaringType.CSharpName();
            }

            if ((member is FieldInfo || member is PropertyInfo) && direction != ActionDirection.Any)
            {
                return $"{member.Name} ({direction.ToString().ToLower()})";
            }

            return member.Name;
        }

        public static string CSharpName(this Type type, bool includeGenericParameters = true)
        {
            return type.CSharpName(TypeQualifier.Name, includeGenericParameters);
        }

        public static string CSharpFullName(this Type type, bool includeGenericParameters = true)
        {
            return type.CSharpName(TypeQualifier.Namespace, includeGenericParameters);
        }

        public static string CSharpUniqueName(this Type type, bool includeGenericParameters = true)
        {
            return type.CSharpName(TypeQualifier.GlobalNamespace, includeGenericParameters);
        }

        public static string CSharpFileName(this Type type, bool includeNamespace, bool includeGenericParameters = false)
        {
            var fileName = type.CSharpName(includeNamespace ? TypeQualifier.Namespace : TypeQualifier.Name, includeGenericParameters);

            if (!includeGenericParameters && type.IsGenericType && fileName.Contains('<'))
            {
                fileName = fileName.Substring(0, fileName.IndexOf('<'));
            }

            fileName = fileName.ReplaceMultiple(illegalTypeFileNameCharacters, '_')
                .Trim('_')
                .RemoveConsecutiveCharacters('_');

            return fileName;
        }

        private static string CSharpName(this Type type, TypeQualifier qualifier, bool includeGenericParameters = true)
        {
            if (type == null) return "";

            if (primitives.ContainsKey(type))
            {
                return primitives[type];
            }
            else if (type.IsGenericParameter)
            {
                return includeGenericParameters ? type.Name : "";
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var nonNullable = Nullable.GetUnderlyingType(type);

                var underlyingName = nonNullable?.CSharpName(qualifier, includeGenericParameters) ?? "Unknown";

                return underlyingName + "?";
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

                var tempTypeName = tempType.CSharpName(qualifier, includeGenericParameters);
                return tempTypeName + arrayString;
            }
            else
            {
                var name = type.Name;

                if (type.IsGenericType && name.Contains('`'))
                {
                    name = name.Substring(0, name.IndexOf('`'));
                }

                var genericArguments = (IEnumerable<Type>)type.GetGenericArguments();

                if (type.IsNested)
                {
                    name = type.DeclaringType.CSharpName(qualifier, includeGenericParameters) + "." + name;

                    if (type.DeclaringType.IsGenericType)
                    {
                        genericArguments = genericArguments.Skip(type.DeclaringType.GetGenericArguments().Length);
                    }
                }

                if (!type.IsNested)
                {
                    if ((qualifier == TypeQualifier.Namespace || qualifier == TypeQualifier.GlobalNamespace) && !string.IsNullOrEmpty(type.Namespace))
                    {
                        name = type.Namespace + "." + name;
                    }

                    if (qualifier == TypeQualifier.GlobalNamespace)
                    {
                        name = "global::" + name;
                    }
                }

                if (genericArguments.Any())
                {
                    name += "<";
                    name += string.Join(includeGenericParameters ? ", " : ",", genericArguments.Select(t => t.CSharpName(qualifier, includeGenericParameters)).ToArray());
                    name += ">";
                }

                return name;
            }
        }
    }
}