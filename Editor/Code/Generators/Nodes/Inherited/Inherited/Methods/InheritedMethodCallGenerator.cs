using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(InheritedMethodCall))]
    public class InheritedMethodCallGenerator : NodeGenerator<InheritedMethodCall>
    {
        private Dictionary<ValueOutput, string> outputNames;
        public InheritedMethodCallGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            var thisKeyword = Unit.showThisKeyword ? "this".ConstructHighlight() + "." : string.Empty;
            writer.WriteIndented(thisKeyword);
            writer.Write(Unit.member.name);
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

            var thisKeyword = Unit.showThisKeyword ? "this".ConstructHighlight() + "." : string.Empty;
            writer.Write(thisKeyword + Unit.member.name).Parentheses(w =>
            {
                GenerateArguments(writer, data);
            });
        }

        private void GenerateArguments(CodeWriter writer, ControlGenerationData data)
        {
            var method = Unit.member.isMethod ? Unit.member.methodInfo as MethodBase : Unit.member.constructorInfo;
            if (method == null) return;

            var parameters = method.GetParameters();
            int startIndex = Unit.member.isExtension && !Unit.member.isInvokedAsExtension ? 1 : 0;

            for (int i = startIndex; i < parameters.Length; i++)
            {
                var param = parameters[i];
                var input = Unit.InputParameters.TryGetValue(i, out var p) ? p : null;

                if (param.HasOutModifier())
                {
                    if (i != startIndex)
                        writer.ParameterSeparator();

                    string name = data.AddLocalNameInScope(param.Name, param.ParameterType).VariableHighlight();
                    writer.Write("out var ".ConstructHighlight() + name);

                    if (Unit.OutputParameters.TryGetValue(i, out var outValue) && !outputNames.ContainsKey(outValue))
                    {
                        outputNames.Add(outValue, "&" + name);
                    }
                }

                if (input == null)
                    continue;

                if (param.ParameterType.IsByRef)
                {
                    if (i != startIndex)
                        writer.ParameterSeparator();

                    if (input == null)
                    {
                        writer.Error($"Missing input for {param.Name}");
                        continue;
                    }

                    if (!input.hasValidConnection || (input.hasValidConnection && !input.connection.source.unit.IsValidRefUnit()))
                    {
                        writer.Error($"{input.key.Replace("%", "")} needs connection to a Get Variable or Get Member unit");
                        continue;
                    }

                    writer.Write("ref ".ConstructHighlight());
                    GenerateValue(input, data, writer);

                    var name = data.AddLocalNameInScope(param.Name, param.ParameterType).VariableHighlight();

                    if (Unit.OutputParameters.TryGetValue(i, out var outValue) && !outputNames.ContainsKey(outValue))
                        outputNames.Add(outValue, "&" + name);
                }
                else if (param.IsOptional && !input.hasValidConnection && !input.hasDefaultValue)
                {
                    continue;
                }
                else if (param.IsDefined(typeof(ParamArrayAttribute), false) && !input.hasValidConnection)
                {
                    continue;
                }
                else
                {
                    if (i != startIndex)
                        writer.ParameterSeparator();

                    GenerateValue(input, data, writer);
                }
            }
        }
    }
}