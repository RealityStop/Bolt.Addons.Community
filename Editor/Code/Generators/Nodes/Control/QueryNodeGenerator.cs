using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Linq;
using Unity.VisualScripting;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(QueryNode))]
    public class QueryNodeGenerator : LocalVariableGenerator
    {
        private QueryNode Unit => unit as QueryNode;
        public QueryNodeGenerator(Unit unit) : base(unit)
        {
        }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "System.Linq";
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (output == Unit.result)
            {
                if (!Unit.enter.hasValidConnection)
                {
                    writer.WriteErrorDiagnostic("The 'enter' ControlInput requires a connection", "Could not generate QueryNode");
                    return;
                }
                writer.GetVariable(variableName);
            }
            else if (output == Unit.item)
            {
                writer.GetVariable("item");
            }
        }

        private void GenerateOperation(CodeWriter writer, ControlGenerationData data)
        {
            switch (Unit.operation)
            {
                case QueryOperation.Any:
                case QueryOperation.Sum:
                    GenerateValue(Unit.collection, data, writer);
                    writer.Write("." + Unit.operation + "()");
                    break;

                case QueryOperation.Count:
                    GenerateValue(Unit.collection, data, writer);
                    writer.Write("." + Unit.operation + "() > " + "0".NumericHighlight());
                    break;

                case QueryOperation.AnyWithCondition:
                case QueryOperation.First:
                case QueryOperation.FirstOrDefault:
                case QueryOperation.Single:
                case QueryOperation.Last:
                case QueryOperation.LastOrDefault:
                case QueryOperation.Where:
                    GenerateLambda(writer, Unit.operation, Unit.condition, data);
                    break;

                case QueryOperation.OrderBy:
                case QueryOperation.OrderByDescending:
                    GenerateLambda(writer, Unit.operation, Unit.key, data);
                    break;

                case QueryOperation.Select:
                    GenerateLambda(writer, Unit.operation, Unit.value, data);
                    break;

                case QueryOperation.Skip:
                case QueryOperation.Take:
                    GenerateValue(Unit.collection, data, writer);
                    writer.Write("." + GetOperationMethod(Unit.operation) + "(");
                    GenerateValue(Unit.value, data, writer);
                    writer.Write(")");
                    break;
            }
        }

        private void GenerateLambda(CodeWriter writer, QueryOperation op, ValueInput returnValue, ControlGenerationData data)
        {
            GenerateValue(Unit.collection, data, writer);
            writer.Write("." + GetOperationMethod(op) + "(");
            writer.MultilineLambda(
                writer.Action(() =>
                {
                    if (Unit.body != null)
                        GenerateChildControl(Unit.body, data, writer);
                    writer.Return(writer.Action(() => GenerateValue(returnValue, data, writer)), WriteOptions.IndentedNewLineAfter);
                }),
                "item"
            );
            writer.Write(")");
        }

        private string GetOperationMethod(QueryOperation operation)
        {
            return operation switch
            {
                QueryOperation.AnyWithCondition => "Any",
                _ => operation.ToString()
            };
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            Type type = Unit.result?.type ?? typeof(object);

            if (type.IsGenericType)
            {
                var sourceType = GetSourceType(Unit.collection, data, writer);
                if (sourceType.IsGenericType)
                {
                    var actualType = Unit.operation == QueryOperation.OrderBy || Unit.operation == QueryOperation.OrderByDescending
                    ? typeof(IOrderedEnumerable<>)
                    : typeof(IEnumerable<>);
                    type = actualType.MakeGenericType(sourceType.GetGenericArguments()[0]);
                }
            }
            else if (type == typeof(object))
            {
                switch (Unit.operation)
                {
                    case QueryOperation.First:
                    case QueryOperation.FirstOrDefault:
                    case QueryOperation.Single:
                    case QueryOperation.Last:
                    case QueryOperation.LastOrDefault:
                        var sourceType = GetSourceType(Unit.collection, data, writer);
                        if (sourceType != typeof(object) && sourceType.IsGenericType)
                        {
                            type = sourceType.GetGenericArguments()[0];
                        }
                        break;
                }
            }

            data.CreateSymbol(Unit, type);

            variableName = data.AddLocalNameInScope("queryOperation", type);
            variableType = type;

            writer.CreateVariable(variableType, variableName, writer.Action(() => GenerateOperation(writer, data)));

            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}