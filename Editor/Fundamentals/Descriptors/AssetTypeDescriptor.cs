using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    
    [Descriptor(typeof(AssetType))]
    public class AssetTypeDescriptor : UnitDescriptor<AssetType>
    {
        public AssetTypeDescriptor(AssetType target) : base(target)
        {
        }
    
        protected override string DefinedSubtitle()
        {
            return target.asset.title;
        }
    }
    
}