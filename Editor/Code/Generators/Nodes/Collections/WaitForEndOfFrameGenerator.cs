using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(WaitForEndOfFrameUnit))]
    public class WaitForEndOfFrameGenerator : NodeGenerator<WaitForEndOfFrameUnit>
    {
        public WaitForEndOfFrameGenerator(Unit unit) : base(unit)
        {
        }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "UnityEngine";
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            data.SetHasReturned(true);
            writer.YieldReturn(writer.Action(() => writer.New(typeof(WaitForEndOfFrame))), WriteOptions.IndentedNewLineAfter);

            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}
