using Bolt.Addons.Community.Utility;
using Bolt.Addons.Libraries.CSharp;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Code
{
    [Serializable]
    [Inspectable]
    public abstract class ConstructorDeclaration : Macro<FlowGraph>
    {
        public AccessModifier scope;
        public ConstructorModifier modifier;

        [Inspectable]
        public ClassAsset classAsset;

        [Inspectable]
        public StructAsset structAsset;

        [Serialize]
        [InspectorWide]
        public List<TypeParam> parameters = new List<TypeParam>();

#if UNITY_EDITOR
        public bool opened;
        public bool parametersOpened;
#endif
    }
}
