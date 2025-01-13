using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.VisualScripting.Community
{

    [NodeGenerator(typeof(SetVariable))]
    public class SetVariableGenerator : LocalVariableGenerator
    {
        private SetVariable Unit => unit as SetVariable;
        public SetVariableGenerator(Unit unit) : base(unit)
        {
            if (Unit.kind == VariableKind.Scene)
            {
                NameSpaces = "UnityEngine.SceneManagement";
            }
            else
            {
                NameSpaces = string.Empty;
            }
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;
            if (Unit.kind == VariableKind.Scene)
            {
                NameSpaces = "UnityEngine.SceneManagement";
            }
            else
            {
                NameSpaces = string.Empty;
            }
            if (Unit.name.hasValidConnection)
            {
                var variables = typeof(Unity.VisualScripting.Variables).As().CSharpName(true, true);
                var kind = string.Empty;
                switch (Unit.kind)
                {
                    case VariableKind.Flow:
                        return MakeSelectableForThisUnit(CodeUtility.ToolTip("Flow Variables do not support connected names", "Could not generate Flow Variable", ""));
                    case VariableKind.Graph:
                        return MakeSelectableForThisUnit(CodeUtility.ToolTip("Graph Variables do not support connected names", "Could not generate Graph Variable", ""));
                    case VariableKind.Object:
                        kind = MakeSelectableForThisUnit(variables + $".Object(") + $"{GenerateValue(Unit.@object, data)}{MakeSelectableForThisUnit(")")}";
                        break;
                    case VariableKind.Scene:
                        kind = MakeSelectableForThisUnit(variables + $"." + "ActiveScene".VariableHighlight());
                        if (!Unit.name.hasValidConnection && VisualScripting.Variables.ActiveScene.IsDefined(Unit.defaultValues[Unit.name.key] as string))
                        {
                            var identification = VisualScripting.Variables.ActiveScene.GetDeclaration(Unit.defaultValues[Unit.name.key] as string).typeHandle.Identification;
                            if (string.IsNullOrEmpty(identification))
                                variableType = typeof(object);
                            else
                                variableType = Type.GetType(identification);
                        }
                        else
                            variableType = typeof(object);
                        break;
                    case VariableKind.Application:
                        kind = MakeSelectableForThisUnit(variables + "." + "Application".VariableHighlight());
                        if (!Unit.name.hasValidConnection && VisualScripting.Variables.Application.IsDefined(Unit.defaultValues[Unit.name.key] as string))
                        {
                            var identification = VisualScripting.Variables.Application.GetDeclaration(Unit.defaultValues[Unit.name.key] as string).typeHandle.Identification;
                            if (string.IsNullOrEmpty(identification))
                                variableType = typeof(object);
                            else
                                variableType = Type.GetType(identification);
                        }
                        else
                            variableType = typeof(object);
                        break;
                    case VariableKind.Saved:
                        kind = MakeSelectableForThisUnit(variables + "." + "Saved".VariableHighlight());
                        if (!Unit.name.hasValidConnection && VisualScripting.Variables.Saved.IsDefined(Unit.defaultValues[Unit.name.key] as string))
                        {
                            var identification = VisualScripting.Variables.Saved.GetDeclaration(Unit.defaultValues[Unit.name.key] as string).typeHandle.Identification;
                            if (string.IsNullOrEmpty(identification))
                                variableType = typeof(object);
                            else
                                variableType = Type.GetType(identification);
                        }
                        else
                            variableType = typeof(object);
                        break;
                }
                var nameCode = GenerateValue(Unit.name, data);
                data.SetExpectedType(variableType);
                var code = CodeBuilder.Indent(indent) + kind + MakeSelectableForThisUnit(".Set(") + nameCode + MakeSelectableForThisUnit(", ") + $"{(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : MakeSelectableForThisUnit("null".ConstructHighlight()))}" + MakeSelectableForThisUnit(");") + "\n";
                output += code;
                data.RemoveExpectedType();
                output += GetNextUnit(Unit.assigned, data, indent);
                return output;
            }
            else
            {
                var variables = typeof(Unity.VisualScripting.Variables).As().CSharpName(true, true);
                switch (Unit.kind)
                {
                    case VariableKind.Object:
                        return CodeBuilder.Indent(indent) + MakeSelectableForThisUnit($"{variables}$" + "." + $"Object({GenerateValue(Unit.@object, data)})" + ".Set(") + $"{GenerateValue(Unit.name, data)}{MakeSelectableForThisUnit(", ")}{(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : MakeSelectableForThisUnit("null".ConstructHighlight()))}" + MakeSelectableForThisUnit(");") + "\n" + GetNextUnit(Unit.assigned, data, indent);
                    case VariableKind.Scene:
                        return CodeBuilder.Indent(indent) + MakeSelectableForThisUnit($"{variables}$" + "." + "ActiveScene".VariableHighlight() + ".Set(") + $"{GenerateValue(Unit.name, data)}{MakeSelectableForThisUnit(", ")}{(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : MakeSelectableForThisUnit("null".ConstructHighlight()))}" + MakeSelectableForThisUnit(");") + "\n" + GetNextUnit(Unit.assigned, data, indent);
                    case VariableKind.Application:
                        return CodeBuilder.Indent(indent) + MakeSelectableForThisUnit($"{variables}{"." + "Application".VariableHighlight() + ".Set("}") + $"{GenerateValue(Unit.name, data)}{MakeSelectableForThisUnit(", ")}{(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : MakeSelectableForThisUnit("null".ConstructHighlight()))}" + MakeSelectableForThisUnit(");") + "\n" + GetNextUnit(Unit.assigned, data, indent);
                    case VariableKind.Saved:
                        return CodeBuilder.Indent(indent) + MakeSelectableForThisUnit($"{variables}{"." + "Saved".VariableHighlight() + ".Set("}") + $"{GenerateValue(Unit.name, data)}{MakeSelectableForThisUnit(", ")}{(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : MakeSelectableForThisUnit("null".ConstructHighlight()))}" + MakeSelectableForThisUnit(");") + "\n" + GetNextUnit(Unit.assigned, data, indent);
                }
                var name = data.GetVariableName(Unit.defaultValues[Unit.name.key] as string);
                if (data.ContainsNameInAnyScope(name))
                {
                    variableName = name;
                    variableType = data.GetVariableType(name);
                    data.SetExpectedType(variableType);
                    var code = MakeSelectableForThisUnit($"{name.VariableHighlight()} = ") + GenerateValue(Unit.input, data) + MakeSelectableForThisUnit(";");
                    data.RemoveExpectedType();
                    data.CreateSymbol(Unit, variableType, code);
                    output += CodeBuilder.Indent(indent) + code + "\n";
                    output += GetNextUnit(Unit.assigned, data, indent);
                    return output;
                }
                else
                {
                    var type = GetSourceType(Unit.input, data) ?? data.GetExpectedType() ?? typeof(object);
                    var inputType = type.As().CSharpName(false, true);
                    variableType = Unit.input.hasValidConnection ? Unit.input.connection.source.type : typeof(object);
                    var newName = data.AddLocalNameInScope(name, variableType);
                    data.SetExpectedType(variableType);
                    data.CreateSymbol(Unit, variableType, $"{inputType} {newName.VariableHighlight()} = {(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : "null".ConstructHighlight())};");
                    variableName = newName;
                    output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit($"{inputType} {newName.VariableHighlight()} = ") + (Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : MakeSelectableForThisUnit("null".ConstructHighlight())) + MakeSelectableForThisUnit(";") + "\n";
                    data.RemoveExpectedType();
                    output += GetNextUnit(Unit.assigned, data, indent);
                    return output;
                }
            }
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (!Unit.assign.hasValidConnection) return $"/* ControlInput {Unit.assign.key} requires connection on {Unit.GetType()} with variable name ({GenerateValue(Unit.name, data)}) */".WarningHighlight();
            if (output == Unit.output && (Unit.kind == VariableKind.Object || Unit.kind == VariableKind.Scene || Unit.kind == VariableKind.Application || Unit.kind == VariableKind.Saved))
            {
                return GenerateValue(Unit.input, data);
            }
            else if (output == Unit.output && !Unit.name.hasValidConnection)
            {
                return MakeSelectableForThisUnit(variableName.VariableHighlight());
            }
            else return GenerateValue(Unit.input, data);
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.@object && !input.hasValidConnection)
            {
                return MakeSelectableForThisUnit("gameObject".VariableHighlight());
            }
            if (input == Unit.input)
            {
                data.SetExpectedType(variableType ?? typeof(object));
                var code = base.GenerateValue(input, data);
                data.RemoveExpectedType();
                return code;
            }
            else if (input == Unit.@object)
            {
                data.SetExpectedType(typeof(GameObject));
                var code = base.GenerateValue(input, data);
                data.RemoveExpectedType();
                return code;
            }
            return base.GenerateValue(input, data);
        }
    }
}