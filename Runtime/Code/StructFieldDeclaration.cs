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
#if VISUAL_SCRIPTING_1_7
        [Serialize]
        public SerializableType typeHandle;
#else
        [Serialize]
        public string typeHandleIdentification;
#endif

        public void OnAfterDeserialize()
        {
#if VISUAL_SCRIPTING_1_7
            if (!string.IsNullOrEmpty(typeHandle.Identification))
            {
                type = Type.GetType(typeHandle.Identification);
            }
#else
            if (!string.IsNullOrEmpty(typeHandleIdentification))
            {
                type = Type.GetType(typeHandleIdentification);
            }
#endif
            OnChanged?.Invoke();
        }

        public void OnBeforeSerialize()
        {
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