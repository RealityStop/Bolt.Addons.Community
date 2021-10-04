using System;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [Serializable]
    public sealed class SystemType : ISerializationCallbackReceiver
    {
        public Type type;
        [SerializeField][HideInInspector]
        private string qualifiedName;

        public SystemType() { }
        public SystemType(Type type) { this.type = type; }

        public void OnAfterDeserialize()
        {
            if (!(string.IsNullOrWhiteSpace(qualifiedName) || string.IsNullOrEmpty(qualifiedName)))
            {
                type = Type.GetType(qualifiedName);
            }
        }

        public void OnBeforeSerialize()
        {
            if (type == null)
            {
                qualifiedName = string.Empty;
                return;
            }

            qualifiedName = type.AssemblyQualifiedName;
        }
    }
}