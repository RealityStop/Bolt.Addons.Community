using System.Collections;
using System.Text;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(RandomNumbersv2))]
    public class RandomNumbersv2Generator : LocalVariableGenerator
    {
        RandomNumbersv2 Unit => (RandomNumbersv2)unit;
        public RandomNumbersv2Generator(Unit unit) : base(unit) { NameSpaces = "System.Collections,Unity.VisualScripting.Community"; }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (Unit.input.hasValidConnection)
            {
                return MakeClickableForThisUnit(variableName.VariableHighlight());
            }
            else
            {
                string count = GenerateValue(Unit.count, data);
                string minimum = GenerateValue(Unit.minimum, data);
                string maximum = GenerateValue(Unit.maximum, data);
                return CodeBuilder.CallCSharpUtilityMethod(Unit, MakeClickableForThisUnit("RandomNumbers"), new string[] { count, minimum, maximum, Unit.integer.As().Code(false, Unit), Unit.aotList.As().Code(false, Unit), Unit.unique.As().Code(false, Unit) });
            }
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            string indentStr = CodeBuilder.Indent(indent);
            variableName = data.AddLocalNameInScope("randomNumbersList", typeof(IList));

            string count = GenerateValue(Unit.count, data);
            string min = GenerateValue(Unit.minimum, data);
            string max = GenerateValue(Unit.maximum, data);
            string isInt = Unit.integer.As().Code(false, Unit);
            string isAot = Unit.aotList.As().Code(false, Unit);
            string isUnique = Unit.unique.As().Code(false, Unit);

            string methodCall = CodeBuilder.CallCSharpUtilityMethod(Unit, MakeClickableForThisUnit("RandomNumbers"), new[] { count, min, max, isInt, isAot, isUnique });

            return new StringBuilder().Append(indentStr).Append(MakeClickableForThisUnit($"{"IList".TypeHighlight()} {variableName.VariableHighlight()} = ") + $"{methodCall}{MakeClickableForThisUnit(";")}").AppendLine().AppendLine(GetNextUnit(Unit.exit, data, indent)).ToString();
        }

    }
}
