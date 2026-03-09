
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(WaitForNextFrameUnit))]
    public class WaitForNextFrameGenerator : NodeGenerator<WaitForNextFrameUnit>
    {
        public WaitForNextFrameGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            data.SetHasReturned(true);
            writer.YieldReturn(writer.Action(() => writer.Null()), WriteOptions.IndentedNewLineAfter);

            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}
