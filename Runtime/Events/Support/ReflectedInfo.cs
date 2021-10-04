using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.DefinedEvents.Support.ReflectedInfo")]
    class ReflectedInfo
    {
        public static ReflectedInfo For<T>()
        {
            return For(typeof(T));
        }

        public static ReflectedInfo For(Type T)
        {
            lock (Reflections)
            {
                if (Reflections.ContainsKey(T))
                {
                    return Reflections[T];
                }
                var newInfo = new ReflectedInfo(T);
                Reflections.Add(T, newInfo);
                return newInfo;
            }
        }

        private static Dictionary<Type, ReflectedInfo> Reflections = new Dictionary<Type, ReflectedInfo>();




        public Dictionary<string, FieldInfo> reflectedFields { get; } = new Dictionary<string, FieldInfo>();

        public Dictionary<string, PropertyInfo> reflectedProperties { get; } = new Dictionary<string, PropertyInfo>();

        private Type type;


        public ReflectedInfo(Type t)
        {
            type = t;
            DefineInputsForEventType();
        }

        private void DefineInputsForEventType()
        {
            reflectedFields.Clear();
            reflectedProperties.Clear();

            foreach (var field in type.GetFields())
            {
                if (field.IsPublic)
                {
                    reflectedFields.Add(field.Name, field);
                }
            }

            foreach (var property in type.GetProperties())
            {
                var setMethod = property.GetSetMethod();

                if (setMethod != null)
                {
                    if (setMethod.IsPublic)
                    {
                        reflectedProperties.Add(property.Name, property);
                    }
                }
            }
        }
    }
}