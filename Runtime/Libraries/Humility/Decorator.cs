using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    /// <summary>
    /// Binds itself to a type. Allows for easy lookup of a type, based on an objects instance.
    /// Perfect for creating plugins and modules where the implementation (the decorated type) is seperate from the decorator type.
    /// </summary>
    /// <typeparam name="TDecorator"></typeparam>
    /// <typeparam name="TDecoratorAttribute"></typeparam>
    /// <typeparam name="TDecoratedType"></typeparam>
    [Serializable]
    public abstract class Decorator<TDecorator, TDecoratorAttribute, TDecoratedType>
        where TDecoratorAttribute : DecoratorAttribute
        where TDecorator : Decorator<TDecorator, TDecoratorAttribute, TDecoratedType>
    {
        private static Dictionary<Type, Type> singleDecoratorTypes;
        private static Dictionary<Type, List<Type>> multiDecoratorTypes;
        private static Dictionary<TDecoratedType, List<object>> multiDecorators = new Dictionary<TDecoratedType, List<object>>();
        private static Dictionary<TDecoratedType, TDecorator> singleDecorators = new Dictionary<TDecoratedType, TDecorator>();
        public static TDecorator activeDecorator; /// <summary> The current active editor. </summary>
        public static TDecoratedType activeDecorated; /// <summary> The current active editor. </summary>
        [SerializeField] public TDecoratedType decorated; 

        /// <summary>
        /// Find a decorator based on its type.
        /// </summary>
        /// <param name="decorated"></param>
        /// <returns></returns>
        public static TDecoratorType GetMultiDecorator<TDecoratorType>(TDecoratedType decorated, params object[] parameters) where TDecoratorType : Decorator<TDecorator, TDecoratorAttribute, TDecoratedType>
        {
            if (decorated == null) return null;
            var decorator = GetDecoratorFromDecorated<TDecoratorType>(decorated, parameters);
            if (decorator.decorated == null) decorator.decorated = decorated;

            return decorator;
        }

        public static TDecorator GetSingleDecorator(TDecoratedType decorated, params object[] parameters)
        {
            if (decorated == null) return null;
            var decorator = GetDecoratorFromDecorated(decorated, parameters);
            if (decorator.decorated == null) decorator.decorated = decorated;

            return decorator;
        }

        public static T GetSingleDecorator<T>(TDecoratedType decorated, params object[] parameters) where T : TDecorator
        {
            return GetSingleDecorator(decorated) as T;
        }

        private static Type GetDecoratorType(Type decoratedType)
        {
            if (decoratedType == null) return null;

            if (singleDecoratorTypes == null) CacheSingleDecorators();

            Type result = null;
            if (singleDecoratorTypes.TryGetValue(decoratedType, out result)) return result;

            return GetDecoratorType(decoratedType.BaseType);
        }

        private static Type GetDecoratorType(Type decoratedType, Type decoratorType)
        {
            if (decoratedType == null) return null;

            if (multiDecoratorTypes == null) CacheMultiDecorators();
            if (!multiDecoratorTypes.ContainsKey(decoratedType) || !multiDecoratorTypes[decoratedType].Any((d) => { return d == decoratorType; })) CacheMultiDecorators();

            Type result = null;
            result = multiDecoratorTypes[decoratedType].First((type) => { return type == decoratorType; });

            if (result != null) return result;

            return GetDecoratorType(decoratedType.BaseType, decoratorType);
        }

        private static TDecoratorType GetDecoratorFromDecorated<TDecoratorType>(TDecoratedType decorated, params object[] parameters) where TDecoratorType : Decorator<TDecorator, TDecoratorAttribute, TDecoratedType>
        {
            var hasDecorator = multiDecorators.ContainsKey(decorated) && multiDecorators[decorated].Any((d) => { return d.GetType() == typeof(TDecoratorType); });

            if (!hasDecorator)
            {
                Type type = decorated.GetType();
                if (!multiDecorators.ContainsKey(decorated)) multiDecorators.Add(decorated, new List<object>());
                Type decoratorType = GetDecoratorType(type, typeof(TDecoratorType));
                var decorator = (TDecoratorType)Activator.CreateInstance(decoratorType, parameters);
                decorator.decorated = decorated;
                multiDecorators[decorated].Add(decorator);
                return decorator;
            }

            for (int i = 0; i < multiDecorators[decorated].Count; i++)
            {
                if (multiDecorators[decorated][i] as TDecoratorType != null) return multiDecorators[decorated][i] as TDecoratorType;
            }
            return (TDecoratorType)multiDecorators[decorated][0];
        }

        private static TDecorator GetDecoratorFromDecorated(TDecoratedType decorated, params object[] parameters)
        {
            var hasDecorator = singleDecorators.ContainsKey(decorated);

            if (!hasDecorator)
            {
                Type type = decorated.GetType();
                Type decoratorType = GetDecoratorType(type);
                var decorator = (TDecorator)Activator.CreateInstance(decoratorType, parameters);
                decorator.decorated = decorated;
                singleDecorators.Add(decorated, decorator);
                return decorator;
            }
            return singleDecorators[decorated];
        }

        private static void CacheSingleDecorators()
        {
            singleDecoratorTypes = new Dictionary<Type, Type>();

            Type[] decorators = typeof(TDecorator).Get().Derived();

            for (int i = 0; i < decorators.Length; i++)
            {
                if (decorators[i].IsAbstract) continue;
                var attribs = decorators[i].GetCustomAttributes(typeof(TDecoratorAttribute), false);
                if (attribs == null || attribs.Length == 0) continue;
                TDecoratorAttribute attrib = attribs[0] as TDecoratorAttribute;
                if (singleDecoratorTypes.ContainsKey(attrib.type))
                {
                    singleDecoratorTypes[attrib.type] = decorators[i];
                }
                else
                {
                    singleDecoratorTypes.Add(attrib.type, decorators[i]);
                }
            }
        }
        private static void CacheMultiDecorators()
        {
            multiDecoratorTypes = new Dictionary<Type, List<Type>>();

            Type[] decorators = typeof(TDecorator).Get().Derived();

            for (int i = 0; i < decorators.Length; i++)
            {
                if (decorators[i].IsAbstract) continue;
                var attribs = decorators[i].GetCustomAttributes(typeof(TDecoratorAttribute), false);
                if (attribs == null || attribs.Length == 0) continue;
                TDecoratorAttribute attrib = attribs[0] as TDecoratorAttribute;
                if (multiDecoratorTypes.ContainsKey(attrib.type))
                {
                    multiDecoratorTypes[attrib.type].Add(decorators[i]);
                }
                else
                {
                    multiDecoratorTypes.Add(attrib.type, new List<Type>() { decorators[i] });
                }
            }
        }
    }
}
