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
            if (Unit.name.hasValidConnection || (Unit.@object != null && Unit.@object.hasValidConnection))
            {
                var variables = typeof(Unity.VisualScripting.Variables).As().CSharpName(true, true);
                var kind = string.Empty;
                data.SetExpectedType(variableType);
                data.CreateSymbol(Unit, variableType, $"{variables}{kind}.Set({GenerateValue(Unit.name, data)}, {(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : "null".ConstructHighlight())});");
                switch (Unit.kind)
                {
                    case VariableKind.Flow:
                        return MakeSelectableForThisUnit("/* Flow Variables are not supported connected names */".WarningHighlight());
                    case VariableKind.Graph:
                        return MakeSelectableForThisUnit("/* Graph Variables do not support connected names */".WarningHighlight());
                    case VariableKind.Object:
                        kind = MakeSelectableForThisUnit(variables + $".Object(") + $"{GenerateValue(Unit.@object, data)}{MakeSelectableForThisUnit(")")}";
                        break;
                    case VariableKind.Scene:
                        kind = MakeSelectableForThisUnit(variables + $".Scene({"SceneManager".TypeHighlight()}.GetActiveScene())");
                        break;
                    case VariableKind.Application:
                        kind = MakeSelectableForThisUnit(variables + "." + "Application".VariableHighlight());
                        break;
                    case VariableKind.Saved:
                        kind = MakeSelectableForThisUnit(variables + "." + "Saved".VariableHighlight());
                        break;
                }
                var nameCode = GenerateValue(Unit.name, data);
                output += CodeBuilder.Indent(indent) + kind + MakeSelectableForThisUnit(".Set(") + nameCode + MakeSelectableForThisUnit(", ") + $"{(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : MakeSelectableForThisUnit("null".ConstructHighlight()))}" + MakeSelectableForThisUnit(");") + "\n";
                data.RemoveExpectedType();
                output += GetNextUnit(Unit.assigned, data, indent);
                return output;
            }
            else
            {
                var variables = typeof(Unity.VisualScripting.Variables).As().CSharpName(true, true);
                switch (Unit.kind)
                {
                    case VariableKind.Scene:
                        return CodeBuilder.Indent(indent) + MakeSelectableForThisUnit($"{variables}.Scene({"SceneManager".TypeHighlight()}.GetActiveScene()).Set(") + $"{GenerateValue(Unit.name, data)}{MakeSelectableForThisUnit(", ")}{(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : MakeSelectableForThisUnit("null".ConstructHighlight()))}" + MakeSelectableForThisUnit(");") + "\n" + GetNextUnit(Unit.assigned, data, indent);
                    case VariableKind.Application:
                        return CodeBuilder.Indent(indent) + MakeSelectableForThisUnit($"{variables}{".Application.Set("}") + $"{GenerateValue(Unit.name, data)}{MakeSelectableForThisUnit(", ")}{(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : MakeSelectableForThisUnit("null".ConstructHighlight()))}" + MakeSelectableForThisUnit(");") + "\n" + GetNextUnit(Unit.assigned, data, indent);
                    case VariableKind.Saved:
                        return CodeBuilder.Indent(indent) + MakeSelectableForThisUnit($"{variables}{".Saved.Set("}") + $"{GenerateValue(Unit.name, data)}{MakeSelectableForThisUnit(", ")}{(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : MakeSelectableForThisUnit("null".ConstructHighlight()))}" + MakeSelectableForThisUnit(");") + "\n" + GetNextUnit(Unit.assigned, data, indent);
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
            if (!Unit.name.hasValidConnection)
            {
                return MakeSelectableForThisUnit(variableName.VariableHighlight());
            }
            else return "";
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.@object && !input.hasValidConnection)
            {
                return MakeSelectableForThisUnit("gameObject".VariableHighlight());
            }
            if (input == Unit.input)
            {
                data.SetExpectedType(variableType);
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