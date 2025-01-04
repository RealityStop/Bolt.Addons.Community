using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [Inspectable]
    [RenamedFrom("Bolt.Addons.Community.Code.StructFieldDeclaration")]
    public sealed class StructFieldDeclaration : FieldDeclaration, ISerializationCallbackReceiver
    {
        [Serialize]
        public SerializableType typeHandle;

        public void OnAfterDeserialize()
        {
            if (!string.IsNullOrEmpty(typeHandle.Identification))
            {
                type = Type.GetType(typeHandle.Identification);
            }
            OnSerialized?.Invoke();
        }

        public void OnBeforeSerialize()
        {
            if (type == null)
            {
                typeHandle = new SerializableType();
                return;
            }
            else
            {
                typeHandle.Identification = type.AssemblyQualifiedName;
            }
        }
    }
}