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
                var name = data.TryGetGraphPointer(out var graphPointer) && CanPredictConnection(Unit.name, data) ? Flow.Predict<string>(Unit.name, graphPointer.AsReference()) : Unit.defaultValues[Unit.name.key] as string;
                switch (Unit.kind)
                {
                    case VariableKind.Flow:
                        return MakeSelectableForThisUnit(CodeUtility.ToolTip("Flow Variables do not support connected names", "Could not generate Flow Variable", ""));
                    case VariableKind.Graph:
                        return MakeSelectableForThisUnit(CodeUtility.ToolTip("Graph Variables do not support connected names", "Could not generate Graph Variable", ""));
                    case VariableKind.Object:
                        kind = MakeSelectableForThisUnit(variables + ".Object(") + $"{GenerateValue(Unit.@object, data)}{MakeSelectableForThisUnit(")")}";
                        if (VisualScripting.Variables.Object(GetTarget(data)).IsDefined(name))
                        {
                            var identification = VisualScripting.Variables.Object(GetTarget(data)).GetDeclaration(name).typeHandle.Identification;
                            if (string.IsNullOrEmpty(identification))
                                variableType = typeof(object);
                            else
                                variableType = Type.GetType(identification);
                        }
                        else
                            variableType = typeof(object);
                        break;
                    case VariableKind.Scene:
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
                        kind = MakeSelectableForThisUnit(variables + "." + "ActiveScene".VariableHighlight());
=======
                        kind = MakeClickableForThisUnit(GetSceneKind(data, variables));
>>>>>>> Stashed changes
=======
                        kind = MakeClickableForThisUnit(GetSceneKind(data, variables));
>>>>>>> Stashed changes
=======
                        kind = MakeClickableForThisUnit(GetSceneKind(data, variables));
>>>>>>> Stashed changes
                        if (VisualScripting.Variables.ActiveScene.IsDefined(name))
                        {
                            var identification = VisualScripting.Variables.ActiveScene.GetDeclaration(name).typeHandle.Identification;
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
                        if (VisualScripting.Variables.Application.IsDefined(name))
                        {
                            var identification = VisualScripting.Variables.Application.GetDeclaration(name).typeHandle.Identification;
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
                        if (VisualScripting.Variables.Saved.IsDefined(name))
                        {
                            var identification = VisualScripting.Variables.Saved.GetDeclaration(name).typeHandle.Identification;
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
                var variables = typeof(VisualScripting.Variables).As().CSharpName(true, true);
                var name = Unit.defaultValues[Unit.name.key] as string;
                switch (Unit.kind)
                {
                    case VariableKind.Object:
                        var target = GetTarget(data);
                        if (target != null && VisualScripting.Variables.Object(target).IsDefined(name))
                        {
                            var identification = VisualScripting.Variables.Object(GetTarget(data)).GetDeclaration(name).typeHandle.Identification;
                            if (string.IsNullOrEmpty(identification))
                                variableType = typeof(object);
                            else
                                variableType = Type.GetType(identification);
                        }
                        else
                            variableType = typeof(object);
                        return CodeBuilder.Indent(indent) + MakeSelectableForThisUnit($"{variables}" + "." + $"Object(") + GenerateValue(Unit.@object, data) + MakeSelectableForThisUnit(")" + ".Set(") + $"{GenerateValue(Unit.name, data)}{MakeSelectableForThisUnit(", ")}{(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : MakeSelectableForThisUnit("null".ConstructHighlight()))}" + MakeSelectableForThisUnit(");") + "\n" + GetNextUnit(Unit.assigned, data, indent);
                    case VariableKind.Scene:
                        if (VisualScripting.Variables.ActiveScene.IsDefined(name))
                        {
                            var identification = VisualScripting.Variables.ActiveScene.GetDeclaration(name).typeHandle.Identification;
                            if (string.IsNullOrEmpty(identification))
                                variableType = typeof(object);
                            else
                                variableType = Type.GetType(identification);
                        }
                        else
                            variableType = typeof(object);
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
                        return CodeBuilder.Indent(indent) + MakeSelectableForThisUnit($"{variables}" + "." + "ActiveScene".VariableHighlight() + ".Set(") + $"{GenerateValue(Unit.name, data)}{MakeSelectableForThisUnit(", ")}{(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : MakeSelectableForThisUnit("null".ConstructHighlight()))}" + MakeSelectableForThisUnit(");") + "\n" + GetNextUnit(Unit.assigned, data, indent);
=======
                        return CodeBuilder.Indent(indent) + MakeClickableForThisUnit(GetSceneKind(data, variables) + ".Set(") + $"{GenerateValue(Unit.name, data)}{MakeClickableForThisUnit(", ")}{(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : MakeClickableForThisUnit("null".ConstructHighlight()))}" + MakeClickableForThisUnit(");") + "\n" + GetNextUnit(Unit.assigned, data, indent);
>>>>>>> Stashed changes
=======
                        return CodeBuilder.Indent(indent) + MakeClickableForThisUnit(GetSceneKind(data, variables) + ".Set(") + $"{GenerateValue(Unit.name, data)}{MakeClickableForThisUnit(", ")}{(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : MakeClickableForThisUnit("null".ConstructHighlight()))}" + MakeClickableForThisUnit(");") + "\n" + GetNextUnit(Unit.assigned, data, indent);
>>>>>>> Stashed changes
=======
                        return CodeBuilder.Indent(indent) + MakeClickableForThisUnit(GetSceneKind(data, variables) + ".Set(") + $"{GenerateValue(Unit.name, data)}{MakeClickableForThisUnit(", ")}{(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : MakeClickableForThisUnit("null".ConstructHighlight()))}" + MakeClickableForThisUnit(");") + "\n" + GetNextUnit(Unit.assigned, data, indent);
>>>>>>> Stashed changes
                    case VariableKind.Application:
                        if (VisualScripting.Variables.Application.IsDefined(name))
                        {
                            var identification = VisualScripting.Variables.Application.GetDeclaration(name).typeHandle.Identification;
                            if (string.IsNullOrEmpty(identification))
                                variableType = typeof(object);
                            else
                                variableType = Type.GetType(identification);
                        }
                        else
                            variableType = typeof(object);
                        return CodeBuilder.Indent(indent) + MakeSelectableForThisUnit($"{variables}{"." + "Application".VariableHighlight() + ".Set("}") + $"{GenerateValue(Unit.name, data)}{MakeSelectableForThisUnit(", ")}{(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : MakeSelectableForThisUnit("null".ConstructHighlight()))}" + MakeSelectableForThisUnit(");") + "\n" + GetNextUnit(Unit.assigned, data, indent);
                    case VariableKind.Saved:
                        if (VisualScripting.Variables.Saved.IsDefined(name))
                        {
                            var identification = VisualScripting.Variables.Saved.GetDeclaration(name).typeHandle.Identification;
                            if (string.IsNullOrEmpty(identification))
                                variableType = typeof(object);
                            else
                                variableType = Type.GetType(identification);
                        }
                        else
                            variableType = typeof(object);
                        return CodeBuilder.Indent(indent) + MakeSelectableForThisUnit($"{variables}{"." + "Saved".VariableHighlight() + ".Set("}") + $"{GenerateValue(Unit.name, data)}{MakeSelectableForThisUnit(", ")}{(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : MakeSelectableForThisUnit("null".ConstructHighlight()))}" + MakeSelectableForThisUnit(");") + "\n" + GetNextUnit(Unit.assigned, data, indent);
                }
                var _name = data.GetVariableName(name);
                if (data.ContainsNameInAnyScope(_name))
                {
                    variableName = _name;
                    variableType = data.GetVariableType(_name);
                    data.SetExpectedType(variableType);
                    var code = MakeSelectableForThisUnit($"{_name.VariableHighlight()} = ") + GenerateValue(Unit.input, data) + MakeSelectableForThisUnit(";");
                    data.RemoveExpectedType();
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
                    data.CreateSymbol(Unit, variableType, code);
=======
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
                    var code = MakeClickableForThisUnit($"{_name.LegalMemberName().VariableHighlight()} = ") + inputCode + MakeClickableForThisUnit(";");
                    data.CreateSymbol(Unit, variableType);
>>>>>>> Stashed changes
                    output += CodeBuilder.Indent(indent) + code + "\n";
                    output += GetNextUnit(Unit.assigned, data, indent);
                    return output;
                }
                else
                {
                    var type = GetSourceType(Unit.input, data) ?? data.GetExpectedType() ?? typeof(object);
                    var inputType = type.As().CSharpName(false, true);
                    variableType = type;
                    var newName = data.AddLocalNameInScope(_name, variableType);
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

        private string GetSceneKind(ControlGenerationData data, string variables)
        {
            return typeof(Component).IsAssignableFrom(data.ScriptType) ? variables + ".Scene(" + "gameObject".VariableHighlight() + "." + "scene".VariableHighlight() + ")" : variables + "." + "Application".VariableHighlight();
        }

        private GameObject GetTarget(ControlGenerationData data)
        {
            if (!Unit.@object.hasValidConnection && Unit.defaultValues[Unit.@object.key] == null && data.TryGetGameObject(out var gameObject))
            {
                return gameObject;
            }
            else if (!Unit.@object.hasValidConnection && Unit.defaultValues[Unit.@object.key] != null)
            {
                return Unit.defaultValues[Unit.@object.key].ConvertTo<GameObject>();
            }
            else
            {
                if (data.TryGetGraphPointer(out var graphPointer))
                {
                    if (Unit.@object.hasValidConnection && CanPredictConnection(Unit.@object, data))
                    {
                        try
                        {
                            return Flow.Predict(Unit.@object.GetPesudoSource(), graphPointer.AsReference()) as GameObject;
                        }
                        catch (InvalidOperationException ex)
                        {
                            Debug.LogError(ex);
                            return null; // Don't break code view so just log the error and return null.
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                return null;
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
            if (input == Unit.@object && !input.hasValidConnection && Unit.defaultValues[input.key] == null)
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