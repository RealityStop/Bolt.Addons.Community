using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [InitializeOnLoad]
    public static class EditorTypeUtility
    {
        public static Texture GetTypeIcon(this Type type)
        {
            try
            {
                var icon = type.Icon()[IconSize.Small];
                return ScaleIcon(icon, 16);

            }
            catch (NotSupportedException)
            {
                if (type is FakeGenericParameterType fakeGenericParameterType)
                {
                    return ScaleIcon(fakeGenericParameterType.BaseType.Icon()[IconSize.Small]);
                }
                else if (type.IsArray)
                {
                    return ScaleIcon(typeof(Array).Icon()[IconSize.Small]);
                }
                else if (type.IsGenericType)
                {
                    return ScaleIcon(type.GetGenericTypeDefinition().Icon()[IconSize.Small]);
                }
                return typeof(object).Icon()[IconSize.Small];
            }
            catch (NullReferenceException)
            {
                return typeof(object).Icon()[IconSize.Small];
            }
        }

        public static EditorTexture GetTypeIconAsEditorTexture(this Type type)
        {
            try
            {
                return type.Icon();

            }
            catch (NotSupportedException)
            {
                if (type is FakeGenericParameterType fakeGenericParameterType)
                {
                    return fakeGenericParameterType.BaseType.Icon();
                }
                else if (type.IsArray)
                {
                    return typeof(Array).Icon();
                }
                else if (type.IsGenericType)
                {
                    return type.GetGenericTypeDefinition().Icon();
                }
                return typeof(object).Icon();
            }
            catch (Exception)
            {
                return typeof(object).Icon();
            }
        }

        private static Texture2D ScaleIcon(Texture2D icon, int size = 16)
        {
            if (icon.width == size && icon.height == size)
                return icon;

            var rt = RenderTexture.GetTemporary(size, size);
            Graphics.Blit(icon, rt);

            var resized = new Texture2D(size, size, TextureFormat.RGBA32, false);
            RenderTexture.active = rt;
            resized.ReadPixels(new Rect(0, 0, size, size), 0, 0);
            resized.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);

            return resized;
        }

        public static bool IsClosedGeneric(Type type)
        {
            if (type.IsGenericParameter)
                return false;

            if (type.IsGenericType)
            {
                foreach (var arg in type.GetGenericArguments())
                {
                    if (!IsClosedGeneric(arg))
                        return false;
                }
            }

            if (type.IsArray)
                return IsClosedGeneric(type.GetElementType());

            return true;
        }

        public static bool IsRuntimeType(Type type)
        {
            if (type.IsGenericType)
            {
                foreach (var arg in type.GetGenericArguments())
                {
                    if (!IsRuntimeType(arg))
                        return false;
                }
            }

            if (type.IsArray)
                return IsRuntimeType(type.GetElementType());

            return Codebase.IsRuntimeType(type) && !type.Namespace.Contains("NUnit");
        }

        public static string RootNamespace(this Type type)
        {
            return type.Namespace?.PartBefore('.');
        }

        public static bool IsValidOverridableMethod(this MethodInfo m, bool required)
        {
            if (m == null) return false;

            if (m.IsSpecialName)
                return false;

            if (!m.Overridable())
                return false;

            if (!(m.IsPublic || m.IsFamily))
                return false;

            if (m.Name == "Finalize")
                return false;

            if (m.IsConstructor && m.IsStatic)
                return false;

            return !required || m.IsAbstract;
        }

        public static bool IsValidOverridableProperty(this PropertyInfo p, bool required)
        {
            if (p == null) return false;

            if (p.GetIndexParameters().Length > 0)
                return false;

            var getter = p.GetMethod;
            var setter = p.SetMethod;

            if (!p.Overridable())
                return false;

            bool visible =
                (getter != null && (getter.IsPublic || getter.IsFamily)) ||
                (setter != null && (setter.IsPublic || setter.IsFamily));

            if (!visible)
                return false;

            return !required ||
            (p.GetMethod != null && p.GetMethod.IsAbstract) ||
            (p.SetMethod != null && p.SetMethod.IsAbstract);
        }

        public static bool InheritsFromGeneric(this Type type, Type genericBase)
        {
            if (type == null || genericBase == null)
                return false;

            if (!genericBase.IsGenericTypeDefinition)
                throw new ArgumentException("genericBase must be a generic type definition, e.g. typeof(MyClass<>)");

            while (type != null && type != typeof(object))
            {
                var current = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (current == genericBase)
                    return true;

                type = type.BaseType;
            }

            return false;
        }

        public static bool InheritsFromGeneric(this Type type, Type genericBase, out Type result)
        {
            if (type == null || genericBase == null)
            {
                result = null;
                return false;
            }

            if (!genericBase.IsGenericTypeDefinition)
                throw new ArgumentException("genericBase must be a generic type definition, e.g. typeof(MyClass<>)");

            while (type != null && type != typeof(object))
            {
                var current = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (current == genericBase)
                {
                    result = type;
                    return true;
                }

                type = type.BaseType;
            }
            result = null;
            return false;
        }
    }
}
