using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    
    [Descriptor(typeof(AssetFieldUnit))]
    public class AssetFieldUnitDescriptor : UnitDescriptor<AssetFieldUnit>
    {
        public AssetFieldUnitDescriptor(AssetFieldUnit target) : base(target)
        {
        }
    
        protected override string DefinedSurtitle()
        {
            return target.field.parentAsset.title;
        }
    
        protected override EditorTexture DefinedIcon()
        {
            return target.field.type.Icon();
        }
    
        protected override string DefinedTitle()
        {
            return target.field.parentAsset.title + "." + target.field.FieldName;
        }
    
        protected override string DefinedShortTitle()
        {
            return target.field.FieldName;
        }
    }
    
}