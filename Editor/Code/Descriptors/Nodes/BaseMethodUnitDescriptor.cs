using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    
    [Descriptor(typeof(BaseMethodCall))]
    public class BaseMethodUnitDescriptor : UnitDescriptor<BaseMethodCall>
    {
        public BaseMethodUnitDescriptor(BaseMethodCall target) : base(target)
        {
        }
    
        protected override EditorTexture DefinedIcon()
        {
            return target.member.type.Icon();
        }
    
        protected override string DefinedTitle()
        {
            return "base." + target.member.name;
        }
    
        protected override string DefinedShortTitle()
        {
            return target.member.name;
        }
    }
    
}