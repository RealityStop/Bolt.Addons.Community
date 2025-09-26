#if VISUAL_SCRIPTING_1_7
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public abstract class HasGraphGenerator<TGraph, TMacro, TMachine> : LocalVariableGenerator
        where TGraph : class, IGraph, new()
        where TMacro : Macro<TGraph>
        where TMachine : Machine<TGraph, TMacro>
    {
        protected HasGraph<TGraph, TMacro, TMachine> Unit => unit as HasGraph<TGraph, TMacro, TMachine>;
        public HasGraphGenerator(Unit unit) : base(unit)
        {
            NameSpaces = "Unity.VisualScripting.Community";
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (output == Unit.hasGraphOutput)
            {
                if (!Unit.enter.hasValidConnection)
                {
                    data.SetExpectedType(typeof(GameObject));
                    string goExpr = GenerateValue(Unit.target, data);
                    data.RemoveExpectedType();
                    data.SetExpectedType(typeof(TMacro));
                    string macroCode = GenerateValue(Unit.graphInput, data);
                    data.RemoveExpectedType();
                    var builder = Unit.CreateClickableString();
                    builder.InvokeMember(typeof(CSharpUtility), "HasGraph", new Type[] { typeof(TGraph), typeof(TMacro), typeof(TMachine) }, false, p1 => p1.Ignore(goExpr), p2 => p2.Ignore(macroCode));
                    return builder;
                }
                else return MakeClickableForThisUnit(variableName.VariableHighlight());
            }

            return base.GenerateValue(output, data);
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            variableName = data.AddLocalNameInScope("hasGraphValue", typeof(bool));
            variableType = typeof(bool);
            data.SetExpectedType(typeof(GameObject));
            string goExpr = GenerateValue(Unit.target, data);
            data.RemoveExpectedType();
            data.SetExpectedType(typeof(TMacro));
            string macroCode = GenerateValue(Unit.graphInput, data);
            data.RemoveExpectedType();
            var builder = Unit.CreateClickableString();
            builder.Indent(indent);
            builder.Clickable("bool ".ConstructHighlight()).Clickable(variableName.VariableHighlight()).Equal(true)
            .InvokeMember(typeof(CSharpUtility), "HasGraph", new Type[] { typeof(TGraph), typeof(TMacro), typeof(TMachine) }, false, p1 => p1.Ignore(goExpr), p2 => p2.Ignore(macroCode))
            .EndLine();
            builder.Ignore(GetNextUnit(Unit.exit, data, indent));
            return builder;
        }
    }
}
#endif