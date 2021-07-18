using Unity.VisualScripting;
using System;
using System.Collections.Generic;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Code
{
    [Serializable]
    [Inspectable]
    public sealed class InterfaceMethodItem
    {
        [Inspectable]
        public string name;
        [Inspectable]
        public Type returnType = typeof(object);
        [Inspectable]
        [InspectorWide]
        public List<TypeParam> parameters = new List<TypeParam>();

#if UNITY_EDITOR
        public bool opened, parametersOpened;
#endif
    }
}