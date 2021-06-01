using UnityEngine;
using Unity.VisualScripting;
using System;
using Bolt.Addons.Integrations.Continuum;
using System.Collections.Generic;

namespace Bolt.Addons.Community.Code.Editor
{
    [CreateAssetMenu(menuName = "Visual Scripting/Community/Action")]
    [Serializable]
    public sealed class ActionAsset : CodeAsset, ISerializationCallbackReceiver
    {
        [Inspectable]
        public SystemType type = new SystemType() { type = typeof(Action) };

        [Inspectable]
        public List<GenericDeclaration> generics = new List<GenericDeclaration>();

        [SerializeField]
        [HideInInspector]
        private string qualifiedName;

        [Inspectable]
        public string displayName;

        public void OnAfterDeserialize()
        {
            if (!(string.IsNullOrWhiteSpace(qualifiedName) || string.IsNullOrEmpty(qualifiedName)))
            {
                type.type = Type.GetType(qualifiedName);
            }
        }

        public void OnBeforeSerialize()
        {
            if (type == null)
            {
                qualifiedName = string.Empty;
                return;
            }

            qualifiedName = type.type.AssemblyQualifiedName;
        }
    }

    [Inspectable]
    [Serializable]
    public sealed class GenericDeclaration
    {
        [Inspectable]
        public string name;
        [Inspectable]
        public SystemType type = new SystemType() { type = typeof(int) };
        [Inspectable]
        public SystemType constraint = new SystemType() { type = typeof(object) };
    }
}
