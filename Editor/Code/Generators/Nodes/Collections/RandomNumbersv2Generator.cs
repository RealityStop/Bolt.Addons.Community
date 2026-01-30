using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(RandomNumbersv2))]
    public class RandomNumbersv2Generator : LocalVariableGenerator
    {
        RandomNumbersv2 Unit => (RandomNumbersv2)unit;

        public RandomNumbersv2Generator(Unit unit) : base(unit)
        {
        }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "System.Collections";
            yield return "Unity.VisualScripting.Community";
        }


        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (Unit.input.hasValidConnection)
            {
                writer.Write(variableName.VariableHighlight());
                return;
            }

            writer.CallCSharpUtilityMethod("RandomNumbers",
                writer.Action(() => GenerateValue(Unit.count, data, writer)),
                writer.Action(() => GenerateValue(Unit.minimum, data, writer)),
                writer.Action(() => GenerateValue(Unit.maximum, data, writer)),
                Unit.integer.As().Code(false),
                Unit.aotList.As().Code(false),
                Unit.unique.As().Code(false)
            );
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            variableName = data.AddLocalNameInScope("randomNumbersList", typeof(IList));

            writer.CreateVariable(typeof(IList), variableName, writer.Action(() =>
                {
                    writer.CallCSharpUtilityMethod("RandomNumbers",
                    writer.Action(() => GenerateValue(Unit.count, data, writer)),
                    writer.Action(() => GenerateValue(Unit.minimum, data, writer)),
                    writer.Action(() => GenerateValue(Unit.maximum, data, writer)),
                    Unit.integer.As().Code(false),
                    Unit.aotList.As().Code(false),
                    Unit.unique.As().Code(false)
                    );
                })
            );

            writer.WriteEnd(EndWriteOptions.LineEnd);

            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}
