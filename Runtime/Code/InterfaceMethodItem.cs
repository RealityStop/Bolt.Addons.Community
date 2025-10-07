using Unity.VisualScripting;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Utility;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [Inspectable]
    [RenamedFrom("Bolt.Addons.Community.Code.InterfaceMethodItem")]
    public sealed class InterfaceMethodItem
    {
        [Inspectable]
        public string name;
        [Inspectable]
        public SystemType returnType = new SystemType(typeof(void));
        [Inspectable]
        [InspectorWide]
        [Serialize]
        public List<TypeParam> parameters = new List<TypeParam>();

#if UNITY_EDITOR
        public bool opened, parametersOpened;
#endif
    }
}