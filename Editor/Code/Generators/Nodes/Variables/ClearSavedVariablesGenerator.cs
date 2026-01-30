using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(ClearSavedVariables))]
    public class ClearSavedVarsGenerator : NodeGenerator<ClearSavedVariables>
    {
        public ClearSavedVarsGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented();
            writer.CallCSharpUtilityMethod("ClearSavedVariables");
            writer.WriteEnd(EndWriteOptions.LineEnd);
        }
    }
}
