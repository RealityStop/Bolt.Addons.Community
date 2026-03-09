using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(GetDictionaryVariableItem))]
    public class GetDictionaryVariableItemGenerator : LocalVariableGenerator
    {
        private GetDictionaryVariableItem Unit => unit as GetDictionaryVariableItem;
        public GetDictionaryVariableItemGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (Unit.name.hasValidConnection || Unit.kind == VariableKind.Object)
            {
                if (Unit.kind == VariableKind.Object && !Unit.name.hasValidConnection)
                {
                    GenerateDisconnectedVariableCodeInternal(data, writer);
                }
                else
                {
                    GenerateConnectedVariableCodeInternal(data, writer);
                }
            }
            else
            {
                GenerateDisconnectedVariableCodeInternal(data, writer);
            }
        }

        private void GenerateConnectedVariableCodeInternal(ControlGenerationData data, CodeWriter writer)
        {
            variableName = data.TryGetGraphPointer(out var graphPointer) && CanPredictConnection(Unit.name, data) ? Flow.Predict<string>(Unit.name, graphPointer.AsReference()) : "";
            variableType = GetVariableType(variableName, data, true);
            if (data.GetExpectedType()?.IsAssignableFrom(variableType) ?? false)
                data.MarkExpectedTypeMet(variableType);

            data.CreateSymbol(Unit, variableType ?? typeof(object));

            WriteKind(writer, data);
            writer.Write(".GetDictionaryVariable");
            writer.Parentheses(inside => GenerateValue(Unit.name, data, inside));
            writer.Brackets(inside => GenerateValue(Unit.key, data, inside));
        }

        private void WriteKind(CodeWriter writer, ControlGenerationData data)
        {
            switch (Unit.kind)
            {
                case VariableKind.Object:
                    writer.Write(writer.GetTypeNameHighlighted(typeof(Unity.VisualScripting.Variables)) + ".Object").Parentheses(w => GenerateValue(Unit.@object, data, w));
                    break;
                case VariableKind.Scene:
                    writer.Write(GetSceneKind(data, writer.GetTypeNameHighlighted(typeof(Unity.VisualScripting.Variables))));
                    break;
                case VariableKind.Application:
                    writer.Write(writer.GetTypeNameHighlighted(typeof(Unity.VisualScripting.Variables)) + "." + "Application".VariableHighlight());
                    break;
                case VariableKind.Saved:
                    writer.Write(writer.GetTypeNameHighlighted(typeof(Unity.VisualScripting.Variables)) + "." + "Saved".VariableHighlight());
                    break;
            }
        }

        private void GenerateDisconnectedVariableCodeInternal(ControlGenerationData data, CodeWriter writer)
        {
            var name = (Unit.defaultValues[Unit.name.key] as string).LegalMemberName();
            variableType = GetVariableType(name, data, true);

            if (Unit.kind == VariableKind.Object || Unit.kind == VariableKind.Scene || Unit.kind == VariableKind.Application || Unit.kind == VariableKind.Saved)
            {
                var expectedType = data.GetExpectedType();
                var hasExpectedType = expectedType != null;
                Type targetType = null;
                var declaration = IsVariableDefined(data, name) ? GetVariableDeclaration(data, name) : null;

#if VISUAL_SCRIPTING_1_7
                var isExpectedType =
                    hasExpectedType &&
                    (
                        (variableType != null && expectedType.IsAssignableFrom(variableType)) ||
                        (declaration != null && !string.IsNullOrEmpty(declaration.typeHandle.Identification) &&
                            expectedType.IsAssignableFrom(Type.GetType(declaration.typeHandle.Identification))) ||
                        (data.TryGetVariableType(data.GetVariableName(name), out targetType) &&
                            expectedType.IsAssignableFrom(targetType))
                    );
#else
                var isExpectedType =
                    hasExpectedType &&
                    (
                        (variableType != null && expectedType.IsAssignableFrom(variableType)) ||
                        (declaration != null && declaration.value != null &&
                            expectedType.IsAssignableFrom(declaration.value.GetType())) ||
                        (data.TryGetVariableType(data.GetVariableName(name), out targetType) &&
                            expectedType.IsAssignableFrom(targetType))
                    );
#endif
                if (isExpectedType)
                    data.MarkExpectedTypeMet(variableType);

                if (!IsVariableDefined(data, name))
                {
                    if (Unit.specifyFallback)
                        GenerateValue(Unit.fallback, data, writer);
                    else
                    {
                        using (writer.CodeDiagnosticScope($"Could not find variable with name \"{name}\"", CodeDiagnosticKind.Warning))
                            writer.Error($"Variable not found");
                    }
                    return;
                }

                WriteKind(writer, data);
                writer.Write(".GetDictionaryVariable");
                writer.Parentheses(inside => writer.Object(name));
                writer.Brackets(inside => GenerateValue(Unit.key, data, inside));
            }
            else
            {
                variableName = data.GetVariableName(name);
                data.CreateSymbol(Unit, variableType);
                var expectedType = data.GetExpectedType();
                var hasExpectedType = expectedType != null;
                Type targetType = null;
                var declaration = IsVariableDefined(data, name) ? GetVariableDeclaration(data, name) : null;

#if VISUAL_SCRIPTING_1_7
                var isExpectedType =
                    (hasExpectedType && variableType != null && expectedType.IsAssignableFrom(variableType))
                    || (hasExpectedType && declaration != null &&
                        !string.IsNullOrEmpty(declaration.typeHandle.Identification) &&
                        expectedType.IsAssignableFrom(Type.GetType(declaration.typeHandle.Identification)))
                    || (hasExpectedType && data.TryGetVariableType(data.GetVariableName(name), out targetType) &&
                        expectedType.IsAssignableFrom(targetType));
#else
                var isExpectedType =
                    (hasExpectedType && variableType != null && expectedType.IsAssignableFrom(variableType))
                    || (hasExpectedType && declaration != null &&
                        declaration.value != null &&
                        expectedType.IsAssignableFrom(declaration.value.GetType()))
                    || (hasExpectedType && data.TryGetVariableType(data.GetVariableName(name), out targetType) &&
                        expectedType.IsAssignableFrom(targetType));
#endif
                if (isExpectedType)
                    data.MarkExpectedTypeMet(variableType);

                if (!data.ContainsNameInAnyScope(variableName))
                {
                    if (Unit.specifyFallback)
                        GenerateValue(Unit.fallback, data, writer);
                    else
                    {
                        using (writer.CodeDiagnosticScope($"Could not find variable with name \"{name}\"", CodeDiagnosticKind.Warning))
                            writer.Error($"Variable not found");
                    }
                    return;
                }

                writer.Write(variableName.VariableHighlight());
                writer.Brackets(inside => GenerateValue(Unit.key, data, inside));
            }
        }

        private string GetSceneKind(ControlGenerationData data, string variables)
        {
            return typeof(Component).IsAssignableFrom(data.ScriptType) ? variables + ".Scene(" + "gameObject".VariableHighlight() + "." + "scene".VariableHighlight() + ")" : variables + "." + "ActiveScene".VariableHighlight();
        }

        public override IEnumerable<string> GetNamespaces()
        {
            if (Unit.kind == VariableKind.Scene)
                yield return "UnityEngine.SceneManagement";
        }

        private Type GetVariableType(string name, ControlGenerationData data, bool checkDecleration)
        {
            if (checkDecleration)
            {
                var isDefined = IsVariableDefined(data, name);
                if (isDefined)
                {
                    var declaration = GetVariableDeclaration(data, name);
#if VISUAL_SCRIPTING_1_7
                    return declaration?.typeHandle.Identification != null
                        ? Type.GetType(declaration.typeHandle.Identification)
                        : null;
#else
                    return declaration?.value != null
                        ? declaration.value.GetType()
                        : null;
#endif
                }
            }
            Type targetType = null;
            return data.TryGetVariableType(data.GetVariableName(name), out targetType) ? targetType : data.GetExpectedType() ?? typeof(object);
        }

        private bool IsVariableDefined(ControlGenerationData data, string name)
        {
            var target = Unit.kind == VariableKind.Object ? GetTarget(data) : null;
            return Unit.kind switch
            {
                VariableKind.Object => target != null && VisualScripting.Variables.Object(target).IsDefined(name),
                VariableKind.Scene => VisualScripting.Variables.ActiveScene.IsDefined(name),
                VariableKind.Application => VisualScripting.Variables.Application.IsDefined(name),
                VariableKind.Saved => VisualScripting.Variables.Saved.IsDefined(name),
                _ => false,
            };
        }

        private VariableDeclaration GetVariableDeclaration(ControlGenerationData data, string name)
        {
            var target = Unit.kind == VariableKind.Object ? GetTarget(data) : null;
            return Unit.kind switch
            {
                VariableKind.Graph => unit.graph.variables.GetDeclaration(name),
                VariableKind.Object => target != null ? VisualScripting.Variables.Object(target).GetDeclaration(name) : null,
                VariableKind.Scene => VisualScripting.Variables.ActiveScene.GetDeclaration(name),
                VariableKind.Application => VisualScripting.Variables.Application.GetDeclaration(name),
                VariableKind.Saved => VisualScripting.Variables.Saved.GetDeclaration(name),
                _ => null,
            };
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
    }
}