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
        private static readonly Dictionary<string, Type> typeCache = new();

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
                        return MakeClickableForThisUnit(CodeUtility.ErrorTooltip("Flow Variables do not support connected names", "Could not generate Flow Variable", ""));
                    case VariableKind.Graph:
                        return MakeClickableForThisUnit(CodeUtility.ErrorTooltip("Graph Variables do not support connected names", "Could not generate Graph Variable", ""));
                    case VariableKind.Object:
                        kind = MakeClickableForThisUnit(variables + ".Object(") + $"{GenerateValue(Unit.@object, data)}{MakeClickableForThisUnit(")")}";
                        if (VisualScripting.Variables.Object(GetTarget(data)).IsDefined(name))
                        {
                            variableType = ResolveVariableType(VisualScripting.Variables.Object(GetTarget(data)), name);
                        }
                        else
                            variableType = typeof(object);
                        break;
                    case VariableKind.Scene:
                        kind = MakeClickableForThisUnit(GetSceneKind(data, variables));
                        if (VisualScripting.Variables.ActiveScene.IsDefined(name))
                        {
                            variableType = ResolveVariableType(VisualScripting.Variables.ActiveScene, name);
                        }
                        else
                            variableType = typeof(object);
                        break;
                    case VariableKind.Application:
                        kind = MakeClickableForThisUnit(variables + "." + "Application".VariableHighlight());
                        if (VisualScripting.Variables.Application.IsDefined(name))
                        {
                            variableType = ResolveVariableType(VisualScripting.Variables.Application, name);
                        }
                        else
                            variableType = typeof(object);
                        break;
                    case VariableKind.Saved:
                        kind = MakeClickableForThisUnit(variables + "." + "Saved".VariableHighlight());
                        if (VisualScripting.Variables.Saved.IsDefined(name))
                        {
                            variableType = ResolveVariableType(VisualScripting.Variables.Saved, name);
                        }
                        else
                            variableType = typeof(object);
                        break;
                }
                var nameCode = GenerateValue(Unit.name, data);
                data.SetExpectedType(variableType);
                var code = CodeBuilder.Indent(indent) + kind + MakeClickableForThisUnit(".Set(") + nameCode + MakeClickableForThisUnit(", ") + $"{(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : MakeClickableForThisUnit("null".ConstructHighlight()))}" + MakeClickableForThisUnit(");") + "\n";
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
                            variableType = ResolveVariableType(VisualScripting.Variables.Object(target), name);
                        }
                        else
                            variableType = typeof(object);
                        return CodeBuilder.Indent(indent) + MakeClickableForThisUnit($"{variables}" + "." + $"Object(") + GenerateValue(Unit.@object, data) + MakeClickableForThisUnit(")" + ".Set(") + $"{GenerateValue(Unit.name, data)}{MakeClickableForThisUnit(", ")}{(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : MakeClickableForThisUnit("null".ConstructHighlight()))}" + MakeClickableForThisUnit(");") + "\n" + GetNextUnit(Unit.assigned, data, indent);
                    case VariableKind.Scene:
                        if (VisualScripting.Variables.ActiveScene.IsDefined(name))
                        {
                            variableType = ResolveVariableType(VisualScripting.Variables.ActiveScene, name);
                        }
                        else
                            variableType = typeof(object);
                        return CodeBuilder.Indent(indent) + MakeClickableForThisUnit(GetSceneKind(data, variables) + ".Set(") + $"{GenerateValue(Unit.name, data)}{MakeClickableForThisUnit(", ")}{(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : MakeClickableForThisUnit("null".ConstructHighlight()))}" + MakeClickableForThisUnit(");") + "\n" + GetNextUnit(Unit.assigned, data, indent);
                    case VariableKind.Application:
                        if (VisualScripting.Variables.Application.IsDefined(name))
                        {
                            variableType = ResolveVariableType(VisualScripting.Variables.Application, name);
                        }
                        else
                            variableType = typeof(object);
                        return CodeBuilder.Indent(indent) + MakeClickableForThisUnit($"{variables}{"." + "Application".VariableHighlight() + ".Set("}") + $"{GenerateValue(Unit.name, data)}{MakeClickableForThisUnit(", ")}{(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : MakeClickableForThisUnit("null".ConstructHighlight()))}" + MakeClickableForThisUnit(");") + "\n" + GetNextUnit(Unit.assigned, data, indent);
                    case VariableKind.Saved:
                        if (VisualScripting.Variables.Saved.IsDefined(name))
                        {
                            variableType = ResolveVariableType(VisualScripting.Variables.Saved, name);
                        }
                        else
                            variableType = typeof(object);
                        return CodeBuilder.Indent(indent) + MakeClickableForThisUnit($"{variables}{"." + "Saved".VariableHighlight() + ".Set("}") + $"{GenerateValue(Unit.name, data)}{MakeClickableForThisUnit(", ")}{(Unit.input.hasValidConnection ? GenerateValue(Unit.input, data) : MakeClickableForThisUnit("null".ConstructHighlight()))}" + MakeClickableForThisUnit(");") + "\n" + GetNextUnit(Unit.assigned, data, indent);
                }
                var _name = data.GetVariableName(name.LegalMemberName());
                CodeBuilder.Indent(indent); // To Ensure indentation is correct
                if (data.ContainsNameInAnyScope(_name))
                {
                    variableName = _name.LegalMemberName();
                    variableType = data.GetVariableType(_name);
                    data.SetExpectedType(variableType);
                    var inputCode = GenerateValue(Unit.input, data);
                    data.RemoveExpectedType();
                    var code = MakeClickableForThisUnit($"{_name.LegalMemberName().VariableHighlight()} = ") + inputCode + MakeClickableForThisUnit(";");
                    data.CreateSymbol(Unit, variableType);
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
                    var inputCode = GenerateValue(Unit.input, data);
                    data.RemoveExpectedType();
                    data.CreateSymbol(Unit, variableType);
                    variableName = newName.LegalMemberName();
                    output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit($"{inputType} {variableName.VariableHighlight()} = ") + (Unit.input.hasValidConnection ? inputCode : MakeClickableForThisUnit("null".ConstructHighlight())) + MakeClickableForThisUnit(";") + "\n";
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
                            return Flow.Predict<GameObject>(Unit.@object.GetPesudoSource(), graphPointer.AsReference());
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
            if (!Unit.assign.hasValidConnection) return MakeClickableForThisUnit($"/* ControlInput {Unit.assign.key} requires connection on {Unit.GetType()} with variable name ({GenerateValue(Unit.name, data)}) */".WarningHighlight());
            if (output == Unit.output && (Unit.kind == VariableKind.Object || Unit.kind == VariableKind.Scene || Unit.kind == VariableKind.Application || Unit.kind == VariableKind.Saved))
            {
                return GenerateValue(Unit.input, data);
            }
            else if (output == Unit.output && !Unit.name.hasValidConnection)
            {
                if (data.ContainsNameInAnyScope(variableName))
                    return MakeClickableForThisUnit(variableName.VariableHighlight());
                else return MakeClickableForThisUnit($"/* Could not find variable with name \"{variableName}\" */".WarningHighlight());
            }
            else return GenerateValue(Unit.input, data);
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.@object && !input.hasValidConnection && Unit.defaultValues[input.key] == null)
            {
                return MakeClickableForThisUnit("gameObject".VariableHighlight());
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

        private static Type GetCachedType(string typeId)
        {
            if (!typeCache.TryGetValue(typeId, out var type))
            {
                type = Type.GetType(typeId) ?? typeof(object);
                typeCache[typeId] = type;
            }
            return type;
        }

        private Type ResolveVariableType(VariableDeclarations declarations, string name)
        {
            if (declarations.IsDefined(name))
            {
                var id = declarations.GetDeclaration(name).typeHandle.Identification;
                return string.IsNullOrEmpty(id) ? typeof(object) : GetCachedType(id);
            }
            return typeof(object);
        }

    }
}