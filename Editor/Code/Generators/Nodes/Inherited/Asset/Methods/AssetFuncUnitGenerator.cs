using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(AssetFuncUnit))]
    public class AssetFuncUnitGenerator : NodeGenerator<AssetFuncUnit>
    {
        public AssetFuncUnitGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.Write(Unit.method.methodName);
        }
    }
}
