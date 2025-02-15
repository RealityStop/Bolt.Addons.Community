using Unity.VisualScripting.Community.Utility;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [Inspectable]
    [TypeIcon(typeof(Field))]
    [RenamedFrom("Bolt.Addons.Community.Code.ConstructorDeclaration")]
    public abstract class ConstructorDeclaration : Macro<FlowGraph>
    {
        public AccessModifier scope;
        public ConstructorModifier modifier;

        [Inspectable]
        public CodeAsset parentAsset;
        [RenamedFrom("CallType")]
        public ConstructorInitializer initalizerType = ConstructorInitializer.None;

        [InspectorWide]
        public List<TypeParam> parameters = new List<TypeParam>();

#if UNITY_EDITOR
        public bool opened;
        public bool parametersOpened;
#endif
    }

    /// <summary>
    /// This is a empty class used for the typeIcon
    /// </summary>
    public class Field { }
}
