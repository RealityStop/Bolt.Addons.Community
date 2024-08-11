using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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

    public static List<T> MergeLists<T>(params IList[] lists)
    {
        List<T> mergedList = new List<T>();

        foreach (System.Collections.IList list in lists)
        {
            foreach (var item in list)
            {
                if (item is T convertedItem)
                {
                    mergedList.Add(convertedItem);
                }
                else
                {
                    Debug.LogWarning($"{item} is not {typeof(T).As().CSharpName(false, false, false).RemoveHighlights().RemoveMarkdown()} skipping");
                }
            }
        }

        return mergedList;
    }

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
                else
                {
                    Debug.LogError($"Could not add {key} to merged dictionary the key already exists");
                }
            }
        }
        return mergedDictionary;
    }

    public static Dictionary<Tkey, TValue> MergeDictionariesReplace<Tkey, TValue>(params Dictionary<Tkey, TValue>[] dictionaries)
    {
        Dictionary<Tkey, TValue> mergedDictionary = new();

        foreach (var dictionary in dictionaries)
        {
            foreach (var key in dictionary.Keys)
            {
                if (mergedDictionary.ContainsKey(key))
                {
                    mergedDictionary[key] = dictionary[key];
                }
                else
                {
                    mergedDictionary.Add(key, dictionary[key]);
                }
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
