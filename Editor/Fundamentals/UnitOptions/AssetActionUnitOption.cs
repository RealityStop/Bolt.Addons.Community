using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    [FuzzyOption(typeof(AssetActionUnit))]
    public class AssetActionUnitOption : UnitOption<AssetActionUnit>
    {
        public AssetActionUnitOption(AssetActionUnit assetFuncUnit) : base(assetFuncUnit)
        {
            sourceScriptGuids =  Unity.VisualScripting.LinqUtility.ToHashSet(UnitBase.GetScriptGuids(unit.method.returnType));
        }
    
        protected override string Label(bool human)
        {
            return "this." + unit.method.methodName;
        }
    
        public override bool favoritable => false;
    
        protected override int Order()
        {
            return 0;
        }
    
        public override string SearchResultLabel(string query)
        {
            return "this." + unit.method.methodName;
        }
    
        protected override string Haystack(bool human)
        {
            return $"this{(human ? ": " : ".")}{Label(human)}";
        }
    } 
}
