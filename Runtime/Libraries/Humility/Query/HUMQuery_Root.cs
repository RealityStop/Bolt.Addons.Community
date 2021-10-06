using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMQuery
    {
        /// <summary>
        /// Performs a for loop with an action.
        /// </summary>
        public static Data.For<T> For<T>(this IList<T> collection, System.Action<IList<T>, int> action)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                action(collection, i);
            }

            return new Data.For<T>(collection);
        }

        /// <summary>
        /// Begins a for operation on a collection.
        /// </summary>
        public static Data.For<T> For<T>(this IList<T> collection)
        {
            return new Data.For<T>(collection);
        }

        public static TKey[] KeysToArray<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            return dictionary.Keys.ToArray();
        }

        public static List<TKey> KeysToList<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            return dictionary.Keys.ToList();
        }

        public static TValue[] ValuesToArray<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            return dictionary.Values.ToArray();
        }

        public static List<TValue> ValuesToList<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            return dictionary.Values.ToList();
        }

        /// <summary>
        /// Defines new keys in a dictionary if it does not exist.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="deserializedDictionary"></param>
        /// <param name="key"></param>
        /// <param name="onCreated"></param>
        /// <param name="onAdded"></param>
        /// <returns></returns>
        public static TValue Define<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Dictionary<TKey, TValue> deserializedDictionary, TKey key, Func<TValue, TValue> onCreated, Action<TValue> exists)
        {
            TValue value = (TValue)HUMValue.Create().New(typeof(TValue));
            
            if (deserializedDictionary.ContainsKey(key))
            {
                value = deserializedDictionary[key];
                exists?.DynamicInvoke(value);
            }
            else
            {
                value = onCreated(value);
                dictionary.Add(key, value);
            }

            return value; 
        }

        public static TValue DefineValueByKey<TValue>(this Dictionary<Type, TValue> dictionary, Dictionary<Type, TValue> deserializedDictionary, Type key, Func<TValue, TValue> onCreated, Action<TValue> exists)
        {
            TValue value = (TValue)HUMValue.Create().New(key);

            if (deserializedDictionary.ContainsKey(key))
            {
                value = deserializedDictionary[key];
                exists?.DynamicInvoke(value);
            }
            else
            {
                value = onCreated(value);
                dictionary.Add(key, value);
            }

            return value;
        }

        /// <summary>
        /// Removes all unused values from one dictionary, that don't exist in another.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        public static void Undefine<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Dictionary<TKey, TValue> deserializedDictionary, Action<TValue> onRemoved)
        {
            var keys = deserializedDictionary.Keys.ToList();
            var values = deserializedDictionary.Values.ToList();

            for (int i = 0; i < deserializedDictionary.Count; i++)
            {
                if (!dictionary.ContainsKey(keys[i]))
                {
                    var value = deserializedDictionary[keys[i]];
                    deserializedDictionary.Remove(keys[i]);
                    onRemoved(value);
                }
            }
        }

        /// <summary>
        /// Removes all unused values from one dictionary, that don't exist in another.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        public static void Undefine<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Dictionary<TKey, TValue> deserializedDictionary, Action<TValue> onRemoved, ref bool isNew)
        {
            if (!isNew)
            {
                var keys = deserializedDictionary.Keys.ToList();
                var values = deserializedDictionary.Values.ToList();

                for (int i = 0; i < deserializedDictionary.Count; i++)
                {
                    if (!dictionary.ContainsKey(keys[i]))
                    {
                        var value = deserializedDictionary[keys[i]];
                        deserializedDictionary.Remove(keys[i]);
                        onRemoved(value);
                    }
                }
            }

            isNew = true;
        }

        public static void MergeUnique<T>(this List<T> list, List<T> other)
        {
            for (int i = 0; i < other?.Count; i++)
            {
                if (!list.Contains(other[i])) list.Add(other[i]);
            }
        }

        public static void RemoveUnmatched<T>(this List<T> list, List<T> checkList)
        {
            var removables = new List<T>();
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 0; j < checkList.Count; j++)
                {
                    if (!checkList.Contains(list[i])) removables.Add(list[i]);
                }
            }

            for (int i = 0; i < removables.Count; i++)
            {
                list.Remove(removables[i]);
            }
        }

        public static void RemoveUnmatchedTypes<T>(this List<T> list, List<T> checkList)
        {
            var removables = new List<T>();
            var checkListTypes = new List<Type>();

            for (int i = 0; i < checkList.Count; i++)
            {
                checkListTypes.Add(checkList[i].GetType());
            }

            for (int i = 0; i < list.Count; i++)
            {
                var hadType = false;
                for (int j = 0; j < checkListTypes.Count; j++)
                {
                    if (list[i].GetType() == checkListTypes[j]) hadType = true;
                }
                if (!hadType) removables.Add(list[i]);
            }

            for (int i = 0; i < removables.Count; i++)
            {
                list.Remove(removables[i]);
            }
        }

        public static void AddNonContainedTypes<T>(this List<T> list, List<T> checkList)
        {
            var checkListTypes = new List<Type>();
            var addables = new List<T>();

            for (int i = 0; i < checkList.Count; i++)
            {
                checkListTypes.Add(checkList[i].GetType());
            }

            for (int i = 0; i < list.Count; i++)
            {
                var hadType = false;
                for (int j = 0; j < checkListTypes.Count; j++)
                {
                    if (list[i].GetType() == checkListTypes[j]) hadType = true;
                }
                if (!hadType) addables.Add(checkList[i]);
            }

            for (int i = 0; i < addables.Count; i++)
            {
                list.Add(addables[i]);
            }
        }
    }
}