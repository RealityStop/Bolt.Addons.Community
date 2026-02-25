using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static class SerializedPropertyExtensions
    {
        public static T GetSerializedValue<T>(this SerializedProperty property)
        {
            object @object = property.serializedObject.targetObject;
            string[] propertyNames = property.propertyPath.Split('.');

            // Clear the property path from "Array" and "data[i]".
            if (propertyNames.Length >= 3 && propertyNames[propertyNames.Length - 2] == "Array")
                propertyNames = propertyNames.Take(propertyNames.Length - 2).ToArray();

            // Get the last object of the property path.
            foreach (string path in propertyNames)
            {
                @object = @object.GetType()
                    .GetField(path, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    .GetValue(@object);
            }

            if (@object.GetType().GetInterfaces().Contains(typeof(IList<T>)))
            {
                int propertyIndex = int.Parse(property.propertyPath[property.propertyPath.Length - 2].ToString());

                return ((IList<T>)@object)[propertyIndex];
            }
            else return (T)@object;
        }
    }

}
