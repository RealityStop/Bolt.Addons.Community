using Unity.VisualScripting.Community.Utility;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [Inspectable]
    [RenamedFrom("Bolt.Addons.Community.Code.ConstructorDeclaration")]
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
