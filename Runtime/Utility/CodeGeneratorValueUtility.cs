using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

public static class CodeGeneratorValueUtility
{
    private static Dictionary<ValueInput, (string name, Object value)> ValueInputObjectValueHandlers = new Dictionary<ValueInput, (string name, Object value)>();
    private static Dictionary<string, Object> ObjectValueHandlers = new Dictionary<string, Object>();

    public static void AddValue(ValueInput valueInput, string VariableName, Object VariableValue)
    {
        if (!ValueInputObjectValueHandlers.ContainsKey(valueInput))
            ValueInputObjectValueHandlers.Add(valueInput, (VariableName, VariableValue));
    }

    public static Dictionary<string, Object> GetAllValues()
    {
        return ValueInputObjectValueHandlers.Select(kvp => kvp.Value).ToDictionary(v => v.name, v => v.value).Concat(ObjectValueHandlers).ToDictionary(v => v.Key, v => v.Value);
    }
}