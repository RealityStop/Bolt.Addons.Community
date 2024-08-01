using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    
    [FuzzyOption(typeof(AssetType))]
    public class AssetTypeOption : UnitOption<AssetType>
    {
        public AssetTypeOption(AssetType unit) : base(unit)
        {
        }
    
        public override IUnit InstantiateUnit()
        {
            return new AssetType(unit.asset);
        }
    }
    
}