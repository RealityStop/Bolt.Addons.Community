using Unity.VisualScripting;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Interface", menuName = "Visual Scripting/Community/Code/Interface")]
    [RenamedFrom("Bolt.Addons.Community.Code.InterfaceAsset")]
    public sealed class InterfaceAsset : CodeAsset
    {
        [Inspectable]
        [InspectorWide]
        public List<InterfacePropertyItem> variables = new List<InterfacePropertyItem>();

        [Inspectable]
        [InspectorWide]
        public List<InterfaceMethodItem> methods = new List<InterfaceMethodItem>();

#if UNITY_EDITOR
        public bool methodsOpen;
        public bool propertiesOpen;
        public Texture2D icon;
#endif
    }
}