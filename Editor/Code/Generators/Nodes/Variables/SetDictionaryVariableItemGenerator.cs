using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(SetDictionaryVariableItem))]
    public class SetDictionaryVariableItemGenerator : LocalVariableGenerator
    {
        private static readonly Dictionary<string, Type> typeCache = new Dictionary<string, Type>();

        private SetDictionaryVariableItem Unit => unit as SetDictionaryVariableItem;
        public SetDictionaryVariableItemGenerator(Unit unit) : base(unit)
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
                        NameSpaces = "Unity.VisualScripting.Community";
                        variableType = ResolveVariableType(VisualScripting.Variables.Object(GetTarget(data)), name);
                        break;
                    case VariableKind.Scene:
                        kind = MakeClickableForThisUnit(GetSceneKind(data, variables));
                        variableType = ResolveVariableType(VisualScripting.Variables.ActiveScene, name);
                        NameSpaces += ",Unity.VisualScripting.Community";
                        break;
                    case VariableKind.Application:
                        kind = MakeClickableForThisUnit(variables + "." + "Application".VariableHighlight());
                        NameSpaces = "Unity.VisualScripting.Community";
                        variableType = ResolveVariableType(VisualScripting.Variables.Application, name);
                        break;
                    case VariableKind.Saved:
                        kind = MakeClickableForThisUnit(variables + "." + "Saved".VariableHighlight());
                        NameSpaces = "Unity.VisualScripting.Community";
                        variableType = ResolveVariableType(VisualScripting.Variables.Saved, name);
                        break;
                }
                output += GetCode(kind, indent, data);
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
                        variableType = ResolveVariableType(VisualScripting.Variables.Object(target), name);
                        NameSpaces = "Unity.VisualScripting.Community";
                        return GetCode(MakeClickableForThisUnit($"{variables}" + "." + $"Object(") + GenerateValue(Unit.@object, data) + MakeClickableForThisUnit(")"), indent, data) + GetNextUnit(Unit.assigned, data, indent);
                    case VariableKind.Scene:
                        variableType = ResolveVariableType(VisualScripting.Variables.ActiveScene, name);
                        NameSpaces += ",Unity.VisualScripting.Community";
                        return GetCode(MakeClickableForThisUnit($"{variables}" + "." + "ActiveScene".VariableHighlight()), indent, data) + GetNextUnit(Unit.assigned, data, indent);
                    case VariableKind.Application:
                        variableType = ResolveVariableType(VisualScripting.Variables.Application, name);
                        NameSpaces = "Unity.VisualScripting.Community";
                        return GetCode(MakeClickableForThisUnit(GetSceneKind(data, variables)), indent, data) + GetNextUnit(Unit.assigned, data, indent);
                    case VariableKind.Saved:
                        variableType = ResolveVariableType(VisualScripting.Variables.Saved, name);
                        NameSpaces = "Unity.VisualScripting.Community";
                        return GetCode(MakeClickableForThisUnit($"{variables}" + "." + "Saved".VariableHighlight()), indent, data) + GetNextUnit(Unit.assigned, data, indent);
                }
                var _name = data.GetVariableName(name.LegalMemberName());
                CodeBuilder.Indent(indent);
                if (data.ContainsNameInAnyScope(_name))
                {
                    NameSpaces = string.Empty;
                    variableName = _name.LegalMemberName();
                    variableType = data.GetVariableType(_name);
                    data.SetExpectedType(GetDictionaryValueType(variableType));
                    var inputCode = GenerateValue(Unit.newValue, data);
                    data.RemoveExpectedType();
                    var code = MakeClickableForThisUnit($"{variableName.VariableHighlight()}[") + GenerateValue(Unit.key, data) + MakeClickableForThisUnit("]" + " = ") + inputCode + MakeClickableForThisUnit(";");
                    data.CreateSymbol(Unit, variableType);
                    output += CodeBuilder.Indent(indent) + code + "\n";
                    output += GetNextUnit(Unit.assigned, data, indent);
                    return output;
                }
                else
                {
                    return MakeClickableForThisUnit($"/* Could not find variable with name \"{name}\" */".WarningHighlight()) + "\n";
                }
            }
        }

        private string GetSceneKind(ControlGenerationData data, string variables)
        {
            return typeof(Component).IsAssignableFrom(data.ScriptType) ? variables + ".Scene(" + "gameObject".VariableHighlight() + "." + "scene".VariableHighlight() + ")" : variables + "." + "Application".VariableHighlight();
        }

        private string GetCode(string kind, int indent, ControlGenerationData data)
        {
            var nameCode = GenerateValue(Unit.name, data);
            var builder = Unit.CreateClickableString();
            builder.Indent(indent);
            // Todo : use GeneratorData to store and reuse local variable instead of creating a new one each time.
            var variable = data.AddLocalNameInScope("dictionaryVariable", variableType).VariableHighlight();
            builder.Clickable("var ".ConstructHighlight()).Clickable(variable).Equal(true).CallCSharpUtilityExtensitionMethod(kind, MakeClickableForThisUnit("GetDictionaryVariable"), nameCode).Clickable(";").NewLine();
            builder.Indent(indent);
            builder.Clickable(variable).Clickable("[").Ignore(GenerateValue(Unit.key, data)).Clickable("]").Equal(true).Ignore(Unit.newValue.hasValidConnection ? GenerateValue(Unit.newValue, data) : MakeClickableForThisUnit("null".ConstructHighlight())).Clickable(";").NewLine();
            return builder.ToString();
        }

        private Type GetDictionaryValueType(Type type)
        {
            if (type.IsGenericType && typeof(IDictionary).IsAssignableFrom(type))
                return type.GetGenericArguments()[1];
            return typeof(object);
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
                return GenerateValue(Unit.newValue, data);
            }
            else if (output == Unit.output && !Unit.name.hasValidConnection)
            {
                if (data.ContainsNameInAnyScope(variableName))
                    return MakeClickableForThisUnit(variableName.VariableHighlight() + "[") + GenerateValue(Unit.key, data) + MakeClickableForThisUnit("]");
                else return MakeClickableForThisUnit($"/* Could not find variable with name \"{variableName}\" */".WarningHighlight());
            }
            else return GenerateValue(Unit.newValue, data);
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.@object && !input.hasValidConnection && Unit.defaultValues[input.key] == null)
            {
                return MakeClickableForThisUnit("gameObject".VariableHighlight());
            }
            if (input == Unit.newValue)
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
            if (!declarations.IsDefined(name))
                return typeof(object);

            var declaration = declarations.GetDeclaration(name);

#if VISUAL_SCRIPTING_1_7
            var id = declaration.typeHandle.Identification;
            return string.IsNullOrEmpty(id) ? typeof(object) : GetCachedType(id);
#else
            return declaration.value != null ? declaration.value.GetType() : typeof(object);
#endif
        }
    }
}