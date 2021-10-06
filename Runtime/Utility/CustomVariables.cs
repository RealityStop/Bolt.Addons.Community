using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;
using System.Collections;

namespace Unity.VisualScripting.Community.Utility
{
    [IncludeInSettings(true)]
    [Serializable]
    [Inspectable]
    [RenamedFrom("Bolt.Addons.Community.Utility.Editor.CustomVariables")]
    public sealed class CustomVariables
    {
        [SerializeField]
        [Inspectable]
        [InspectorWide]
        public List<CustomVariable> variables = new List<CustomVariable>();
        public event Action onVariablesChanged = () => { };

        public bool Has(string name)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i].name == name) return true;
            }

            return false;
        }

        public object Get(string name)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i].name == name) return variables[i].value;
            }

            return null;
        }

        public T Get<T>(string name)
        {
            return (T)Get(name);
        }

        public void Set(string name, object value)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i].name == name)
                {
                    variables[i].value = value;
                    return;
                }
            }

            variables.Add(new CustomVariable() { name = name, value = value });
            onVariablesChanged();
        }

        public void CopyFrom(CustomVariables other, bool forceValue = false)
        {
            for (int comparerIndex = 0; comparerIndex < other.variables.Count; comparerIndex++)
            {
                var name = other.variables[comparerIndex].name;
                var value = other.variables[comparerIndex].value;

                if (value != null && value.GetType().Is().Enumerable()) value = ((IEnumerable)value).ConvertTo(value.GetType());
                if (!Has(name) || forceValue) Set(name, value);
            } 
        }

        public void Clear()
        {
            variables.Clear();
        }
    }
}