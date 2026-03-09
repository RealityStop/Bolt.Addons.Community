using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(AssetMethodCallUnit))]
    public class AssetMethodCallUnitGenerator : NodeGenerator<AssetMethodCallUnit>
    {
        private Dictionary<ValueOutput, string> outputNames;
        public AssetMethodCallUnitGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented(Unit.method.methodName);
            writer.Write("(");
            GenerateArguments(writer, data);
            writer.Write(")");
            writer.Write(";");
            writer.NewLine();
            GenerateExitControl(Unit.exit, data, writer);
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (Unit.enter != null && !Unit.enter.hasValidConnection && Unit.OutputParameters.Count > 0)
            {
                writer.Error("Control Port Enter requires a connection");
                return;
            }

            if (Unit.OutputParameters.ContainsValue(output))
            {
                var transformedKey = outputNames[output].Replace("&", "").Replace("%", "");
                writer.GetVariable(transformedKey);
                return;
            }

            writer.Write(Unit.method.methodName).Parentheses(w => 
            {
                GenerateArguments(writer, data);
            });
        }

        private void GenerateArguments(CodeWriter writer, ControlGenerationData data)
        {
            var method = Unit.method;
            if (method == null) return;

            var parameters = method.parameters;

            for (int i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                var input = Unit.InputParameters.TryGetValue(i, out var p) ? p : null;

                if (parameter.modifier == ParameterModifier.Out)
                {
                    if (i != 0)
                        writer.ParameterSeparator();

                    string name = data.AddLocalNameInScope(parameter.name, parameter.type).VariableHighlight();
                    writer.Write("out var ".ConstructHighlight() + name);

                    if (Unit.OutputParameters.TryGetValue(i, out var outValue) && !outputNames.ContainsKey(outValue))
                    {
                        outputNames.Add(outValue, "&" + name);
                    }
                }

                if (input == null)
                    continue;

                if (parameter.modifier == ParameterModifier.Ref)
                {
                    if (i != 0)
                        writer.ParameterSeparator();

                    if (input == null)
                    {
                        writer.Error($"Missing input for {parameter.name}");
                        continue;
                    }

                    if (!input.hasValidConnection || (input.hasValidConnection && !input.connection.source.unit.IsValidRefUnit()))
                    {
                        writer.Error($"{input.key.Replace("%", "")} needs connection to a Get Variable or Get Member unit");
                        continue;
                    }

                    writer.Write("ref ".ConstructHighlight());
                    GenerateValue(input, data, writer);

                    var name = data.AddLocalNameInScope(parameter.name, parameter.type).VariableHighlight();

                    if (Unit.OutputParameters.TryGetValue(i, out var outValue) && !outputNames.ContainsKey(outValue))
                        outputNames.Add(outValue, "&" + name);
                }
                else if (parameter.hasDefault && !input.hasValidConnection && !input.hasDefaultValue)
                {
                    continue;
                }
                else if (parameter.modifier == ParameterModifier.Params && !input.hasValidConnection)
                {
                    continue;
                }
                else
                {
                    if (i != 0)
                        writer.ParameterSeparator();

                    GenerateValue(input, data, writer);
                }
            }
        }
    }
}