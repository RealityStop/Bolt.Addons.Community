using Unity.VisualScripting.Community.Utility;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Inspectable]
    [Serializable]
    [RenamedFrom("Bolt.Addons.Community.Code.AttributeDeclaration")]
    public class AttributeDeclaration
    {
        [Inspectable]
        [Serialize]
        [SerializeField]
        private SystemType attributeType = new SystemType();
        public List<TypeParam> parameters = new List<TypeParam>();
        public int constructor;
        private string serializedTypeName;

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
                SetParameters();
            }
        }

        public Type GetAttributeType()
        {
            return attributeType.type;
        }

        private void SetParameters()
        {
        }
    }
}