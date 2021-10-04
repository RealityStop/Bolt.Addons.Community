using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMValue
    {
        /// <summary>
        /// Converts a value into a string with type name referenced. Used to serialize data for Deserialize().
        /// </summary>
        public static string Serialized(this object value)
        {
            Type type = value?.GetType();

            if (type == null) return "null";
            if (type == typeof(bool)) return value.ToString().ToLower() + "[Type:Boolean]";
            if (type == typeof(float)) return value.ToString() + "[Type:Float]";
            if (type == typeof(int)) return value.ToString() + "[Type:Int]";
            if (type == typeof(string)) return value + "[Type:String]";
            if (type == typeof(Type)) return ((Type)value).FullName + "[Type:Type]";
            if (type == typeof(UnityEngine.GameObject)) return "null";
             
            return string.Empty;
        }

        /// <summary>
        /// Deserializes a Serialized() string back into its original value.
        /// </summary>
        public static object Deserialized(this string str)
        {
            if (str.Contains("[Type:Boolean]"))
        
            {
                if (str.Contains("true")) return true;
                if (str.Contains("false")) return false;
            }

            if (str.Contains("[Type:Float]"))
            {
                return float.Parse(str.Replace("[Type:Float]", string.Empty));
            }

            if (str.Contains("[Type:String]"))
            {
                return str.Replace("[Type:String]", string.Empty);
            }

            if (str.Contains("[Type:Int]"))
            {
                return int.Parse(str.Replace("[Type:Int]", string.Empty));
            }

            if (str.Contains("[Type:Type]"))
            {
                return Type.GetType(str.Replace("[Type:Type]", string.Empty));
            }

            if (str.Contains("[Type:GameObject"))
            {
                var parsedInt = int.Parse(str.Replace("[Type:GameObject]", string.Empty));
#if UNITY_EDITOR
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GetAssetPath(parsedInt));
                return asset;
#endif
            }

            return null;
        }

        public static bool Changed(ref object refValue, object value, bool assign = true)
        {
            var changed = refValue != value;
            if (assign) refValue = value;
            return changed;
        }

        public static bool Changed(this object refValue, object value, Action<object> onValueChanged)
        {
            var changed = refValue != value;
            if (refValue != value)
            {
                onValueChanged?.Invoke(value);
            }
            return changed;
        }

        /// <summary>
        /// Begins a creation operation.
        /// </summary>
        public static Data.Create Create()
        {
            return new Data.Create();
        }

        /// <summary>
        /// Begins a type check operation.
        /// </summary>
        public static Data.Is Is<T>(this T value) where T : class
        {
            return new Data.Is(value);
        }

        /// <summary>
        /// Checks if an object is null, if it is, return 'value', else return the original.
        /// </summary>
        public static object Fallback(ref object @object, object value)
        {
            if (@object == null) return value;
            return @object;
        }

        /// <summary>
        /// Checks if an object is null, if it is, return 'value', else return the original.
        /// </summary>
        public static T Fallback<T>(ref T @object, T value)
        {
            if (@object == null) return value;
            return @object;
        }

        /// <summary>
        /// Begins the operation of ensuring a GameObject has, is, or does something.
        /// </summary>
        public static Data.Ensure Ensure(this GameObject target)
        {
            return new Data.Ensure(target);
        }

        /// <summary>
        /// Begins the operation of ensuring a Transforms GameObject has, is, or does something.
        /// </summary>
        public static Data.Ensure Ensure(this Transform target)
        {
            return new Data.Ensure(target.gameObject);
        }

        /// <summary>
        /// Begins the operation of converting a value.
        /// </summary>
        public static Data.As As(this object value)
        {
            return new Data.As(value);
        }
    }
}