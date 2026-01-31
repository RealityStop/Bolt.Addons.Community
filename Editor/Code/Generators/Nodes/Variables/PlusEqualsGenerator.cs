using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(PlusEquals))]
    public sealed class PlusEqualsGenerator : LocalVariableGenerator
    {
        public PlusEqualsGenerator(Unit unit) : base(unit)
        {
        }

        public override IEnumerable<string> GetNamespaces()
        {
            if (Unit.kind == VariableKind.Scene)
                yield return "UnityEngine.SceneManagement";
        }

        private static readonly Dictionary<string, Type> typeCache = new Dictionary<string, Type>();

        private PlusEquals Unit => unit as PlusEquals;
        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (Unit.name.hasValidConnection)
            {
                GenerateConnectedNameCodeInternal(data, writer);
            }
            else
            {
                GenerateDirectNameCodeInternal(data, writer);
            }
        }
        private static string variablesTypeString = typeof(Unity.VisualScripting.Variables).As().CSharpName(true, true);
        private void GenerateConnectedNameCodeInternal(ControlGenerationData data, CodeWriter writer)
        {
            var name = data.TryGetGraphPointer(out var graphPointer) && CanPredictConnection(Unit.name, data) ? Flow.Predict<string>(Unit.name, graphPointer.AsReference()) : Unit.defaultValues[Unit.name.key] as string;

            switch (Unit.kind)
            {
                case VariableKind.Flow:
                case VariableKind.Graph:
                    writer.WriteErrorDiagnostic($"{Unit.kind} Variables do not support connected names", $"Could not generate {Unit.kind} Variable", WriteOptions.IndentedNewLineAfter);
                    GenerateExitControl(Unit.assigned, data, writer);
                    return;

                case VariableKind.Object:
                    if (VisualScripting.Variables.Object(GetTarget(data)).IsDefined(name))
                        variableType = ResolveVariableType(VisualScripting.Variables.Object(GetTarget(data)), name);
                    else
                        variableType = typeof(float);
                    break;

                case VariableKind.Scene:
                    if (VisualScripting.Variables.ActiveScene.IsDefined(name))
                        variableType = ResolveVariableType(VisualScripting.Variables.ActiveScene, name);
                    else
                        variableType = typeof(float);
                    break;

                case VariableKind.Application:
                    if (VisualScripting.Variables.Application.IsDefined(name))
                        variableType = ResolveVariableType(VisualScripting.Variables.Application, name);
                    else
                        variableType = typeof(float);
                    break;

                case VariableKind.Saved:
                    if (VisualScripting.Variables.Saved.IsDefined(name))
                        variableType = ResolveVariableType(VisualScripting.Variables.Saved, name);
                    else
                        variableType = typeof(float);
                    break;
            }

            writer.WriteIndented();

            WriteKind(writer, data);
            writer.Write(".Set(");
            GenerateValue(Unit.name, data, writer);
            writer.ParameterSeparator();
            WriteKind(writer, data);
            writer.Write(".Get<");
            writer.Write(variableType.As().CSharpName(false, true).TypeHighlight());
            writer.Write(">(");
            GenerateValue(Unit.name, data, writer);
            writer.Write(")");
            writer.Write(" + ");
            GenerateValue(Unit.amount, data, writer);
            writer.Write(");");
            writer.NewLine();

            GenerateExitControl(Unit.assigned, data, writer);
        }

        private void WriteKind(CodeWriter writer, ControlGenerationData data)
        {
            switch (Unit.kind)
            {
                case VariableKind.Object:
                    writer.Write(variablesTypeString + ".Object").Parentheses(w => GenerateValue(Unit.@object, data, w));
                    break;
                case VariableKind.Scene:
                    writer.Write(GetSceneKind(data, variablesTypeString));
                    break;
                case VariableKind.Application:
                    writer.Write(variablesTypeString + "." + "Application".VariableHighlight());
                    break;
                case VariableKind.Saved:
                    writer.Write(variablesTypeString + "." + "Saved".VariableHighlight());
                    break;
            }
        }

        private void GenerateDirectNameCodeInternal(ControlGenerationData data, CodeWriter writer)
        {
            var name = Unit.defaultValues[Unit.name.key] as string;

            if (Unit.kind == VariableKind.Object || Unit.kind == VariableKind.Scene || Unit.kind == VariableKind.Application || Unit.kind == VariableKind.Saved)
            {
                switch (Unit.kind)
                {
                    case VariableKind.Object:
                        {
                            var target = GetTarget(data);
                            variableType = target != null && VisualScripting.Variables.Object(target).IsDefined(name)
                                ? ResolveVariableType(VisualScripting.Variables.Object(target), name)
                                : typeof(float);
                            GenerateConnectedKindCodeInternal(data, writer);
                            return;
                        }
                    case VariableKind.Scene:
                        {
                            variableType = VisualScripting.Variables.ActiveScene.IsDefined(name)
                                ? ResolveVariableType(VisualScripting.Variables.ActiveScene, name)
                                : typeof(float);
                            GenerateConnectedKindCodeInternal(data, writer);
                            return;
                        }
                    case VariableKind.Application:
                        {
                            variableType = VisualScripting.Variables.Application.IsDefined(name)
                                ? ResolveVariableType(VisualScripting.Variables.Application, name)
                                : typeof(float);
                            GenerateConnectedKindCodeInternal(data, writer);
                            return;
                        }
                    case VariableKind.Saved:
                        {
                            variableType = VisualScripting.Variables.Saved.IsDefined(name)
                                ? ResolveVariableType(VisualScripting.Variables.Saved, name)
                                : typeof(float);
                            GenerateConnectedKindCodeInternal(data, writer);
                            return;
                        }
                }
            }

            // Flow/Graph variable kinds use direct scope-local variables
            var _name = data.GetVariableName(name.LegalMemberName());
            if (data.ContainsNameInAnyScope(_name))
            {
                variableName = _name.LegalMemberName();
                variableType = data.GetVariableType(_name);
                data.CreateSymbol(Unit, variableType);

                writer.WriteIndented();
                writer.Write(variableName.VariableHighlight());
                writer.Write(" += ");
                GenerateValue(Unit.amount, data, writer);
                writer.Write(";");
                writer.NewLine();
                GenerateExitControl(Unit.assigned, data, writer);
            }
            else
            {
                writer.WriteIndented().Error($"Could not find variable").NewLine();
                GenerateExitControl(Unit.assigned, data, writer);
            }
        }

        private void GenerateConnectedKindCodeInternal(ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented();
            WriteKind(writer, data);
            writer.Write(".Set(");
            GenerateValue(Unit.name, data, writer);
            writer.ParameterSeparator();
            WriteKind(writer, data);
            writer.Write(".Get<");
            writer.Write(variableType.As().CSharpName(false, true));
            writer.Write(">(");
            GenerateValue(Unit.name, data, writer);
            writer.Write(")");
            writer.Write(" + ");
            GenerateValue(Unit.amount, data, writer);
            writer.Write(");");
            writer.NewLine();
            GenerateExitControl(Unit.assigned, data, writer);
        }

        private string GetSceneKind(ControlGenerationData data, string variables)
        {
            return typeof(Component).IsAssignableFrom(data.ScriptType) ? variables + ".Scene(" + "gameObject".VariableHighlight() + "." + "scene".VariableHighlight() + ")" : variables + "." + "ActiveScene".VariableHighlight();
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

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (!Unit.assign.hasValidConnection)
            {
                writer.WriteErrorDiagnostic($"ControlInput {Unit.assign.key} requires connection on {Unit.GetType()}", "Missing Connection");
                return;
            }

            if (output == Unit.postIncrement && (Unit.kind == VariableKind.Object || Unit.kind == VariableKind.Scene || Unit.kind == VariableKind.Application || Unit.kind == VariableKind.Saved))
            {
                var variables = writer.GetTypeNameHighlighted(typeof(VisualScripting.Variables));

                switch (Unit.kind)
                {
                    case VariableKind.Object:
                        writer.Write(variables);
                        writer.Write(".Object".VariableHighlight());
                        writer.Write("(");
                        GenerateValue(Unit.@object, data, writer);
                        writer.Write(")");
                        writer.Write($".Get<{variableType.As().CSharpName(false, true)}>".TypeHighlight());
                        writer.Write("(");
                        GenerateValue(Unit.name, data, writer);
                        writer.Write(")");
                        break;

                    case VariableKind.Scene:
                        writer.Write(variables);
                        writer.Write(GetSceneKind(data, variablesTypeString));
                        writer.Write($".Get<{variableType.As().CSharpName(false, true)}>".TypeHighlight());
                        writer.Write("(");
                        GenerateValue(Unit.name, data, writer);
                        writer.Write(")");
                        break;

                    case VariableKind.Application:
                        writer.Write(variables);
                        writer.Write(".Application".VariableHighlight());
                        writer.Write($".Get<{variableType.As().CSharpName(false, true)}>".TypeHighlight());
                        writer.Write("(");
                        GenerateValue(Unit.name, data, writer);
                        writer.Write(")");
                        break;

                    case VariableKind.Saved:
                        writer.Write(variables);
                        writer.Write(".Saved".VariableHighlight());
                        writer.Write($".Get<{variableType.As().CSharpName(false, true)}>".TypeHighlight());
                        writer.Write("(");
                        GenerateValue(Unit.name, data, writer);
                        writer.Write(")");
                        break;
                }
            }
            else if (output == Unit.postIncrement && !Unit.name.hasValidConnection)
            {
                writer.Write(variableName.VariableHighlight());
            }
            else
            {
                GenerateValue(Unit.amount, data, writer);
            }
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