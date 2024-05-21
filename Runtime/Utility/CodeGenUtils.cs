using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public static class CodeGenUtils
{
    public static System.Collections.IList MergeLists(params System.Collections.IList[] lists)
    {
        List<object> mergedList = new();

        foreach (System.Collections.IList list in lists)
        {
            mergedList.Add(list);
        }

        return mergedList;
    }

    public static AotDictionary MergeDictionaries(params IDictionary[] dictionaries)
    {
        AotDictionary mergedDictionary = new();

        foreach (var dictionary in dictionaries)
        {
            for (var i = 0; i < dictionary.Count; i++)
            {
                if (!(mergedDictionary.Keys as List<object>).Contains((mergedDictionary.Keys as List<object>)[i]))
                {
                    mergedDictionary.Add((mergedDictionary.Keys as List<object>)[i], dictionary[i]);
                }
                else 
                {
                    Debug.LogError($"Could not add {(mergedDictionary.Keys as List<object>)[i]} to merged dictionary the key already exists");
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
