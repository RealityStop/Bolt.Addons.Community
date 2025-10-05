using UnityEngine;
using Unity.VisualScripting;
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    [CreateAssetMenu(menuName = "Visual Scripting/Community/Code/Delegate")]
    [RenamedFrom("Bolt.Addons.Community.Code.DelegateAsset")]
    [Serializable]
    public sealed class DelegateAsset : CodeAsset
    {
        [Inspectable]
        [SerializeField]
        [Serialize]
        public SystemType type = new SystemType(typeof(Action));

        [Inspectable]
        public string displayName;
    }
}
