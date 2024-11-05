using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [Inspectable]
    [RenamedFrom("Bolt.Addons.Community.Code.ClassFieldDeclaration")]
    public sealed class ClassFieldDeclaration : FieldDeclaration, ISerializationCallbackReceiver
    {
        [Serialize]
        public SerializableType typeHandle;

        [InspectorToggleLeft]
        [Serialize]
        public object defaultValue = 0;

        [Serialize]
        private SerializationData serializedValue;

        public void OnAfterDeserialize()
        {
            type = Type.GetType(typeHandle.Identification);
            defaultValue = serializedValue.Deserialize();
            OnSerialized?.Invoke();
        }

        public void OnBeforeSerialize()
        {
            if (defaultValue == null)
            {
                serializedValue = new SerializationData();
                return;
            }
            else
            {
                serializedValue = defaultValue.Serialize();
            }

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