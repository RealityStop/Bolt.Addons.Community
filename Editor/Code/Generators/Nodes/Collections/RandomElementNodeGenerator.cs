using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(RandomElementNode))]
    public class RandomElementNodeGenerator : LocalVariableGenerator
    {
        RandomElementNode Unit => (RandomElementNode)unit;

        public RandomElementNodeGenerator(Unit unit) : base(unit)
        {
        }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "Unity.VisualScripting.Community";
        }


        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (Unit.enter.hasValidConnection)
            {
                writer.Write(variableName.VariableHighlight());
                writer.Dot();
                writer.Write(output == Unit.key ? "key".VariableHighlight() : "value".VariableHighlight());
                return;
            }

            writer.CallCSharpUtilityMethod("GetRandomElement", writer.Action(() => GenerateValue(Unit.collection, data, writer)), Unit.Dictionary.As().Code(false));

            writer.Dot();
            writer.Write(output == Unit.key ? "key".VariableHighlight() : "value".VariableHighlight());
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            variableName = data.AddLocalNameInScope("randomElement", typeof((object, object)));

            writer.CreateVariable(variableName, writer.Action(() =>
            {
                writer.CallCSharpUtilityMethod("GetRandomElement", writer.Action(() => GenerateValue(Unit.collection, data, writer)), Unit.Dictionary.As().Code(false));
            }));

            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}