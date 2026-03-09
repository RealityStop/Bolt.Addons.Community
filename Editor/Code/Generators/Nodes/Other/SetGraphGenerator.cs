#if VISUAL_SCRIPTING_1_7
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    public abstract class SetGraphGenerator<TGraph, TMacro, TMachine> : NodeGenerator<SetGraph<TGraph, TMacro, TMachine>>
        where TGraph : class, IGraph, new()
        where TMacro : Macro<TGraph>
        where TMachine : Machine<TGraph, TMacro>
    {
        public SetGraphGenerator(Unit unit) : base(unit)
        {
        }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "Unity.VisualScripting.Community";
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (output == Unit.graphOutput)
            {
                GenerateValue(Unit.graphInput, data, writer);
                return;
            }
            base.GenerateValueInternal(output, data, writer);
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.CallCSharpUtilityGenericMethod("SetGraph", new CodeWriter.TypeParameter[] { typeof(TGraph), typeof(TMacro), typeof(TMachine) },
            writer.Action(() => GenerateValue(Unit.target, data, writer)),
            writer.Action(() => GenerateValue(Unit.graphInput, data, writer)));

            writer.WriteEnd(EndWriteOptions.LineEnd);

            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}
#endif