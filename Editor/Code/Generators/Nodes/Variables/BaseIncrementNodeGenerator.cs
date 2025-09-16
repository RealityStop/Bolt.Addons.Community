using System;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public abstract class BaseIncrementDecrementNodeGenerator<TUnit> : LocalVariableGenerator where TUnit : IncrementNode
    {
        private static readonly Dictionary<string, Type> typeCache = new();

        protected TUnit Unit => (TUnit)unit;
        protected string preVariable;
        protected string postVariable;

        protected abstract string OperationKeyword { get; }
        protected abstract string MethodName { get; }

        public BaseIncrementDecrementNodeGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (!Unit.assign.hasValidConnection)
                return $"/* ControlInput {Unit.assign.key} requires connection */".WarningHighlight();

            if (Unit.kind == VariableKind.Flow || Unit.kind == VariableKind.Graph)
            {
                if (output == Unit.preIncrement)
                    return MakeClickableForThisUnit(preVariable.VariableHighlight());
                if (output == Unit.postIncrement)
                    return MakeClickableForThisUnit(postVariable.VariableHighlight());
            }

            return output == Unit.preIncrement ? MakeClickableForThisUnit($"{variableName}.pre".VariableHighlight()) : output == Unit.postIncrement ? MakeClickableForThisUnit($"{variableName}.post".VariableHighlight()) : base.GenerateValue(output, data);
        }
        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            if (Unit.kind == VariableKind.Scene)
                NameSpaces = "UnityEngine.SceneManagement";
            else
                NameSpaces = string.Empty;

            return Unit.name.hasValidConnection ? GenerateConnectedNameCode(data, indent) : GenerateDirectNameCode(data, indent);
        }

        private string GenerateConnectedNameCode(ControlGenerationData data, int indent)
        {
            var output = string.Empty;
            var nameCode = GenerateValue(Unit.name, data);
            var kindAccessor = GetVariableKindAccessor(Unit.kind, string.Empty, data, out var variableType);

            if (string.IsNullOrEmpty(kindAccessor))
            {
                return MakeClickableForThisUnit(CodeUtility.ToolTip(
                    $"{Unit.kind} Variables do not support connected names",
                    "Could not generate Variable",
                    string.Empty));
            }

            variableName = data.AddLocalNameInScope($"{MethodName.ToLower()}Value");
            if (variableType == typeof(object)) variableType = typeof(float);

            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("var ".ConstructHighlight()) + MakeClickableForThisUnit(variableName.VariableHighlight()) + MakeClickableForThisUnit(" = ") +
                      CodeBuilder.CallCSharpUtilityMethod(Unit, MakeClickableForThisUnit(MethodName),
                          kindAccessor + MakeClickableForThisUnit($".Get<{variableType.As().CSharpName(false, true)}>(") + nameCode + MakeClickableForThisUnit(")")) + MakeClickableForThisUnit(";") + "\n";

            output += CodeBuilder.Indent(indent) + kindAccessor + MakeClickableForThisUnit(".Set(") + nameCode + MakeClickableForThisUnit(", " + variableName.VariableHighlight() + "." + "post".VariableHighlight() + ");") + "\n";

            output += GetNextUnit(Unit.assigned, data, indent);
            return output;
        }

        private string GenerateDirectNameCode(ControlGenerationData data, int indent)
        {
            var output = string.Empty;
            var name = Unit.defaultValues[Unit.name.key] as string;
            var legalName = name.LegalMemberName();
            variableName = legalName;

            if (Unit.kind == VariableKind.Flow || Unit.kind == VariableKind.Graph)
            {
                if (!data.ContainsNameInAnyScope(legalName))
                    output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit($"{"var".ConstructHighlight()} {legalName.VariableHighlight()} = {0.As().Code(false)};") + "\n";
                preVariable = data.AddLocalNameInScope($"pre{legalName}").VariableHighlight();
                postVariable = data.AddLocalNameInScope($"post{legalName}").VariableHighlight();
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit($"{"var".ConstructHighlight()} {preVariable} = {legalName.VariableHighlight()};") + "\n";
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit($"{legalName.VariableHighlight()}{OperationKeyword};") + "\n";
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit($"{"var".ConstructHighlight()} {postVariable} = {legalName.VariableHighlight()};") + "\n";
                output += GetNextUnit(Unit.assigned, data, indent);
                return output;
            }

            var localName = data.AddLocalNameInScope($"{MethodName.ToLower()}{legalName}").VariableHighlight();
            var accessor = GetVariableKindAccessor(Unit.kind, name, data, out var variableType);
            if (variableType == typeof(object)) variableType = typeof(float);

            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("var ".ConstructHighlight()) + MakeClickableForThisUnit(localName) + MakeClickableForThisUnit(" = ") +
                      CodeBuilder.CallCSharpUtilityMethod(Unit, MakeClickableForThisUnit(MethodName),
                          accessor + MakeClickableForThisUnit($".Get<{variableType.As().CSharpName(false, true)}>({name.As().Code(false)})")) + MakeClickableForThisUnit(";") + "\n";

            output += CodeBuilder.Indent(indent) + accessor + MakeClickableForThisUnit(".Set(") +
                      name.As().Code(false, Unit) + MakeClickableForThisUnit($", {localName.VariableHighlight()}.{"post".VariableHighlight()});") + "\n";

            variableName = localName;
            output += GetNextUnit(Unit.assigned, data, indent);

            return output;
        }

        protected string GetVariableKindAccessor(VariableKind kind, string name, ControlGenerationData data, out Type resolvedType)
        {
            resolvedType = typeof(object);
            var variables = typeof(Unity.VisualScripting.Variables).As().CSharpName(true, true);

            switch (kind)
            {
                case VariableKind.Object:
                    var target = GetTarget(data);
                    if (target != null && VisualScripting.Variables.Object(target).IsDefined(name))
                        resolvedType = ResolveVariableType(VisualScripting.Variables.Object(target), name);
                    return MakeClickableForThisUnit($"{variables}.Object(") + GenerateValue(Unit.@object, data) + MakeClickableForThisUnit(")");
                case VariableKind.Scene:
                    if (VisualScripting.Variables.ActiveScene.IsDefined(name))
                        resolvedType = ResolveVariableType(VisualScripting.Variables.ActiveScene, name);
                    return MakeClickableForThisUnit(GetSceneKind(data, variables));
                case VariableKind.Application:
                    if (VisualScripting.Variables.Application.IsDefined(name))
                        resolvedType = ResolveVariableType(VisualScripting.Variables.Application, name);
                    return MakeClickableForThisUnit($"{variables}.{"Application".VariableHighlight()}");
                case VariableKind.Saved:
                    if (VisualScripting.Variables.Saved.IsDefined(name))
                        resolvedType = ResolveVariableType(VisualScripting.Variables.Saved, name);
                    return MakeClickableForThisUnit($"{variables}.{"Saved".VariableHighlight()}");
                default:
                    return string.Empty;
            }
        }

        private string GetSceneKind(ControlGenerationData data, string variables)
        {
            return typeof(Component).IsAssignableFrom(data.ScriptType) ? variables + ".Scene(" + "gameObject".VariableHighlight() + "." + "scene".VariableHighlight() + ")" : variables + "." + "Application".VariableHighlight();
        }

        private GameObject GetTarget(ControlGenerationData data)
        {
            var objInput = Unit.@object;
            if (!objInput.hasValidConnection && Unit.defaultValues[objInput.key] == null && data.TryGetGameObject(out var gameObject))
                return gameObject;
            if (!objInput.hasValidConnection && Unit.defaultValues[objInput.key] != null)
                return Unit.defaultValues[objInput.key].ConvertTo<GameObject>();
            if (objInput.hasValidConnection && data.TryGetGraphPointer(out var graphPointer) && CanPredictConnection(objInput, data))
            {
                try
                {
                    return Flow.Predict(objInput.GetPesudoSource(), graphPointer.AsReference()) as GameObject;
                }
                catch (InvalidOperationException ex)
                {
                    Debug.LogError(ex);
                }
            }
            return null;
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.@object && !input.hasValidConnection && Unit.defaultValues[input.key] == null)
                return MakeClickableForThisUnit("gameObject".VariableHighlight());

            if (input == Unit.@object)
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