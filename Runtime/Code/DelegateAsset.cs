using UnityEngine;
using Unity.VisualScripting;
using System;
using Bolt.Addons.Libraries.CSharp;
using System.Collections.Generic;

namespace Bolt.Addons.Community.Code
{
    [CreateAssetMenu(menuName = "Visual Scripting/Community/Code/Delegate")]
    [Serializable]
    public sealed class DelegateAsset : CodeAsset
    {
        [Inspectable]
        [SerializeField]
        public SystemType type = new SystemType() { type = typeof(Action) };

        [Inspectable]
        public List<GenericDeclaration> generics = new List<GenericDeclaration>();

        [Inspectable]
        public string displayName;
    }
}
