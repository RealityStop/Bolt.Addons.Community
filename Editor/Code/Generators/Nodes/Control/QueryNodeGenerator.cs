using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(QueryNode))]
    public class QueryNodeGenerator : NodeGenerator<QueryNode>
    {
        public QueryNodeGenerator(Unit unit) : base(unit)
        {
            NameSpaces = "System.Linq";
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (output == Unit.result)
            {
                switch (Unit.operation)
                {
                    case QueryOperation.Any:
                    case QueryOperation.Sum:
                        return $"{GenerateValue(Unit.collection, data)}{MakeClickableForThisUnit("." + Unit.operation + "()")}";

                    case QueryOperation.Count:
                        return $"{GenerateValue(Unit.collection, data)}{MakeClickableForThisUnit("." + Unit.operation + $"() > {"0".NumericHighlight()}")}";

                    case QueryOperation.AnyWithCondition:
                    case QueryOperation.First:
                    case QueryOperation.FirstOrDefault:
                    case QueryOperation.Single:
                    case QueryOperation.Last:
                    case QueryOperation.LastOrDefault:
                    case QueryOperation.Where:
                        return GenerateLambda(Unit.operation, Unit.condition, data);

                    case QueryOperation.OrderBy:
                    case QueryOperation.OrderByDescending:
                        return GenerateLambda(Unit.operation, Unit.key, data);

                    case QueryOperation.Select:
                        return GenerateLambda(Unit.operation, Unit.value, data);

                    case QueryOperation.Skip:
                    case QueryOperation.Take:
                        return $"{GenerateValue(Unit.collection, data)}{MakeClickableForThisUnit("." + GetOperationMethod(Unit.operation) + "(")}{GenerateValue(Unit.value, data)}{MakeClickableForThisUnit(")")}";
                }
            }
            else if (output == Unit.item)
            {
                return MakeClickableForThisUnit("item".VariableHighlight());
            }
            return base.GenerateValue(output, data);
        }

        private string GenerateLambda(QueryOperation op, ValueInput returnValue, ControlGenerationData data)
        {
            var indent = CodeBuilder.currentIndent;
            var lambdaBody =
                GetNextUnit(Unit.body, data, indent + 1) +
                CodeBuilder.GetCurrentIndent(Unit.body.hasValidConnection ? 0 : 1) +
                GenerateValue(returnValue, data).Return(true, Unit);

            return $"{GenerateValue(Unit.collection, data)}{MakeClickableForThisUnit("." + GetOperationMethod(op) + "(")}" +
                   CodeBuilder.MultiLineLambda(Unit, MakeClickableForThisUnit("item".VariableHighlight()), lambdaBody, indent);
        }

        private string GetOperationMethod(QueryOperation operation)
        {
            return operation switch
            {
                QueryOperation.AnyWithCondition => "Any",
                _ => operation.ToString()
            };
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return GetNextUnit(Unit.exit, data, indent);
        }
    }
}