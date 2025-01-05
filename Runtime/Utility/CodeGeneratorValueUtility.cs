using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

public static class CodeGeneratorValueUtility
{
    private static Dictionary<Object, Dictionary<string, Object>> ObjectValueHandlers = new Dictionary<Object, Dictionary<string, Object>>();
    public static Object currentAsset;
    public static void AddValue(Object target, string variableName, Object variableValue)
    {
        if (!ObjectValueHandlers.ContainsKey(target))
        {
            ObjectValueHandlers.Add(target, new Dictionary<string, Object>() { { variableName, variableValue } });
        }
        else if (!ObjectValueHandlers[target].ContainsKey(variableName))
        {
            ObjectValueHandlers[target].Add(variableName, variableValue);
        }
    }

    public static bool TryGetVariable(Object target, Object value, out string variableName)
    {
        if (ObjectValueHandlers.ContainsKey(target) && ObjectValueHandlers[target].ContainsValue(value))
        {
            variableName = ObjectValueHandlers[target].First(kvp => kvp.Value == value).Key;
            return true;
        }
        variableName = "";
        return false;
    }
    public static Dictionary<string, Object> GetAllValues(Object target)
    {
        return ObjectValueHandlers[target];
    }
}