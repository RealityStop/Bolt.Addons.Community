using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(AssetActionUnit))]
    public class AssetActionUnitGenerator : NodeGenerator<AssetActionUnit>
    {
        public AssetActionUnitGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.Write(Unit.method.methodName);
        }
    } 
}
