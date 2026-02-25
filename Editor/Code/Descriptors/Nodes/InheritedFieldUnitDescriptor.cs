using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    
    [Descriptor(typeof(InheritedFieldUnit))]
    public class InheritedFieldUnitDescriptor : UnitDescriptor<InheritedFieldUnit>
    {
        public InheritedFieldUnitDescriptor(InheritedFieldUnit target) : base(target)
        {
        }
    
        protected override EditorTexture DefinedIcon()
        {
            return target.member.type.Icon();
        }
    
        protected override string DefinedTitle()
        {
            return "this." + target.member.name;
        }
    
        protected override string DefinedShortTitle()
        {
            return target.member.name;
        }
    }
    
}