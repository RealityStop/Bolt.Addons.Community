using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(SetVariable))]
    public class SetVariableGenerator : LocalVariableGenerator
    {
        private static readonly Dictionary<string, Type> typeCache = new Dictionary<string, Type>();

        private SetVariable Unit => unit as SetVariable;
        public SetVariableGenerator(Unit unit) : base(unit)
        {
        }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "UnityEngine.SceneManagement";
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            string variablesType = typeof(VisualScripting.Variables).As().CSharpName(true, true);
            bool hasConnectedName = Unit.name.hasValidConnection;

            data.TryGetGraphPointer(out GraphPointer graphPointer);

            string name = hasConnectedName
                ? graphPointer != null && CanPredictConnection(Unit.name, data)
                    ? Flow.Predict<string>(Unit.name, graphPointer.AsReference())
                    : Unit.defaultValues[Unit.name.key] as string
                : Unit.defaultValues[Unit.name.key] as string;

            if (string.IsNullOrEmpty(name))
                return;

            if (hasConnectedName)
            {
                switch (Unit.kind)
                {
                    case VariableKind.Flow:
                        using (writer.CodeDiagnosticScope("Flow Variables do not support connected names", CodeDiagnosticKind.Error))
                            writer.Error("Could not generate Flow Variable");
                        GenerateExitControl(Unit.assigned, data, writer);
                        return;

                    case VariableKind.Graph:
                        using (writer.CodeDiagnosticScope("Graph Variables do not support connected names", CodeDiagnosticKind.Error))
                        {
                            writer.Error("Could not generate Graph Variable");
                        }
                        GenerateExitControl(Unit.assigned, data, writer);
                        return;

                    case VariableKind.Object:
                        writer.WriteIndented(variablesType + ".Object(");
                        GenerateValue(Unit.@object, data, writer);
                        writer.Write(")");
                        ResolveVariableTypeSafe(VisualScripting.Variables.Object(GetTarget(data)), name, data);
                        break;

                    case VariableKind.Scene:
                        var variables = VisualScripting.Variables.ActiveScene;
                        if (graphPointer != null && graphPointer.scene != null)
                        {
                            VisualScripting.Variables.Scene(graphPointer.scene);
                        }
                        writer.WriteIndented(GetSceneKind(data, variablesType));
                        ResolveVariableTypeSafe(variables, name, data);
                        break;

                    case VariableKind.Application:
                        writer.WriteIndented(variablesType + "." + "Application".VariableHighlight());
                        ResolveVariableTypeSafe(VisualScripting.Variables.Application, name, data);
                        break;

                    case VariableKind.Saved:
                        writer.WriteIndented(variablesType + "." + "Saved".VariableHighlight());
                        ResolveVariableTypeSafe(VisualScripting.Variables.Saved, name, data);
                        break;
                }

                writer.Write(".Set(");
                GenerateValue(Unit.name, data, writer);
                writer.ParameterSeparator();

                if (Unit.input.hasValidConnection)
                {
                    using (data.Expect(variableType))
                        GenerateValue(Unit.input, data, writer);
                }
                else
                {
                    writer.Null();
                }

                writer.Write(");").NewLine();

                GenerateExitControl(Unit.assigned, data, writer);

                return;
            }

            switch (Unit.kind)
            {
                case VariableKind.Object:
                    ResolveVariableTypeSafe(
                        GetTarget(data) != null ? VisualScripting.Variables.Object(GetTarget(data)) : null,
                        name, data);

                    writer.WriteIndented().InvokeMember(
                        writer.Action(w =>
                            w.InvokeMember(
                                variablesType,
                                "Object",
                                writer.Action(w2 => GenerateValue(Unit.@object, data, w2))
                            )
                        ),
                        "Set",
                        writer.Action(w2 => GenerateValue(Unit.name, data, w2)),
                        writer.Action(w2 => WriteInputValue(data, w2))
                    ).Write(";").NewLine();

                    GenerateExitControl(Unit.assigned, data, writer);
                    return;

                case VariableKind.Scene:
                    var variables = VisualScripting.Variables.ActiveScene;
                    if (graphPointer != null && graphPointer.scene != null)
                    {
                        VisualScripting.Variables.Scene(graphPointer.scene);
                    }
                    ResolveVariableTypeSafe(variables, name, data);

                    writer.WriteIndented().InvokeMember(
                        writer.Action(w => w.Write(GetSceneKind(data, variablesType))),
                        "Set",
                        writer.Action(w2 => GenerateValue(Unit.name, data, w2)),
                        writer.Action(w2 => WriteInputValue(data, w2))
                    ).Write(";").NewLine();

                    GenerateExitControl(Unit.assigned, data, writer);
                    return;

                case VariableKind.Application:
                    ResolveVariableTypeSafe(VisualScripting.Variables.Application, name, data);

                    writer.WriteIndented().InvokeMember(
                        writer.Action(w => w.GetMember(variablesType, "Application")),
                        "Set",
                        writer.Action(w2 => GenerateValue(Unit.name, data, w2)),
                        writer.Action(w2 => WriteInputValue(data, w2))
                    ).Write(";").NewLine();

                    GenerateExitControl(Unit.assigned, data, writer);
                    return;

                case VariableKind.Saved:
                    ResolveVariableTypeSafe(VisualScripting.Variables.Saved, name, data);

                    writer.WriteIndented().InvokeMember(
                        writer.Action(w => w.GetMember(variablesType, "Saved")),
                        "Set",
                        writer.Action(w2 => GenerateValue(Unit.name, data, w2)),
                        writer.Action(w2 => WriteInputValue(data, w2))
                    ).Write(";").NewLine();

                    GenerateExitControl(Unit.assigned, data, writer);
                    return;
            }

            string scopedName = data.GetVariableName(name.LegalVariableName()).LegalVariableName();

            if (data.ContainsNameInAncestorScope(scopedName))
            {
                variableName = scopedName;
                variableType = data.GetVariableType(scopedName);

                writer.SetVariable(variableName, writer.Action(w =>
                {
                    using (data.Expect(variableType))
                        GenerateValue(Unit.input, data, w);
                }));

                data.CreateSymbol(Unit, variableType);
                GenerateExitControl(Unit.assigned, data, writer);
                return;
            }

            variableType = GetSourceType(Unit.input, data, writer);

            variableName = data.AddLocalNameInScope(scopedName, variableType);

            writer.CreateVariable(variableType, variableName, writer.Action(w =>
            {
                using (data.Expect(variableType))
                    GenerateValue(Unit.input, data, w);
            }));

            data.CreateSymbol(Unit, variableType);
            GenerateExitControl(Unit.assigned, data, writer);
        }

        private void ResolveVariableTypeSafe(VariableDeclarations declarations, string name, ControlGenerationData data)
        {
            UnitSymbol unitSymbol = null;
            variableType = declarations != null && declarations.IsDefined(name)
                ? ResolveVariableType(declarations, name)
                : data.TryGetSymbol(Unit, out unitSymbol) ? unitSymbol.Type : data.GetExpectedType() ?? typeof(object);
        }

        private void WriteInputValue(ControlGenerationData data, CodeWriter writer)
        {
            if (Unit.input.hasValidConnection)
            {
                using (data.Expect(variableType))
                    GenerateValue(Unit.input, data, writer);
            }
            else
            {
                writer.Null();
            }
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
                writer.Error($"ControlInput {Unit.assign.key} requires connection on {Unit.GetType()}");
                return;
            }

            if (output == Unit.output && (Unit.kind == VariableKind.Object || Unit.kind == VariableKind.Scene || Unit.kind == VariableKind.Application || Unit.kind == VariableKind.Saved))
            {
                GenerateValue(Unit.input, data, writer);
                return;
            }
            else if (output == Unit.output && !Unit.name.hasValidConnection)
            {
                if (data.ContainsNameInAnyScope(variableName))
                {
                    writer.Write(variableName.VariableHighlight());
                    return;
                }
                else
                {
                    writer.Error($"Could not find variable with name \"{variableName}\"");
                    return;
                }
            }
            else
            {
                GenerateValue(Unit.input, data, writer);
            }
        }

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.@object && !input.hasValidConnection && Unit.defaultValues[input.key] == null)
            {
                writer.Write("gameObject".VariableHighlight());
                return;
            }
            if (input == Unit.input && Unit.input.hasValidConnection)
            {
                GenerateConnectedValueCasted(input, data, writer, variableType ?? typeof(object), () => ShouldCast(Unit.input, data, variableType ?? typeof(object), writer), false, false);

                return;
            }

            base.GenerateValueInternal(input, data, writer);
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