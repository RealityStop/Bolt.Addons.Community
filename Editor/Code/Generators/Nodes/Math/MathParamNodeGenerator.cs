using System;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(MathParamNode))]
    public class MathParamNodeGenerator : NodeGenerator<MathParamNode>
    {
        public MathParamNodeGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            for (int i = 0; i < Unit.argumentCount; i++)
            {
                if (i != 0)
                    writer.Write(MathOperator());

                GenerateValue(Unit.arguments[i], data, writer);
            }
        }

        private string MathOperator()
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