using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(PlusEquals))]
    public class PlusEqualsGenerator : LocalVariableGenerator
    {
        public PlusEqualsGenerator(Unit unit) : base(unit)
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
        private static readonly Dictionary<string, Type> typeCache = new();

        private PlusEquals Unit => unit as PlusEquals;
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
                var nameCode = base.GenerateValue(Unit.name, data);
                data.SetExpectedType(variableType);
                var code = Unit.CreateClickableString(CodeBuilder.Indent(indent)).Ignore(kind).OpenParentheses(".Set").Ignore(base.GenerateValue(Unit.name, data)).Comma(" ").Ignore(kind).Clickable($".Get<{variableType.As().CSharpName(false, true)}>(").Ignore(base.GenerateValue(Unit.name, data)).CloseParentheses().Clickable(" + ").Ignore(GenerateValue(Unit.amount, data)).EndStatement().Ignore("\n");
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
                        {
                            var target = GetTarget(data);
                            if (target != null && VisualScripting.Variables.Object(target).IsDefined(name))
                            {
                                variableType = ResolveVariableType(VisualScripting.Variables.Object(target), name);
                            }
                            else
                                variableType = typeof(float);
                            var kind = MakeClickableForThisUnit($"{variables}" + ".Object(") + GenerateValue(Unit.@object, data) + MakeClickableForThisUnit(")");
                            return Unit.CreateClickableString(CodeBuilder.Indent(indent)).Ignore(kind).OpenParentheses(".Set").Ignore(base.GenerateValue(Unit.name, data)).Comma(" ").Ignore(kind).Clickable($".Get<{variableType.As().CSharpName(false, true)}>(").Ignore(base.GenerateValue(Unit.name, data)).CloseParentheses().Clickable(" + ").Ignore(GenerateValue(Unit.amount, data)).EndStatement().Ignore("\n").Ignore(GetNextUnit(Unit.assigned, data, indent));
                        }
                    case VariableKind.Scene:
                        {
                            if (VisualScripting.Variables.ActiveScene.IsDefined(name))
                            {
                                variableType = ResolveVariableType(VisualScripting.Variables.ActiveScene, name);
                            }
                            else
                                variableType = typeof(float);
                            var kind = MakeClickableForThisUnit(GetSceneKind(data, variables));
                            return Unit.CreateClickableString(CodeBuilder.Indent(indent)).Ignore(kind).OpenParentheses(".Set").Ignore(base.GenerateValue(Unit.name, data)).Comma(" ").Ignore(kind).Clickable($".Get<{variableType.As().CSharpName(false, true)}>(").Ignore(base.GenerateValue(Unit.name, data)).CloseParentheses().Clickable(" + ").Ignore(GenerateValue(Unit.amount, data)).EndStatement().Ignore("\n").Ignore(GetNextUnit(Unit.assigned, data, indent));
                        }
                    case VariableKind.Application:
                        {
                            if (VisualScripting.Variables.Application.IsDefined(name))
                            {
                                variableType = ResolveVariableType(VisualScripting.Variables.Application, name);
                            }
                            else
                                variableType = typeof(float);

                            var kind = MakeClickableForThisUnit($"{variables}" + "." + "Application".VariableHighlight());
                            return Unit.CreateClickableString(CodeBuilder.Indent(indent)).Ignore(kind).OpenParentheses(".Set").Ignore(base.GenerateValue(Unit.name, data)).Comma(" ").Ignore(kind).Clickable($".Get<{variableType.As().CSharpName(false, true)}>(").Ignore(base.GenerateValue(Unit.name, data)).CloseParentheses().Clickable(" + ").Ignore(GenerateValue(Unit.amount, data)).EndStatement().Ignore("\n").Ignore(GetNextUnit(Unit.assigned, data, indent));
                        }
                    case VariableKind.Saved:
                        {
                            if (VisualScripting.Variables.Saved.IsDefined(name))
                            {
                                variableType = ResolveVariableType(VisualScripting.Variables.Saved, name);
                            }
                            else
                                variableType = typeof(float);
                            var kind = MakeClickableForThisUnit($"{variables}" + "." + "Saved".VariableHighlight());
                            return Unit.CreateClickableString(CodeBuilder.Indent(indent)).Ignore(kind).OpenParentheses(".Set").Ignore(base.GenerateValue(Unit.name, data)).Comma(" ").Ignore(kind).Clickable($".Get<{variableType.As().CSharpName(false, true)}>(").Ignore(base.GenerateValue(Unit.name, data)).CloseParentheses().Clickable(" + ").Ignore(GenerateValue(Unit.amount, data)).EndStatement().Ignore("\n").Ignore(GetNextUnit(Unit.assigned, data, indent));
                        }
                }
                var _name = data.GetVariableName(name.LegalMemberName());
                CodeBuilder.Indent(indent); // To Ensure indentation is correct
                if (data.ContainsNameInAnyScope(_name))
                {
                    var inputCode = GenerateValue(Unit.amount, data);
                    variableName = _name.LegalMemberName();
                    variableType = data.GetVariableType(_name);
                    data.CreateSymbol(Unit, variableType);
                    output += Unit.CreateClickableString(CodeBuilder.Indent(indent)).Clickable(variableName.VariableHighlight()).Clickable(" += ").Ignore(inputCode).Clickable(";").Ignore("\n");
                    output += GetNextUnit(Unit.assigned, data, indent);
                    return output;
                }
                else
                {
                    output += Unit.CreateClickableString(CodeBuilder.Indent(indent)).Clickable($"/* Variable {_name} could not be found */".WarningHighlight()).Ignore("\n");
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
            if (!Unit.assign.hasValidConnection) return $"/* ControlInput {Unit.assign.key} requires connection on {Unit.GetType()} with variable name ({base.GenerateValue(Unit.name, data)}) */".WarningHighlight();
            if (output == Unit.postIncrement && (Unit.kind == VariableKind.Object || Unit.kind == VariableKind.Scene || Unit.kind == VariableKind.Application || Unit.kind == VariableKind.Saved))
            {
                var builder = Unit.CreateClickableString();
                var variables = typeof(VisualScripting.Variables).As().CSharpName(true, true);
                var name = base.GenerateValue(Unit.name, data);

                switch (Unit.kind)
                {
                    case VariableKind.Object:
                        builder.Clickable(variables).Parentheses(".Object", inside => inside.Ignore(GenerateValue(Unit.@object, data))).Clickable($".Get<{variableType.As().CSharpName(false, true)}>").Parentheses(inside => inside.Ignore(name));
                        break;

                    case VariableKind.Scene:
                        builder.Clickable(variables).Clickable(".ActiveScene".VariableHighlight()).Clickable($".Get<{variableType.As().CSharpName(false, true)}>").Parentheses(inside => inside.Ignore(name));
                        break;

                    case VariableKind.Application:
                        builder.Clickable(variables).Clickable(".Application".VariableHighlight()).Clickable($".Get<{variableType.As().CSharpName(false, true)}>").Parentheses(inside => inside.Ignore(name));
                        break;

                    case VariableKind.Saved:
                        builder.Clickable(variables).Clickable(".Saved".VariableHighlight()).Clickable($".Get<{variableType.As().CSharpName(false, true)}>").Parentheses(inside => inside.Ignore(name));
                        break;
                }

                return builder.ToString();
            }
            else if (output == Unit.postIncrement && !Unit.name.hasValidConnection)
            {
                return MakeClickableForThisUnit(variableName.VariableHighlight());
            }
            return GenerateValue(Unit.amount, data);
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.@object && !input.hasValidConnection && Unit.defaultValues[input.key] == null)
            {
                return MakeClickableForThisUnit("gameObject".VariableHighlight());
            }
            if (input == Unit.amount)
            {
                data.SetExpectedType(variableType ?? typeof(float));
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