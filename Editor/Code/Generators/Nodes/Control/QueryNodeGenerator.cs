using Unity.VisualScripting;
using System.Collections.Generic;
using System.Linq;
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(QueryNode))]
    public sealed class QueryNodeGenerator : NodeGenerator<QueryNode>
    {
        public QueryNodeGenerator(QueryNode unit) : base(unit)
        {
        }

                public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            
            if (output == Unit.result)
            {
                return GenerateResultValue(data, true);
            }
            else if (output == Unit.item)
            {
                return GenerateItemValue(data);
            }
            return base.GenerateValue(output, data);
        }

        private string GenerateResultValue(ControlGenerationData data, bool output)
        {
            var semiColon = !output ? ";" : string.Empty; 
            switch (Unit.operation)
            {
                case QueryOperation.Any:
                    return GenerateValue(Unit.collection, data).TargetMethodCall("Any") + semiColon;

                case QueryOperation.Count:
                    return $"{GenerateValue(Unit.collection, data)}".TargetMethodCall("Count") + " > " + 0.ToString().NumericHighlight() + semiColon;

                case QueryOperation.Sum:
                    return GenerateValue(Unit.collection, data).TargetMethodCall("Sum") + semiColon;

                case QueryOperation.AnyWithCondition:
                    return GenerateValue(Unit.collection, data).TargetMethodCall("Any", null, GenerateConditionLambda(data));

                case QueryOperation.First:
                    return $"({GenerateValue(Unit.collection, data)}.Cast<object>().First({GenerateConditionLambda(data)}))";

                case QueryOperation.FirstOrDefault:
                    return $"({GenerateValue(Unit.collection, data)}.Cast<object>().FirstOrDefault({GenerateConditionLambda(data)}))";

                case QueryOperation.Last:
                    return $"({GenerateValue(Unit.collection, data)}.Cast<object>().Last({GenerateConditionLambda(data)}))";

                case QueryOperation.LastOrDefault:
                    return $"({GenerateValue(Unit.collection, data)}.Cast<object>().LastOrDefault({GenerateConditionLambda(data)}))";

                case QueryOperation.Where:
                    return $"({GenerateValue(Unit.collection, data)}.Cast<object>().Where({GenerateConditionLambda(data)}))";

                case QueryOperation.OrderBy:
                    return $"({GenerateValue(Unit.collection, data)}.Cast<object>().OrderBy({GenerateKeySelector(data)}))";

                case QueryOperation.OrderByDescending:
                    return $"({GenerateValue(Unit.collection, data)}.Cast<object>().OrderByDescending({GenerateKeySelector(data)}))";

                case QueryOperation.Select:
                    return $"({GenerateValue(Unit.collection, data)}.Cast<object>().Select({GenerateSelector(data)}))";

                case QueryOperation.Skip:
                    return $"({GenerateValue(Unit.collection, data)}.Cast<object>().Skip({GenerateValue(Unit.value, data)}))";

                case QueryOperation.Take:
                    return $"({GenerateValue(Unit.collection, data)}.Cast<object>().Take({GenerateValue(Unit.value, data)}))";

                default:
                    return "/* Unsupported operation */".WarningHighlight();
            }
        }

        private string GenerateItemValue(ControlGenerationData data)
        {
            return "current"; // Assumes `current` is set correctly in the implementation
        }

        private string GenerateConditionLambda(ControlGenerationData data)
        {
            return $"item => {{ GenerateValue(Unit.condition, data) }}";
        }

        private string GenerateKeySelector(ControlGenerationData data)
        {
            return $"item => {{ /* Lambda for key: {GenerateValue(Unit.key, data)} */ }}".WarningHighlight();
        }

        private string GenerateSelector(ControlGenerationData data)
        {
            return $"item => {{ /* Lambda for select: {GenerateValue(Unit.value, data)} */ }}".WarningHighlight();
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            if (input == Unit.enter && !Unit.result.hasValidConnection)
            {
                var output = CodeBuilder.Indent(indent) + GenerateResultValue(data, false) + "\n";
                output += GetNextUnit(Unit.exit, data, indent);
                return output;
            }
            else
            {
                return GetNextUnit(Unit.exit, data, indent);
            }
        }
    }
}
