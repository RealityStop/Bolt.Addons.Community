using System;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public static class EditorIconUtility
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

        public static bool IsValidOverridableMethod(this MethodInfo m)
        {
            if (m == null) return false;

            if (!m.Overridable())
                return false;

            if (!(m.IsPublic || m.IsFamily))
                return false;

            if (m.IsSpecialName)
                return false;

            if (m.Name == "Finalize")
                return false;

            if (m.IsConstructor && m.IsStatic)
                return false;

            return true;
        }

        public static bool IsValidOverridableProperty(this PropertyInfo p)
        {
            if (p == null) return false;

            if (p.GetIndexParameters().Length > 0)
                return false;

            var getter = p.GetMethod;
            var setter = p.SetMethod;

            if (p.Overridable())
                return false;

            bool visible =
                (getter != null && (getter.IsPublic || getter.IsFamily)) ||
                (setter != null && (setter.IsPublic || setter.IsFamily));

            if (!visible)
                return false;

            return true;
        }
    }
}
