#if VISUAL_SCRIPTING_1_7
using System;
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
            NameSpaces = "Unity.VisualScripting.Community";
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (output == Unit.graphOutput)
            {
                return GenerateValue(Unit.graphInput, data);
            }

            return base.GenerateValue(output, data);
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            data.SetExpectedType(typeof(GameObject));
            string goExpr = GenerateValue(Unit.target, data);
            data.RemoveExpectedType();
            data.SetExpectedType(typeof(TMacro));
            string macroCode = GenerateValue(Unit.graphInput, data);
            data.RemoveExpectedType();
            var builder = Unit.CreateClickableString();
            builder.Indent(indent);
            builder.InvokeMember(typeof(CSharpUtility), "SetGraph", new Type[] { typeof(TGraph), typeof(TMacro), typeof(TMachine) }, false, p1 => p1.Ignore(goExpr), p2 => p2.Ignore(macroCode))
            .EndLine();
            builder.Ignore(GetNextUnit(Unit.exit, data, indent));
            return builder;
        }
    }
}
#endif