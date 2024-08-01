using Unity.VisualScripting.Community.Utility;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;

namespace Unity.VisualScripting.Community
{
    [Inspectable]
    [Serializable]
    [RenamedFrom("Bolt.Addons.Community.Code.AttributeDeclaration")]
    public class AttributeDeclaration
    {
        [Inspectable]
        [SerializeField]
        private SystemType attributeType = new SystemType();
        [SerializeField]
        public List<TypeParam> parameters = new List<TypeParam>();
        public int constructor = 0;
        public int selectedconstructor;

#if UNITY_EDITOR
        public bool opened;
#endif

        /// <summary>
        /// Sets the field attributeType
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void SetType<T>() where T : Attribute
        {
            SetType(typeof(T));
        }

        /// <summary>
        /// Sets the field attributeType
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void SetType(Type type)
        {
            if (attributeType.type != type)
            {
                attributeType.type = type;
            }
        }

        public Type GetAttributeType()
        {
            return attributeType.type;
        }

        public void AddParameter(string name, Type type, object value)
        {
            TypeParam matchingParam = parameters.FirstOrDefault(param => param.name == name);

            if (matchingParam != null)
            {
                matchingParam.defaultValue = value;
            }
            else
            {
                TypeParam paramValue = new TypeParam
                {
                    name = name,
                    type = type,
                    defaultValue = value
                };

                parameters.Add(paramValue);
            }
        }
    }
}