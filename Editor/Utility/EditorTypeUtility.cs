using System;
using System.Linq;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public static class EditorTypeUtility
    {
        public static Texture GetTypeIcon(this Type type)
        {
            try
            {
                return type.Icon()[IconSize.Small];
            }
            catch (NotSupportedException)
            {
                if (type is FakeGenericParameterType fakeGenericParameterType)
                {
                    return fakeGenericParameterType.BaseType.Icon()[IconSize.Small];
                }
                else if (type.IsArray)
                {
                    return typeof(Array).Icon()[IconSize.Small];
                }
                else if (type.IsGenericType)
                {
                    return type.GetGenericTypeDefinition().Icon()[IconSize.Small];
                }
                return typeof(object).Icon()[IconSize.Small];
            }
            catch (NullReferenceException)
            {
                return typeof(object).Icon()[IconSize.Small];
            }

        }
    }
}
