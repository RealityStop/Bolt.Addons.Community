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
        /// <summary>
        /// This is only so that its possible to override the SystemObjectInspector
        /// To draw the inspector for the value without the Type
        /// </summary>
        public SerializableType typeHandle;

        [InspectorToggleLeft]
        public object defaultValue = 0;

        [Serialize]
        private object serlizer;

        [SerializeField]
        [HideInInspector]
        private SerializationData serializedValue;

        [SerializeField]
        [HideInInspector]
        private SerializationData serializedValueType;

        public void OnAfterDeserialize()
        {
            type = (Type)serializedValueType.Deserialize();
            serlizer = serializedValue.Deserialize();
            defaultValue = serlizer;
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
                serlizer = defaultValue.Serialize();
            }

            if (type == null)
            {
                serializedValueType = new SerializationData();
                return;
            }
            else
            {
                serializedValueType = type.Serialize();
            }
        }
    }
}
