using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

public static class CSharpUtility
{
    public static IList MergeLists(params IList[] lists)
    {
        List<object> mergedList = new();

        foreach (System.Collections.IList list in lists)
        {
            foreach (var item in list)
            {
                mergedList.Add(item);
            }
        }

        return mergedList;
    }

    public static List<T> MergeLists<T>(params IEnumerable<object>[] lists)
    {
        var mergedList = new List<T>();

        foreach (var list in lists)
        {
            foreach (var item in list)
            {
                if (item is T convertedItem)
                {
                    mergedList.Add(convertedItem);
                }
                else
                {
                    Debug.LogWarning($"{item} is not {typeof(T).As().CSharpName(false, true, false)}, skipping.");
                }
            }
        }

        return mergedList;
    }

    public static object ConvertType<T>(this T value, Type type)
    {
        if (value.IsConvertibleTo(type, true))
        {
            return value.ConvertTo(type);
        }
        else return value;
    }

    private static readonly HashSet<(GameObject, EventHook, System.Action<CustomEventArgs>)> registeredEvents = new HashSet<(GameObject, EventHook, System.Action<CustomEventArgs>)>();

    public static void RegisterCustomEvent(GameObject target, System.Action<CustomEventArgs> action)
    {
        var hook = new EventHook(EventHooks.Custom, target);
        var eventKey = (target, hook, action);

        if (!registeredEvents.Contains(eventKey))
        {
            registeredEvents.Add(eventKey);
            EventBus.Register(hook, action);
        }
    }

    public static object GetArgument(this CustomEventArgs args, int index, Type targetType)
    {
        return args.arguments[index].ConvertType(targetType);
    }

    public static void Bind(this IDelegate @delegate, IDelegate delegateToBind)
    {
        @delegate.Bind(delegateToBind);
    }

    public static void Bind(this IDelegate @delegate, Delegate delegateToBind)
    {
        @delegate.GetDelegate();
    }

    public static object CreateWaitForSeconds(float time, bool unscaled)
    {
        return unscaled ? new WaitForSecondsRealtime(time) : new WaitForSeconds(time);
    }

    public static bool Chance(float probability)
    {
        probability = Mathf.Clamp01(probability / 100f);
        return UnityEngine.Random.value <= probability;
    }

    /// <summary>
    /// Merges two or more dictionaries together.
    /// </summary>
    /// <remarks>
    /// If the same key is found more than once, only the value
    /// of the first dictionary with this key will be used.
    /// </remarks>
    public static AotDictionary MergeDictionaries(params IDictionary[] dictionaries)
    {
        AotDictionary mergedDictionary = new();

        foreach (var dictionary in dictionaries)
        {
            foreach (var key in dictionary.Keys)
            {
                if (!mergedDictionary.Contains(key))
                {
                    mergedDictionary.Add(key, dictionary[key]);
                }
            }
        }
        return mergedDictionary;
    }

    /// <summary>
    /// Merges two or more dictionaries together.
    /// </summary>
    /// <remarks>
    /// If the same key is found more than once, only the value
    /// of the first dictionary with this key will be used.
    /// </remarks>
    public static Dictionary<TKey, TValue> MergeDictionaries<TKey, TValue>(params IDictionary[] dictionaries)
    {
        Dictionary<TKey, TValue> mergedDictionary = new();

        foreach (var dictionary in dictionaries)
        {
            foreach (var key in dictionary.Keys)
            {
                if (key is TKey convertedKey)
                {
                    if (!mergedDictionary.ContainsKey(convertedKey))
                    {
                        if (dictionary[key] is TValue convertedValue)
                        {
                            mergedDictionary.Add(convertedKey, convertedValue);
                        }
                        else
                        {
                            Debug.LogWarning($"{dictionary[key]} is not {typeof(TValue).As().CSharpName(false, true, false)}, skipping.");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"{key} is not {typeof(TKey).As().CSharpName(false, true, false)}, skipping.");
                }
            }
        }
        return mergedDictionary;
    }

    /// <summary>
    /// Merges two or more dictionaries together.
    /// </summary>
    /// <remarks>
    /// If the same key is found more than once, it will be
    /// replaced with the latest key.
    /// </remarks>
    public static Dictionary<Tkey, TValue> MergeDictionariesReplace<Tkey, TValue>(params Dictionary<Tkey, TValue>[] dictionaries)
    {
        Dictionary<Tkey, TValue> mergedDictionary = new();

        foreach (var dictionary in dictionaries)
        {
            foreach (var key in dictionary.Keys)
            {
                mergedDictionary[key] = dictionary[key];
            }
        }

        return mergedDictionary;
    }

    /// <summary>
    /// Merges two or more dictionaries together.
    /// </summary>
    /// <remarks>
    /// If the same key is found more than once, it will be
    /// replaced with the latest key.
    /// </remarks>
    public static IDictionary MergeDictionariesReplace(params IDictionary[] dictionaries)
    {
        IDictionary mergedDictionary = new Dictionary<object, object>();

        foreach (var dictionary in dictionaries)
        {
            foreach (var key in dictionary.Keys)
            {
                mergedDictionary[key] = dictionary[key];
            }
        }

        return mergedDictionary;
    }

    public static float CalculateAverage(params float[] values)
    {
        if (values.Length == 0)
        {
            return 0f;
        }

        float sum = 0f;
        foreach (float value in values)
        {
            sum += value;
        }

        return sum / values.Length;
    }

    public static float CalculateMax(params float[] values)
    {
        if (values.Length == 0)
        {
            return 0f;
        }

        var value = values.Max();

        return value;
    }

    public static float CalculateMin(params float[] values)
    {
        if (values.Length == 0)
        {
            return 0f;
        }

        var value = values.Min();

        return value;
    }

    public static float Normalize(float value)
    {
        if (value == 0)
        {
            return 0f;
        }

        return value / Mathf.Abs(value);
    }
}
