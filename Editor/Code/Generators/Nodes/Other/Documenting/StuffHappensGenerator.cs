
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(StuffHappens))]
    public class StuffHappensGenerator : NodeGenerator<StuffHappens>
    {

        public StuffHappensGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.Comment("Stuff Happens", WriteOptions.IndentedNewLineAfter);
            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}
