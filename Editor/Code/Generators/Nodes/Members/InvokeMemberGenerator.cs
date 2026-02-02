using Unity;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using System;
using System.Reflection;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(InvokeMember))]
    public sealed class InvokeMemberGenerator : LocalVariableGenerator
    {
        private InvokeMember Unit => unit as InvokeMember;
        private Dictionary<ValueOutput, string> outputNames;
        public InvokeMemberGenerator(InvokeMember unit) : base(unit)
        {
        }

        public override IEnumerable<string> GetNamespaces()
        {
            if (Unit.member.isExtension)
            {
                yield return Unit.member.info.DeclaringType.Namespace;
            }
            else
            {
                yield return Unit.member.declaringType.Namespace;
            }
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            outputNames = new Dictionary<ValueOutput, string>();
            bool hasResultConnection = Unit.result != null && Unit.result.hasValidConnection;

            if (hasResultConnection)
            {
                variableName = data.AddLocalNameInScope(Unit.member.info.SelectedName(true).LegalVariableName(false) + "_Variable", Unit.result.type);
                variableType = Unit.result.type;

                writer.WriteIndented("var ".ConstructHighlight());
                writer.Write(variableName.VariableHighlight());
                writer.Equal();
            }
            else if (Unit.result != null)
            {
                variableType = Unit.result.type;
            }
            else
            {
                variableType = Unit.member.declaringType;
            }

            if (Unit.member.isConstructor)
            {
                if (!hasResultConnection)
                    writer.WriteIndented();
                writer.Write("new ".ConstructHighlight());
                writer.Write(Unit.member.pseudoDeclaringType);

                writer.Parentheses(w =>
                {
                    GenerateArguments(w, data);
                });
            }
            else
            {

                if (Unit.member.isInvokedAsExtension)
                {
                    if (!hasResultConnection)
                        writer.WriteIndented();
                    GenerateValue(Unit.target, data, writer);
                }
                else if (Unit.member.isExtension)
                {
                    if (!hasResultConnection)
                        writer.WriteIndented();
                    GenerateValue(Unit.inputParameters[0], data, writer);
                }
                else if (Unit.target == null)
                {
                    if (!hasResultConnection)
                        writer.WriteIndented();
                    writer.Write(Unit.member.pseudoDeclaringType);
                }
                else
                {
                    if (!hasResultConnection)
                        writer.WriteIndented();
                    GenerateValue(Unit.target, data, writer);
                    writer.Write(Unit.target.GetComponent(GetSourceType(Unit.target, data, writer), Unit.target.type, true, true));
                }

                writer.Dot();
                writer.Write(Unit.member.name);
                writer.Parentheses(w => GenerateArguments(w, data));
            }

            writer.Write(";");
            writer.NewLine();

            GenerateExitControl(Unit.exit, data, writer);
        }

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == null)
            {
                writer.Write("/* Value Input is null */".ErrorHighlight());
                return;
            }

            if (input.hasValidConnection)
            {
                GenerateConnectedValue(input, data, writer);
            }
            else if (input.hasDefaultValue)
            {
                var isGameObject = input.type == typeof(GameObject);
                var isComponent = typeof(Component).IsAssignableFrom(input.type);
                if (input == Unit.target && (isGameObject || isComponent))
                {
                    writer.Write("gameObject".VariableHighlight());
                    if (isComponent)
                    {
                        writer.GetComponent(null, input.type);
                    }
                }
                else if (input == Unit.target && !input.nullMeansSelf)
                {
                    writer.Error($"\"{input.key} Requires Input\"");
                }
                else
                {
                    writer.Write(input.unit.defaultValues[input.key].As().Code(true, true, true, "", false, true));
                }
            }
            else
            {
                if (Unit.member.isMethod)
                {
                    if (Unit.member.methodInfo.GetParameters()[Unit.inputParameters.FirstOrDefault(parameter => parameter.Value == input).Key].IsDefined(typeof(ParamArrayAttribute), true))
                    {
                        return;
                    }
                    else if (Unit.member.methodInfo.GetParameters()[Unit.inputParameters.FirstOrDefault(parameter => parameter.Value == input).Key].IsOptional)
                    {
                        return;
                    }
                }
                else if (Unit.member.isConstructor)
                {
                    if (Unit.member.constructorInfo.GetParameters()[Unit.inputParameters.FirstOrDefault(parameter => parameter.Value == input).Key].IsDefined(typeof(ParamArrayAttribute), true))
                    {
                        return;
                    }
                    else if (Unit.member.constructorInfo.GetParameters()[Unit.inputParameters.FirstOrDefault(parameter => parameter.Value == input).Key].IsOptional)
                    {
                        return;
                    }
                }
                writer.Error($"\"{input.key} Requires Input\"");
            }
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (output == Unit.result)
            {
                if (!Unit.enter.hasValidConnection && Unit.outputParameters.Count > 0)
                {
                    writer.Write($"/* Control Port Enter of {Unit.member.ToDeclarer()} requires a connection */".ErrorHighlight());
                    return;
                }

                if (Unit.enter.hasValidConnection)
                {
                    if (data.ContainsNameInAncestorScope(variableName))
                        writer.Write(variableName.VariableHighlight());
                    else
                        writer.Write($"/* Variable '{variableName}' not found in scope */".ErrorHighlight());
                    return;
                }

                variableType = Unit.result.type;

                if (Unit.member.isConstructor)
                {
                    writer.Write("new ".ConstructHighlight());
                    writer.Write(Unit.member.pseudoDeclaringType);
                    writer.Parentheses(w => GenerateArguments(w, data));
                }
                else
                {
                    if (Unit.member.isInvokedAsExtension)
                        GenerateValue(Unit.target, data, writer);
                    else if (Unit.member.isExtension)
                        GenerateValue(Unit.inputParameters[0], data, writer);
                    else if (Unit.target == null)
                        writer.Write(Unit.member.pseudoDeclaringType);
                    else
                    {
                        GenerateValue(Unit.target, data, writer);
                        writer.Write(Unit.target.GetComponent(GetSourceType(Unit.target, data, writer), Unit.target.type, true, true));
                    }

                    writer.Dot();
                    writer.Write(Unit.member.name);
                    writer.Parentheses(w => GenerateArguments(w, data));
                }

                return;
            }

            if (Unit.outputParameters.ContainsValue(output))
            {
                var transformedKey = outputNames[output].Replace("&", "").Replace("%", "");
                writer.Write(transformedKey.VariableHighlight());
                return;
            }

            if (output == Unit.targetOutput)
            {
                GenerateValue(Unit.target, data, writer);
                return;
            }

            base.GenerateValueInternal(output, data, writer);
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
                var input = Unit.inputParameters.TryGetValue(i, out var p) ? p : null;

                if (param.HasOutModifier())
                {
                    if (i != startIndex)
                        writer.ParameterSeparator();

                    string name = data.AddLocalNameInScope(param.Name, param.ParameterType).VariableHighlight();
                    writer.Write("out var ".ConstructHighlight() + name);

                    if (Unit.outputParameters.TryGetValue(i, out var outValue) && !outputNames.ContainsKey(outValue))
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

                    if (Unit.outputParameters.TryGetValue(i, out var outValue) && !outputNames.ContainsKey(outValue))
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