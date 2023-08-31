using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community.Generated
{
    public static class GeneratedGenerators
    {
        private static Dictionary<Type, Func<object, string>> DefaultValueFormatters = new Dictionary<Type, Func<object, string>>
            {
                { typeof(float), value => $"{value}f".NumericHighlight() },
                { typeof(string), value => $"\"{value}\"".StringHighlight() },
                { typeof(bool), value => value.ToString().ToLower().ConstructHighlight() },
                { typeof(Color), value => "new".ConstructHighlight() + " Color".ConstructHighlight() + $"{value.ToString().Replace("RGBA", "")}" }
            };

        [NodeGenerator(typeof(Unity.VisualScripting.CreateStruct))]
        public sealed class CreateStructGenerator : NodeGenerator<Unity.VisualScripting.CreateStruct>
        {
            public CreateStructGenerator(Unity.VisualScripting.CreateStruct unit) : base(unit)
            {
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {

                var output = string.Empty;
                output += (Unit.exit.hasValidConnection ? (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent) : string.Empty);
                return output;
               
            }

            public override string GenerateValue(ValueOutput output)
            {
                if (output == Unit.output)
                {
                    return $"new {Unit.type.CSharpFullName()}()";
                }

                return base.GenerateValue(output);
            }

            private string GenerateStructInstance()
            {
                var parameterValues = new List<string>();

                foreach (var parameter in Unit.valueInputs)
                {
                    var sourceUnit = parameter.connections.FirstOrDefault()?.source.unit as Unit;
                    var destinationValueInput = parameter.connections.FirstOrDefault()?.destination as ValueInput;

                    var generatedValue = sourceUnit.GenerateValue(destinationValueInput);
                    parameterValues.Add(generatedValue);
                }

                var parameterString = string.Join(", ", parameterValues);

                return $"new {Unit.type.CSharpFullName()}({GenerateArguments(Unit.valueInputs as List<ValueInput>)})";
            }

            private string GenerateArguments(List<ValueInput> arguments)
            {
                var argumentValues = arguments.Select(arg =>
                {
                    if (arg.hasValidConnection)
                    {
                        return GenerateValue(arg);
                    }
                    else if (arg.hasDefaultValue && DefaultValueFormatters.ContainsKey(Unit.defaultValues[arg.key].GetType()))
                    {
                        return DefaultValueFormatters[Unit.defaultValues[arg.key].GetType()](Unit.defaultValues[arg.key]);
                    }
                    else
                    {
                        return string.Empty;
                    }
                });
                return string.Join(", ", argumentValues);
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
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {
                return base.GenerateControl(input, data, indent);
            }

            public override string GenerateValue(ValueOutput output)
            {
                if (Unit.target != null)
                {
                    if (Unit.target.hasValidConnection)
                    {
                        var memberName = Unit.member.targetTypeName;

                        var outputCode = GenerateValue(Unit.target) + $".{memberName}";

                        return outputCode;
                    }
                }
                return CodeBuilder.WarningHighlight("/* Get Member Requires Input */");
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
                            return Unit.defaultValues[input.key].ToString();
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
            public InvokeMemberGenerator(Unity.VisualScripting.InvokeMember unit) : base(unit)
            {
            }

            public override string GenerateValue(ValueInput input)
            {
                if (input.hasValidConnection)
                {
                    return (input.connection.source.unit as Unit).GenerateValue(input.connection.source);
                }
                else if (input.hasDefaultValue)
                {
                    if (typeof(Type).IsAssignableFrom(Unit.defaultValues[input.key].GetType()))
                    {
                        var type = (Type)Unit.defaultValues[input.key];
                        return $"typeof({type.CSharpFullName()})";
                    }
                    else
                    {
                        return Unit.defaultValues[input.key].ToString(); ;
                    }
                }
                else
                {
                    return $"/* \"{input.key} Requires Input\"\" */";
                }
            }

            public override string GenerateValue(ValueOutput output)
            {
                if (output == Unit.result)
                {
                    if (output.hasValidConnection)
                    {
                        if (Unit.member.isConstructor)
                        {
                            return $"new {Unit.member.declaringType}({GenerateArguments(Unit.inputParameters.Values.ToList())})";
                        }
                        else
                        {
                            if (Unit.inputParameters.Count > 0)
                            {
                                string Output = string.Empty;

                                if (Unit.target != null)
                                {
                                    if (Unit.target.hasValidConnection)
                                    {
                                        Output += (Unit.target.connection.source.unit as Unit).GenerateValue(Unit.target.connection.source);
                                        Output += Unit.member.ToDeclarer().ToString().Remove(0, Unit.member.ToDeclarer().ToString().IndexOf(".")) + $"({GenerateArguments(Unit.inputParameters.Values.ToList())})";
                                        return Output;
                                    }
                                    else if (Unit.target.hasDefaultValue)
                                    {
                                        Output += $"\"{Unit.defaultValues[Unit.target.key]}\"";
                                        return Output;
                                    }
                                }

                                return $"{Unit.member.targetType.Namespace + "." + Unit.member.ToDeclarer()}({GenerateArguments(Unit.inputParameters.Values.ToList())})";
                            }
                        }
                    }
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
                    else if (arg.hasDefaultValue && DefaultValueFormatters.ContainsKey(Unit.defaultValues[arg.key].GetType()))
                    {
                        return DefaultValueFormatters[Unit.defaultValues[arg.key].GetType()](Unit.defaultValues[arg.key]);
                    }
                    else
                    {
                        return string.Empty;
                    }
                }); 
                return string.Join(", ", argumentValues);
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {
                if (input == Unit.enter)
                {
                    var output = string.Empty;

                    if (Unit.target != null)
                    {
                        output += (Unit.target.hasValidConnection ? GenerateValue(Unit.target) + Unit.member.ToDeclarer().ToString().Remove(0, Unit.member.ToDeclarer().ToString().IndexOf('.')) : Unit.member.ToPseudoDeclarer()) + $"({(GenerateArguments(Unit.inputParameters.Values.ToList()).Length > 0 ? GenerateArguments(Unit.inputParameters.Values.ToList()) : string.Empty)}); \n";
                    }
                    else 
                    {
                        output += Unit.member.targetType.Namespace+ "." + Unit.member.ToDeclarer() + $"({(GenerateArguments(Unit.inputParameters.Values.ToList()).Length > 0 ? GenerateArguments(Unit.inputParameters.Values.ToList()) : string.Empty)}); \n";
                    }
                    output += (Unit.exit.hasAnyConnection ? (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent) : string.Empty);

                    return output;
                }

                return base.GenerateControl(input, data, indent);
            }
        }


        [NodeGenerator(typeof(Unity.VisualScripting.SetMember))]
        public sealed class SetMemberGenerator : NodeGenerator<Unity.VisualScripting.SetMember>
        {
            public SetMemberGenerator(Unity.VisualScripting.SetMember unit) : base(unit)
            {
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {
                var output = string.Empty;

                if (input == Unit.assign)
                {

                    var value = GenerateValue(Unit.input);

                    output += CodeBuilder.Indent(indent) + $"{GenerateValue(Unit.target)}{Unit.member.ToPseudoDeclarer().ToString().Remove(0, Unit.member.ToDeclarer().ToString().IndexOf('.'))} = {value};\n";

                    output += (Unit.assigned.hasAnyConnection ? (Unit.assigned.connection.destination.unit as Unit).GenerateControl(Unit.assigned.connection.destination, data, indent) : string.Empty);
                }

                return output;
            }

            public override string GenerateValue(ValueOutput output)
            {
                if (output == Unit.output)
                {
                    if (Unit.output.hasValidConnection && Unit.target.hasValidConnection)
                    {
                        return $"{Unit.member}";
                    }
                    else
                    {
                        if (Unit.target.hasDefaultValue)
                        {
                            return $"{Unit.member}";
                        }
                        else
                        {
                            return "/*Set Member target Requires Input*/";
                        }
                    }
                }
                else if (output == Unit.targetOutput)
                {
                    if (Unit.targetOutput.hasValidConnection)
                    {
                        return GenerateValue(Unit.target);
                    }
                }

                return base.GenerateValue(output);
            }

            public override string GenerateValue(ValueInput input)
            {
                if (input == Unit.target)
                {
                    if (Unit.target.hasValidConnection)
                    {
                        return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
                    }
                    else
                    {
                        if (Unit.target.hasDefaultValue)
                        {
                            return $"{DefaultValueFormatters[Unit.defaultValues[input.key].GetType()](Unit.defaultValues[input.key])}".Contains("new") ? $"{DefaultValueFormatters[Unit.defaultValues[input.key].GetType()](Unit.defaultValues[input.key])}" : $"new {DefaultValueFormatters[Unit.defaultValues[input.key].GetType()](Unit.defaultValues[input.key])}";
                        }
                        else
                        {
                            return "/*Set Member target Requires Input*/";
                        }
                    }
                }
                else if (input == Unit.input)
                {
                    if (Unit.input.hasValidConnection)
                    {
                        return ((Unit)input.connection.source.unit).GenerateValue(input.connection.source);
                    }
                    else if(Unit.input.hasDefaultValue)
                    {
                        return DefaultValueFormatters[Unit.defaultValues[input.key].GetType()](Unit.defaultValues[input.key]);
                    }
                }

                return base.GenerateValue(input);
            } 
        }


        [NodeGenerator(typeof(Unity.VisualScripting.CountItems))]
        public sealed class CountItemsGenerator : NodeGenerator<Unity.VisualScripting.CountItems>
        {
            private string list;
            public CountItemsGenerator(Unity.VisualScripting.CountItems unit) : base(unit)
            {
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {
                return base.GenerateControl(input, data, indent);
            }

            public override string GenerateValue(ValueOutput output)
            {
                if (output == Unit.count)
                {
                    var list = "System.Collections.Generic." + GenerateValue(Unit.collection);
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
                        // Generate code for the input value
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

        [NodeGenerator(typeof(Unity.VisualScripting.ClearDictionary))]
        public sealed class ClearDictionaryGenerator : NodeGenerator<Unity.VisualScripting.ClearDictionary>
        {
            public ClearDictionaryGenerator(Unity.VisualScripting.ClearDictionary unit) : base(unit)
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

        [NodeGenerator(typeof(Unity.VisualScripting.CreateDictionary))]
        public sealed class CreateDictionaryGenerator : NodeGenerator<Unity.VisualScripting.CreateDictionary>
        {
            public CreateDictionaryGenerator(Unity.VisualScripting.CreateDictionary unit) : base(unit)
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

        [NodeGenerator(typeof(Unity.VisualScripting.DictionaryContainsKey))]
        public sealed class DictionaryContainsKeyGenerator : NodeGenerator<Unity.VisualScripting.DictionaryContainsKey>
        {
            public DictionaryContainsKeyGenerator(Unity.VisualScripting.DictionaryContainsKey unit) : base(unit)
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

        [NodeGenerator(typeof(Unity.VisualScripting.GetDictionaryItem))]
        public sealed class GetDictionaryItemGenerator : NodeGenerator<Unity.VisualScripting.GetDictionaryItem>
        {
            public GetDictionaryItemGenerator(Unity.VisualScripting.GetDictionaryItem unit) : base(unit)
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

        [NodeGenerator(typeof(Unity.VisualScripting.MergeDictionaries))]
        public sealed class MergeDictionariesGenerator : NodeGenerator<Unity.VisualScripting.MergeDictionaries>
        {
            public MergeDictionariesGenerator(Unity.VisualScripting.MergeDictionaries unit) : base(unit)
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

        [NodeGenerator(typeof(Unity.VisualScripting.RemoveDictionaryItem))]
        public sealed class RemoveDictionaryItemGenerator : NodeGenerator<Unity.VisualScripting.RemoveDictionaryItem>
        {
            public RemoveDictionaryItemGenerator(Unity.VisualScripting.RemoveDictionaryItem unit) : base(unit)
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

        [NodeGenerator(typeof(Unity.VisualScripting.SetDictionaryItem))]
        public sealed class SetDictionaryItemGenerator : NodeGenerator<Unity.VisualScripting.SetDictionaryItem>
        {
            public SetDictionaryItemGenerator(Unity.VisualScripting.SetDictionaryItem unit) : base(unit)
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

        [NodeGenerator(typeof(Unity.VisualScripting.FirstItem))]
        public sealed class FirstItemGenerator : NodeGenerator<Unity.VisualScripting.FirstItem>
        {
            public FirstItemGenerator(Unity.VisualScripting.FirstItem unit) : base(unit)
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

        [NodeGenerator(typeof(Unity.VisualScripting.LastItem))]
        public sealed class LastItemGenerator : NodeGenerator<Unity.VisualScripting.LastItem>
        {
            public LastItemGenerator(Unity.VisualScripting.LastItem unit) : base(unit)
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

        [NodeGenerator(typeof(Unity.VisualScripting.AddListItem))]
        public sealed class AddListItemGenerator : NodeGenerator<Unity.VisualScripting.AddListItem>
        {
            public AddListItemGenerator(Unity.VisualScripting.AddListItem unit) : base(unit)
            {
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {
                var output = string.Empty;

                if (input == Unit.enter)
                {
                    var list = GenerateValue(Unit.listInput);

                    // Generate code to clear the list
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
            }

            public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
            {
                var output = string.Empty;

                if (input == Unit.enter)
                {
                    if (Unit.listInput.hasValidConnection)
                    {
                        var list = GenerateValue(Unit.listInput);

                        // Generate code to clear the list
                        output += CodeBuilder.Indent(indent) + $"{list}.Clear();\n";
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
                        var script = $"new List<object> {{ {string.Join(", ", connectedInputs)} }}";

                        return script;
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

    [NodeGenerator(typeof(Unity.VisualScripting.RemoveListItem))]
    public sealed class RemoveListItemGenerator : NodeGenerator<Unity.VisualScripting.RemoveListItem>
    {
        public RemoveListItemGenerator(Unity.VisualScripting.RemoveListItem unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.RemoveListItemAt))]
    public sealed class RemoveListItemAtGenerator : NodeGenerator<Unity.VisualScripting.RemoveListItemAt>
    {
        public RemoveListItemAtGenerator(Unity.VisualScripting.RemoveListItemAt unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.SetListItem))]
    public sealed class SetListItemGenerator : NodeGenerator<Unity.VisualScripting.SetListItem>
    {
        public SetListItemGenerator(Unity.VisualScripting.SetListItem unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.Break))]
    public sealed class BreakGenerator : NodeGenerator<Unity.VisualScripting.Break>
    {
        public BreakGenerator(Unity.VisualScripting.Break unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.Cache))]
    public sealed class CacheGenerator : NodeGenerator<Unity.VisualScripting.Cache>
    {
        private string randomString;

        public CacheGenerator(Unity.VisualScripting.Cache unit) : base(unit)
        {
            // Generate the random string only once
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

                output += CodeBuilder.Indent(indent) + $"for (int i = {initialization}; i < {condition}; i += {iterator})";
                output += "\n";
                output += CodeBuilder.OpenBody(indent);
                output += "\n";

                if (Unit.body.hasAnyConnection)
                {
                    output += (Unit.body.connection.destination.unit as Unit).GenerateControl(Unit.body.connection.destination, data, indent +2);
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
            return base.GenerateValue(output);
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

                output += CodeBuilder.Indent(indent) + $"foreach (var item in {collection})";
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
            return base.GenerateValue(output);
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


    [NodeGenerator(typeof(Unity.VisualScripting.If))]
    public sealed class IfGenerator : NodeGenerator<Unity.VisualScripting.If>
    {
        public IfGenerator(Unity.VisualScripting.If unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.Once))]
    public sealed class OnceGenerator : NodeGenerator<Unity.VisualScripting.Once>
    {
        private string uniqueId;

        public OnceGenerator(Unity.VisualScripting.Once unit) : base(unit)
        {
            // Generate a unique identifier for the Once node
            uniqueId = GenerateRandomString();
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            if (input == Unit.enter)
            {
                output += CodeBuilder.Indent(indent) + $"var Once_{uniqueId} = false;";
                output += "\n";
                output += CodeBuilder.Indent(indent) + $"if (!Once_{uniqueId})";
                output += "\n";
                output += CodeBuilder.OpenBody(indent);
                output += "\n";

                if (Unit.once.hasAnyConnection)
                {
                    output += (Unit.once.connection.destination.unit as Unit).GenerateControl(Unit.once.connection.destination, data, indent + 1);
                    output += "\n";
                }

                output += CodeBuilder.Indent(indent + 1) + $"Once_{uniqueId} = true;";
                output += "\n";
                output += CodeBuilder.CloseBody(indent);
                output += "\n";
            }
            else if (input == Unit.reset)
            {
                output += CodeBuilder.Indent(indent) + $"Once_{uniqueId} = false;";
                output += "\n";
            }

            if (Unit.after.hasValidConnection)
            {
                if (Unit.after.hasAnyConnection)
                {
                    output += (Unit.after.connection.destination.unit as Unit).GenerateControl(Unit.after.connection.destination, data, indent);
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

    [NodeGenerator(typeof(Unity.VisualScripting.SelectOnEnum))]

    public sealed class SelectOnEnumGenerator : NodeGenerator<Unity.VisualScripting.SelectOnEnum>
    {
        public SelectOnEnumGenerator(Unity.VisualScripting.SelectOnEnum unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.SelectOnInteger))]
    public sealed class SelectOnIntegerGenerator : NodeGenerator<Unity.VisualScripting.SelectOnInteger>
    {
        public SelectOnIntegerGenerator(Unity.VisualScripting.SelectOnInteger unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.SelectOnString))]
    public sealed class SelectOnStringGenerator : NodeGenerator<Unity.VisualScripting.SelectOnString>
    {
        public SelectOnStringGenerator(Unity.VisualScripting.SelectOnString unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.SelectUnit))]
    public sealed class SelectUnitGenerator : NodeGenerator<Unity.VisualScripting.SelectUnit>
    {
        public SelectUnitGenerator(Unity.VisualScripting.SelectUnit unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.SwitchOnEnum))]
    public sealed class SwitchOnEnumGenerator : NodeGenerator<Unity.VisualScripting.SwitchOnEnum>
    {
        public SwitchOnEnumGenerator(Unity.VisualScripting.SwitchOnEnum unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.SwitchOnInteger))]
    public sealed class SwitchOnIntegerGenerator : NodeGenerator<Unity.VisualScripting.SwitchOnInteger>
    {
        public SwitchOnIntegerGenerator(Unity.VisualScripting.SwitchOnInteger unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.SwitchOnString))]
    public sealed class SwitchOnStringGenerator : NodeGenerator<Unity.VisualScripting.SwitchOnString>
    {
        public SwitchOnStringGenerator(Unity.VisualScripting.SwitchOnString unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.Throw))]
    public sealed class ThrowGenerator : NodeGenerator<Unity.VisualScripting.Throw>
    {
        public ThrowGenerator(Unity.VisualScripting.Throw unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnDrawGizmos))]
    public sealed class OnDrawGizmosGenerator : NodeGenerator<Unity.VisualScripting.OnDrawGizmos>
    {
        public OnDrawGizmosGenerator(Unity.VisualScripting.OnDrawGizmos unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnDrawGizmosSelected))]
    public sealed class OnDrawGizmosSelectedGenerator : NodeGenerator<Unity.VisualScripting.OnDrawGizmosSelected>
    {
        public OnDrawGizmosSelectedGenerator(Unity.VisualScripting.OnDrawGizmosSelected unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnBeginDrag))]
    public sealed class OnBeginDragGenerator : NodeGenerator<Unity.VisualScripting.OnBeginDrag>
    {
        public OnBeginDragGenerator(Unity.VisualScripting.OnBeginDrag unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnButtonClick))]
    public sealed class OnButtonClickGenerator : NodeGenerator<Unity.VisualScripting.OnButtonClick>
    {
        public OnButtonClickGenerator(Unity.VisualScripting.OnButtonClick unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnDrag))]
    public sealed class OnDragGenerator : NodeGenerator<Unity.VisualScripting.OnDrag>
    {
        public OnDragGenerator(Unity.VisualScripting.OnDrag unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnDrop))]
    public sealed class OnDropGenerator : NodeGenerator<Unity.VisualScripting.OnDrop>
    {
        public OnDropGenerator(Unity.VisualScripting.OnDrop unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnDropdownValueChanged))]
    public sealed class OnDropdownValueChangedGenerator : NodeGenerator<Unity.VisualScripting.OnDropdownValueChanged>
    {
        public OnDropdownValueChangedGenerator(Unity.VisualScripting.OnDropdownValueChanged unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnEndDrag))]
    public sealed class OnEndDragGenerator : NodeGenerator<Unity.VisualScripting.OnEndDrag>
    {
        public OnEndDragGenerator(Unity.VisualScripting.OnEndDrag unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnGUI))]
    public sealed class OnGUIGenerator : NodeGenerator<Unity.VisualScripting.OnGUI>
    {
        public OnGUIGenerator(Unity.VisualScripting.OnGUI unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnTransformParentChanged))]
    public sealed class OnTransformParentChangedGenerator : NodeGenerator<Unity.VisualScripting.OnTransformParentChanged>
    {
        public OnTransformParentChangedGenerator(Unity.VisualScripting.OnTransformParentChanged unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnButtonInput))]
    public sealed class OnButtonInputGenerator : NodeGenerator<Unity.VisualScripting.OnButtonInput>
    {
        public OnButtonInputGenerator(Unity.VisualScripting.OnButtonInput unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnKeyboardInput))]
    public sealed class OnKeyboardInputGenerator : NodeGenerator<Unity.VisualScripting.OnKeyboardInput>
    {
        public OnKeyboardInputGenerator(Unity.VisualScripting.OnKeyboardInput unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnMouseDown))]
    public sealed class OnMouseDownGenerator : NodeGenerator<Unity.VisualScripting.OnMouseDown>
    {
        public OnMouseDownGenerator(Unity.VisualScripting.OnMouseDown unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnMouseDrag))]
    public sealed class OnMouseDragGenerator : NodeGenerator<Unity.VisualScripting.OnMouseDrag>
    {
        public OnMouseDragGenerator(Unity.VisualScripting.OnMouseDrag unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnMouseEnter))]
    public sealed class OnMouseEnterGenerator : NodeGenerator<Unity.VisualScripting.OnMouseEnter>
    {
        public OnMouseEnterGenerator(Unity.VisualScripting.OnMouseEnter unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnMouseExit))]
    public sealed class OnMouseExitGenerator : NodeGenerator<Unity.VisualScripting.OnMouseExit>
    {
        public OnMouseExitGenerator(Unity.VisualScripting.OnMouseExit unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnMouseInput))]
    public sealed class OnMouseInputGenerator : NodeGenerator<Unity.VisualScripting.OnMouseInput>
    {
        public OnMouseInputGenerator(Unity.VisualScripting.OnMouseInput unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnMouseOver))]
    public sealed class OnMouseOverGenerator : NodeGenerator<Unity.VisualScripting.OnMouseOver>
    {
        public OnMouseOverGenerator(Unity.VisualScripting.OnMouseOver unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnMouseUp))]
    public sealed class OnMouseUpGenerator : NodeGenerator<Unity.VisualScripting.OnMouseUp>
    {
        public OnMouseUpGenerator(Unity.VisualScripting.OnMouseUp unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnMouseUpAsButton))]
    public sealed class OnMouseUpAsButtonGenerator : NodeGenerator<Unity.VisualScripting.OnMouseUpAsButton>
    {
        public OnMouseUpAsButtonGenerator(Unity.VisualScripting.OnMouseUpAsButton unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.FixedUpdate))]
    public sealed class FixedUpdateGenerator : NodeGenerator<Unity.VisualScripting.FixedUpdate>
    {
        public FixedUpdateGenerator(Unity.VisualScripting.FixedUpdate unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.LateUpdate))]
    public sealed class LateUpdateGenerator : NodeGenerator<Unity.VisualScripting.LateUpdate>
    {
        public LateUpdateGenerator(Unity.VisualScripting.LateUpdate unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnDestroy))]
    public sealed class OnDestroyGenerator : NodeGenerator<Unity.VisualScripting.OnDestroy>
    {
        public OnDestroyGenerator(Unity.VisualScripting.OnDestroy unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnDisable))]
    public sealed class OnDisableGenerator : NodeGenerator<Unity.VisualScripting.OnDisable>
    {
        public OnDisableGenerator(Unity.VisualScripting.OnDisable unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnEnable))]
    public sealed class OnEnableGenerator : NodeGenerator<Unity.VisualScripting.OnEnable>
    {
        public OnEnableGenerator(Unity.VisualScripting.OnEnable unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnDestinationReached))]
    public sealed class OnDestinationReachedGenerator : NodeGenerator<Unity.VisualScripting.OnDestinationReached>
    {
        public OnDestinationReachedGenerator(Unity.VisualScripting.OnDestinationReached unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnCollisionEnter))]
    public sealed class OnCollisionEnterGenerator : NodeGenerator<Unity.VisualScripting.OnCollisionEnter>
    {
        public OnCollisionEnterGenerator(Unity.VisualScripting.OnCollisionEnter unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnCollisionExit))]
    public sealed class OnCollisionExitGenerator : NodeGenerator<Unity.VisualScripting.OnCollisionExit>
    {
        public OnCollisionExitGenerator(Unity.VisualScripting.OnCollisionExit unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnCollisionStay))]
    public sealed class OnCollisionStayGenerator : NodeGenerator<Unity.VisualScripting.OnCollisionStay>
    {
        public OnCollisionStayGenerator(Unity.VisualScripting.OnCollisionStay unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnControllerColliderHit))]
    public sealed class OnControllerColliderHitGenerator : NodeGenerator<Unity.VisualScripting.OnControllerColliderHit>
    {
        public OnControllerColliderHitGenerator(Unity.VisualScripting.OnControllerColliderHit unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnJointBreak))]
    public sealed class OnJointBreakGenerator : NodeGenerator<Unity.VisualScripting.OnJointBreak>
    {
        public OnJointBreakGenerator(Unity.VisualScripting.OnJointBreak unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnParticleCollision))]
    public sealed class OnParticleCollisionGenerator : NodeGenerator<Unity.VisualScripting.OnParticleCollision>
    {
        public OnParticleCollisionGenerator(Unity.VisualScripting.OnParticleCollision unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnTriggerEnter))]
    public sealed class OnTriggerEnterGenerator : NodeGenerator<Unity.VisualScripting.OnTriggerEnter>
    {
        public OnTriggerEnterGenerator(Unity.VisualScripting.OnTriggerEnter unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnTriggerExit))]
    public sealed class OnTriggerExitGenerator : NodeGenerator<Unity.VisualScripting.OnTriggerExit>
    {
        public OnTriggerExitGenerator(Unity.VisualScripting.OnTriggerExit unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnTriggerStay))]
    public sealed class OnTriggerStayGenerator : NodeGenerator<Unity.VisualScripting.OnTriggerStay>
    {
        public OnTriggerStayGenerator(Unity.VisualScripting.OnTriggerStay unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnCollisionEnter2D))]
    public sealed class OnCollisionEnter2DGenerator : NodeGenerator<Unity.VisualScripting.OnCollisionEnter2D>
    {
        public OnCollisionEnter2DGenerator(Unity.VisualScripting.OnCollisionEnter2D unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnCollisionExit2D))]
    public sealed class OnCollisionExit2DGenerator : NodeGenerator<Unity.VisualScripting.OnCollisionExit2D>
    {
        public OnCollisionExit2DGenerator(Unity.VisualScripting.OnCollisionExit2D unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnCollisionStay2D))]
    public sealed class OnCollisionStay2DGenerator : NodeGenerator<Unity.VisualScripting.OnCollisionStay2D>
    {
        public OnCollisionStay2DGenerator(Unity.VisualScripting.OnCollisionStay2D unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnJointBreak2D))]
    public sealed class OnJointBreak2DGenerator : NodeGenerator<Unity.VisualScripting.OnJointBreak2D>
    {
        public OnJointBreak2DGenerator(Unity.VisualScripting.OnJointBreak2D unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnTriggerEnter2D))]
    public sealed class OnTriggerEnter2DGenerator : NodeGenerator<Unity.VisualScripting.OnTriggerEnter2D>
    {
        public OnTriggerEnter2DGenerator(Unity.VisualScripting.OnTriggerEnter2D unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnTriggerExit2D))]
    public sealed class OnTriggerExit2DGenerator : NodeGenerator<Unity.VisualScripting.OnTriggerExit2D>
    {
        public OnTriggerExit2DGenerator(Unity.VisualScripting.OnTriggerExit2D unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnTriggerStay2D))]
    public sealed class OnTriggerStay2DGenerator : NodeGenerator<Unity.VisualScripting.OnTriggerStay2D>
    {
        public OnTriggerStay2DGenerator(Unity.VisualScripting.OnTriggerStay2D unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnBecameInvisible))]
    public sealed class OnBecameInvisibleGenerator : NodeGenerator<Unity.VisualScripting.OnBecameInvisible>
    {
        public OnBecameInvisibleGenerator(Unity.VisualScripting.OnBecameInvisible unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.OnBecameVisible))]
    public sealed class OnBecameVisibleGenerator : NodeGenerator<Unity.VisualScripting.OnBecameVisible>
    {
        public OnBecameVisibleGenerator(Unity.VisualScripting.OnBecameVisible unit) : base(unit)
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
                string eventName = GenerateValue(Unit.name);
                string eventArgs = GenerateAllArguments(Unit.arguments);

                output += CodeBuilder.Indent(indent) + $"CustomEvent.Trigger({GenerateValue(Unit.target)}, {eventName}, {eventArgs})";
                output += "\n";
            }

            return output;
        }

        private string GenerateAllArguments(List<ValueInput> arguments)
        {
            List<string> argumentValues = new List<string>();

            foreach (var argument in arguments)
            {
                string argumentValue = GenerateValue(argument);
                argumentValues.Add(argumentValue);
            }

            return $"new object[] {{ {string.Join(", ", argumentValues)} }}";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input == Unit.name || input == Unit.target || Unit.arguments.Contains(input))
            {
                if (input == Unit.name)
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
                else if (input == Unit.target)
                {
                    if (input.hasValidConnection)
                    {
                        return (input.connection.source.unit as Unit).GenerateValue(input.connection.source);
                    }
                    else
                    {
                        return CodeBuilder.WarningHighlight("/*Requires Object Input*/");
                    }
                }
                else
                {
                    return (input.connection.source.unit as Unit).GenerateValue(input.connection.source);
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

    [NodeGenerator(typeof(Unity.VisualScripting.Literal))]
    public sealed class LiteralGenerator : NodeGenerator<Unity.VisualScripting.Literal>
    {
        public LiteralGenerator(Unity.VisualScripting.Literal unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.And))]
    public sealed class AndGenerator : NodeGenerator<Unity.VisualScripting.And>
    {
        public AndGenerator(Unity.VisualScripting.And unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.Equal))]
    public sealed class EqualGenerator : NodeGenerator<Unity.VisualScripting.Equal>
    {
        public EqualGenerator(Unity.VisualScripting.Equal unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.ExclusiveOr))]
    public sealed class ExclusiveOrGenerator : NodeGenerator<Unity.VisualScripting.ExclusiveOr>
    {
        public ExclusiveOrGenerator(Unity.VisualScripting.ExclusiveOr unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.Greater))]
    public sealed class GreaterGenerator : NodeGenerator<Unity.VisualScripting.Greater>
    {
        public GreaterGenerator(Unity.VisualScripting.Greater unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.GreaterOrEqual))]
    public sealed class GreaterOrEqualGenerator : NodeGenerator<Unity.VisualScripting.GreaterOrEqual>
    {
        public GreaterOrEqualGenerator(Unity.VisualScripting.GreaterOrEqual unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.Less))]
    public sealed class LessGenerator : NodeGenerator<Unity.VisualScripting.Less>
    {
        public LessGenerator(Unity.VisualScripting.Less unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.LessOrEqual))]
    public sealed class LessOrEqualGenerator : NodeGenerator<Unity.VisualScripting.LessOrEqual>
    {
        public LessOrEqualGenerator(Unity.VisualScripting.LessOrEqual unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.Negate))]
    public sealed class NegateGenerator : NodeGenerator<Unity.VisualScripting.Negate>
    {
        public NegateGenerator(Unity.VisualScripting.Negate unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.NotEqual))]
    public sealed class NotEqualGenerator : NodeGenerator<Unity.VisualScripting.NotEqual>
    {
        public NotEqualGenerator(Unity.VisualScripting.NotEqual unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.Or))]
    public sealed class OrGenerator : NodeGenerator<Unity.VisualScripting.Or>
    {
        public OrGenerator(Unity.VisualScripting.Or unit) : base(unit)
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            if (output == Unit.sum)
            {
                if (output.hasValidConnection)
                {
                    var connectedInputs = Unit.inputs
                        .Where(input => input is ValueInput)
                        .Select(input => GenerateValue((ValueInput)input));

                    var script = string.Join(" + ", connectedInputs);

                    return script;
                }
            }

            return base.GenerateValue(output);
        }


        public override string GenerateValue(ValueInput input)
        {
            if (Unit.multiInputs.Contains(input))
            {
                if (input.hasValidConnection)
                {
                    var summands = new List<string>();

                    var sourceUnit = input.connection.source.unit as Unit;
                    summands.Add(sourceUnit.GenerateValue(input.connection.source));

                    return $"{string.Join(" + ", summands)}";
                }
                else
                {
                    return "/* Port Requires input */";
                }
            }

            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            return base.GenerateValue(input);
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

    [NodeGenerator(typeof(Unity.VisualScripting.GraphOutput))]
    public sealed class GraphOutputGenerator : NodeGenerator<Unity.VisualScripting.GraphOutput>
    {
        public GraphOutputGenerator(Unity.VisualScripting.GraphOutput unit) : base(unit)
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

                output += CodeBuilder.Indent(indent) + $"if ({valueToCheck} == null)";
                output += "\n";
                output += CodeBuilder.OpenBody(indent);
                output += "\n";

                if (Unit.ifNull.hasAnyConnection)
                {
                    var nullindent = CodeBuilder.Indent(indent);
                    output += nullindent + (Unit.ifNull.connection.destination.unit as Unit).GenerateControl(Unit.ifNull.connection.destination, data, indent + 1);
                }

                output += "\n" + CodeBuilder.CloseBody(indent);

                if (Unit.ifNotNull.hasValidConnection) 
                {
                    output += "\n";
                    output += CodeBuilder.Indent(indent) + (Unit.ifNotNull.connection.destination.unit as Unit).GenerateControl(Unit.ifNotNull.connection.destination, data, indent);
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
            return "/* Does not support Timer */";
        }

        public override string GenerateValue(ValueOutput output)
        {
            return "/* Does not support Timer */";
        }

        public override string GenerateValue(ValueInput input)
        {
            return "/* Does not support Timer */";
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

            output += "yield return new WaitForEndOfFrame();";

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
            return "/* Watf for flow Not supported */";
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

            output += "yield return null;\n";

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

            output += CodeBuilder.Indent(indent);
            if (!Unit.unscaledTime.hasValidConnection)
            {
                if (!GenerateValue(Unit.unscaledTime).Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    output += "yield return new WaitForSeconds(" + GenerateValue(Unit.seconds) + ");\n";
                }
                else
                {
                    output += "yield return new WaitForSecondsRealtime(" + GenerateValue(Unit.seconds) + ");\n";
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
                return "/* Use default value WaitForSeconds does not support connected value */";
            }
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input == Unit.seconds)
            {
                if (Unit.seconds.hasValidConnection)
                {
                    return (Unit.seconds.connection.source.unit as Unit).GenerateValue(Unit.seconds.connection.source);
                }
                else
                {
                    return Unit.defaultValues[input.key].ToString();
                }
            }
            else if (input == Unit.unscaledTime)
            {
                if (Unit.unscaledTime.hasValidConnection)
                {
                    return false.ToString();
                }
                else
                {
                    return Unit.defaultValues[input.key].ToString();
                }
            }

            return base.GenerateValue(input);
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
                output += $"yield return new WaitUntil(() => {GenerateValue(Unit.condition)}; \n";
            }
            else
            {
                return "/*WaitUntil requires condition*/";
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
                output += $"yield return new WaitWhile(() => {GenerateValue(Unit.condition)};\n";
            }
            else
            {
                return "/*WaitWhile requires condition*/";
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
                    if (connection.destination is ValueInput connectedInput)
                    {
                        return GenerateValue(Unit.name);
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
        public SetVariableGenerator(Unity.VisualScripting.SetVariable unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            data.mustReturn = true;

            if (input == Unit.assign)
            {
                if (!data.localNames.Contains(GenerateValue(Unit.name)))
                {
                    output += "\n";
                    data.AddLocalName(GenerateValue(Unit.name));
                    if (Unit.input.hasValidConnection && Unit.input.connection.source.unit.GetType() != typeof(Null))
                    {
                        output += CodeBuilder.Indent(indent) + $"var {GenerateValue(Unit.name)}".ConstructHighlight();
                        output += $" = {GenerateValue(Unit.input)};";
                    }
                    else
                    {
                        if (Unit.graph.variables.IsDefined(GenerateValue(Unit.name)))
                        {
                            output += CodeBuilder.Indent(indent) + CodeBuilder.TypeHighlight($"{Unit.graph.variables.Get(GenerateValue(Unit.name)).GetType().DisplayName()}") + $" {GenerateValue(Unit.name)}";
                            if (Unit.graph.variables.Get(GenerateValue(Unit.name)).GetType().IsValueType)
                            {
                                if (!Unit.graph.variables.Get(GenerateValue(Unit.name)).GetType().IsStruct())
                                {
                                    if (Unit.graph.variables.Get(GenerateValue(Unit.name)).GetType() != typeof(float))
                                    {
                                        output += " = " + Unit.graph.variables.Get(GenerateValue(Unit.name)) + ";";
                                    }
                                    else
                                    {
                                        output += " = " + Unit.graph.variables.Get(GenerateValue(Unit.name)) + "f;";
                                    }
                                }
                                else
                                {
                                    output += " = " + CodeBuilder.ConstructHighlight("new ") + Unit.graph.variables.Get(GenerateValue(Unit.name)).GetType().DisplayName() + "()";
                                }
                            }
                            else if (Unit.graph.variables.Get(GenerateValue(Unit.name)).GetType().GetConstructors().Count() > 0)
                            {
                                if (Unit.graph.variables.Get(GenerateValue(Unit.name)).GetType() != typeof(string))
                                {
                                    output += " = " + CodeBuilder.ConstructHighlight("new ") + Unit.graph.variables.Get(GenerateValue(Unit.name)) + "();";
                                }
                                else
                                {
                                    output += " = " + CodeBuilder.ConstructHighlight("new ") + Unit.graph.variables.Get(GenerateValue(Unit.name)) + "(\"\");";
                                }
                            }
                            else
                            {
                                output += " = null;";
                            }
                        }

                        output += "\n";
                    }
                }


                else
                {
                    output += "\n";
                    output += CodeBuilder.Indent(indent) + $"{GenerateValue(Unit.name)}".ConstructHighlight() + $" = {GenerateValue(Unit.input)};";

                    output += "\n";
                }

                output += (Unit.assigned.hasAnyConnection ? (Unit.assigned.connection.destination.unit as Unit).GenerateControl(Unit.assigned.connection.destination, data, indent) : string.Empty);
                return output;
            }

            return base.GenerateControl(input, data, indent);
        }
        public override string GenerateValue(ValueOutput output)
        {
            if (output == Unit.output)
            {
                foreach (var connection in output.connections)
                {
                    if (connection.destination is ValueInput connectedInput)
                    {
                        string generatedValue = GenerateValue(Unit.name);
                        return $"{(generatedValue.Length > 0 ? generatedValue : string.Empty)}";
                    }
                }
            }

            return base.GenerateValue(output); // Output value is the same as the corresponding input value
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input == Unit.name || input == Unit.input)
            {
                if (input == Unit.name)
                {
                    if (Unit.name.hasValidConnection)
                    {
                        string generatedValue = (Unit.name.connection.source.unit as Unit).GenerateValue(Unit.name.connection.source);
                        return generatedValue.Length > 0 ? generatedValue : string.Empty;
                    }
                    else
                    {
                        string defaultValue = Unit.defaultValues[input.key].ToString();
                        return defaultValue.Length > 0 ? defaultValue : string.Empty;
                    }
                }
                else if (input == Unit.input)
                {
                    if (Unit.input.hasValidConnection)
                    {
                        return (Unit.input.connection.source.unit as Unit).GenerateValue(Unit.input.connection.source);
                    }
                    else
                    {
                        return " = null";
                    }
                }
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

            var control = Unit.controlInputs[input.key];

            if (input == control)
            {
                var methodContent = GenerateMethodContent(Unit, input, data);

                if (Unit.valueOutputs.Count > 0 && Unit.nest.graph.title != string.Empty)
                {
                    output += $"{CodeBuilder.Indent(indent)}{Unit.valueOutputs[0].type.DisplayName()} {Unit.nest.graph.title}()\n";
                    output += CodeBuilder.Indent(indent) + "{\n";
                    output += CodeBuilder.Indent(indent) + 1 + methodContent;
                    output += CodeBuilder.Indent(indent) + "}\n";
                }
                else if (Unit.valueOutputs.Count == 0)
                {
                    output += "/* Add a value Output for a return type */";
                } 
                else if (Unit.nest.graph.title == string.Empty) 
                {
                    output += "/* Add a title for the local function */";
                }
            }

            foreach (var connection in Unit.controlOutputs)
            {
                if (connection.hasValidConnection)
                {

                    output += (connection.connection.destination.unit as Unit).GenerateControl(connection.connection.destination, data, indent);
                    output += "\n";
                    
                }
            }

            return output;
        }

        private string GenerateMethodContent(SubgraphUnit unit, ControlInput input, ControlGenerationData data)
        {
            var output = string.Empty;

            foreach (var _input in unit.nest.graph.controlConnections)
            {
                var connectedUnit = _input.destination.unit as Unit;
                output += connectedUnit.GenerateControl(_input.destination, data, 1);
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




#if USING_NEW_INPUT_SYSTEM
    [NodeGenerator(typeof(Unity.VisualScripting.InputSystem.OnInputSystemEventButton))]
    public sealed class OnInputSystemEventButtonGenerator : NodeGenerator<Unity.VisualScripting.InputSystem.OnInputSystemEventButton>
    {
        public OnInputSystemEventButtonGenerator(Unity.VisualScripting.InputSystem.OnInputSystemEventButton unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.InputSystem.OnInputSystemEventFloat))]
    public sealed class OnInputSystemEventFloatGenerator : NodeGenerator<Unity.VisualScripting.InputSystem.OnInputSystemEventFloat>
    {
        public OnInputSystemEventFloatGenerator(Unity.VisualScripting.InputSystem.OnInputSystemEventFloat unit) : base(unit)
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

    [NodeGenerator(typeof(Unity.VisualScripting.InputSystem.OnInputSystemEventVector2))]
    public sealed class OnInputSystemEventVector2Generator : NodeGenerator<Unity.VisualScripting.InputSystem.OnInputSystemEventVector2>
    {
        public OnInputSystemEventVector2Generator(Unity.VisualScripting.InputSystem.OnInputSystemEventVector2 unit) : base(unit)
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
#endif

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
                output += $"UnityEngine.Debug.Log({GenerateValue(Unit.format)});";
                output += (Unit.output.hasAnyConnection ? (Unit.output.connection.destination.unit as Unit).GenerateControl(Unit.output.connection.destination, data, indent) : string.Empty);
            }

            return output;
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
                    var value = Unit.defaultValues[input.key].ToString();

                    if (value == "")
                    {
                        return "\"\"";
                    }
                    else { return $"\"{value}\""; }
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
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var generatedCode = string.Empty;

            var generatorLogicMethod = unit.GetType().GetMethod("GeneratorLogic", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (generatorLogicMethod != null)
            {
                var _data = new object[] { data, indent };
                var methodResult = generatorLogicMethod.Invoke(unit, _data) as string;
                generatedCode += methodResult;
            }

            if (Unit.Exit != null)
            {
                generatedCode += (Unit.Exit.hasAnyConnection ? (Unit.Exit.connection.destination.unit as Unit).GenerateControl(Unit.Exit.connection.destination, data, indent) : string.Empty);
            }
            return generatedCode;
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
                    var methodResult = generatorLogicMethod.Invoke(unit, new object[0]) as string;
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

}





