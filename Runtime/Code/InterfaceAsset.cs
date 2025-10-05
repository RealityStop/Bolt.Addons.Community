using Unity.VisualScripting;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.CSharp;

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
        [Serialize]
        public List<InterfaceMethodItem> methods = new List<InterfaceMethodItem>();

        [Inspectable]
        [InspectorWide]
        public List<SystemType> interfaces = new List<SystemType>();

#if UNITY_EDITOR
        public bool methodsOpen;
        public bool interfacesOpened;
        public bool propertiesOpen;
        public Texture2D icon;
#endif
    }
}