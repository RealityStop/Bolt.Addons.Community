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
#if VISUAL_SCRIPTING_1_7
        [Serialize]
        public SerializableType typeHandle;
#else
        [Serialize]
        public string typeHandleIdentification;
#endif

        [InspectorToggleLeft]
        [Serialize]
        public object defaultValue = 0;

        [Serialize]
        private SerializationData serializedValue;

        public void OnAfterDeserialize()
        {
#if VISUAL_SCRIPTING_1_7
            type = Type.GetType(typeHandle.Identification);
#else
            type = Type.GetType(typeHandleIdentification);
#endif
            defaultValue = serializedValue.Deserialize();
            OnChanged?.Invoke();
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
#if VISUAL_SCRIPTING_1_7
                typeHandle = new SerializableType();
#else
                typeHandleIdentification = null;
#endif
                return;
            }
            else
            {
#if VISUAL_SCRIPTING_1_7
                typeHandle.Identification = type.AssemblyQualifiedName;
#else
                typeHandleIdentification = type.AssemblyQualifiedName;
#endif
            }
        }
    }
}