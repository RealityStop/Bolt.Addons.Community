using System;
using UnityEngine;

namespace Bolt.Addons.Libraries.CSharp
{
    [Serializable]
    public sealed class SystemType : ISerializationCallbackReceiver
    {
        public Type type;
        [SerializeField][HideInInspector]
        private string qualifiedName;

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