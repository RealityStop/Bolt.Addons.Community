using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [CustomPropertyDrawer(typeof(SystemType))]
    public sealed class SystemTypeDrawer : PropertyDrawer
    {
        private bool cached;
        private List<InheritsAttribute> inheritance = new List<InheritsAttribute>();
        private Type[] typesOf;

        private void Cache(SerializedProperty property)
        {
            if (!cached)
            {
                inheritance = fieldInfo.GetCustomAttributes(typeof(InheritsAttribute)).Cast<InheritsAttribute>().ToList();
                cached = true;
            }
        }
         
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Cache(property);

            var buttonRect = position.Set().Height(20);
            SystemType type = property.GetSerializedValue<SystemType>();
            var stringName = type.type == null ? "Null" : type.type.Name + "(" + type.type.FullName.Replace("." + type.type.Name, string.Empty) + ")";

            if (GUI.Button(buttonRect, stringName))
            {
                GenericMenu menu = new GenericMenu();

                if (typesOf == null || typesOf.Length == 0)
                {
                    if (inheritance.Count > 0)
                    {
                        for (int i = 0; i < inheritance.Count; i++)
                        {
                            typesOf = inheritance[i].type.Get().Derived();

                            for (int j = 0; j < typesOf.Length; j++)
                            {
                                var index = j;
                                menu.AddItem(new GUIContent(typesOf[j].FullName.Replace(".", "/")), false, () =>
                                {
                                    type.type = typesOf[index];
                                });
                            }
                        }
                    }
                    else
                    {
                        typesOf = typeof(object).Get().Derived();

                        for (int j = 0; j < typesOf.Length; j++)
                        {
                            var index = j;
                            menu.AddItem(new GUIContent(typesOf[j].FullName.Replace(".", "/")), false, () =>
                            {
                                type.type = typesOf[index];
                            });
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < typesOf.Length; j++)
                    {
                        var index = j;
                        menu.AddItem(new GUIContent(typesOf[j].FullName.Replace(".", "/")), false, () =>
                        {
                            type.type = typesOf[index];
                        });
                    }
                }

                menu.ShowAsContext();
            }
        }
    }

}