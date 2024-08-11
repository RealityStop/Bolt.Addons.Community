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
    public class SetVariableGenerator : LocalVariableGenerator<SetVariable>
    {
        public SetVariableGenerator(Unit unit) : base(unit)
        {
        }
    
        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;
            if (Unit.kind == VariableKind.Scene)
            {
                NameSpace = "UnityEngine.SceneManagement";
            }
            else
            {
                NameSpace = string.Empty;
            }
            if (Unit.name.hasValidConnection)
            {
                var variables = typeof(Unity.VisualScripting.Variables).As().CSharpName(true, true);
                var kind = string.Empty;
    
                switch (Unit.kind)
                {
                    case VariableKind.Flow:
                        return "/* Flow Variables are not supported connected names */";
                    case VariableKind.Graph:
                        return "/* Graph Variables do not support connected names */";
                    case VariableKind.Object:
                        kind = $".Object({GenerateValue(Unit.@object, data)}, data)";
                        break;
                    case VariableKind.Scene:
                        kind = $".Scene({"SceneManager".TypeHighlight()}.GetActiveScene())";
                        break;
                    case VariableKind.Application:
                        kind = ".Application";
                        break;
                    case VariableKind.Saved:
                        kind = ".Saved";
                        break;
                }
                output += CodeBuilder.Indent(indent) + CodeUtility.MakeSelectable(Unit, $"{variables}{kind}.Set({GenerateValue(Unit.name, data)}, {(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : "null".ConstructHighlight())});") + "\n";
                output += GetNextUnit(Unit.assigned, data, indent);
                return output;
            }
            else
            {
                var variables = typeof(Unity.VisualScripting.Variables).As().CSharpName(true, true);
                switch (Unit.kind)
                {
                    case VariableKind.Scene:
                    return CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + $"{variables}.Scene({"SceneManager".TypeHighlight()}.GetActiveScene()).Set({GenerateValue(Unit.name, data)}, {(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : "null".ConstructHighlight())});") + "\n";
                    case VariableKind.Application:
                        return CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + $"{variables}.Application.Set({GenerateValue(Unit.name, data)}, {(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : "null".ConstructHighlight())});") + "\n";
                    case VariableKind.Saved:
                        return CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + $"{variables}.Saved.Set({GenerateValue(Unit.name, data)}, {(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : "null".ConstructHighlight())});") + "\n";
                }
                var name = data.GetVariableName(Unit.defaultValues[Unit.name.key] as string);
                if (data.ContainsNameInAnyScope(name) || data.ContainsNameInAnyScope(Unit.defaultValues[Unit.name.key] as string))
                {
                    variableName = name;
                    data.expectedType = data.GetVariableType(name);
                    output += CodeBuilder.Indent(indent) + CodeUtility.MakeSelectable(Unit, $"{name.VariableHighlight()} = {GenerateValue(Unit.input, data)};") + "\n";
                    output += GetNextUnit(Unit.assigned, data, indent);
                    return output;
                }
                else
                {
                    var inputType = Unit.input.hasValidConnection ? Unit.input.connection.source.type.As().CSharpName(false, true, true) : typeof(object).As().CSharpName(false, true);
                    variableType = Unit.input.hasValidConnection ? Unit.input.connection.source.type : typeof(object);
                    var newName = data.AddLocalNameInScope(name, variableType);
                    variableName = newName;
                    data.expectedType = variableType;
                    output += CodeBuilder.Indent(indent) + CodeUtility.MakeSelectable(Unit, $"{inputType} {newName.VariableHighlight()} = {(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : "null".ConstructHighlight())};") + "\n";
                    output += GetNextUnit(Unit.assigned, data, indent);
                    return output;
                }
            }
        }
    
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if(!Unit.assign.hasValidConnection) return $"/* ControlInput {Unit.assign.key} requires connection on {Unit.GetType()} with variable name ({GenerateValue(Unit.name, data)}) */";
            if(!Unit.name.hasValidConnection)
            {
                return variableName.VariableHighlight();
            }
            else return "";
        }
    
        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.@object && !input.hasValidConnection)
            {
                return "gameObject".VariableHighlight();
            }
            return base.GenerateValue(input, data);
        }
    }
}