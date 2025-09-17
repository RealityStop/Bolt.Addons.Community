using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community
{

    [NodeGenerator(typeof(AssetType))]
    public class AssetTypeGenerator : NodeGenerator<AssetType>
    {
        public AssetTypeGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {

            return MakeClickableForThisUnit("typeof".ConstructHighlight() + "(" + Unit.asset.title.TypeHighlight() + ")");
        }
    }

}