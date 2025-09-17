using System.Collections;
using System.Text;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(RandomElementNode))]
    public class RandomElementNodeGenerator : LocalVariableGenerator
    {
        RandomElementNode Unit => (RandomElementNode)unit;

        public RandomElementNodeGenerator(Unit unit) : base(unit) { NameSpaces = "Unity.VisualScripting.Community"; }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (Unit.enter.hasValidConnection)
            {
                if (output == Unit.key)
                    return MakeClickableForThisUnit($"{variableName.VariableHighlight()}.{"key".VariableHighlight()}");
                else
                    return MakeClickableForThisUnit($"{variableName.VariableHighlight()}.{"value".VariableHighlight()}");
            }
            else
            {
                var collection = GenerateValue(Unit.collection, data);
                var methodCall = CodeBuilder.CallCSharpUtilityMethod(Unit, MakeClickableForThisUnit("GetRandomElement"), new[]
                {
                    collection,
                    Unit.Dictionary.As().Code(false, Unit)
                });

                if (output == Unit.key)
                    return methodCall + $"{MakeClickableForThisUnit($".{"key".VariableHighlight()}")}";
                else
                    return methodCall + $"{MakeClickableForThisUnit($".{"value".VariableHighlight()}")}";
            }
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            string indentStr = CodeBuilder.Indent(indent);
            variableName = data.AddLocalNameInScope("randomElement", typeof((object, object)));

            string collection = GenerateValue(Unit.collection, data);
            string isDict = Unit.Dictionary.As().Code(false, Unit);

            string methodCall = CodeBuilder.CallCSharpUtilityMethod(Unit, MakeClickableForThisUnit("GetRandomElement"), new[] { collection, isDict });

            StringBuilder sb = new();
            sb.AppendLine($"{indentStr}{MakeClickableForThisUnit($"{"var".ConstructHighlight()} {variableName.VariableHighlight()} = ")}{methodCall}{MakeClickableForThisUnit(";")}");

            return sb.AppendLine(GetNextUnit(Unit.exit, data, indent)).ToString();
        }
    }
}