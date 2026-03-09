using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMValue_Children
    {
        /// <summary>
        /// Invokes an action if the object is null.
        /// </summary>
        public static HUMValue.Data.IsNull Null(this HUMValue.Data.Is @is, Action onNull = null)
        {
            if (@is.value == null)
            {
                onNull?.Invoke();
                return new HUMValue.Data.IsNull(@is, true);
            }
            else
            {
                return new HUMValue.Data.IsNull(@is, false);
            }
        }

        /// <summary>
        /// Invokes an action if the object checked for null, but was not null.
        /// </summary>
        public static void Else(this HUMValue.Data.IsNull isNull, Action notNull = null)
        {
            if (!isNull.isNull) notNull?.Invoke();
        }

        /// <summary>
        /// Invokes an action if an object was checked for being not null, and was.
        /// </summary>
        public static void Else(this HUMValue.Data.NotNull isNotNull, Action notNull = null)
        {
            if (isNotNull.isNull) notNull?.Invoke();
        }

        /// <summary>
        /// Invokes an action if the object is not null.
        /// </summary>
        public static HUMValue.Data.NotNull NotNull(this HUMValue.Data.Is @is, Action notNull = null)
        {
            if (@is.value != null)
            {
                notNull?.Invoke();
                return new HUMValue.Data.NotNull(@is, false);
            }
            else
            {
                return new HUMValue.Data.NotNull(@is, true);
            }
        }

        /// <summary>
        /// Begins an or operation if an object is checked for null but isn't.
        /// </summary>
        public static HUMValue.Data.Or Or<T>(this HUMValue.Data.IsNull isNull, T value) where T : class
        {
            return new HUMValue.Data.Or(value, isNull.isNull);
        }

        /// <summary>
        /// Begins an or operation if an object is check for not being null, but is.
        /// </summary>
        public static HUMValue.Data.Or Or<T>(this HUMValue.Data.NotNull notNull, T value) where T : class
        {
            return new HUMValue.Data.Or(value, notNull.isNull);
        }

        /// <summary>
        /// Creates an instance of a type with optional constructor parameters.
        /// </summary>
        public static T New<T>(this HUMValue.Data.Create create, params object[] constructorParameters)
        {
            var type = typeof(T);
            if (type.IsValueType) return default(T);
            return (T)Activator.CreateInstance(type, constructorParameters);
        }

        /// <summary>
        /// Create an instance of a type with optional constructor parameters.
        /// </summary>
        public static object New(this HUMValue.Data.Create create, Type type, params object[] constructorParameters)
        { 
            if (type.IsAbstract) return null;
            if (type == typeof(int)) return 0;
            if (type == typeof(float)) return 0f;
            if (type == typeof(bool)) return false;
            if (type == typeof(string)) return string.Empty;
            if (type.Inherits(typeof(UnityEngine.Object))) return null;
            return Activator.CreateInstance(type, constructorParameters);
        }

        /// <summary>
        /// Begins an operation of checking if a type was not something, is this.
        /// </summary>
        public static HUMValue.Data.Is Is(this HUMValue.Data.Or or)
        {
            return new HUMValue.Data.Is(or.value);
        }

        /// <summary>
        /// Ensures that a MonoBehaviour script exists on the GameObject.
        /// </summary>
        public static TMonoBehaviour Exists<TMonoBehaviour>(this HUMValue.Data.MBehaviour mono, out TMonoBehaviour component)
            where TMonoBehaviour : MonoBehaviour
        {
            component = mono.ensure.target.GetComponent<TMonoBehaviour>();
            if (component == null) component = mono.ensure.target.AddComponent<TMonoBehaviour>();
            return component;
        }

        /// <summary>
        /// Ensures that a MonoBehaviour script exists on the GameObject.
        /// </summary>
        public static TMonoBehaviour Exists<TMonoBehaviour>(this HUMValue.Data.MBehaviour mono)
            where TMonoBehaviour : MonoBehaviour
        {
            var component = mono.ensure.target.GetComponent<TMonoBehaviour>();
            if (component != null) return component;
            return mono.ensure.target.AddComponent<TMonoBehaviour>();
        }

        /// <summary>
        /// Ensures that a MonoBehaviour of a type of interface exists. If it does not, add TDerivedBackup, else we return the interface.
        /// </summary>
        public static TInterface Exists<TInterface, TDerivedBackup>(this HUMValue.Data.Interface mono, out TInterface interf)
            where TInterface : class
            where TDerivedBackup : MonoBehaviour
        {
            interf = mono.ensure.ensure.target.GetComponent<TInterface>();
            if (interf == null) interf = mono.ensure.ensure.target.AddComponent<TDerivedBackup>() as TInterface;
            return interf;
        }

        /// <summary>
        /// Begins the operation of checking interfaces with MonoBehaviours.
        /// </summary>
        public static HUMValue.Data.Interface Interface(this HUMValue.Data.MBehaviour ensureMono)
        {
            return new HUMValue.Data.Interface(ensureMono);
        }

        /// <summary>
        /// Begins the operation of checking a MonoBehaviour.
        /// </summary>
        public static HUMValue.Data.MBehaviour Behaviour(this HUMValue.Data.Ensure ensureData)
        {
            return new HUMValue.Data.MBehaviour(ensureData);
        }
    }
}
