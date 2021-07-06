using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using System;

namespace Bolt.Addons.Community.Code
{
    [CreateAssetMenu(menuName = "Visual Scripting/Community/Code/Struct")]
    public class StructAsset : MemberTypeAsset<StructFieldDeclaration>
    {
    }

    [Serializable]
    public abstract class MemberTypeAsset<TFieldDeclaration> : CodeAsset where TFieldDeclaration : FieldDeclaration
    {
        [Inspectable]
        [InspectorWide]
        public List<TFieldDeclaration> fields = new List<TFieldDeclaration>();
        [Inspectable]
        public bool inspectable = true;
        [Inspectable]
        public bool serialized = true;
        [Inspectable]
        public bool includeInSettings = true;
        [Inspectable]
        public bool definedEvent;
        [Inspectable]
        public int order;
        public bool fieldsOpened;
    }
}
