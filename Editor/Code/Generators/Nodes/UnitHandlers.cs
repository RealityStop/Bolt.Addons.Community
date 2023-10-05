using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using UnityEngine.Windows;

namespace Unity.VisualScripting.Community.Generated
{
    public sealed class GeneratedGenerators
    {

        [NodeGenerator(typeof(Unity.VisualScripting.CreateStruct))]
        public sealed class CreateStructGenerator : NodeGenerator<Unity.VisualScripting.CreateStruct>
        {
            public CreateStructGenerator(Unity.VisualScripting.CreateStruct unit) : base(unit)
            {
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {

                var output = string.Empty;
                if (Unit.enter.hasValidConnection && !Unit.output.hasValidConnection) output += GenerateStructInstance(indent);
                output += (Unit.exit.hasValidConnection ? (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent) : string.Empty);
                return CodeBuilder.Indent(indent) + output;

            }

            public override string GenerateValue(ValueOutput output)
            {
                if (output == Unit.output)
                {
                    return $"new {Unit.type.As().CSharpName()}()";
                }

                return base.GenerateValue(output);
            }

            private string GenerateStructInstance(int indent)
            {
                return $"new {Unit.type.As().CSharpName()}();\n";
            }
        }


        [NodeGenerator(typeof(Unity.VisualScripting.Expose))]
        public sealed class ExposeGenerator : NodeGenerator<Unity.VisualScripting.Expose>
        {
            public ExposeGenerator(Unity.VisualScripting.Expose unit) : base(unit)
            {
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {
                return base.GenerateControl(input, data, indent);
            }

            public override string GenerateValue(ValueOutput output)
            {
                return base.GenerateValue(output);
            }

            public override string GenerateValue(ValueInput input)
            {
                return base.GenerateValue(input);
            }
        }

        [NodeGenerator(typeof(Unity.VisualScripting.GetMember))]
        public sealed class GetMemberGenerator : NodeGenerator<Unity.VisualScripting.GetMember>
        {
            public GetMemberGenerator(Unity.VisualScripting.GetMember unit) : base(unit)
            {
                NameSpace = Unit.member.declaringType.Namespace;
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {
                return base.GenerateControl(input, data, indent);
            }

            public override string GenerateValue(ValueOutput output)
            {
                var transformText = (CSharpPreview.AutomaticallyGetTransform && Unit.member.targetType == typeof(Transform)) ? ".transform" : string.Empty;

                if (Unit.target != null)
                {
                    if (Unit.target.hasValidConnection)
                    {
                        string type;

                        if (Unit.member.isField)
                        {
                            type = Unit.member.fieldInfo.Name;
                        }
                        else if (Unit.member.isProperty)
                        {
                            type = Unit.member.name;
                        }
                        else
                        {
                            type = Unit.member.ToPseudoDeclarer().ToString();
                        }
                        var outputCode = GenerateValue(Unit.target) + transformText + $".{type}";

                        return outputCode;
                    }
                    else
                    {
                        return $"{GenerateValue(Unit.target)}.{Unit.member.name}";
                    }
                }
                else
                {
                    return Unit.member.ToString();
                }
            }


            public override string GenerateValue(ValueInput input)
            {
                if (Unit.target != null)
                {
                    if (input == Unit.target)
                    {
                        if (Unit.target.hasValidConnection)
                        {
                            return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
                        }
                        else if (Unit.target.hasDefaultValue)
                        {
                            var defaultValue = Unit.defaultValues[input.key];

                            if (Unit.target.type == typeof(GameObject))
                            {
                                return "gameObject";
                            }
                            else
                            {
                                return defaultValue.As().Code(false, true, true);
                            }

                        }
                        else
                        {
                            return "/* Target Requires Input */";
                        }
                    }
                }

                return base.GenerateValue(input);
            }
        }


        [NodeGenerator(typeof(Unity.VisualScripting.InvokeMember))]
        public sealed class InvokeMemberGenerator : NodeGenerator<Unity.VisualScripting.InvokeMember>
        {
            private int codeindent;

            public InvokeMemberGenerator(Unity.VisualScripting.InvokeMember unit) : base(unit)
            {
                NameSpace = Unit.member.declaringType.Namespace;
            }

            public override string GenerateValue(ValueInput input)
            {
                if (input.hasValidConnection)
                {
                    return (input.connection.source.unit as Unit).GenerateValue(input.connection.source);
                }
                else if (input.hasDefaultValue)
                {
                    return Unit.defaultValues[input.key].As().Code(true, false, true);
                }
                else
                {
                    return $"/* \"{input.key} Requires Input\" */";
                }
            }

            public override string GenerateValue(ValueOutput output)
            {
                var transformText = (CSharpPreview.AutomaticallyGetTransform && Unit.member.targetType == typeof(Transform)) ? ".transform" : string.Empty;

                if (output == Unit.result)
                {
                    if (output.hasValidConnection)
                    {
                        if (Unit.enter.hasValidConnection)
                        {
                            if (Unit.member.isConstructor)
                            {
                                return $"new ".ConstructHighlight() + $"{Unit.member.declaringType}".TypeHighlight() + $"({GenerateArguments(Unit.inputParameters.Values.ToList())})";
                            }
                            else if (Unit.outputParameters.Count > 0)
                            {
                                string Memberparams = string.Empty;
                                var count = 0;

                                foreach (var param in Unit.member.GetParameterInfos())
                                {
                                    if (param.IsOut)
                                    {
                                        Memberparams += "out ".ConstructHighlight() + $"{param.ParameterType.As().CSharpName(false, false, true)} {param.Name}".Replace("&", "").Replace("%", "");
                                        Memberparams += ", ";
                                    }
                                    else
                                    {
                                        Memberparams += $"{GenerateValue(Unit.valueInputs[count])}";

                                        if (count < Unit.inputParameters.Count - 1)
                                        {
                                            Memberparams += ", ";
                                        }
                                        count++;
                                    }
                                }

                                string methodCall = (Unit.target != null ? Unit.target.hasValidConnection ? GenerateValue(Unit.target) + transformText + Unit.member.ToDeclarer().ToString().Remove(0, Unit.member.ToDeclarer().ToString().IndexOf('.')) : $"/* {Unit.member.name} Requires Target */" : Unit.member.ToPseudoDeclarer().ToSafeString());

                                return $"{methodCall}({Memberparams})";
                            }
                            else
                            {
                                string methodCall = (Unit.target != null ? Unit.target.hasValidConnection ? GenerateValue(Unit.target) + transformText + Unit.member.ToDeclarer().ToString().Remove(0, Unit.member.ToDeclarer().ToString().IndexOf('.')) : $"/* {Unit.member.name} Requires Target */" : Unit.member.ToPseudoDeclarer().ToSafeString());

                                return $"{methodCall}({(GenerateArguments(Unit.inputParameters.Values.ToList()).Length > 0 ? GenerateArguments(Unit.inputParameters.Values.ToList()) : string.Empty)})";
                            }

                        }
                        else
                        {
                            if (Unit.member.isConstructor)
                            {
                                return $"new ".ConstructHighlight() + $"{Unit.member.declaringType}".TypeHighlight() + $"({GenerateArguments(Unit.inputParameters.Values.ToList())})";
                            }
                            else
                            {
                                if (Unit.valueInputs.Count > 0)
                                {
                                    string outputParams = (Unit.outputParameters.Count > 0 ? "," + string.Join(", ", Unit.outputParameters.Select(p => $" out ".ConstructHighlight() + $"{p.Value.type} {p.Value.key.Replace("&", "").Replace("%", "")}")) : string.Empty);

                                    string Output = string.Empty;

                                    if (Unit.target != null)
                                    {
                                        if (Unit.target.hasValidConnection)
                                        {
                                            Output += (Unit.target.connection.source.unit as Unit).GenerateValue(Unit.target.connection.source) + transformText;
                                            Output += Unit.member.ToDeclarer().ToString().Remove(0, Unit.member.ToDeclarer().ToString().IndexOf(".")) + $"({GenerateArguments(Unit.inputParameters.Values.ToList())}" + $"{outputParams})";
                                            return Output;
                                        }
                                        else if (Unit.target.hasDefaultValue)
                                        {
                                            Output += CodeBuilder.Indent(codeindent) + Unit.defaultValues[Unit.target.key].As().Code(true, false, true, "") + transformText;
                                            return Output;
                                        }
                                        else
                                        {
                                            Output += $"/* {Unit.member.name} Requires Target */";
                                            return Output;
                                        }
                                    }
                                    return $"{Unit.member.ToPseudoDeclarer()}({GenerateArguments(Unit.inputParameters.Values.ToList())}" + $"{outputParams})";
                                }
                            }
                        }
                    }
                }
                else if (Unit.outputParameters.ContainsValue(output))
                {
                    return output.key.Replace("&", "").Replace("%", "").VariableHighlight();
                }
                return base.GenerateValue(output);
            }

            private string GenerateArguments(List<ValueInput> arguments)
            {
                var argumentValues = arguments.Select(arg =>
                {
                    if (arg.hasValidConnection)
                    {
                        return GenerateValue(arg);
                    }
                    else if (arg.hasDefaultValue)
                    {
                        return Unit.defaultValues[arg.key].As().Code(true, false, true, "");
                    }
                    else
                    {
                        return string.Empty;
                    }
                });

                var argumentString = string.Join(", ", argumentValues).Trim();

                if (argumentString.EndsWith(","))
                {
                    argumentString = argumentString.Substring(0, argumentString.Length - 1);
                }

                return argumentString;
            }


            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {
                codeindent = indent;

                var transformText = (CSharpPreview.AutomaticallyGetTransform && Unit.member.targetType == typeof(Transform)) ? ".transform" : string.Empty;

                if (input == Unit.enter)
                {
                    var output = string.Empty;

                    if (Unit.target != null)
                    {
                        if (Unit.target.hasValidConnection)
                        {
                            if (Unit.result != null)
                            {
                                if (!Unit.result.hasValidConnection)
                                {
                                    output += "\n";
                                    output += CodeBuilder.Indent(indent) + (Unit.target.hasValidConnection ? GenerateValue(Unit.target) + transformText + Unit.member.ToDeclarer().ToString().Remove(0, Unit.member.ToDeclarer().ToString().IndexOf('.')) : Unit.member.ToPseudoDeclarer()) + $"({(GenerateArguments(Unit.inputParameters.Values.ToList()).Length > 0 ? GenerateArguments(Unit.inputParameters.Values.ToList()) : string.Empty)}); \n";
                                }
                            }
                        }
                        else
                        {
                            output += $"/* {Unit.member.name} Requires Target */";
                        }
                    }
                    else
                    {
                        if (Unit.result != null)
                        {
                            if (!Unit.result.hasValidConnection)
                            {
                                if (Unit.member.isConstructor)
                                {
                                    output += CodeBuilder.Indent(indent) + $"new ".ConstructHighlight() + $"{Unit.member.declaringType}".TypeHighlight() + $"({GenerateArguments(Unit.inputParameters.Values.ToList())});\n";
                                }
                                else
                                {
                                    output += CodeBuilder.Indent(indent) + Unit.member.targetType + $"({(GenerateArguments(Unit.inputParameters.Values.ToList()).Length > 0 ? GenerateArguments(Unit.inputParameters.Values.ToList()) : string.Empty)}); \n";
                                }
                            }
                        }
                        else
                        {
                            output += "\n";
                            output += CodeBuilder.Indent(indent) + Unit.member.ToPseudoDeclarer() + $"({(GenerateArguments(Unit.inputParameters.Values.ToList()).Length > 0 ? GenerateArguments(Unit.inputParameters.Values.ToList()) : string.Empty)}); \n";
                        }
                    }

                    output += (Unit.exit.hasAnyConnection ? (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent) : string.Empty);

                    return output;
                }

                return base.GenerateControl(input, data, indent);
            }
        }

        [NodeGenerator(typeof(SetMember))]
        public sealed class SetMemberGenerator : NodeGenerator<SetMember>
        {
            public SetMemberGenerator(SetMember unit) : base(unit)
            {
                NameSpace = Unit.member.declaringType.Namespace;
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {
                if (input == Unit.assign)
                {
                    var output = string.Empty;
                    var targetValue = GenerateValue(Unit.target);
                    var memberName = Unit.member.name;
                    var inputValue = GenerateValue(Unit.input);

                    var transformText = (CSharpPreview.AutomaticallyGetTransform && Unit.member.targetType == typeof(Transform)) ? ".transform" : string.Empty;
                    output += "\n" + CodeBuilder.Indent(indent) + $"{targetValue}{transformText}.{memberName} = {inputValue};\n";

                    output += (Unit.assigned.hasValidConnection) ? (Unit.assigned.connection.destination.unit as Unit).GenerateControl(Unit.assigned.connection.destination, data, indent) : string.Empty;

                    return CodeBuilder.Indent(indent) + output;
                }

                return null;
            }

            public override string GenerateValue(ValueOutput output)
            {
                var transformText = (CSharpPreview.AutomaticallyGetTransform && Unit.member.targetType == typeof(Transform)) ? ".transform" : string.Empty;

                return GenerateValue(Unit.target) + transformText + $".{Unit.member.name}";
            }

            public override string GenerateValue(ValueInput input)
            {
                if (input.hasValidConnection)
                {
                    return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
                }
                else if (input.hasDefaultValue)
                {
                    return Unit.defaultValues[input.key].As().Code(true, true, true, "");
                }
                else
                {
                    return $"/* {input.key} requires input */";
                }
            }
        }

        [NodeGenerator(typeof(Unity.VisualScripting.CountItems))]
        public sealed class CountItemsGenerator : NodeGenerator<Unity.VisualScripting.CountItems>
        {
            private string list;

            public CountItemsGenerator(Unity.VisualScripting.CountItems unit) : base(unit)
            {
                NameSpace = "System.Collections.Generic";
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {
                return base.GenerateControl(input, data, indent);
            }

            public override string GenerateValue(ValueOutput output)
            {
                if (output == Unit.count)
                {
                    var list = GenerateValue(Unit.collection);
                    return list + ".Count";
                }

                return base.GenerateValue(output);
            }

            public override string GenerateValue(ValueInput input)
            {
                if (input == Unit.collection)
                {
                    if (input.hasValidConnection)
                    {
                        return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
                    }
                }

                return base.GenerateValue(input);
            }


        }

        [NodeGenerator(typeof(Unity.VisualScripting.AddDictionaryItem))]
        public sealed class AddDictionaryItemGenerator : NodeGenerator<Unity.VisualScripting.AddDictionaryItem>
        {
            public AddDictionaryItemGenerator(Unity.VisualScripting.AddDictionaryItem unit) : base(unit)
            {
                NameSpace = "System.Collections.Generic";
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {
                var output = string.Empty;

                output += CodeBuilder.Indent(indent) + $"{GenerateValue(Unit.dictionaryInput)}.Add({GenerateValue(Unit.key)}, {GenerateValue(Unit.value)});\n";

                output += (Unit.exit.hasValidConnection) ? "\n" + (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent) : string.Empty;
                return output;
            }

            public override string GenerateValue(ValueOutput output)
            {
                return GenerateValue(Unit.dictionaryInput);
            }

            public override string GenerateValue(ValueInput input)
            {
                if (input.hasValidConnection)
                {
                    return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
                }
                else if (input.hasDefaultValue)
                {
                    return Unit.defaultValues[input.key].As().Code(true, true, true, "");
                }
                else
                {
                    return $"/* {input.key} requires input */";
                }
            }


        }

        [NodeGenerator(typeof(Unity.VisualScripting.ClearDictionary))]
        public sealed class ClearDictionaryGenerator : NodeGenerator<Unity.VisualScripting.ClearDictionary>
        {
            public ClearDictionaryGenerator(Unity.VisualScripting.ClearDictionary unit) : base(unit)
            {
                NameSpace = "System.Collections.Generic";
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {
                var output = string.Empty;

                output += CodeBuilder.Indent(indent) + GenerateValue(Unit.dictionaryInput) + ".Clear();\n";

                output += (Unit.exit.hasValidConnection) ? "\n" + (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent) : string.Empty;

                return output;
            }

            public override string GenerateValue(ValueOutput output)
            {
                return GenerateValue(Unit.dictionaryInput);
            }

            public override string GenerateValue(ValueInput input)
            {
                if (input.hasValidConnection)
                {
                    return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
                }
                else if (input.hasDefaultValue)
                {
                    return Unit.defaultValues[input.key].As().Code(true, true, true, "");
                }
                else
                {
                    return $"/* {input.key} requires input */";
                }
            }


        }

        [NodeGenerator(typeof(Unity.VisualScripting.CreateDictionary))]
        public sealed class CreateDictionaryGenerator : NodeGenerator<Unity.VisualScripting.CreateDictionary>
        {
            public CreateDictionaryGenerator(Unity.VisualScripting.CreateDictionary unit) : base(unit)
            {
                NameSpace = "System.Collections.Generic";
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {
                return base.GenerateControl(input, data, indent);
            }

            public override string GenerateValue(ValueOutput output)
            {
                return "new ".ConstructHighlight() + "Dictionary<object, object>".TypeHighlight() + "()";
            }

            public override string GenerateValue(ValueInput input)
            {
                return base.GenerateValue(input);
            }


        }

        [NodeGenerator(typeof(Unity.VisualScripting.DictionaryContainsKey))]
        public sealed class DictionaryContainsKeyGenerator : NodeGenerator<Unity.VisualScripting.DictionaryContainsKey>
        {
            public DictionaryContainsKeyGenerator(Unity.VisualScripting.DictionaryContainsKey unit) : base(unit)
            {
                NameSpace = "System.Collections.Generic";
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {
                return base.GenerateControl(input, data, indent);
            }

            public override string GenerateValue(ValueOutput output)
            {
                return $"{GenerateValue(Unit.dictionary)}.ContainsKey({GenerateValue(Unit.key)})";
            }

            public override string GenerateValue(ValueInput input)
            {
                if (input.hasValidConnection)
                {
                    return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
                }
                else if (input.hasDefaultValue)
                {
                    return Unit.defaultValues[input.key].As().Code(true, true, true, "");
                }
                else
                {
                    return $"/* {input.key} requires input */";
                }
            }


        }

        [NodeGenerator(typeof(Unity.VisualScripting.GetDictionaryItem))]
        public sealed class GetDictionaryItemGenerator : NodeGenerator<Unity.VisualScripting.GetDictionaryItem>
        {
            public GetDictionaryItemGenerator(Unity.VisualScripting.GetDictionaryItem unit) : base(unit)
            {
                NameSpace = "System.Collections.Generic";
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {
                return base.GenerateControl(input, data, indent);
            }

            public override string GenerateValue(ValueOutput output)
            {
                return $"{GenerateValue(Unit.dictionary)}[{GenerateValue(Unit.key)}]";
            }

            public override string GenerateValue(ValueInput input)
            {
                if (input.hasValidConnection)
                {
                    return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
                }
                else if (input.hasDefaultValue)
                {
                    return Unit.defaultValues[input.key].As().Code(true, true, true, "");
                }
                else
                {
                    return $"/* {input.key} requires input */";
                }
            }
        }

        [NodeGenerator(typeof(Unity.VisualScripting.MergeDictionaries))]
        public sealed class MergeDictionariesGenerator : NodeGenerator<Unity.VisualScripting.MergeDictionaries>
        {
            public MergeDictionariesGenerator(Unity.VisualScripting.MergeDictionaries unit) : base(unit)
            {
                NameSpace = "System.Collections.Generic";
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {
                return base.GenerateControl(input, data, indent);
            }

            public override string GenerateValue(ValueOutput output)
            {
                var builder = new StringBuilder();

                var parameters = string.Empty;

                foreach (var Input in Unit.multiInputs)
                {
                    if (Input.hasValidConnection)
                    {
                        parameters += GenerateValue(Input);

                        if (!Input.Equals(Unit.multiInputs.Last(input => input.hasValidConnection)))
                        {
                            parameters += ", ";
                        }
                    }
                }

                builder.AppendLine($"CodeGenUtils.MergeDictionaries({parameters})");

                return builder.ToString();
            }

            public override string GenerateValue(ValueInput input)
            {
                if (input.hasValidConnection)
                {
                    return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
                }
                else if (input.hasDefaultValue)
                {
                    return Unit.defaultValues[input.key].As().Code(true, true, true, "");
                }
                else
                {
                    return $"/* {input.key} requires input */";
                }
            }
        }

        [NodeGenerator(typeof(Unity.VisualScripting.RemoveDictionaryItem))]
        public sealed class RemoveDictionaryItemGenerator : NodeGenerator<Unity.VisualScripting.RemoveDictionaryItem>
        {
            public RemoveDictionaryItemGenerator(Unity.VisualScripting.RemoveDictionaryItem unit) : base(unit)
            {
                NameSpace = "System.Collections.Generic";
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {
                var output = string.Empty;

                output += CodeBuilder.Indent(indent) + $"{GenerateValue(Unit.dictionaryInput)}.Remove({GenerateValue(Unit.key)});\n";

                output += (Unit.exit.hasValidConnection) ? "\n" + (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent) : string.Empty;
                return output;
            }

            public override string GenerateValue(ValueOutput output)
            {
                return GenerateValue(Unit.dictionaryInput);
            }

            public override string GenerateValue(ValueInput input)
            {
                if (input.hasValidConnection)
                {
                    return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
                }
                else if (input.hasDefaultValue)
                {
                    return Unit.defaultValues[input.key].As().Code(true, true, true, "");
                }
                else
                {
                    return $"/* {input.key} requires input */";
                }
            }
        }

        [NodeGenerator(typeof(Unity.VisualScripting.SetDictionaryItem))]
        public sealed class SetDictionaryItemGenerator : NodeGenerator<Unity.VisualScripting.SetDictionaryItem>
        {
            public SetDictionaryItemGenerator(Unity.VisualScripting.SetDictionaryItem unit) : base(unit)
            {
                NameSpace = "System.Collections.Generic";
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {
                var output = string.Empty;

                output += CodeBuilder.Indent(indent) + $"{GenerateValue(Unit.dictionary)}[{GenerateValue(Unit.key)}] = {GenerateValue(Unit.value)};\n";

                output += (Unit.exit.hasValidConnection) ? "\n" + (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent) : string.Empty;
                return output;
            }

            public override string GenerateValue(ValueInput input)
            {
                if (input.hasValidConnection)
                {
                    return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
                }
                else if (input.hasDefaultValue)
                {
                    return Unit.defaultValues[input.key].As().Code(true, true, true, "");
                }
                else
                {
                    return $"/* {input.key} requires input */";
                }
            }
        }

        [NodeGenerator(typeof(Unity.VisualScripting.FirstItem))]
        public sealed class FirstItemGenerator : NodeGenerator<Unity.VisualScripting.FirstItem>
        {
            public FirstItemGenerator(Unity.VisualScripting.FirstItem unit) : base(unit)
            {
                NameSpace = "System.Collections.Generic";
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {
                return base.GenerateControl(input, data, indent);
            }

            public override string GenerateValue(ValueOutput output)
            {
                return GenerateValue(Unit.collection) + "[0]";
            }

            public override string GenerateValue(ValueInput input)
            {
                if (input.hasValidConnection)
                {
                    return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
                }
                else if (input.hasDefaultValue)
                {
                    return Unit.defaultValues[input.key].As().Code(true, true, true, "");
                }
                else
                {
                    return $"/* {input.key} requires input */";
                }
            }
        }

        [NodeGenerator(typeof(Unity.VisualScripting.LastItem))]
        public sealed class LastItemGenerator : NodeGenerator<Unity.VisualScripting.LastItem>
        {
            public LastItemGenerator(Unity.VisualScripting.LastItem unit) : base(unit)
            {
                NameSpace = "System.Collections.Generic";
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {
                return base.GenerateControl(input, data, indent);
            }

            public override string GenerateValue(ValueOutput output)
            {
                return GenerateValue(Unit.collection) + $"[{GenerateValue(Unit.collection)}.Count - 1]";
            }

            public override string GenerateValue(ValueInput input)
            {
                if (input.hasValidConnection)
                {
                    return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
                }
                else if (input.hasDefaultValue)
                {
                    return Unit.defaultValues[input.key].As().Code(true, true, true, "");
                }
                else
                {
                    return $"/* {input.key} requires input */";
                }
            }


        }

        [NodeGenerator(typeof(Unity.VisualScripting.AddListItem))]
        public sealed class AddListItemGenerator : NodeGenerator<Unity.VisualScripting.AddListItem>
        {
            public AddListItemGenerator(Unity.VisualScripting.AddListItem unit) : base(unit)
            {
                NameSpace = "System.Collections.Generic";
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {
                var output = string.Empty;

                if (input == Unit.enter)
                {
                    var list = GenerateValue(Unit.listInput);

                    output += CodeBuilder.Indent(indent) + $"{list}.Add({GenerateValue(Unit.item)});\n";
                }

                output += (Unit.exit.hasAnyConnection ? (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent) : string.Empty);
                return output;
            }

            public override string GenerateValue(ValueOutput output)
            {
                return base.GenerateValue(output);
            }

            public override string GenerateValue(ValueInput input)
            {
                if (input == Unit.listInput)
                {
                    if (Unit.listInput.hasValidConnection)
                    {
                        return (Unit.listInput.connection.source.unit as Unit).GenerateValue(Unit.listInput.connection.source);
                    }
                }
                else if (input == Unit.item)
                {
                    if (Unit.item.hasValidConnection)
                    {
                        return (Unit.item.connection.source.unit as Unit).GenerateValue(Unit.item.connection.source);
                    }
                }

                return base.GenerateValue(input);
            }


        }

        [NodeGenerator(typeof(Unity.VisualScripting.ClearList))]
        public sealed class ClearListGenerator : NodeGenerator<Unity.VisualScripting.ClearList>
        {
            public ClearListGenerator(Unity.VisualScripting.ClearList unit) : base(unit)
            {
                NameSpace = "System.Collections.Generic";
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {
                var output = string.Empty;

                if (input == Unit.enter)
                {
                    if (Unit.listInput.hasValidConnection)
                    {
                        var list = GenerateValue(Unit.listInput);

                        output += $"{list}.Clear();\n";
                    }

                    output += (Unit.exit.hasAnyConnection ? (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent) : string.Empty);
                }

                return output;
            }

            public override string GenerateValue(ValueOutput output)
            {
                if (output == Unit.listOutput)
                {
                    if (Unit.listOutput.hasValidConnection)
                    {
                        var sourceInput = GenerateValue(Unit.listInput);

                        return $"{sourceInput}";
                    }
                }

                return base.GenerateValue(output);
            }

            public override string GenerateValue(ValueInput input)
            {
                if (input == Unit.listInput)
                {
                    if (Unit.listInput.hasValidConnection)
                    {
                        return (Unit.listInput.connection.source.unit as Unit).GenerateValue(Unit.listInput.connection.source);
                    }
                }

                return base.GenerateValue(input);
            }
        }

        [NodeGenerator(typeof(Unity.VisualScripting.CreateList))]
        public sealed class CreateListGenerator : NodeGenerator<Unity.VisualScripting.CreateList>
        {
            public CreateListGenerator(Unity.VisualScripting.CreateList unit) : base(unit)
            {
                NameSpace = "System.Collections.Generic";
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {
                return base.GenerateControl(input, data, indent);
            }

            public override string GenerateValue(ValueOutput output)
            {

                if (output == Unit.list)
                {
                    if (output.hasValidConnection)
                    {
                        var connectedInputs = Unit.inputs.Select(input => GenerateValue((ValueInput)input));
                        var firstInput = Unit.multiInputs.FirstOrDefault();
                        if (firstInput != null && firstInput.hasValidConnection)
                        {
                            string elementType = string.Empty;

                            var secondInput = Unit.multiInputs.ElementAtOrDefault(1);
                            if (secondInput != null)
                            {
                                elementType = secondInput != null && secondInput.hasValidConnection && secondInput.connection.source.type == firstInput.connection.source.type
                                    ? firstInput.connection.source.type.DisplayName()
                                    : "object";
                            }
                            else
                            {
                                elementType = firstInput.connection.source.type.DisplayName();
                            }
                            var script = $"new List<{elementType}> {{ {string.Join(", ", connectedInputs)} }}";
                            return script;
                        }
                    }
                }

                return base.GenerateValue(output);
            }

            public override string GenerateValue(ValueInput input)
            {
                if (Unit.multiInputs.Contains(input))
                {
                    if (input.hasValidConnection)
                        return (Unit.multiInputs[Unit.multiInputs.IndexOf(input)].connection.source.unit as Unit).GenerateValue(Unit.multiInputs[Unit.multiInputs.IndexOf(input)].connection.source);
                }
                return base.GenerateValue(input);
            }
        }
    }


    [NodeGenerator(typeof(Unity.VisualScripting.GetListItem))]
    public sealed class GetListItemGenerator : NodeGenerator<Unity.VisualScripting.GetListItem>
    {
        public GetListItemGenerator(Unity.VisualScripting.GetListItem unit) : base(unit)
        {
            NameSpace = "System.Collections.Generic";
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            if (output == Unit.item)
            {
                if (output.hasValidConnection)
                {
                    var sourceInput = GenerateValue(Unit.list);

                    var index = GenerateValue(Unit.index);

                    return $"{sourceInput}[{index.Replace("f", "")}]";
                }
            }

            return base.GenerateValue(output);
        }


        public override string GenerateValue(ValueInput input)
        {
            if (input == Unit.list || input == Unit.index)
            {
                if (input == Unit.list)
                {
                    if (input.hasValidConnection)
                    {
                        var sourceNode = Unit.list.connection.source.unit as Unit;
                        var sourceValue = sourceNode.GenerateValue(Unit.list.connection.source);
                        return sourceValue;
                    }
                    else
                    {
                        return "/*Get list item Requires list input*/";
                    }
                }
                else
                {
                    if (Unit.index.hasValidConnection)
                    {
                        var sourceNode = Unit.index.connection.source.unit as Unit;
                        var sourceValue = sourceNode.GenerateValue(Unit.index.connection.source);
                        return sourceValue;
                    }
                    else
                    {
                        return Unit.defaultValues[input.key].ToString();
                    }
                }
            }

            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(InsertListItem))]
    public sealed class InsertListItemGenerator : NodeGenerator<InsertListItem>
    {
        public InsertListItemGenerator(InsertListItem unit) : base(unit)
        {
            NameSpace = "System.Collections.Generic";
        }
        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;
            if (input == Unit.enter)
            {
                output += CodeBuilder.Indent(indent) + GenerateValue(Unit.listInput).ConstructHighlight();
                output += $".Insert({GenerateValue(Unit.index)}, {GenerateValue(Unit.item)});";
                output += "\n";
                output += "\n";
                output += (Unit.exit.hasAnyConnection ? (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent) : string.Empty);
                output += "\n";
                return output;
            }

            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input == Unit.listInput || input == Unit.index || input == Unit.item)
            {
                if (Unit.listInput.hasAnyConnection && input == Unit.listInput)
                {
                    return (Unit.listInput.connection.source.unit as Unit).GenerateValue(Unit.listInput.connection.source);
                }
                else if (input == Unit.index)
                {
                    if (Unit.index.hasValidConnection)
                    {
                        return (Unit.index.connection.source.unit as Unit).GenerateValue(Unit.index.connection.source).Replace("f", "");
                    }
                    else
                    {
                        return Unit.defaultValues[input.key].ToString();
                    }
                }
                else
                {
                    return (Unit.item.connection.source.unit as Unit).GenerateValue(Unit.item.connection.source);
                }
            }
            return base.GenerateValue(input);
        }

    }

    [NodeGenerator(typeof(Unity.VisualScripting.ListContainsItem))]
    public sealed class ListContainsItemGenerator : NodeGenerator<Unity.VisualScripting.ListContainsItem>
    {
        public ListContainsItemGenerator(Unity.VisualScripting.ListContainsItem unit) : base(unit)
        {
            NameSpace = "System.Collections.Generic";
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input == Unit.list || input == Unit.item)
            {
                if (input.hasValidConnection && input == Unit.list)
                {
                    var sourceNode = Unit.list.connection.source.unit as Unit;
                    var sourceValue = sourceNode.GenerateValue(Unit.list.connection.source);
                    return sourceValue;
                }
                else if (input == Unit.item && Unit.item.hasValidConnection)
                {

                    var sourceNode = Unit.item.connection.source.unit as Unit;
                    var sourceValue = sourceNode.GenerateValue(Unit.item.connection.source);
                    return sourceValue;

                }
            }

            return base.GenerateValue(input);
        }

        public override string GenerateValue(ValueOutput output)
        {
            if (output == Unit.contains)
            {
                if (output.hasValidConnection)
                {
                    var sourceInput = GenerateValue(Unit.list);

                    var item = GenerateValue(Unit.item);

                    return $"{sourceInput}.Contains({item})";
                }
            }

            return base.GenerateValue(output);
        }
    }


    [NodeGenerator(typeof(Unity.VisualScripting.MergeLists))]
    public sealed class MergeListsGenerator : NodeGenerator<Unity.VisualScripting.MergeLists>
    {
        public MergeListsGenerator(Unity.VisualScripting.MergeLists unit) : base(unit)
        {
            NameSpace = "System.Collections";
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            var builder = new StringBuilder();

            var parameters = string.Empty;

            foreach (var Input in Unit.multiInputs)
            {
                if (Input.hasValidConnection)
                {
                    parameters += GenerateValue(Input);

                    if (!Input.Equals(Unit.multiInputs.Last(input => input.hasValidConnection)))
                    {
                        parameters += ", ";
                    }
                }
            }

            builder.AppendLine($"CodeGenUtils.MergeLists({parameters})");

            return builder.ToString();
        }


        public override string GenerateValue(ValueInput input)
        {
            return (input.connection.source.unit as Unit).GenerateValue(input.connection.source);
        }
    }


    [NodeGenerator(typeof(Unity.VisualScripting.RemoveListItem))]
    public sealed class RemoveListItemGenerator : NodeGenerator<Unity.VisualScripting.RemoveListItem>
    {
        public RemoveListItemGenerator(Unity.VisualScripting.RemoveListItem unit) : base(unit)
        {
            NameSpace = "System.Collections.Generic";
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            output += GenerateValue(Unit.listInput) + $".Remove({GenerateValue(Unit.item)})\n";
            output += Unit.exit.hasValidConnection ? (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent) : string.Empty;
            return CodeBuilder.Indent(indent) + output;
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return (input.connection.source.unit as Unit).GenerateValue(input.connection.source);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.RemoveListItemAt))]
    public sealed class RemoveListItemAtGenerator : NodeGenerator<Unity.VisualScripting.RemoveListItemAt>
    {
        public RemoveListItemAtGenerator(Unity.VisualScripting.RemoveListItemAt unit) : base(unit)
        {
            NameSpace = "System.Collections.Generic";
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.SetListItem))]
    public sealed class SetListItemGenerator : NodeGenerator<Unity.VisualScripting.SetListItem>
    {
        public SetListItemGenerator(Unity.VisualScripting.SetListItem unit) : base(unit)
        {
            NameSpace = "System.Collections.Generic";
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Cache))]
    public sealed class CacheGenerator : NodeGenerator<Unity.VisualScripting.Cache>
    {
        private string randomString;

        public CacheGenerator(Unity.VisualScripting.Cache unit) : base(unit)
        {
            randomString = GenerateRandomString();
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            if (input == Unit.enter)
            {
                var cachedValue = GenerateValue(Unit.input);

                output += CodeBuilder.Indent(indent) + $"var cachedValue_{randomString} = {cachedValue};";
                output += "\n";

                if (Unit.exit.hasAnyConnection)
                {
                    output += "\n";
                    output += (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent);
                    output += "\n";
                }
            }

            return output;
        }

        public override string GenerateValue(ValueOutput output)
        {
            if (output == Unit.output)
            {
                foreach (var connection in output.connections)
                {
                    if (connection.destination is ValueInput connectedInput)
                    {
                        return $"cachedValue_{randomString}";
                    }
                }
            }

            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input == Unit.input)
            {
                if (input.hasValidConnection)
                {
                    return (input.connection.source.unit as Unit).GenerateValue(input.connection.source);
                }
            }

            return base.GenerateValue(input);
        }

        private string GenerateRandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new System.Random();
            var randomString = new StringBuilder();

            for (int i = 0; i < 5; i++) // You can adjust the length of the random string here
            {
                randomString.Append(chars[random.Next(chars.Length)]);
            }

            return randomString.ToString();
        }
    }


    [NodeGenerator(typeof(Unity.VisualScripting.For))]
    public sealed class ForGenerator : NodeGenerator<Unity.VisualScripting.For>
    {
        public ForGenerator(Unity.VisualScripting.For unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            if (input == Unit.enter)
            {
                var initialization = GenerateValue(Unit.firstIndex);
                var condition = GenerateValue(Unit.lastIndex);
                var iterator = GenerateValue(Unit.step);

                output += $"for".ControlHighlight() + "(int".ConstructHighlight() + " i ".VariableHighlight() + $"= {initialization}; " + "i".VariableHighlight() + $" < {condition}; " + "i".VariableHighlight() + $" += {iterator})";
                output += "\n";
                output += CodeBuilder.OpenBody(indent);
                output += "\n";

                if (Unit.body.hasAnyConnection)
                {
                    output += (Unit.body.connection.destination.unit as Unit).GenerateControl(Unit.body.connection.destination, data, indent + 1);
                }

                output += CodeBuilder.CloseBody(indent);
            }

            if (Unit.exit.hasAnyConnection)
            {
                output += "\n";
                output += (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent);
            }


            return output;
        }

        public override string GenerateValue(ValueOutput output)
        {
            return "i".VariableHighlight();
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input == Unit.firstIndex || input == Unit.lastIndex || input == Unit.step)
            {
                if (input == Unit.firstIndex)
                {
                    if (input.hasValidConnection)
                    {
                        return (input.connection.source.unit as Unit).GenerateValue(input.connection.source);
                    }
                    else
                    {
                        return Unit.defaultValues[input.key].ToString();
                    }
                }
                else if (input == Unit.lastIndex)
                {
                    if (input.hasValidConnection)
                    {
                        return (input.connection.source.unit as Unit).GenerateValue(input.connection.source);
                    }
                    else
                    {
                        return Unit.defaultValues[input.key].ToString();
                    }
                }
                else if (input == Unit.step)
                {
                    if (input.hasValidConnection)
                    {
                        return (input.connection.source.unit as Unit).GenerateValue(input.connection.source);
                    }
                    else
                    {
                        return Unit.defaultValues[input.key].ToString();
                    }
                }
            }

            return base.GenerateValue(input);
        }
    }



    [NodeGenerator(typeof(Unity.VisualScripting.ForEach))]
    public sealed class ForEachGenerator : NodeGenerator<Unity.VisualScripting.ForEach>
    {
        public ForEachGenerator(Unity.VisualScripting.ForEach unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            if (input == Unit.enter)
            {
                var collection = GenerateValue(Unit.collection);

                output += "\n" + CodeBuilder.Indent(indent) + $"foreach".ControlHighlight() + " (" + "var".ConstructHighlight() + " item".VariableHighlight() + " in ".ConstructHighlight() + $"{collection})";
                output += "\n";
                output += CodeBuilder.OpenBody(indent);
                output += "\n";

                if (Unit.body.hasAnyConnection)
                {
                    output += (Unit.body.connection.destination.unit as Unit).GenerateControl(Unit.body.connection.destination, data, indent + 1);
                    output += "\n";
                }

                output += CodeBuilder.CloseBody(indent);
            }

            if (Unit.exit.hasAnyConnection)
            {
                output += "\n";
                output += (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent);
                output += "\n";
            }

            return output;
        }

        public override string GenerateValue(ValueOutput output)
        {
            return "item".VariableHighlight();
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input == Unit.collection)
            {
                if (input.hasValidConnection)
                {
                    return (input.connection.source.unit as Unit).GenerateValue(input.connection.source);
                }
            }

            return base.GenerateValue(input);
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.Once))]
    public sealed class OnceGenerator : NodeGenerator<Unity.VisualScripting.Once>
    {

        public OnceGenerator(Unity.VisualScripting.Once unit) : base(unit)
        {
            // Generate a unique identifier for the Once node
            UniqueID = GenerateRandomString();
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            if (input == Unit.enter)
            {
                output += CodeBuilder.Indent(indent) + $"if".ConstructHighlight() + $"(!Once_{UniqueID})";
                output += "\n";
                output += CodeBuilder.OpenBody(indent);
                output += "\n";

                if (Unit.once.hasAnyConnection)
                {
                    output += (Unit.once.connection.destination.unit as Unit).GenerateControl(Unit.once.connection.destination, data, indent + 1);
                    output += "\n";
                }

                output += CodeBuilder.Indent(indent + 1) + $"Once_{UniqueID} = " + "true".ConstructHighlight() + ";";
                output += "\n";
                output += CodeBuilder.CloseBody(indent);
                output += "\n";

                if (Unit.after.hasValidConnection)
                {
                    output += (Unit.after.connection.destination.unit as Unit).GenerateControl(Unit.after.connection.destination, data, indent);
                }
            }

            else if (input == Unit.reset)
            {
                output += CodeBuilder.Indent(indent) + $"Once_{UniqueID} = " + "false".ConstructHighlight() + ";";
            }

            return output;
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }

        private string GenerateRandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new System.Random();
            var randomString = new StringBuilder();

            for (int i = 0; i < 5; i++)
            {
                randomString.Append(chars[random.Next(chars.Length)]);
            }

            return randomString.ToString();
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.SelectOnFlow))]
    public sealed class SelectOnFlowGenerator : NodeGenerator<Unity.VisualScripting.SelectOnFlow>
    {
        public SelectOnFlowGenerator(Unity.VisualScripting.SelectOnFlow unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Sequence))]
    public sealed class SequenceGenerator : NodeGenerator<Unity.VisualScripting.Sequence>
    {
        public SequenceGenerator(Unity.VisualScripting.Sequence unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            string output = string.Empty;
            foreach (ControlOutput _output in Unit.multiOutputs)
            {
                output += _output.hasValidConnection ? (_output.connection.destination.unit as Unit).GenerateControl(_output.connection.destination, data, indent) + "\n" : string.Empty;
            }
            return output;
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Throw))]
    public sealed class ThrowGenerator : NodeGenerator<Unity.VisualScripting.Throw>
    {
        public ThrowGenerator(Unity.VisualScripting.Throw unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            if (!Unit.custom)
            {
                return CodeBuilder.Indent(indent) + $"throw ".ControlHighlight() + $" new ".ConstructHighlight() + "Exception".TypeHighlight() + $"({GenerateValue(Unit.message)})";
            }
            else
            {
                return CodeBuilder.Indent(indent) + $"throw ".ControlHighlight() + $"{GenerateValue(Unit.exception)}";
            }
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.ToggleFlow))]
    public sealed class ToggleFlowGenerator : NodeGenerator<Unity.VisualScripting.ToggleFlow>
    {
        public ToggleFlowGenerator(Unity.VisualScripting.ToggleFlow unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.ToggleValue))]
    public sealed class ToggleValueGenerator : NodeGenerator<Unity.VisualScripting.ToggleValue>
    {
        public ToggleValueGenerator(Unity.VisualScripting.ToggleValue unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.TryCatch))]
    public sealed class TryCatchGenerator : NodeGenerator<Unity.VisualScripting.TryCatch>
    {
        public TryCatchGenerator(Unity.VisualScripting.TryCatch unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var code = string.Empty;
            code += CodeBuilder.Indent(indent) + "try".ConstructHighlight() + "\n";
            code += CodeBuilder.OpenBody(indent) + "\n";
            code += (Unit.@try.hasValidConnection) ? (Unit.@try.connection.destination.unit as Unit).GenerateControl(Unit.@try.connection.destination, data, indent + 1) : string.Empty;
            code += "\n" + CodeBuilder.CloseBody(indent) + "\n";
            code += CodeBuilder.Indent(indent) + "catch".ConstructHighlight() + $" ({Unit.exceptionType}) \n";
            code += CodeBuilder.OpenBody(indent) + "\n";
            code += (Unit.@catch.hasValidConnection) ? (Unit.@catch.connection.destination.unit as Unit).GenerateControl(Unit.@catch.connection.destination, data, indent + 1) : string.Empty;
            code += "\n" + CodeBuilder.CloseBody(indent) + "\n";
            code += CodeBuilder.Indent(indent) + "finally".ConstructHighlight() + "\n";
            code += CodeBuilder.OpenBody(indent) + "\n";
            code += (Unit.@finally.hasValidConnection) ? (Unit.@finally.connection.destination.unit as Unit).GenerateControl(Unit.@finally.connection.destination, data, indent + 1) : string.Empty;
            code += "\n" + CodeBuilder.CloseBody(indent) + "\n";

            return code;
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.While))]
    public sealed class WhileGenerator : NodeGenerator<Unity.VisualScripting.While>
    {
        public WhileGenerator(Unity.VisualScripting.While unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.BoltAnimationEvent))]
    public sealed class BoltAnimationEventGenerator : NodeGenerator<Unity.VisualScripting.BoltAnimationEvent>
    {
        public BoltAnimationEventGenerator(Unity.VisualScripting.BoltAnimationEvent unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.BoltNamedAnimationEvent))]
    public sealed class BoltNamedAnimationEventGenerator : NodeGenerator<Unity.VisualScripting.BoltNamedAnimationEvent>
    {
        public BoltNamedAnimationEventGenerator(Unity.VisualScripting.BoltNamedAnimationEvent unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnAnimatorIK))]
    public sealed class OnAnimatorIKGenerator : NodeGenerator<Unity.VisualScripting.OnAnimatorIK>
    {
        public OnAnimatorIKGenerator(Unity.VisualScripting.OnAnimatorIK unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnAnimatorMove))]
    public sealed class OnAnimatorMoveGenerator : NodeGenerator<Unity.VisualScripting.OnAnimatorMove>
    {
        public OnAnimatorMoveGenerator(Unity.VisualScripting.OnAnimatorMove unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnApplicationFocus))]
    public sealed class OnApplicationFocusGenerator : NodeGenerator<Unity.VisualScripting.OnApplicationFocus>
    {
        public OnApplicationFocusGenerator(Unity.VisualScripting.OnApplicationFocus unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnApplicationLostFocus))]
    public sealed class OnApplicationLostFocusGenerator : NodeGenerator<Unity.VisualScripting.OnApplicationLostFocus>
    {
        public OnApplicationLostFocusGenerator(Unity.VisualScripting.OnApplicationLostFocus unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnApplicationPause))]
    public sealed class OnApplicationPauseGenerator : NodeGenerator<Unity.VisualScripting.OnApplicationPause>
    {
        public OnApplicationPauseGenerator(Unity.VisualScripting.OnApplicationPause unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnApplicationQuit))]
    public sealed class OnApplicationQuitGenerator : NodeGenerator<Unity.VisualScripting.OnApplicationQuit>
    {
        public OnApplicationQuitGenerator(Unity.VisualScripting.OnApplicationQuit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnApplicationResume))]
    public sealed class OnApplicationResumeGenerator : NodeGenerator<Unity.VisualScripting.OnApplicationResume>
    {
        public OnApplicationResumeGenerator(Unity.VisualScripting.OnApplicationResume unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.BoltUnityEvent))]
    public sealed class BoltUnityEventGenerator : NodeGenerator<Unity.VisualScripting.BoltUnityEvent>
    {
        public BoltUnityEventGenerator(Unity.VisualScripting.BoltUnityEvent unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.CustomEvent))]
    public sealed class CustomEventGenerator : NodeGenerator<Unity.VisualScripting.CustomEvent>
    {
        public CustomEventGenerator(Unity.VisualScripting.CustomEvent unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnDrawGizmos))]
    public sealed class OnDrawGizmosGenerator : NodeGenerator<Unity.VisualScripting.OnDrawGizmos>
    {
        public OnDrawGizmosGenerator(Unity.VisualScripting.OnDrawGizmos unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnDrawGizmosSelected))]
    public sealed class OnDrawGizmosSelectedGenerator : NodeGenerator<Unity.VisualScripting.OnDrawGizmosSelected>
    {
        public OnDrawGizmosSelectedGenerator(Unity.VisualScripting.OnDrawGizmosSelected unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnBeginDrag))]
    public sealed class OnBeginDragGenerator : NodeGenerator<Unity.VisualScripting.OnBeginDrag>
    {
        public OnBeginDragGenerator(Unity.VisualScripting.OnBeginDrag unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnButtonClick))]
    public sealed class OnButtonClickGenerator : NodeGenerator<Unity.VisualScripting.OnButtonClick>
    {
        public OnButtonClickGenerator(Unity.VisualScripting.OnButtonClick unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnCancel))]
    public sealed class OnCancelGenerator : NodeGenerator<Unity.VisualScripting.OnCancel>
    {
        public OnCancelGenerator(Unity.VisualScripting.OnCancel unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnDeselect))]
    public sealed class OnDeselectGenerator : NodeGenerator<Unity.VisualScripting.OnDeselect>
    {
        public OnDeselectGenerator(Unity.VisualScripting.OnDeselect unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnDrag))]
    public sealed class OnDragGenerator : NodeGenerator<Unity.VisualScripting.OnDrag>
    {
        public OnDragGenerator(Unity.VisualScripting.OnDrag unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnDrop))]
    public sealed class OnDropGenerator : NodeGenerator<Unity.VisualScripting.OnDrop>
    {
        public OnDropGenerator(Unity.VisualScripting.OnDrop unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnDropdownValueChanged))]
    public sealed class OnDropdownValueChangedGenerator : NodeGenerator<Unity.VisualScripting.OnDropdownValueChanged>
    {
        public OnDropdownValueChangedGenerator(Unity.VisualScripting.OnDropdownValueChanged unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnEndDrag))]
    public sealed class OnEndDragGenerator : NodeGenerator<Unity.VisualScripting.OnEndDrag>
    {
        public OnEndDragGenerator(Unity.VisualScripting.OnEndDrag unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnGUI))]
    public sealed class OnGUIGenerator : NodeGenerator<Unity.VisualScripting.OnGUI>
    {
        public OnGUIGenerator(Unity.VisualScripting.OnGUI unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnInputFieldEndEdit))]
    public sealed class OnInputFieldEndEditGenerator : NodeGenerator<Unity.VisualScripting.OnInputFieldEndEdit>
    {
        public OnInputFieldEndEditGenerator(Unity.VisualScripting.OnInputFieldEndEdit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnInputFieldValueChanged))]
    public sealed class OnInputFieldValueChangedGenerator : NodeGenerator<Unity.VisualScripting.OnInputFieldValueChanged>
    {
        public OnInputFieldValueChangedGenerator(Unity.VisualScripting.OnInputFieldValueChanged unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnMove))]
    public sealed class OnMoveGenerator : NodeGenerator<Unity.VisualScripting.OnMove>
    {
        public OnMoveGenerator(Unity.VisualScripting.OnMove unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnPointerClick))]
    public sealed class OnPointerClickGenerator : NodeGenerator<Unity.VisualScripting.OnPointerClick>
    {
        public OnPointerClickGenerator(Unity.VisualScripting.OnPointerClick unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnPointerDown))]
    public sealed class OnPointerDownGenerator : NodeGenerator<Unity.VisualScripting.OnPointerDown>
    {
        public OnPointerDownGenerator(Unity.VisualScripting.OnPointerDown unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnPointerEnter))]
    public sealed class OnPointerEnterGenerator : NodeGenerator<Unity.VisualScripting.OnPointerEnter>
    {
        public OnPointerEnterGenerator(Unity.VisualScripting.OnPointerEnter unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnPointerExit))]
    public sealed class OnPointerExitGenerator : NodeGenerator<Unity.VisualScripting.OnPointerExit>
    {
        public OnPointerExitGenerator(Unity.VisualScripting.OnPointerExit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnPointerUp))]
    public sealed class OnPointerUpGenerator : NodeGenerator<Unity.VisualScripting.OnPointerUp>
    {
        public OnPointerUpGenerator(Unity.VisualScripting.OnPointerUp unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnScroll))]
    public sealed class OnScrollGenerator : NodeGenerator<Unity.VisualScripting.OnScroll>
    {
        public OnScrollGenerator(Unity.VisualScripting.OnScroll unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnScrollbarValueChanged))]
    public sealed class OnScrollbarValueChangedGenerator : NodeGenerator<Unity.VisualScripting.OnScrollbarValueChanged>
    {
        public OnScrollbarValueChangedGenerator(Unity.VisualScripting.OnScrollbarValueChanged unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnScrollRectValueChanged))]
    public sealed class OnScrollRectValueChangedGenerator : NodeGenerator<Unity.VisualScripting.OnScrollRectValueChanged>
    {
        public OnScrollRectValueChangedGenerator(Unity.VisualScripting.OnScrollRectValueChanged unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnSelect))]
    public sealed class OnSelectGenerator : NodeGenerator<Unity.VisualScripting.OnSelect>
    {
        public OnSelectGenerator(Unity.VisualScripting.OnSelect unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnSliderValueChanged))]
    public sealed class OnSliderValueChangedGenerator : NodeGenerator<Unity.VisualScripting.OnSliderValueChanged>
    {
        public OnSliderValueChangedGenerator(Unity.VisualScripting.OnSliderValueChanged unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnSubmit))]
    public sealed class OnSubmitGenerator : NodeGenerator<Unity.VisualScripting.OnSubmit>
    {
        public OnSubmitGenerator(Unity.VisualScripting.OnSubmit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnToggleValueChanged))]
    public sealed class OnToggleValueChangedGenerator : NodeGenerator<Unity.VisualScripting.OnToggleValueChanged>
    {
        public OnToggleValueChangedGenerator(Unity.VisualScripting.OnToggleValueChanged unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnTransformChildrenChanged))]
    public sealed class OnTransformChildrenChangedGenerator : NodeGenerator<Unity.VisualScripting.OnTransformChildrenChanged>
    {
        public OnTransformChildrenChangedGenerator(Unity.VisualScripting.OnTransformChildrenChanged unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnTransformParentChanged))]
    public sealed class OnTransformParentChangedGenerator : NodeGenerator<Unity.VisualScripting.OnTransformParentChanged>
    {
        public OnTransformParentChangedGenerator(Unity.VisualScripting.OnTransformParentChanged unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnButtonInput))]
    public sealed class OnButtonInputGenerator : NodeGenerator<Unity.VisualScripting.OnButtonInput>
    {
        public OnButtonInputGenerator(Unity.VisualScripting.OnButtonInput unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnKeyboardInput))]
    public sealed class OnKeyboardInputGenerator : NodeGenerator<Unity.VisualScripting.OnKeyboardInput>
    {
        public OnKeyboardInputGenerator(Unity.VisualScripting.OnKeyboardInput unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnMouseDown))]
    public sealed class OnMouseDownGenerator : NodeGenerator<Unity.VisualScripting.OnMouseDown>
    {
        public OnMouseDownGenerator(Unity.VisualScripting.OnMouseDown unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnMouseDrag))]
    public sealed class OnMouseDragGenerator : NodeGenerator<Unity.VisualScripting.OnMouseDrag>
    {
        public OnMouseDragGenerator(Unity.VisualScripting.OnMouseDrag unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnMouseEnter))]
    public sealed class OnMouseEnterGenerator : NodeGenerator<Unity.VisualScripting.OnMouseEnter>
    {
        public OnMouseEnterGenerator(Unity.VisualScripting.OnMouseEnter unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnMouseExit))]
    public sealed class OnMouseExitGenerator : NodeGenerator<Unity.VisualScripting.OnMouseExit>
    {
        public OnMouseExitGenerator(Unity.VisualScripting.OnMouseExit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnMouseInput))]
    public sealed class OnMouseInputGenerator : NodeGenerator<Unity.VisualScripting.OnMouseInput>
    {
        public OnMouseInputGenerator(Unity.VisualScripting.OnMouseInput unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnMouseOver))]
    public sealed class OnMouseOverGenerator : NodeGenerator<Unity.VisualScripting.OnMouseOver>
    {
        public OnMouseOverGenerator(Unity.VisualScripting.OnMouseOver unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnMouseUp))]
    public sealed class OnMouseUpGenerator : NodeGenerator<Unity.VisualScripting.OnMouseUp>
    {
        public OnMouseUpGenerator(Unity.VisualScripting.OnMouseUp unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnMouseUpAsButton))]
    public sealed class OnMouseUpAsButtonGenerator : NodeGenerator<Unity.VisualScripting.OnMouseUpAsButton>
    {
        public OnMouseUpAsButtonGenerator(Unity.VisualScripting.OnMouseUpAsButton unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.FixedUpdate))]
    public sealed class FixedUpdateGenerator : NodeGenerator<Unity.VisualScripting.FixedUpdate>
    {
        public FixedUpdateGenerator(Unity.VisualScripting.FixedUpdate unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.LateUpdate))]
    public sealed class LateUpdateGenerator : NodeGenerator<Unity.VisualScripting.LateUpdate>
    {
        public LateUpdateGenerator(Unity.VisualScripting.LateUpdate unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnDestroy))]
    public sealed class OnDestroyGenerator : NodeGenerator<Unity.VisualScripting.OnDestroy>
    {
        public OnDestroyGenerator(Unity.VisualScripting.OnDestroy unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnDisable))]
    public sealed class OnDisableGenerator : NodeGenerator<Unity.VisualScripting.OnDisable>
    {
        public OnDisableGenerator(Unity.VisualScripting.OnDisable unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnEnable))]
    public sealed class OnEnableGenerator : NodeGenerator<Unity.VisualScripting.OnEnable>
    {
        public OnEnableGenerator(Unity.VisualScripting.OnEnable unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Start))]
    public sealed class StartGenerator : NodeGenerator<Unity.VisualScripting.Start>
    {
        public StartGenerator(Unity.VisualScripting.Start unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.trigger.connection?.destination;
            if (!Unit.trigger.hasAnyConnection)
                return "\n";

            return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                .GenerateControl(destination, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Update))]
    public sealed class UpdateGenerator : NodeGenerator<Unity.VisualScripting.Update>
    {
        public UpdateGenerator(Unity.VisualScripting.Update unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnDestinationReached))]
    public sealed class OnDestinationReachedGenerator : NodeGenerator<Unity.VisualScripting.OnDestinationReached>
    {
        public OnDestinationReachedGenerator(Unity.VisualScripting.OnDestinationReached unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }
    /*
        [NodeGenerator(typeof(Unity.VisualScripting.OnCollisionEnter))]
        public sealed class OnCollisionEnterGenerator : NodeGenerator<Unity.VisualScripting.OnCollisionEnter>
        {
            public OnCollisionEnterGenerator(Unity.VisualScripting.OnCollisionEnter unit) : base(unit)
            {
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {
                var destination = Unit.controlOutputs
                    .Select(port => port.connection?.destination)
                    .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

                if (destination != null)
                {
                    return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                        .GenerateControl(destination, data, indent);
                }

                return "\n";
            }

            public override string GenerateValue(ValueOutput output)
            {
                return base.GenerateValue(output);
            }

            public override string GenerateValue(ValueInput input)
            {
                return base.GenerateValue(input);
            }
        }
    */
    [NodeGenerator(typeof(Unity.VisualScripting.CollisionEventUnit))]
    public sealed class CollisionEventGenerator : NodeGenerator<Unity.VisualScripting.CollisionEventUnit>
    {
        public CollisionEventGenerator(Unity.VisualScripting.CollisionEventUnit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            if (output != Unit.data)
            {
                return "collision." + output.key;
            }
            else
            {
                return "collision";
            }
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnCollisionExit))]
    public sealed class OnCollisionExitGenerator : NodeGenerator<Unity.VisualScripting.OnCollisionExit>
    {
        public OnCollisionExitGenerator(Unity.VisualScripting.OnCollisionExit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return "collision." + output.key;
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnCollisionStay))]
    public sealed class OnCollisionStayGenerator : NodeGenerator<Unity.VisualScripting.OnCollisionStay>
    {
        public OnCollisionStayGenerator(Unity.VisualScripting.OnCollisionStay unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnControllerColliderHit))]
    public sealed class OnControllerColliderHitGenerator : NodeGenerator<Unity.VisualScripting.OnControllerColliderHit>
    {
        public OnControllerColliderHitGenerator(Unity.VisualScripting.OnControllerColliderHit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnJointBreak))]
    public sealed class OnJointBreakGenerator : NodeGenerator<Unity.VisualScripting.OnJointBreak>
    {
        public OnJointBreakGenerator(Unity.VisualScripting.OnJointBreak unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnParticleCollision))]
    public sealed class OnParticleCollisionGenerator : NodeGenerator<Unity.VisualScripting.OnParticleCollision>
    {
        public OnParticleCollisionGenerator(Unity.VisualScripting.OnParticleCollision unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnTriggerEnter))]
    public sealed class OnTriggerEnterGenerator : NodeGenerator<Unity.VisualScripting.OnTriggerEnter>
    {
        public OnTriggerEnterGenerator(Unity.VisualScripting.OnTriggerEnter unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnTriggerExit))]
    public sealed class OnTriggerExitGenerator : NodeGenerator<Unity.VisualScripting.OnTriggerExit>
    {
        public OnTriggerExitGenerator(Unity.VisualScripting.OnTriggerExit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnTriggerStay))]
    public sealed class OnTriggerStayGenerator : NodeGenerator<Unity.VisualScripting.OnTriggerStay>
    {
        public OnTriggerStayGenerator(Unity.VisualScripting.OnTriggerStay unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnCollisionEnter2D))]
    public sealed class OnCollisionEnter2DGenerator : NodeGenerator<Unity.VisualScripting.OnCollisionEnter2D>
    {
        public OnCollisionEnter2DGenerator(Unity.VisualScripting.OnCollisionEnter2D unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnCollisionExit2D))]
    public sealed class OnCollisionExit2DGenerator : NodeGenerator<Unity.VisualScripting.OnCollisionExit2D>
    {
        public OnCollisionExit2DGenerator(Unity.VisualScripting.OnCollisionExit2D unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnCollisionStay2D))]
    public sealed class OnCollisionStay2DGenerator : NodeGenerator<Unity.VisualScripting.OnCollisionStay2D>
    {
        public OnCollisionStay2DGenerator(Unity.VisualScripting.OnCollisionStay2D unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnJointBreak2D))]
    public sealed class OnJointBreak2DGenerator : NodeGenerator<Unity.VisualScripting.OnJointBreak2D>
    {
        public OnJointBreak2DGenerator(Unity.VisualScripting.OnJointBreak2D unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnTriggerEnter2D))]
    public sealed class OnTriggerEnter2DGenerator : NodeGenerator<Unity.VisualScripting.OnTriggerEnter2D>
    {
        public OnTriggerEnter2DGenerator(Unity.VisualScripting.OnTriggerEnter2D unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnTriggerExit2D))]
    public sealed class OnTriggerExit2DGenerator : NodeGenerator<Unity.VisualScripting.OnTriggerExit2D>
    {
        public OnTriggerExit2DGenerator(Unity.VisualScripting.OnTriggerExit2D unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnTriggerStay2D))]
    public sealed class OnTriggerStay2DGenerator : NodeGenerator<Unity.VisualScripting.OnTriggerStay2D>
    {
        public OnTriggerStay2DGenerator(Unity.VisualScripting.OnTriggerStay2D unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnBecameInvisible))]
    public sealed class OnBecameInvisibleGenerator : NodeGenerator<Unity.VisualScripting.OnBecameInvisible>
    {
        public OnBecameInvisibleGenerator(Unity.VisualScripting.OnBecameInvisible unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.OnBecameVisible))]
    public sealed class OnBecameVisibleGenerator : NodeGenerator<Unity.VisualScripting.OnBecameVisible>
    {
        public OnBecameVisibleGenerator(Unity.VisualScripting.OnBecameVisible unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.controlOutputs
                .Select(port => port.connection?.destination)
                .FirstOrDefault(output => output != null && output.unit is Unit) as ControlInput;

            if (destination != null)
            {
                return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                    .GenerateControl(destination, data, indent);
            }

            return "\n";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.TriggerCustomEvent))]
    public sealed class TriggerCustomEventGenerator : NodeGenerator<Unity.VisualScripting.TriggerCustomEvent>
    {
        public TriggerCustomEventGenerator(Unity.VisualScripting.TriggerCustomEvent unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            if (input == Unit.enter)
            {
                var eventName = GenerateValue(Unit.name);
                string eventArgs = GenerateAllArguments(Unit.arguments);

                /*if (!CSharpPreview.UseCustomEventsAsMethods)
                 {*/
                output += CodeBuilder.Indent(indent) + $"CustomEvent".TypeHighlight() + $".Trigger({GenerateValue(Unit.target)}, {eventName}, {eventArgs});\n";
                /*                }
                                else 
                                {
                                    output += CodeBuilder.Indent(indent) + GenerateValue(Unit.target) + eventName + $"({eventArgs});\n";
                                }*/
                output += (Unit.exit.hasValidConnection) ? (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent) : string.Empty;
            }

            return output;
        }

        private string GenerateAllArguments(List<ValueInput> arguments)
        {
            List<string> argumentValues = new List<string>();
            /*if (!CSharpPreview.UseCustomEventsAsMethods)
            {
                foreach (var argument in arguments)
                {
                    string argumentValue = GenerateValue(argument);
                    argumentValues.Add(argumentValue);
                }
                return $"new ".ConstructHighlight() + $"object".TypeHighlight() + $"[] {{ {string.Join(", ", argumentValues)} }}";
            }
            else 
            {*/
            foreach (var argument in arguments)
            {
                string argumentValue = GenerateValue(argument);
                argumentValues.Add(argumentValue);
            }
            return string.Join(", ", argumentValues);
            //}
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input == Unit.name || input == Unit.target)
            {
                if (input == Unit.name)
                {
                    if (input.hasValidConnection)
                    {
                        return (input.connection.source.unit as Unit).GenerateValue(input.connection.source);
                    }
                    else
                    {
                        /*if (!CSharpPreview.UseCustomEventsAsMethods)
                        {*/
                        return Unit.defaultValues[input.key].As().Code(false);
                        /*}
                        else 
                        {
                            return Unit.defaultValues[input.key].ToString();
                        }*/
                    }
                }
                else if (input == Unit.target)
                {
                    if (input.hasValidConnection)
                    {
                        return/* (CSharpPreview.UseCustomEventsAsMethods) ? (input.connection.source.unit as Unit).GenerateValue(input.connection.source) + "." : */(input.connection.source.unit as Unit).GenerateValue(input.connection.source);
                    }
                    else
                    {
                        /*if (!CSharpPreview.UseCustomEventsAsMethods)
                        {*/
                        return CodeBuilder.WarningHighlight("/* Requires GameObject Input */");
                        /*}
                        else 
                        {
                            return "";
                        }*/
                    }
                }
            }
            else if (Unit.arguments.Contains(input))
            {
                if (input.hasValidConnection)
                {
                    return (input.connection.source.unit as Unit).GenerateValue(input.connection.source);
                }
                else
                {
                    return $"/* {input.key} Requires Input */";
                }
            }

            return base.GenerateValue(input);
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.Formula))]
    public sealed class FormulaGenerator : NodeGenerator<Unity.VisualScripting.Formula>
    {
        public FormulaGenerator(Unity.VisualScripting.Formula unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.GetScriptGraph))]
    public sealed class GetScriptGraphGenerator : NodeGenerator<Unity.VisualScripting.GetScriptGraph>
    {
        public GetScriptGraphGenerator(Unity.VisualScripting.GetScriptGraph unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.GetScriptGraphs))]
    public sealed class GetScriptGraphsGenerator : NodeGenerator<Unity.VisualScripting.GetScriptGraphs>
    {
        public GetScriptGraphsGenerator(Unity.VisualScripting.GetScriptGraphs unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.HasScriptGraph))]
    public sealed class HasScriptGraphGenerator : NodeGenerator<Unity.VisualScripting.HasScriptGraph>
    {
        public HasScriptGraphGenerator(Unity.VisualScripting.HasScriptGraph unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.SetScriptGraph))]
    public sealed class SetScriptGraphGenerator : NodeGenerator<Unity.VisualScripting.SetScriptGraph>
    {
        public SetScriptGraphGenerator(Unity.VisualScripting.SetScriptGraph unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Comparison))]
    public sealed class ComparisonGenerator : NodeGenerator<Unity.VisualScripting.Comparison>
    {
        public ComparisonGenerator(Unity.VisualScripting.Comparison unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.GenericDivide))]
    public sealed class GenericDivideGenerator : NodeGenerator<Unity.VisualScripting.GenericDivide>
    {
        public GenericDivideGenerator(Unity.VisualScripting.GenericDivide unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"{GenerateValue(Unit.dividend)} / {GenerateValue(Unit.divisor)}";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.GenericModulo))]
    public sealed class GenericModuloGenerator : NodeGenerator<Unity.VisualScripting.GenericModulo>
    {
        public GenericModuloGenerator(Unity.VisualScripting.GenericModulo unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"{GenerateValue(Unit.dividend)} % {GenerateValue(Unit.divisor)}";
        }


        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                return $"/* {input.key} Requires Input */";
            }
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.GenericMultiply))]
    public sealed class GenericMultiplyGenerator : NodeGenerator<Unity.VisualScripting.GenericMultiply>
    {
        public GenericMultiplyGenerator(Unity.VisualScripting.GenericMultiply unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"{GenerateValue(Unit.a)} * {GenerateValue(Unit.b)}";
        }


        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.GenericSubtract))]
    public sealed class GenericSubtractGenerator : NodeGenerator<Unity.VisualScripting.GenericSubtract>
    {
        public GenericSubtractGenerator(Unity.VisualScripting.GenericSubtract unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"{GenerateValue(Unit.minuend)} - {GenerateValue(Unit.subtrahend)}";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.GenericSum))]
    public sealed class GenericSumGenerator : NodeGenerator<Unity.VisualScripting.GenericSum>
    {
        public GenericSumGenerator(Unity.VisualScripting.GenericSum unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }
        public override string GenerateValue(ValueOutput output)
        {
            // Generate code for summing up all inputs
            if (output == Unit.sum)
            {
                // Create a list to store the generated values
                List<string> values = new List<string>();

                // Iterate through all inputs and add their generated values
                foreach (var input in Unit.multiInputs)
                {
                    values.Add(GenerateValue(input));
                }

                // Join the values with ' + ' as separator
                return string.Join(" + ", values);
            }

            return base.GenerateValue(output);
        }


        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }

    }


    [NodeGenerator(typeof(Unity.VisualScripting.ScalarAbsolute))]
    public sealed class ScalarAbsoluteGenerator : NodeGenerator<Unity.VisualScripting.ScalarAbsolute>
    {
        public ScalarAbsoluteGenerator(Unity.VisualScripting.ScalarAbsolute unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"Mathf.Abs({GenerateValue(Unit.input)})";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.ScalarAverage))]
    public sealed class ScalarAverageGenerator : NodeGenerator<Unity.VisualScripting.ScalarAverage>
    {
        public ScalarAverageGenerator(Unity.VisualScripting.ScalarAverage unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            List<string> inputCodeList = Unit.multiInputs.Select(input =>
            {
                return GenerateValue(input);
            }).ToList();

            string Parameters = string.Join(", ", inputCodeList);

            return $"CodeGenUtils.CalculateAverage({Parameters})";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.ScalarDivide))]
    public sealed class ScalarDivideGenerator : NodeGenerator<Unity.VisualScripting.ScalarDivide>
    {
        public ScalarDivideGenerator(Unity.VisualScripting.ScalarDivide unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"{GenerateValue(Unit.dividend)} / {GenerateValue(Unit.divisor)}";
        }


        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.ScalarExponentiate))]
    public sealed class ScalarExponentiateGenerator : NodeGenerator<Unity.VisualScripting.ScalarExponentiate>
    {
        public ScalarExponentiateGenerator(Unity.VisualScripting.ScalarExponentiate unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {

            return $"Math.Pow({GenerateValue(Unit.@base)}, {GenerateValue(Unit.exponent)})";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }
    }


    [NodeGenerator(typeof(Unity.VisualScripting.ScalarLerp))]
    public sealed class ScalarLerpGenerator : NodeGenerator<Unity.VisualScripting.ScalarLerp>
    {
        public ScalarLerpGenerator(Unity.VisualScripting.ScalarLerp unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"Mathf.Lerp({GenerateValue(Unit.a)}, {GenerateValue(Unit.b)}, {GenerateValue(Unit.t)})";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.ScalarMaximum))]
    public sealed class ScalarMaximumGenerator : NodeGenerator<Unity.VisualScripting.ScalarMaximum>
    {
        public ScalarMaximumGenerator(Unity.VisualScripting.ScalarMaximum unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {

            List<string> inputCodeList = Unit.multiInputs.Select(input =>
            {
                return GenerateValue(input);
            }).ToList();

            string Parameters = string.Join(", ", inputCodeList);

            return $"CodeGenUtils.CalculateMax({Parameters})";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.ScalarMinimum))]
    public sealed class ScalarMinimumGenerator : NodeGenerator<Unity.VisualScripting.ScalarMinimum>
    {
        public ScalarMinimumGenerator(Unity.VisualScripting.ScalarMinimum unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            List<string> inputCodeList = Unit.multiInputs.Select(input =>
            {
                return GenerateValue(input);
            }).ToList();

            string Parameters = string.Join(", ", inputCodeList);

            return $"CodeGenUtils.CalculateMin({Parameters})";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.ScalarModulo))]
    public sealed class ScalarModuloGenerator : NodeGenerator<Unity.VisualScripting.ScalarModulo>
    {
        public ScalarModuloGenerator(Unity.VisualScripting.ScalarModulo unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"{GenerateValue(Unit.dividend)} % {GenerateValue(Unit.divisor)}";
        }


        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.ScalarMoveTowards))]
    public sealed class ScalarMoveTowardsGenerator : NodeGenerator<Unity.VisualScripting.ScalarMoveTowards>
    {
        public ScalarMoveTowardsGenerator(Unity.VisualScripting.ScalarMoveTowards unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"Mathf.MoveTowards({GenerateValue(Unit.current)}, {GenerateValue(Unit.target)}, {GenerateValue(Unit.maxDelta)})";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                return $"/* {input.key} Requires Input */";
            }
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.ScalarMultiply))]
    public sealed class ScalarMultiplyGenerator : NodeGenerator<Unity.VisualScripting.ScalarMultiply>
    {
        public ScalarMultiplyGenerator(Unity.VisualScripting.ScalarMultiply unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"{GenerateValue(Unit.a)} * {GenerateValue(Unit.b)}";
        }


        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.ScalarNormalize))]
    public sealed class ScalarNormalizeGenerator : NodeGenerator<Unity.VisualScripting.ScalarNormalize>
    {
        public ScalarNormalizeGenerator(Unity.VisualScripting.ScalarNormalize unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"CodeGenUtils.Normalize({GenerateValue(Unit.input)})";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(true, false, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.ScalarPerSecond))]
    public sealed class ScalarPerSecondGenerator : NodeGenerator<Unity.VisualScripting.ScalarPerSecond>
    {
        public ScalarPerSecondGenerator(Unity.VisualScripting.ScalarPerSecond unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"{GenerateValue(Unit.input)} * Time.deltaTime";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(true, false, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.ScalarRoot))]
    public sealed class ScalarRootGenerator : NodeGenerator<Unity.VisualScripting.ScalarRoot>
    {
        public ScalarRootGenerator(Unity.VisualScripting.ScalarRoot unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            if (output == Unit.root)
            {
                return $"Math.Pow({GenerateValue(Unit.radicand)}, 1 / {GenerateValue(Unit.degree)})";
            }

            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }
    }


    [NodeGenerator(typeof(Unity.VisualScripting.ScalarRound))]
    public sealed class ScalarRoundGenerator : NodeGenerator<Unity.VisualScripting.ScalarRound>
    {
        public ScalarRoundGenerator(Unity.VisualScripting.ScalarRound unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            switch (Unit.rounding)
            {
                case ScalarRound.Rounding.Floor:
                    return $"Mathf.FloorToInt({GenerateValue(Unit.input)})";
                case ScalarRound.Rounding.AwayFromZero:
                    return $"Mathf.RoundToInt({GenerateValue(Unit.input)})";
                case ScalarRound.Rounding.Ceiling:
                    return $"Mathf.CeilToInt({GenerateValue(Unit.input)})";
                default:
                    return base.GenerateValue(output);
            }
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }
    }



    [NodeGenerator(typeof(Unity.VisualScripting.ScalarSubtract))]
    public sealed class ScalarSubtractGenerator : NodeGenerator<Unity.VisualScripting.ScalarSubtract>
    {
        public ScalarSubtractGenerator(Unity.VisualScripting.ScalarSubtract unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"{GenerateValue(Unit.minuend)} - {GenerateValue(Unit.subtrahend)}";
        }


        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.ScalarSum))]
    public sealed class ScalarSumGenerator : NodeGenerator<Unity.VisualScripting.ScalarSum>
    {
        public ScalarSumGenerator(Unity.VisualScripting.ScalarSum unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            // Generate code for summing up all inputs
            if (output == Unit.sum)
            {
                // Create a list to store the generated values
                List<string> values = new List<string>();

                // Iterate through all inputs and add their generated values
                foreach (var input in Unit.multiInputs)
                {
                    values.Add(GenerateValue(input));
                }

                // Join the values with ' + ' as separator
                return string.Join(" + ", values);
            }

            return base.GenerateValue(output);
        }


        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }
    }


    [NodeGenerator(typeof(Unity.VisualScripting.Vector2Absolute))]
    public sealed class Vector2AbsoluteGenerator : NodeGenerator<Unity.VisualScripting.Vector2Absolute>
    {
        public Vector2AbsoluteGenerator(Unity.VisualScripting.Vector2Absolute unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"new Vector2(Mathf.Abs({GenerateValue(Unit.input)}.x), Mathf.Abs({GenerateValue(Unit.input)}.y))";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector2Angle))]
    public sealed class Vector2AngleGenerator : NodeGenerator<Unity.VisualScripting.Vector2Angle>
    {
        public Vector2AngleGenerator(Unity.VisualScripting.Vector2Angle unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"UnityEngine.Vector2.Angle({GenerateValue(Unit.a)}, {GenerateValue(Unit.b)})";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector2Average))]
    public sealed class Vector2AverageGenerator : NodeGenerator<Unity.VisualScripting.Vector2Average>
    {
        public Vector2AverageGenerator(Unity.VisualScripting.Vector2Average unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector2Distance))]
    public sealed class Vector2DistanceGenerator : NodeGenerator<Unity.VisualScripting.Vector2Distance>
    {
        public Vector2DistanceGenerator(Unity.VisualScripting.Vector2Distance unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector2Divide))]
    public sealed class Vector2DivideGenerator : NodeGenerator<Unity.VisualScripting.Vector2Divide>
    {
        public Vector2DivideGenerator(Unity.VisualScripting.Vector2Divide unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector2DotProduct))]
    public sealed class Vector2DotProductGenerator : NodeGenerator<Unity.VisualScripting.Vector2DotProduct>
    {
        public Vector2DotProductGenerator(Unity.VisualScripting.Vector2DotProduct unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector2Lerp))]
    public sealed class Vector2LerpGenerator : NodeGenerator<Unity.VisualScripting.Vector2Lerp>
    {
        public Vector2LerpGenerator(Unity.VisualScripting.Vector2Lerp unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector2Maximum))]
    public sealed class Vector2MaximumGenerator : NodeGenerator<Unity.VisualScripting.Vector2Maximum>
    {
        public Vector2MaximumGenerator(Unity.VisualScripting.Vector2Maximum unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector2Minimum))]
    public sealed class Vector2MinimumGenerator : NodeGenerator<Unity.VisualScripting.Vector2Minimum>
    {
        public Vector2MinimumGenerator(Unity.VisualScripting.Vector2Minimum unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector2Modulo))]
    public sealed class Vector2ModuloGenerator : NodeGenerator<Unity.VisualScripting.Vector2Modulo>
    {
        public Vector2ModuloGenerator(Unity.VisualScripting.Vector2Modulo unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"{GenerateValue(Unit.dividend)} % {GenerateValue(Unit.divisor)}";
        }


        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector2MoveTowards))]
    public sealed class Vector2MoveTowardsGenerator : NodeGenerator<Unity.VisualScripting.Vector2MoveTowards>
    {
        public Vector2MoveTowardsGenerator(Unity.VisualScripting.Vector2MoveTowards unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector2Multiply))]
    public sealed class Vector2MultiplyGenerator : NodeGenerator<Unity.VisualScripting.Vector2Multiply>
    {
        public Vector2MultiplyGenerator(Unity.VisualScripting.Vector2Multiply unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            // Check if both inputs have valid connections
            if (Unit.a.hasValidConnection && Unit.b.hasValidConnection)
            {
                string vectorA = GenerateValue(Unit.a);
                string vectorB = GenerateValue(Unit.b);

                return $"new Vector2({vectorA}.x * {vectorB}.x, {vectorA}.y * {vectorB}.y)";
            }

            // Handle other cases if needed
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input
                return $"/* {input.key} Requires Input */";
            }
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector2Normalize))]
    public sealed class Vector2NormalizeGenerator : NodeGenerator<Unity.VisualScripting.Vector2Normalize>
    {
        public Vector2NormalizeGenerator(Unity.VisualScripting.Vector2Normalize unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"{GenerateValue(Unit.input)}.normalized";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input
                return $"/* {input.key} Requires Input */";
            }
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector2PerSecond))]
    public sealed class Vector2PerSecondGenerator : NodeGenerator<Unity.VisualScripting.Vector2PerSecond>
    {
        public Vector2PerSecondGenerator(Unity.VisualScripting.Vector2PerSecond unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"{GenerateValue(Unit.input)} * Time.deltaTime";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input
                return $"/* {input.key} Requires Input */";
            }
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector2Project))]
    public sealed class Vector2ProjectGenerator : NodeGenerator<Unity.VisualScripting.Vector2Project>
    {
        public Vector2ProjectGenerator(Unity.VisualScripting.Vector2Project unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"Vector2.Dot({GenerateValue(Unit.a)}, {GenerateValue(Unit.b)}) * {GenerateValue(Unit.b)}.normalized";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input
                return $"/* {input.key} Requires Input */";
            }
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector2Round))]
    public sealed class Vector2RoundGenerator : NodeGenerator<Unity.VisualScripting.Vector2Round>
    {
        public Vector2RoundGenerator(Unity.VisualScripting.Vector2Round unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            if (output == Unit.output)
            {
                // Generate code to round each component of the input Vector2 separately
                string xRoundingCode = GenerateRoundingCode(Unit.rounding, GenerateValue(Unit.input), "x");
                string yRoundingCode = GenerateRoundingCode(Unit.rounding, GenerateValue(Unit.input), "y");

                return $"new Vector2({xRoundingCode}, {yRoundingCode})";
            }
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                return $"/* {input.key} Requires Input *//*";
            }
        }

        private string GenerateRoundingCode(Round<Vector2, Vector2>.Rounding roundingMethod, string value, string component)
        {
            switch (roundingMethod)
            {
                case Round<Vector2, Vector2>.Rounding.Floor:
                    return $"Mathf.Floor({value}.{component})";
                case Round<Vector2, Vector2>.Rounding.AwayFromZero:
                    return $"Mathf.Round({value}.{component})";
                case Round<Vector2, Vector2>.Rounding.Ceiling:
                    return $"Mathf.Ceil({value}.{component})";
                default:
                    return $"{value}.{component}"; // Default to the original value if rounding is not specified
            }
        }
    }


    [NodeGenerator(typeof(Unity.VisualScripting.Vector2Subtract))]
    public sealed class Vector2SubtractGenerator : NodeGenerator<Unity.VisualScripting.Vector2Subtract>
    {
        public Vector2SubtractGenerator(Unity.VisualScripting.Vector2Subtract unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            // Generate code for the subtract operation
            if (output == Unit.difference)
            {
                return $"{GenerateValue(Unit.minuend)} - {GenerateValue(Unit.subtrahend)}";
            }

            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector2Sum))]
    public sealed class Vector2SumGenerator : NodeGenerator<Unity.VisualScripting.Vector2Sum>
    {
        public Vector2SumGenerator(Unity.VisualScripting.Vector2Sum unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            // Generate code for summing up all inputs
            if (output == Unit.sum)
            {
                // Create a list to store the generated values
                List<string> values = new List<string>();

                // Iterate through all inputs and add their generated values
                foreach (var input in Unit.multiInputs)
                {
                    values.Add(GenerateValue(input));
                }

                // Join the values with ' + ' as separator
                return string.Join(" + ", values);
            }

            return base.GenerateValue(output);
        }


        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector3Absolute))]
    public sealed class Vector3AbsoluteGenerator : NodeGenerator<Unity.VisualScripting.Vector3Absolute>
    {
        public Vector3AbsoluteGenerator(Unity.VisualScripting.Vector3Absolute unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector3Angle))]
    public sealed class Vector3AngleGenerator : NodeGenerator<Unity.VisualScripting.Vector3Angle>
    {
        public Vector3AngleGenerator(Unity.VisualScripting.Vector3Angle unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector3Average))]
    public sealed class Vector3AverageGenerator : NodeGenerator<Unity.VisualScripting.Vector3Average>
    {
        public Vector3AverageGenerator(Unity.VisualScripting.Vector3Average unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector3CrossProduct))]
    public sealed class Vector3CrossProductGenerator : NodeGenerator<Unity.VisualScripting.Vector3CrossProduct>
    {
        public Vector3CrossProductGenerator(Unity.VisualScripting.Vector3CrossProduct unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector3Distance))]
    public sealed class Vector3DistanceGenerator : NodeGenerator<Unity.VisualScripting.Vector3Distance>
    {
        public Vector3DistanceGenerator(Unity.VisualScripting.Vector3Distance unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"Vector3.Distance({GenerateValue(Unit.a)}, {GenerateValue(Unit.b)})";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector3Divide))]
    public sealed class Vector3DivideGenerator : NodeGenerator<Unity.VisualScripting.Vector3Divide>
    {
        public Vector3DivideGenerator(Unity.VisualScripting.Vector3Divide unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector3DotProduct))]
    public sealed class Vector3DotProductGenerator : NodeGenerator<Unity.VisualScripting.Vector3DotProduct>
    {
        public Vector3DotProductGenerator(Unity.VisualScripting.Vector3DotProduct unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector3Lerp))]
    public sealed class Vector3LerpGenerator : NodeGenerator<Unity.VisualScripting.Vector3Lerp>
    {
        public Vector3LerpGenerator(Unity.VisualScripting.Vector3Lerp unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector3Maximum))]
    public sealed class Vector3MaximumGenerator : NodeGenerator<Unity.VisualScripting.Vector3Maximum>
    {
        public Vector3MaximumGenerator(Unity.VisualScripting.Vector3Maximum unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector3Minimum))]
    public sealed class Vector3MinimumGenerator : NodeGenerator<Unity.VisualScripting.Vector3Minimum>
    {
        public Vector3MinimumGenerator(Unity.VisualScripting.Vector3Minimum unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector3Modulo))]
    public sealed class Vector3ModuloGenerator : NodeGenerator<Unity.VisualScripting.Vector3Modulo>
    {
        public Vector3ModuloGenerator(Unity.VisualScripting.Vector3Modulo unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"{GenerateValue(Unit.dividend)} % {GenerateValue(Unit.divisor)}";
        }


        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector3MoveTowards))]
    public sealed class Vector3MoveTowardsGenerator : NodeGenerator<Unity.VisualScripting.Vector3MoveTowards>
    {
        public Vector3MoveTowardsGenerator(Unity.VisualScripting.Vector3MoveTowards unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector3Multiply))]
    public sealed class Vector3MultiplyGenerator : NodeGenerator<Unity.VisualScripting.Vector3Multiply>
    {
        public Vector3MultiplyGenerator(Unity.VisualScripting.Vector3Multiply unit) : base(unit)
        {
            NameSpace = "UnityEngine";
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            // Check if both inputs have valid connections
            if (Unit.a.hasValidConnection && Unit.b.hasValidConnection)
            {
                string vectorA = GenerateValue(Unit.a);
                string vectorB = GenerateValue(Unit.b);

                return $"new ".ConstructHighlight() + "Vector3".TypeHighlight() + $"({vectorA}.x * {vectorB}.x,{vectorA}.y * {vectorB}.y,{vectorA}.z * {vectorB}.z)";
            }

            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                return $"/* {input.key} Requires Input */";
            }
        }
    }


    [NodeGenerator(typeof(Unity.VisualScripting.Vector3Normalize))]
    public sealed class Vector3NormalizeGenerator : NodeGenerator<Unity.VisualScripting.Vector3Normalize>
    {
        public Vector3NormalizeGenerator(Unity.VisualScripting.Vector3Normalize unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"Vector3.Normalize({GenerateValue(Unit.input)})";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input
                return $"/* {input.key} Requires Input */";
            }
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector3PerSecond))]
    public sealed class Vector3PerSecondGenerator : NodeGenerator<Unity.VisualScripting.Vector3PerSecond>
    {
        public Vector3PerSecondGenerator(Unity.VisualScripting.Vector3PerSecond unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"{GenerateValue(Unit.input)} * Time.deltaTime";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input
                return $"/* {input.key} Requires Input */";
            }
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector3Project))]
    public sealed class Vector3ProjectGenerator : NodeGenerator<Unity.VisualScripting.Vector3Project>
    {
        public Vector3ProjectGenerator(Unity.VisualScripting.Vector3Project unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"Vector3.Project({GenerateValue(Unit.a)}, {GenerateValue(Unit.a)})";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input
                return $"/* {input.key} Requires Input */";
            }
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector3Round))]
    public sealed class Vector3RoundGenerator : NodeGenerator<Unity.VisualScripting.Vector3Round>
    {
        public Vector3RoundGenerator(Unity.VisualScripting.Vector3Round unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            switch (Unit.rounding)
            {
                case Round<Vector3, Vector3>.Rounding.Floor:
                    return $"Mathf.Floor(new Vector3({GenerateValue(Unit.input)}.x, {GenerateValue(Unit.input)}.y, {GenerateValue(Unit.input)}.z))";
                case Round<Vector3, Vector3>.Rounding.AwayFromZero:
                    return $"Mathf.Round(new Vector3({GenerateValue(Unit.input)}.x, {GenerateValue(Unit.input)}.y, {GenerateValue(Unit.input)}.z))";
                case Round<Vector3, Vector3>.Rounding.Ceiling:
                    return $"Mathf.Ceil(new Vector3({GenerateValue(Unit.input)}.x, {GenerateValue(Unit.input)}.y, {GenerateValue(Unit.input)}.z))";
                default:
                    return base.GenerateValue(output);
            }
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input
                return $"/* {input.key} Requires Input */";
            }
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector3Subtract))]
    public sealed class Vector3SubtractGenerator : NodeGenerator<Unity.VisualScripting.Vector3Subtract>
    {
        public Vector3SubtractGenerator(Unity.VisualScripting.Vector3Subtract unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            // Generate code for the subtract operation
            if (output == Unit.difference)
            {
                return $"{GenerateValue(Unit.minuend)} - {GenerateValue(Unit.subtrahend)}";
            }

            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector3Sum))]
    public sealed class Vector3SumGenerator : NodeGenerator<Unity.VisualScripting.Vector3Sum>
    {
        public Vector3SumGenerator(Unity.VisualScripting.Vector3Sum unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            // Generate code for summing up all inputs
            if (output == Unit.sum)
            {
                // Create a list to store the generated values
                List<string> values = new List<string>();

                // Iterate through all inputs and add their generated values
                foreach (var input in Unit.multiInputs)
                {
                    values.Add(GenerateValue(input));
                }

                // Join the values with ' + ' as separator
                return string.Join(" + ", values);
            }

            return base.GenerateValue(output);
        }


        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(true, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector4Absolute))]
    public sealed class Vector4AbsoluteGenerator : NodeGenerator<Unity.VisualScripting.Vector4Absolute>
    {
        public Vector4AbsoluteGenerator(Unity.VisualScripting.Vector4Absolute unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector4Average))]
    public sealed class Vector4AverageGenerator : NodeGenerator<Unity.VisualScripting.Vector4Average>
    {
        public Vector4AverageGenerator(Unity.VisualScripting.Vector4Average unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector4Distance))]
    public sealed class Vector4DistanceGenerator : NodeGenerator<Unity.VisualScripting.Vector4Distance>
    {
        public Vector4DistanceGenerator(Unity.VisualScripting.Vector4Distance unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector4Divide))]
    public sealed class Vector4DivideGenerator : NodeGenerator<Unity.VisualScripting.Vector4Divide>
    {
        public Vector4DivideGenerator(Unity.VisualScripting.Vector4Divide unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector4DotProduct))]
    public sealed class Vector4DotProductGenerator : NodeGenerator<Unity.VisualScripting.Vector4DotProduct>
    {
        public Vector4DotProductGenerator(Unity.VisualScripting.Vector4DotProduct unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector4Lerp))]
    public sealed class Vector4LerpGenerator : NodeGenerator<Unity.VisualScripting.Vector4Lerp>
    {
        public Vector4LerpGenerator(Unity.VisualScripting.Vector4Lerp unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector4Maximum))]
    public sealed class Vector4MaximumGenerator : NodeGenerator<Unity.VisualScripting.Vector4Maximum>
    {
        public Vector4MaximumGenerator(Unity.VisualScripting.Vector4Maximum unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector4Minimum))]
    public sealed class Vector4MinimumGenerator : NodeGenerator<Unity.VisualScripting.Vector4Minimum>
    {
        public Vector4MinimumGenerator(Unity.VisualScripting.Vector4Minimum unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector4Modulo))]
    public sealed class Vector4ModuloGenerator : NodeGenerator<Unity.VisualScripting.Vector4Modulo>
    {
        public Vector4ModuloGenerator(Unity.VisualScripting.Vector4Modulo unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"{GenerateValue(Unit.dividend)} % {GenerateValue(Unit.divisor)}";
        }


        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector4MoveTowards))]
    public sealed class Vector4MoveTowardsGenerator : NodeGenerator<Unity.VisualScripting.Vector4MoveTowards>
    {
        public Vector4MoveTowardsGenerator(Unity.VisualScripting.Vector4MoveTowards unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector4Multiply))]
    public sealed class Vector4MultiplyGenerator : NodeGenerator<Unity.VisualScripting.Vector4Multiply>
    {
        public Vector4MultiplyGenerator(Unity.VisualScripting.Vector4Multiply unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector4Normalize))]
    public sealed class Vector4NormalizeGenerator : NodeGenerator<Unity.VisualScripting.Vector4Normalize>
    {
        public Vector4NormalizeGenerator(Unity.VisualScripting.Vector4Normalize unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"Vector4.Normalize({GenerateValue(Unit.input)})";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input
                return $"/* {input.key} Requires Input */";
            }
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector4PerSecond))]
    public sealed class Vector4PerSecondGenerator : NodeGenerator<Unity.VisualScripting.Vector4PerSecond>
    {
        public Vector4PerSecondGenerator(Unity.VisualScripting.Vector4PerSecond unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"{GenerateValue(Unit.input)} * Time.deltaTime";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input
                return $"/* {input.key} Requires Input */";
            }
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector4Round))]
    public sealed class Vector4RoundGenerator : NodeGenerator<Unity.VisualScripting.Vector4Round>
    {
        public Vector4RoundGenerator(Unity.VisualScripting.Vector4Round unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            switch (Unit.rounding)
            {
                case Round<Vector4, Vector4>.Rounding.Floor:
                    return $"Mathf.FloorToInt({GenerateValue(Unit.input)})";
                case Round<Vector4, Vector4>.Rounding.AwayFromZero:
                    return $"Mathf.RoundToInt({GenerateValue(Unit.input)})";
                case Round<Vector4, Vector4>.Rounding.Ceiling:
                    return $"Mathf.CeilToInt({GenerateValue(Unit.input)})";
                default:
                    return base.GenerateValue(output);
            }
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector4Subtract))]
    public sealed class Vector4SubtractGenerator : NodeGenerator<Unity.VisualScripting.Vector4Subtract>
    {
        public Vector4SubtractGenerator(Unity.VisualScripting.Vector4Subtract unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            // Generate code for the subtract operation
            if (output == Unit.difference)
            {
                return $"{GenerateValue(Unit.minuend)} - {GenerateValue(Unit.subtrahend)}";
            }

            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Vector4Sum))]
    public sealed class Vector4SumGenerator : NodeGenerator<Unity.VisualScripting.Vector4Sum>
    {
        public Vector4SumGenerator(Unity.VisualScripting.Vector4Sum unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            // Generate code for summing up all inputs
            if (output == Unit.sum)
            {
                // Create a list to store the generated values
                List<string> values = new List<string>();

                // Iterate through all inputs and add their generated values
                foreach (var input in Unit.multiInputs)
                {
                    values.Add(GenerateValue(input));
                }

                // Join the values with ' + ' as separator
                return string.Join(" + ", values);
            }

            return base.GenerateValue(output);
        }


        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.MissingType))]
    public sealed class MissingTypeGenerator : NodeGenerator<Unity.VisualScripting.MissingType>
    {
        public MissingTypeGenerator(Unity.VisualScripting.MissingType unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.GraphInput))]
    public sealed class GraphInputGenerator : NodeGenerator<Unity.VisualScripting.GraphInput>
    {
        public GraphInputGenerator(Unity.VisualScripting.GraphInput unit) : base(unit)
        {
        }

        public override string GenerateValue(ValueOutput output)
        {
            var _output = string.Empty;
            var ValueInput = connectedValueInputs.FirstOrDefault(valueInput => valueInput.key == output.key);

            if (ValueInput != null)
            {
                _output += GenerateValue(ValueInput);
            }
            else
            {
                _output += $"/* Missing Value Input: {output.key} */";
            }

            return _output;
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                return input.unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.GraphOutput))]
    public sealed class GraphOutputGenerator : NodeGenerator<Unity.VisualScripting.GraphOutput>
    {
        public GraphOutputGenerator(Unity.VisualScripting.GraphOutput unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            var controloutput = connectedGraphOutputs.FirstOrDefault(output => output.key == input.key);

            if (controloutput != null)
            {
                output += (controloutput.hasValidConnection) ? GetSingleDecorator(controloutput.connection.destination.unit as Unit, controloutput.connection.destination.unit as Unit)
                .GenerateControl(controloutput.connection.destination, data, indent) : string.Empty;
            }

            return output;
        }


        public override string GenerateValue(ValueOutput output)
        {
            return GenerateValue(Unit.valueInputs[output.key]);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                return $"/* {input.key} Requires Input */";
            }
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Null))]
    public sealed class NullGenerator : NodeGenerator<Unity.VisualScripting.Null>
    {
        public NullGenerator(Unity.VisualScripting.Null unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            if (output == Unit.@null)
            {
                if (Unit.@null.hasValidConnection)
                {
                    return "null".ConstructHighlight();
                }
            }
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.NullCheck))]
    public sealed class NullCheckGenerator : NodeGenerator<Unity.VisualScripting.NullCheck>
    {
        public NullCheckGenerator(Unity.VisualScripting.NullCheck unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            if (input == Unit.enter)
            {
                var valueToCheck = GenerateValue(Unit.input);

                output += CodeBuilder.Indent(indent) + $"if".ConstructHighlight() + $"({valueToCheck}" + " != " + "null".ConstructHighlight() + ")";
                output += "\n";
                output += CodeBuilder.OpenBody(indent);
                output += "\n";

                if (Unit.ifNotNull.hasValidConnection)
                {
                    output += CodeBuilder.Indent(indent) + (Unit.ifNotNull.connection.destination.unit as Unit).GenerateControl(Unit.ifNotNull.connection.destination, data, indent + 1);
                }

                output += "\n" + CodeBuilder.CloseBody(indent);

                if (Unit.ifNull.hasAnyConnection)
                {
                    output += "\n";
                    output += CodeBuilder.Indent(indent) + "else".ConstructHighlight();
                    output += "\n";
                    output += CodeBuilder.OpenBody(indent);
                    output += "\n";
                    output += CodeBuilder.Indent(indent) + (Unit.ifNull.connection.destination.unit as Unit).GenerateControl(Unit.ifNull.connection.destination, data, indent + 1);
                    output += "\n" + CodeBuilder.CloseBody(indent);
                }
                else
                {
                    output += "\n";
                }


            }

            return output;
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input == Unit.input)
            {
                if (input.hasValidConnection)
                {
                    return (input.connection.source.unit as Unit).GenerateValue(input.connection.source);
                }
            }

            return base.GenerateValue(input);
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.NullCoalesce))]
    public sealed class NullCoalesceGenerator : NodeGenerator<Unity.VisualScripting.NullCoalesce>
    {
        public NullCoalesceGenerator(Unity.VisualScripting.NullCoalesce unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {

            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            if (output == Unit.result)
            {
                var valueToCheck = GenerateValue(Unit.input);
                var defaultValue = GenerateValue(Unit.fallback);

                return $"{valueToCheck} ?? {defaultValue}";
            }

            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input == Unit.input || input == Unit.fallback)
            {
                if (input.hasValidConnection)
                {
                    return (input.connection.source.unit as Unit).GenerateValue(input.connection.source);
                }
            }

            return base.GenerateValue(input);
        }
    }


    [NodeGenerator(typeof(Unity.VisualScripting.This))]
    public sealed class ThisGenerator : NodeGenerator<Unity.VisualScripting.This>
    {
        public ThisGenerator(Unity.VisualScripting.This unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            if (output == Unit.self)
            {
                if (Unit.self.hasValidConnection)
                {
                    return "gameObject";
                }
            }
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.Cooldown))]
    public class CooldownGenerator : NodeGenerator<Cooldown>
    {
        public CooldownGenerator(Cooldown unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return "/* Cooldown not supported */";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return "/* Cooldown not supported */";
        }

        public override string GenerateValue(ValueInput input)
        {
            return "/* Cooldown not supported */";
        }
    }



    [NodeGenerator(typeof(Unity.VisualScripting.Timer))]
    public sealed class TimerGenerator : NodeGenerator<Unity.VisualScripting.Timer>
    {
        public TimerGenerator(Unity.VisualScripting.Timer unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return "/* Timer node not supported */";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return "/* Timer node not supported */";
        }

        public override string GenerateValue(ValueInput input)
        {
            return "/* Timer node not supported */";
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.WaitForEndOfFrameUnit))]
    public sealed class WaitForEndOfFrameUnitGenerator : NodeGenerator<Unity.VisualScripting.WaitForEndOfFrameUnit>
    {
        public WaitForEndOfFrameUnitGenerator(Unity.VisualScripting.WaitForEndOfFrameUnit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            output += CodeBuilder.Indent(indent) + "yield return ".ControlHighlight() + "new ".ConstructHighlight() + "WaitForEndOfFrame();";

            if (Unit.exit.hasValidConnection)
            {
                output += (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent);
            }

            return output;
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.WaitForFlow))]
    public sealed class WaitForFlowGenerator : NodeGenerator<Unity.VisualScripting.WaitForFlow>
    {
        public WaitForFlowGenerator(Unity.VisualScripting.WaitForFlow unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return "/* Wait for flow Not supported */";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return "/* Wait for flow Not supported */";
        }

        public override string GenerateValue(ValueInput input)
        {
            return "/* Wait for flow Not supported */";
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.WaitForNextFrameUnit))]
    public sealed class WaitForNextFrameUnitGenerator : NodeGenerator<Unity.VisualScripting.WaitForNextFrameUnit>
    {
        public WaitForNextFrameUnitGenerator(Unity.VisualScripting.WaitForNextFrameUnit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            output += CodeBuilder.Indent(indent) + "yield return ".ControlHighlight() + "null".ConstructHighlight() + ";\n";

            if (Unit.exit.hasValidConnection)
            {
                output += (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent);
            }

            return output;
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.WaitForSecondsUnit))]
    public class WaitForSecondsUnitGenerator : NodeGenerator<WaitForSecondsUnit>
    {
        public WaitForSecondsUnitGenerator(WaitForSecondsUnit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            if (!Unit.unscaledTime.hasValidConnection)
            {
                if (!GenerateValue(Unit.unscaledTime).Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    output += CodeBuilder.Indent(indent) + "yield return ".ControlHighlight() + "new ".ConstructHighlight() + "WaitForSeconds(" + GenerateValue(Unit.seconds) + ");\n";
                }
                else
                {
                    output += CodeBuilder.Indent(indent) + "yield return ".ControlHighlight() + "new ".ConstructHighlight() + "WaitForSecondsRealtime(" + GenerateValue(Unit.seconds) + ");\n";
                }

                output += "\n";

                if (Unit.exit.hasValidConnection)
                {
                    output += (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent);
                }

                return output;
            }
            else
            {
                return CodeBuilder.Indent(indent) + "/* Use default value WaitForSeconds does not support connected value */";
            }
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                return $"/* {input.key} Requires Input */";
            }
        }

    }

    [NodeGenerator(typeof(Unity.VisualScripting.WaitUntilUnit))]
    public sealed class WaitUntilUnitGenerator : NodeGenerator<Unity.VisualScripting.WaitUntilUnit>
    {
        public WaitUntilUnitGenerator(Unity.VisualScripting.WaitUntilUnit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            if (Unit.condition.hasValidConnection)
            {
                output += CodeBuilder.Indent(indent) + $"yield return ".ControlHighlight() + "new ".ConstructHighlight() + "WaitUntil(() => {GenerateValue(Unit.condition)}; \n";
            }
            else
            {
                return CodeBuilder.Indent(indent) + "/*WaitUntil requires condition*/";
            }

            if (Unit.exit.hasValidConnection)
            {
                output += (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent);
            }

            return output;
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (Unit.condition.hasValidConnection)
            {
                return (Unit.condition.connection.source.unit as Unit).GenerateValue(Unit.condition.connection.source);
            }
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.WaitWhileUnit))]
    public sealed class WaitWhileUnitGenerator : NodeGenerator<Unity.VisualScripting.WaitWhileUnit>
    {
        public WaitWhileUnitGenerator(Unity.VisualScripting.WaitWhileUnit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            if (Unit.condition.hasAnyConnection)
            {
                output += CodeBuilder.Indent(indent) + $"yield return ".ControlHighlight() + "new ".ConstructHighlight() + "WaitWhile(() => {GenerateValue(Unit.condition)};\n";
            }
            else
            {
                return CodeBuilder.Indent(indent) + "/*WaitWhile requires condition*/";
            }

            if (Unit.exit.hasValidConnection)
            {
                output += (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent);
            }

            return output;
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (Unit.condition.hasValidConnection)
            {
                return (Unit.condition.connection.source.unit as Unit).GenerateValue(Unit.condition.connection.source);
            }
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(GetVariable))]
    public sealed class GetVariableGenerator : NodeGenerator<GetVariable>
    {
        public GetVariableGenerator(GetVariable unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            if (output == Unit.value)
            {
                foreach (var connection in output.connections)
                {
                    if (connection.destination is ValueInput)
                    {
                        return GenerateValue(Unit.name).VariableHighlight();
                    }
                }
            }

            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input == Unit.name)
            {
                if (Unit.name.hasValidConnection)
                {
                    return (Unit.name.connection.source.unit as Unit).GenerateValue(Unit.name.connection.source).Replace($@"""", "");
                }
                else
                {
                    return Unit.defaultValues[input.key].ToString();
                }
            }

            return base.GenerateValue(input);
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.IsVariableDefined))]
    public sealed class IsVariableDefinedGenerator : NodeGenerator<Unity.VisualScripting.IsVariableDefined>
    {
        public IsVariableDefinedGenerator(Unity.VisualScripting.IsVariableDefined unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.GetApplicationVariable))]
    public sealed class GetApplicationVariableGenerator : NodeGenerator<Unity.VisualScripting.GetApplicationVariable>
    {
        public GetApplicationVariableGenerator(Unity.VisualScripting.GetApplicationVariable unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.GetGraphVariable))]
    public sealed class GetGraphVariableGenerator : NodeGenerator<Unity.VisualScripting.GetGraphVariable>
    {
        public GetGraphVariableGenerator(Unity.VisualScripting.GetGraphVariable unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.GetObjectVariable))]
    public sealed class GetObjectVariableGenerator : NodeGenerator<Unity.VisualScripting.GetObjectVariable>
    {
        public GetObjectVariableGenerator(Unity.VisualScripting.GetObjectVariable unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.GetSavedVariable))]
    public sealed class GetSavedVariableGenerator : NodeGenerator<Unity.VisualScripting.GetSavedVariable>
    {
        public GetSavedVariableGenerator(Unity.VisualScripting.GetSavedVariable unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.GetSceneVariable))]
    public sealed class GetSceneVariableGenerator : NodeGenerator<Unity.VisualScripting.GetSceneVariable>
    {
        public GetSceneVariableGenerator(Unity.VisualScripting.GetSceneVariable unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.IsApplicationVariableDefined))]
    public sealed class IsApplicationVariableDefinedGenerator : NodeGenerator<Unity.VisualScripting.IsApplicationVariableDefined>
    {
        public IsApplicationVariableDefinedGenerator(Unity.VisualScripting.IsApplicationVariableDefined unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.IsGraphVariableDefined))]
    public sealed class IsGraphVariableDefinedGenerator : NodeGenerator<Unity.VisualScripting.IsGraphVariableDefined>
    {
        public IsGraphVariableDefinedGenerator(Unity.VisualScripting.IsGraphVariableDefined unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.IsObjectVariableDefined))]
    public sealed class IsObjectVariableDefinedGenerator : NodeGenerator<Unity.VisualScripting.IsObjectVariableDefined>
    {
        public IsObjectVariableDefinedGenerator(Unity.VisualScripting.IsObjectVariableDefined unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.IsSavedVariableDefined))]
    public sealed class IsSavedVariableDefinedGenerator : NodeGenerator<Unity.VisualScripting.IsSavedVariableDefined>
    {
        public IsSavedVariableDefinedGenerator(Unity.VisualScripting.IsSavedVariableDefined unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.IsSceneVariableDefined))]
    public sealed class IsSceneVariableDefinedGenerator : NodeGenerator<Unity.VisualScripting.IsSceneVariableDefined>
    {
        public IsSceneVariableDefinedGenerator(Unity.VisualScripting.IsSceneVariableDefined unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.SetApplicationVariable))]
    public sealed class SetApplicationVariableGenerator : NodeGenerator<Unity.VisualScripting.SetApplicationVariable>
    {
        public SetApplicationVariableGenerator(Unity.VisualScripting.SetApplicationVariable unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(SetGraphVariable))]
    public sealed class SetGraphVariableGenerator : NodeGenerator<SetGraphVariable>
    {
        private static HashSet<string> declaredVariables = new HashSet<string>();

        public SetGraphVariableGenerator(SetGraphVariable unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            if (input == Unit.assign)
            {
                if (!declaredVariables.Contains(GenerateValue(Unit.name)))
                {
                    declaredVariables.Add(GenerateValue(Unit.name));
                    output += "\n";
                    output += CodeBuilder.Indent(indent) + $"var {Unit.name}".ConstructHighlight() + $" = {GenerateValue(Unit.input)};";
                    output += "\n";
                }
                else
                {
                    output += "\n";
                    output += CodeBuilder.Indent(indent) + $"{Unit.name}".ConstructHighlight() + $" = {GenerateValue(Unit.input)};";
                    output += "\n";
                }
            }

            return output;
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input == Unit.name || input == Unit.input)
            {
                if (input == Unit.name)
                {
                    if (Unit.name.hasValidConnection)
                    {
                        return (Unit.name.connection.source.unit as Unit).GenerateValue(Unit.name.connection.source);
                    }
                    else
                    {
                        return Unit.defaultValues[input.key].ToString();
                    }
                }
                else if (input == Unit.input)
                {
                    return (Unit.input.connection.source.unit as Unit).GenerateValue(Unit.input.connection.source);
                }
            }

            return base.GenerateValue(input);
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.SetObjectVariable))]
    public sealed class SetObjectVariableGenerator : NodeGenerator<Unity.VisualScripting.SetObjectVariable>
    {
        public SetObjectVariableGenerator(Unity.VisualScripting.SetObjectVariable unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.SetSavedVariable))]
    public sealed class SetSavedVariableGenerator : NodeGenerator<Unity.VisualScripting.SetSavedVariable>
    {
        public SetSavedVariableGenerator(Unity.VisualScripting.SetSavedVariable unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.SetSceneVariable))]
    public sealed class SetSceneVariableGenerator : NodeGenerator<Unity.VisualScripting.SetSceneVariable>
    {
        public SetSceneVariableGenerator(Unity.VisualScripting.SetSceneVariable unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.SaveVariables))]
    public sealed class SaveVariablesGenerator : NodeGenerator<Unity.VisualScripting.SaveVariables>
    {
        public SaveVariablesGenerator(Unity.VisualScripting.SaveVariables unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
        }


    }

    [NodeGenerator(typeof(Unity.VisualScripting.SetVariable))]
    public sealed class SetVariableGenerator : NodeGenerator<Unity.VisualScripting.SetVariable>
    {
        public SetVariableGenerator(Unity.VisualScripting.SetVariable unit) : base(unit) { }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            if (input == Unit.assign)
            {
                var varName = GenerateValue(Unit.name);
                var varValue = GenerateValue(Unit.input);

                if (!data.localNames.Contains(varName))
                {
                    data.AddLocalName(varName);
                    output += CodeBuilder.Indent(indent) + ((Unit.input.hasValidConnection) ? (Unit.kind == VariableKind.Flow ? "var".ConstructHighlight() : Unit.input.connection.source.type.As().CSharpName(false, false, true).TypeHighlight()) + " " + GenerateValue(Unit.name).VariableHighlight() + " = " + GenerateValue(Unit.input) + ";\n" : "/* Set variable requires value */ \n");
                }
                else
                {
                    output += CodeBuilder.Indent(indent) + $"{varName}".VariableHighlight() + $" = {varValue};\n";
                }
                output += (Unit.assigned.hasValidConnection) ? (Unit.assigned.connection.destination.unit as Unit).GenerateControl(Unit.assigned.connection.destination, data, indent) : string.Empty;
                return output;
            }

            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            if (output == Unit.output && output.hasValidConnection)
            {
                var varName = GenerateValue(Unit.name);
                return string.IsNullOrEmpty(varName) ? string.Empty : varName.VariableHighlight();
            }

            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input == Unit.name)
            {
                if (input.hasValidConnection)
                {
                    return (Unit.name.connection.source.unit as Unit).GenerateValue(Unit.name.connection.source);
                }
                else
                {
                    if (Unit.defaultValues[input.key].ToString() != string.Empty)
                    {
                        return Unit.defaultValues[input.key].ToString();
                    }
                    else
                    {
                        return "null".ConstructHighlight();
                    }
                }
            }
            else if (input == Unit.input)
            {
                return input.hasValidConnection ? (Unit.input.connection.source.unit as Unit).GenerateValue(Unit.input.connection.source) : "null";
            }

            return base.GenerateValue(input);
        }
    }


    [NodeGenerator(typeof(Unity.VisualScripting.SubgraphUnit))]
    public sealed class SubgraphUnitGenerator : NodeGenerator<Unity.VisualScripting.SubgraphUnit>
    {
        public SubgraphUnitGenerator(Unity.VisualScripting.SubgraphUnit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            var graphinput = Unit.nest.graph.units.FirstOrDefault(unit => unit is GraphInput) as Unit;
            var graphOutput = Unit.nest.graph.units.FirstOrDefault(unit => unit is GraphOutput) as Unit;
            var subgraphName = Unit.nest.graph.title.Length > 0 ? Unit.nest.graph.title : "UnnamedSubgraph";
            if (CSharpPreview.ShowSubgraphComment)
            {
                if (graphinput != null || graphOutput != null)
                {
                    output += "\n" + CodeBuilder.Indent(indent) + $"//Subgraph: \"{subgraphName}\"_Port({input.key}) \n".CommentHighlight();
                }
                else
                {
                    output += "\n" + CodeBuilder.Indent(indent) + $"/* Subgraph \"{subgraphName}\" is empty */ \n";
                }
            }

            var control = Unit.controlInputs[input.key];


            if (input.hasValidConnection)
            {
                if (graphinput != null)
                {
                    var _output = graphinput.controlOutputs[input.key];
                    output += CodeBuilder.Indent(indent) + ((_output.hasValidConnection) ? (_output.connection.destination.unit as Unit).GenerateControl(_output.connection.destination, data, indent) : string.Empty);
                }
            }


            var connectedGraphOutputs = new List<ControlOutput>();

            foreach (var _output in Unit.controlOutputs)
            {
                if (_output.hasValidConnection)
                {
                    connectedGraphOutputs.Add(_output);
                }
            }

            if (graphOutput != null) GetSingleDecorator(graphOutput, graphOutput).connectedGraphOutputs = connectedGraphOutputs;

            var connectedValueInputs = new List<ValueInput>();

            foreach (var _valueinput in Unit.valueInputs)
            {
                if (_valueinput.hasValidConnection || _valueinput.hasDefaultValue)
                {
                    connectedValueInputs.Add(_valueinput);
                }
            }

            if (graphinput != null) GetSingleDecorator(graphinput, graphinput).connectedValueInputs = connectedValueInputs;

            return output;
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                return $"/* {input.key} Requires Input */";
            }
        }

        public override string GenerateValue(ValueOutput output)
        {
            var graphOutput = Unit.nest.graph.units.FirstOrDefault(unit => unit is GraphOutput) as Unit;

            return GetSingleDecorator(graphOutput, graphOutput).GenerateValue(output);
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.Community.BetterIf))]
    public sealed class BetterIfGenerator : NodeGenerator<Unity.VisualScripting.Community.BetterIf>
    {
        public BetterIfGenerator(Unity.VisualScripting.Community.BetterIf unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            if (input == Unit.In)
            {
                output += CodeBuilder.Indent(indent) + "if".ConstructHighlight() + " (" + GenerateValue(Unit.Condition) + ")";
                output += "\n";
                output += CodeBuilder.OpenBody(indent);
                output += "\n";
                output += (Unit.True.hasAnyConnection ? (Unit.True.connection.destination.unit as Unit).GenerateControl(Unit.True.connection.destination, data, indent + 1) : string.Empty);
                output += "\n";
                output += CodeBuilder.CloseBody(indent);

                if (Unit.False.hasAnyConnection)
                {
                    output += "\n";
                    output += CodeBuilder.Indent(indent) + "else".ConstructHighlight();
                    output += "\n";
                    output += CodeBuilder.OpenBody(indent);
                    output += "\n";

                    output += (Unit.False.hasAnyConnection ? (Unit.False.connection.destination.unit as Unit).GenerateControl(Unit.False.connection.destination, data, indent + 1) : string.Empty);
                    output += "\n";
                    output += CodeBuilder.CloseBody(indent);
                }

                if (Unit.Finished.hasAnyConnection)
                {
                    output += "\n";
                    output += (Unit.Finished.connection.destination.unit as Unit).GenerateControl(Unit.Finished.connection.destination, data, indent);
                    output += "\n";
                }
            }

            return output;
        }
        public override string GenerateValue(ValueInput input)
        {
            if (input == Unit.Condition)
            {
                if (Unit.Condition.hasAnyConnection)
                {
                    return (Unit.Condition.connection.source.unit as Unit).GenerateValue(Unit.Condition.connection.source);
                }
            }

            return base.GenerateValue(input);
        }
    }

    [NodeGenerator(typeof(LogNode))]
    public sealed class LogNodeGenerator : NodeGenerator<LogNode>
    {
        public LogNodeGenerator(LogNode unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            if (input == Unit.input)
            {
                output += $"Debug.Log({GenerateValue(Unit.format)});";
                output += (Unit.output.hasAnyConnection ? (Unit.output.connection.destination.unit as Unit).GenerateControl(Unit.output.connection.destination, data, indent) : string.Empty);
            }

            return CodeBuilder.Indent(indent) + output;
        }
        public override string GenerateValue(ValueInput input)
        {
            if (input == Unit.format)
            {
                if (Unit.format.hasAnyConnection)
                {
                    return (Unit.format.connection.source.unit as Unit).GenerateValue(Unit.format.connection.source);
                }
                else if (input.hasDefaultValue)
                {
                    var value = Unit.defaultValues[input.key].As().Code(false);
                }
            }

            return base.GenerateValue(input);
        }
    }

    [NodeGenerator(typeof(FunctionNode))]
    public sealed class FunctionNodeGenerator : NodeGenerator<FunctionNode>
    {
        public FunctionNodeGenerator(FunctionNode unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.invoke.connection?.destination;
            if (!Unit.invoke.hasAnyConnection)
                return "\n";

            return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit)
                .GenerateControl(destination, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            return output.key;
        }
    }

    [NodeGenerator(typeof(GeneratedUnit))]
    public class GeneratedUnitGenerator : NodeGenerator<GeneratedUnit>
    {
        public GeneratedUnitGenerator(GeneratedUnit unit) : base(unit)
        {
            NameSpace = Unit.NameSpace;
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var generatedCode = string.Empty;

            var generatorLogicMethod = unit.GetType().GetMethod("GeneratorLogic", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (generatorLogicMethod != null)
            {
                var _data = new object[] { indent };
                var methodResult = generatorLogicMethod.Invoke(unit, _data) as string;
                generatedCode += methodResult;
            }

            if (Unit.Exit != null)
            {
                generatedCode += (Unit.Exit.hasAnyConnection ? (Unit.Exit.connection.destination.unit as Unit).GenerateControl(Unit.Exit.connection.destination, data, indent) : string.Empty);
            }

            return CodeBuilder.Indent(indent) + generatedCode;
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return (input.connection.source.unit as Unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                return unit.defaultValues[input.key].ToString();
            }
            else
            {
                return $"/*{input.key} Requires Input*/";
            }
        }

        public override string GenerateValue(ValueOutput output)
        {
            if (output.hasValidConnection)
            {
                var generatedCode = string.Empty;
                var generatorLogicMethod = unit.GetType().GetMethod("GeneratorOutput", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (generatorLogicMethod != null)
                {
                    object[] data = new object[1];
                    data[0] = output;
                    var methodResult = generatorLogicMethod.Invoke(unit, data) as string;
                    generatedCode += methodResult;
                }

                return generatedCode;
            }
            else
            {
                return string.Empty;
            }
        }
    }

    [NodeGenerator(typeof(Return))]
    public class ReturnUnitGenerator : NodeGenerator<Return>
    {
        public ReturnUnitGenerator(Return unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return CodeBuilder.Indent(indent) + $"return".ControlHighlight() + $" {GenerateValue(Unit.Data)};";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return (input.connection.source.unit as Unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                return unit.defaultValues[input.key].ToString();
            }
            else
            {
                return $"/*{input.key} Requires Input*/";
            }
        }
    }

    [NodeGenerator(typeof(ActionNode))]
    public class ActionNodeGenerator : NodeGenerator<ActionNode>
    {
        public ActionNodeGenerator(ActionNode unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output)
        {
            if (output == Unit.@delegate)
            {
                var parameters = string.Empty;

                if (Unit.parameters.Count > 0)
                {
                    var count = 0;
                    parameters += "<";
                    foreach (var _param in Unit.parameters)
                    {
                        parameters += _param.type.ToString();

                        if (count != Unit.parameters.Count - 1)
                        {
                            parameters += ", ";
                        }
                    }
                    parameters += ">";
                }
                return $"new {Unit._delegate.DisplayName}{parameters}()";
            }

            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return (input.connection.source.unit as Unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                return unit.defaultValues[input.key].ToString();
            }
            else
            {
                return $"/*{input.key} Requires Input*/";
            }
        }
    }

    [NodeGenerator(typeof(YieldReturn))]
    public class YieldReturnGenerator : NodeGenerator<YieldReturn>
    {
        public YieldReturnGenerator(YieldReturn unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return CodeBuilder.Indent(indent) + $"yield return".ControlHighlight() + $" {GenerateValue(Unit.value)};";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return (input.connection.source.unit as Unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                return unit.defaultValues[input.key].ToString();
            }
            else
            {
                return $"/*{input.key} Requires Input*/";
            }
        }
    }

    [NodeGenerator(typeof(NegativeValueNode))]
    public class NegativeValueNodeGenerator : NodeGenerator<NegativeValueNode>
    {
        public NegativeValueNodeGenerator(NegativeValueNode unit) : base(unit)
        {
        }

        public override string GenerateValue(ValueOutput output)
        {
            switch (Unit.type)
            {
                case NegateType.Float: return $"-{GenerateValue(Unit.Float)}";
                case NegateType.Int: return $"-{GenerateValue(Unit.Int)}";
                case NegateType.Vector2: return $"-{GenerateValue(Unit.Vector2)}";
                case NegateType.Vector3: return $"-{GenerateValue(Unit.Vector3)}";
            }
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }
    }

    [NodeGenerator(typeof(Cast))]
    public class CastGenerator : NodeGenerator<Cast>
    {
        public CastGenerator(Cast unit) : base(unit)
        {
            NameSpace = Unit.CastType.Namespace;
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"({Unit.CastType.As().CSharpName(false).TypeHighlight()}){GenerateValue(Unit.value)}";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                // Handle the case where the input requires an input (error message or throw exception)
                return $"/* {input.key} Requires Input */";
            }
        }
    }

    [NodeGenerator(typeof(AsUnit))]
    public class AsGenerator : NodeGenerator<AsUnit>
    {
        public AsGenerator(AsUnit unit) : base(unit)
        {
            NameSpace = Unit.AsType.Namespace;
        }

        public override string GenerateValue(ValueOutput output)
        {
            return $"({GenerateValue(Unit.value)}" + " as ".ConstructHighlight() + $"{Unit.AsType.As().CSharpName(false).TypeHighlight()})";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                // If the input has a valid connection, generate code for it
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                // If there's a default value, use it
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                return $"/* {input.key} Requires Input ";
            }
        }
    }

    [NodeGenerator(typeof(AssignValueInput))]
    public class AssignValueInputGenerator : NodeGenerator<AssignValueInput>
    {
        public AssignValueInputGenerator(AssignValueInput unit) : base(unit)
        {
            NameSpace = Unit.VariableType.Namespace;
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            bool nullmeansSelf = false;

            if (Unit.VariableType == typeof(GameObject))
            {
                nullmeansSelf = bool.Parse(GenerateValue(Unit.NullMeansSelf).RemoveHighlights().RemoveMarkdown());
            }

            if (Unit.Input.hasValidConnection)
            {
                var output = string.Empty;
                string _default = Unit.DefaultValue ? Unit.Default.hasValidConnection || Unit.Default.hasDefaultValue ? $", {GenerateValue(Unit.Default)}" : ", default" : "";

                output += (Unit.Exit.hasValidConnection) ? (Unit.Exit.connection.destination.unit as Unit).GenerateControl(Unit.Exit.connection.destination, data, indent) : string.Empty;

                string assign = $"{GenerateValue(Unit.Input)} = ValueInput" + $"{(!Unit.DefaultValue || !Unit.Default.hasDefaultValue || Unit.VariableType == typeof(GameObject) ? "<" + Unit.VariableType.As().CSharpName(false, false, true) + ">" : string.Empty)}" + $"(nameof({GenerateValue(Unit.Input)}){_default})" + (nullmeansSelf ? "" : ";");
                string shouldmeanSelf = nullmeansSelf ? $".NullMeansSelf();" : "";
                return assign + shouldmeanSelf + "\n" + output;
            }
            else
            {
                var output = string.Empty;
                output += (Unit.Exit.hasValidConnection) ? (Unit.Exit.connection.destination.unit as Unit).GenerateControl(Unit.Exit.connection.destination, data, indent) : string.Empty;
                return "/* Requires Value Input */\n" + output;
            }


        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                return $"/* {input.key} Requires Input ";
            }
        }
    }

    [NodeGenerator(typeof(AssignControlInput))]
    public class AssignControlInputGenerator : NodeGenerator<AssignControlInput>
    {
        public AssignControlInputGenerator(AssignControlInput unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;
            if (Unit._controlInput.hasValidConnection)
            {
                if (Unit.Exit.hasValidConnection)
                {
                    output += (Unit.Exit.connection.destination.unit as Unit).GenerateControl(Unit.Exit.connection.destination, data, indent);
                }

                if (!Unit.Coroutine)
                {
                    return $"{GenerateValue(Unit._controlInput)} = ControlInput(nameof({GenerateValue(Unit._controlInput)}), {(GenerateValue(Unit.MethodName).Length == 0 ? "/* Requires Method Name */" : $"{GenerateValue(Unit.MethodName)}")}); \n" + output;
                }
                else
                {
                    if (string.IsNullOrEmpty(GenerateValue(Unit.MethodName)) || string.IsNullOrWhiteSpace(GenerateValue(Unit.MethodName)))
                    {
                        return $"{GenerateValue(Unit._controlInput)} = ControlInputCoroutine(nameof({GenerateValue(Unit._controlInput)}), {(GenerateValue(Unit.CoroutineMethodName).Length == 0 ? "/* Requires CoroutineMethod Name */" : $"{GenerateValue(Unit.CoroutineMethodName)}")}); \n" + output;
                    }
                    else
                    {
                        return $"{GenerateValue(Unit._controlInput)} = ControlInputCoroutine(nameof({GenerateValue(Unit._controlInput)}), {GenerateValue(Unit.MethodName)}, {(GenerateValue(Unit.CoroutineMethodName).Length == 0 ? "/* Requires CoroutineMethod Name */" : $"{GenerateValue(Unit.CoroutineMethodName)}")}); \n" + output;
                    }
                }
            }
            else
            {
                if (Unit.Exit.hasValidConnection)
                {
                    output += (Unit.Exit.connection.destination.unit as Unit).GenerateControl(Unit.Exit.connection.destination, data, indent);
                }
                return "/* Requires Control Input */\n" + output;
            }
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                return unit.defaultValues[input.key].ToString();
            }
            else
            {
                return $"/* {input.key} Requires Input ";
            }
        }
    }

    [NodeGenerator(typeof(AssignControlOutput))]
    public class AssignControlOutputGenerator : NodeGenerator<AssignControlOutput>
    {
        public AssignControlOutputGenerator(AssignControlOutput unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;
            if (Unit.controlOutput.hasValidConnection)
            {
                if (Unit.Exit.hasValidConnection)
                {
                    output += (Unit.Exit.connection.destination.unit as Unit).GenerateControl(Unit.Exit.connection.destination, data, indent);
                }

                return $"{GenerateValue(Unit.controlOutput)} = ControlOutput(nameof({GenerateValue(Unit.controlOutput)})); \n" + output;
            }
            else
            {
                if (Unit.Exit.hasValidConnection)
                {
                    output += (Unit.Exit.connection.destination.unit as Unit).GenerateControl(Unit.Exit.connection.destination, data, indent);
                }
                return "/* Requires Control Input */\n" + output;
            }
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                return unit.defaultValues[input.key].As().Code(false, true, true, "");
            }
            else
            {
                return $"/* {input.key} Requires Input ";
            }
        }
    }

    [NodeGenerator(typeof(AssignValueOutput))]
    public class AssignValueOutputGenerator : NodeGenerator<AssignValueOutput>
    {
        public AssignValueOutputGenerator(AssignValueOutput unit) : base(unit)
        {
            NameSpace = Unit.VariableType.Namespace;
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            if (Unit.valueOutput.hasValidConnection)
            {
                var output = string.Empty;
                output += (Unit.Exit.hasValidConnection) ? (Unit.Exit.connection.destination.unit as Unit).GenerateControl(Unit.Exit.connection.destination, data, indent) : string.Empty;
                return $"{GenerateValue(Unit.valueOutput)} = ValueOutput<{Unit.VariableType.As().CSharpName(false, false, true)}>(nameof({GenerateValue(Unit.valueOutput)}){(Unit.triggersMethod ? $", {GenerateValue(Unit.MethodName)}" : string.Empty)}); \n" + output;
            }
            else
            {
                var output = string.Empty;
                output += (Unit.Exit.hasValidConnection) ? (Unit.Exit.connection.destination.unit as Unit).GenerateControl(Unit.Exit.connection.destination, data, indent) : string.Empty;
                return "/* Requires Value Input */\n" + output;
            }
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                return unit.defaultValues[input.key].ToString();
            }
            else
            {
                return $"/* {input.key} Requires Input ";
            }
        }
    }

    [NodeGenerator(typeof(Assignment))]
    public class AssignmentGenerator : NodeGenerator<Unity.VisualScripting.Community.Assignment>
    {
        public AssignmentGenerator(Assignment unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            if (Unit.Input.hasValidConnection && Unit.Output.hasValidConnection)
            {
                var output = string.Empty;
                output += (Unit.Exit.hasValidConnection) ? (Unit.Exit.connection.destination.unit as Unit).GenerateControl(Unit.Exit.connection.destination, data, indent) : string.Empty;
                return $"Assignment({GenerateValue(Unit.Input)}, {GenerateValue(Unit.Output)}); \n" + output;
            }
            else
            {
                var output = string.Empty;
                output += (Unit.Exit.hasValidConnection) ? (Unit.Exit.connection.destination.unit as Unit).GenerateControl(Unit.Exit.connection.destination, data, indent) : string.Empty;
                return "/* Missing Inputs */\n" + output;
            }
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                return unit.defaultValues[input.key].As().Code(false, true, true);
            }
            else
            {
                return $"/* {input.key} Requires Input ";
            }
        }
    }

    [NodeGenerator(typeof(Succession))]
    public class SuccessionGenerator : NodeGenerator<Unity.VisualScripting.Community.Succession>
    {
        public SuccessionGenerator(Succession unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            if (Unit.Input.hasValidConnection && Unit.Output.hasValidConnection)
            {
                var output = string.Empty;
                output += (Unit.Exit.hasValidConnection) ? (Unit.Exit.connection.destination.unit as Unit).GenerateControl(Unit.Exit.connection.destination, data, indent) : string.Empty;
                return $"Succession({GenerateValue(Unit.Input)}, {GenerateValue(Unit.Output)}); \n" + output;
            }
            else
            {
                var output = string.Empty;
                output += (Unit.Exit.hasValidConnection) ? (Unit.Exit.connection.destination.unit as Unit).GenerateControl(Unit.Exit.connection.destination, data, indent) : string.Empty;
                return "/* Missing Inputs */\n" + output;
            }
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                return unit.defaultValues[input.key].As().Code(false, true, true);
            }
            else
            {
                return $"/* {input.key} Requires Input ";
            }
        }
    }

    [NodeGenerator(typeof(Requirement))]
    public class RequirementGenerator : NodeGenerator<Unity.VisualScripting.Community.Requirement>
    {
        public RequirementGenerator(Requirement unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            if (Unit.Input.hasValidConnection && Unit._ControlInput.hasValidConnection)
            {
                var output = string.Empty;
                output += (Unit.Exit.hasValidConnection) ? (Unit.Exit.connection.destination.unit as Unit).GenerateControl(Unit.Exit.connection.destination, data, indent) : string.Empty;
                return $"Requirement({GenerateValue(Unit.Input)}, {GenerateValue(Unit._ControlInput)}); \n" + output;
            }
            else
            {
                var output = string.Empty;
                output += (Unit.Exit.hasValidConnection) ? (Unit.Exit.connection.destination.unit as Unit).GenerateControl(Unit.Exit.connection.destination, data, indent) : string.Empty;
                return "/* Missing Inputs */\n" + output;
            }
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                return unit.defaultValues[input.key].As().Code(false, true, true);
            }
            else
            {
                return $"/* {input.key} Requires Input ";
            }
        }
    }

    [NodeGenerator(typeof(YieldNode))]
    public class YieldNodeGenerator : NodeGenerator<YieldNode>
    {
        public YieldNodeGenerator(YieldNode unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            switch (Unit.type)
            {
                case YieldNode.EnumeratorType.YieldInstruction:
                    output += CodeBuilder.Indent(indent) + $"yield return ".ControlHighlight() + $"{GenerateValue(Unit.instruction)};";
                    break;
                case YieldNode.EnumeratorType.Enumerator:
                    output += CodeBuilder.Indent(indent) + $"yield return ".ControlHighlight() + $"{GenerateValue(Unit.enumerator)};";
                    break;
                case YieldNode.EnumeratorType.Coroutine:
                    output += CodeBuilder.Indent(indent) + $"yield return ".ControlHighlight() + $"{GenerateValue(Unit.coroutine)};";
                    break;
            }

            output += (Unit.exit.hasValidConnection) ? "\n" + (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent) : string.Empty;

            return output;
        }


        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                return unit.defaultValues[input.key].As().Code(false, true, true);
            }
            else
            {
                return $"/* {input.key} Requires Input */";
            }
        }
    }

    [NodeGenerator(typeof(QueryNode))]
    public class QueryNodeGenerator : NodeGenerator<QueryNode>
    {
        string returnString = "return".ControlHighlight();

        public QueryNodeGenerator(QueryNode unit) : base(unit)
        {
            NameSpace = "System.Linq";
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            switch (Unit.operation)
            {
                case QueryOperation.Any:
                    output += CodeBuilder.Indent(indent) + $"var ".ConstructHighlight() + "result ".VariableHighlight() + $"= {GenerateValue(Unit.collection)}" +
                              $".Any();";
                    break;
                case QueryOperation.AnyWithCondition:
                    output += CodeBuilder.Indent(indent) + $"var ".ConstructHighlight() + "result ".VariableHighlight() + $"= {GenerateValue(Unit.collection)}" +
                              $".Any({GenerateLambda(Unit.condition, data, indent)});";
                    break;
                case QueryOperation.First:
                    output += CodeBuilder.Indent(indent) + $"var ".ConstructHighlight() + "result ".VariableHighlight() + $"= {GenerateValue(Unit.collection)}" +
                              $".First({GenerateLambda(Unit.condition, data, indent)});";
                    break;
                case QueryOperation.FirstOrDefault:
                    output += CodeBuilder.Indent(indent) + $"var ".ConstructHighlight() + "result ".VariableHighlight() + $"= {GenerateValue(Unit.collection)}" +
                              $".FirstOrDefault({GenerateLambda(Unit.condition, data, indent)});";
                    break;
                case QueryOperation.OrderBy:
                    output += CodeBuilder.Indent(indent) + $"var ".ConstructHighlight() + "result ".VariableHighlight() + $"= {GenerateValue(Unit.collection)}" +
                              $".OrderBy({GenerateLambda(Unit.key, data, indent)});";
                    break;
                case QueryOperation.OrderByDescending:
                    output += CodeBuilder.Indent(indent) + $"var ".ConstructHighlight() + "result ".VariableHighlight() + $"= {GenerateValue(Unit.collection)}" +
                              $".OrderByDescending({GenerateLambda(Unit.key, data, indent)});";
                    break;
                case QueryOperation.Single:
                    output += CodeBuilder.Indent(indent) + $"var ".ConstructHighlight() + "result ".VariableHighlight() + $"= {GenerateValue(Unit.collection)}" +
                              $".Single({GenerateLambda(Unit.condition, data, indent)});";
                    break;
                case QueryOperation.Where:
                    output += CodeBuilder.Indent(indent) + $"var ".ConstructHighlight() + "result ".VariableHighlight() + $"= {GenerateValue(Unit.collection)}" +
                              $".Where({GenerateLambda(Unit.condition, data, indent)});";
                    break;
                default:
                    break;
            }

            output += (Unit.exit.hasValidConnection) ? "\n" + (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent) : string.Empty;

            return output;
        }

        public override string GenerateValue(ValueOutput output)
        {
            if (output == Unit.item)
            {
                return "x".VariableHighlight();
            }
            else if (output == Unit.result)
            {
                return "result".VariableHighlight();
            }

            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                return unit.defaultValues[input.key].As().Code(false, true, true);
            }
            else
            {
                return $"/* {input.key} Requires Input */";
            }
        }

        private string GenerateLambda(ValueInput predicateInput, ControlGenerationData data, int Indent)
        {
            var body = string.Empty;

            if (Unit.body.hasValidConnection)
            {
                body += GetSingleDecorator(Unit.body.connection.destination.unit as Unit, Unit.body.connection.destination.unit as Unit).GenerateControl(Unit.body.connection.destination, data, Indent + 1) + "\n";
            }

            return CodeBuilder.MultiLineLambda("x".VariableHighlight(), body + "\n" + CodeBuilder.Indent(Indent + 1) + returnString + " " + GenerateValue(predicateInput) + ";", Indent);
        }
    }

    [NodeGenerator(typeof(Unity.VisualScripting.Community.CreateDictionary))]
    public class CreateDictionaryGenerator : NodeGenerator<Unity.VisualScripting.Community.CreateDictionary>
    {
        public CreateDictionaryGenerator(Unity.VisualScripting.Community.CreateDictionary unit) : base(unit)
        {
            NameSpace = "System.Collections.Generic";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return "new ".ConstructHighlight() + "Dictionary".TypeHighlight() + $"<{Unit.KeyType.DisplayName().TypeHighlight()}, {Unit.ValueType.DisplayName().TypeHighlight()}>()";
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
            }
            else if (input.hasDefaultValue)
            {
                return unit.defaultValues[input.key].As().Code(false, true, true);
            }
            else
            {
                return $"/* {input.key} Requires Input */";
            }
        }
    }
}
