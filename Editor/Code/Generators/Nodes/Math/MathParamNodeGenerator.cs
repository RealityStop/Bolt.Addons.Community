using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(MathParamNode))]
    public class MathParamNodeGenerator : NodeGenerator<MathParamNode>
    {
        public MathParamNodeGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return GetArguments(data);
        }

        string GetArguments(ControlGenerationData data)
        {
            var arguments = new List<string>();
            for (var i = 0; i < Unit.argumentCount; i++)
            {
                arguments.Add(GenerateValue(Unit.arguments[i], data));
            }
            return string.Join(MakeClickableForThisUnit(MathType()), arguments);
        }

        string MathType()
        {
            return Unit.OperationType switch
            {
                MathParamNode.MathType.Add => " + ",
                MathParamNode.MathType.Subtract => " - ",
                MathParamNode.MathType.Multiply => " * ",
                MathParamNode.MathType.Divide => " / ",
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }
}
