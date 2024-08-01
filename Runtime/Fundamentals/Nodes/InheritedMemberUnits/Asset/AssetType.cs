using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    
    [UnitCategory("CSharp")]
    [TypeIcon(typeof(Type))]
    public class AssetType : Unit
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public AssetType(){}
        public AssetType(ClassAsset asset)
        {
            this.asset = asset;
        }
    
        public ClassAsset asset;
    
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput type;
        
        protected override void Definition()
        {
            type = ValueOutput<object>(nameof(type), (flow) => asset.title);
        }
    }
    
}