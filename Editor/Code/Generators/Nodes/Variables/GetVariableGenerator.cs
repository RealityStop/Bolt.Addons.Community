using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(GetVariable))]
    public sealed class GetVariableGenerator : LocalVariableGenerator
    {
        private static readonly Dictionary<string, Type> typeCache = new Dictionary<string, Type>();
        private GetVariable Unit => unit as GetVariable;

        public GetVariableGenerator(Unit unit) : base(unit)
        {
        }

        public override IEnumerable<string> GetNamespaces()
        {
            if (Unit.kind == VariableKind.Scene)
                yield return "UnityEngine.SceneManagement";
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            string variablesType = writer.GetTypeNameHighlighted(typeof(VisualScripting.Variables));
            bool hasConnectedName = Unit.name.hasValidConnection;

            data.TryGetGraphPointer(out GraphPointer graphPointer);

            string name = hasConnectedName
                ? graphPointer != null && CanPredictConnection(Unit.name, data)
                    ? Flow.Predict<string>(Unit.name, graphPointer.AsReference())
                    : Unit.defaultValues[Unit.name.key] as string
                : Unit.defaultValues[Unit.name.key] as string;

            if (string.IsNullOrEmpty(name))
            {
                writer.Error("Variable name is empty");
                return;
            }

            if (hasConnectedName)
            {
                GenerateConnected(data, writer, variablesType, graphPointer, name);
                return;
            }

            GenerateDisconnected(data, writer, variablesType, graphPointer, name);
        }

        private void GenerateConnected(ControlGenerationData data, CodeWriter writer, string variablesType, GraphPointer graphPointer, string name)
        {
            switch (Unit.kind)
            {
                case VariableKind.Flow:
                    using (writer.CodeDiagnosticScope("Flow Variables do not support connected names", CodeDiagnosticKind.Error))
                        writer.Error("Could not generate Flow Variable");
                    return;

                case VariableKind.Graph:
                    using (writer.CodeDiagnosticScope("Graph Variables do not support connected names", CodeDiagnosticKind.Error))
                        writer.Error("Could not generate Graph Variable");
                    return;

                case VariableKind.Object:
                    writer.Write(variablesType + ".Object(");
                    GenerateValue(Unit.@object, data, writer);
                    writer.Write(")");
                    ResolveVariableTypeSafe(VisualScripting.Variables.Object(GetTarget(data)), name, data);
                    break;

                case VariableKind.Scene:
                    VariableDeclarations sceneVars = VisualScripting.Variables.ActiveScene;
                    if (graphPointer != null && graphPointer.scene != null)
                        sceneVars = VisualScripting.Variables.Scene(graphPointer.scene);

                    WriteSceneKind(data, variablesType, writer);

                    ResolveVariableTypeSafe(sceneVars, name, data);
                    break;

                case VariableKind.Application:
                    writer.Write(variablesType + "." + "Application".VariableHighlight());
                    ResolveVariableTypeSafe(VisualScripting.Variables.Application, name, data);
                    break;

                case VariableKind.Saved:
                    writer.Write(variablesType + "." + "Saved".VariableHighlight());
                    ResolveVariableTypeSafe(VisualScripting.Variables.Saved, name, data);
                    break;
            }

            string typeString = variableType != null
                ? "<" + writer.GetTypeNameHighlighted(variableType) + ">"
                : string.Empty;

            writer.Write(".Get" + typeString + "(");
            GenerateValue(Unit.name, data, writer);
            writer.Write(")");

            UpdateExpectedType(data);
            data.CreateSymbol(Unit, variableType ?? typeof(object));
        }

        private void GenerateDisconnected(ControlGenerationData data, CodeWriter writer, string variablesType, GraphPointer graphPointer, string name)
        {
            if (Unit.kind == VariableKind.Object ||
                Unit.kind == VariableKind.Scene ||
                Unit.kind == VariableKind.Application ||
                Unit.kind == VariableKind.Saved)
            {
                VariableDeclarations declarations = null;

                if (Unit.kind == VariableKind.Object)
                    declarations = GetTarget(data) != null ? VisualScripting.Variables.Object(GetTarget(data)) : null;
                else if (Unit.kind == VariableKind.Scene)
                    declarations = graphPointer != null && graphPointer.scene != null
                        ? VisualScripting.Variables.Scene(graphPointer.scene)
                        : VisualScripting.Variables.ActiveScene;
                else if (Unit.kind == VariableKind.Application)
                    declarations = VisualScripting.Variables.Application;
                else if (Unit.kind == VariableKind.Saved)
                    declarations = VisualScripting.Variables.Saved;

                ResolveVariableTypeSafe(declarations, name, data);

                string typeString = variableType != null
                    ? "<" + writer.GetTypeNameHighlighted(variableType) + ">"
                    : string.Empty;

                WriteKindPrefix(data, variablesType, writer);
                writer.Write(".Get" + typeString + "(");
                GenerateValue(Unit.name, data, writer);
                writer.Write(")");

                UpdateExpectedType(data);
                data.CreateSymbol(Unit, variableType ?? typeof(object));
                return;
            }

            string scopedName = data.GetVariableName(name.LegalVariableName());

            variableName = scopedName.LegalVariableName();
            variableType = data.GetVariableType(scopedName);
            writer.Write(variableName.VariableHighlight());
            UpdateExpectedType(data);
            data.CreateSymbol(Unit, variableType);
        }

        private void UpdateExpectedType(ControlGenerationData data)
        {
            Type expected = data.GetExpectedType();

            bool met =
                expected != null &&
                variableType != null &&
                expected.IsAssignableFrom(variableType);

            if (met)
                data.MarkExpectedTypeMet(variableType);
        }

        private void WriteKindPrefix(ControlGenerationData data, string variablesType, CodeWriter writer)
        {
            if (Unit.kind == VariableKind.Object)
                writer.InvokeMember(variablesType, "Object", writer.Action(w => WriteObject(data, w)));
            if (Unit.kind == VariableKind.Scene)
                WriteSceneKind(data, variablesType, writer);
            if (Unit.kind == VariableKind.Application)
                writer.GetMember(variablesType, "Application");
            if (Unit.kind == VariableKind.Saved)
                writer.GetMember(variablesType, "Saved");
        }

        private void WriteObject(ControlGenerationData data, CodeWriter writer)
        {
            if (!Unit.@object.hasValidConnection && Unit.defaultValues[Unit.@object.key] == null)
            {
                writer.Write("gameObject".VariableHighlight());
                return;
            }

            GenerateValue(Unit.@object, data, writer);
        }

        private void ResolveVariableTypeSafe(VariableDeclarations declarations, string name, ControlGenerationData data)
        {
            UnitSymbol unitSymbol = null;
            variableType = declarations != null && declarations.IsDefined(name)
                ? ResolveVariableType(declarations, name)
                : data.TryGetSymbol(Unit, out unitSymbol) ? unitSymbol.Type : data.GetExpectedType() ?? typeof(object);
        }

        private void WriteSceneKind(ControlGenerationData data, string variables, CodeWriter writer)
        {
            writer.Write(typeof(Component).IsAssignableFrom(data.ScriptType)
                ? variables + ".Scene(" + "gameObject".VariableHighlight() + "." + "scene".VariableHighlight() + ")"
                : variables + "." + "ActiveScene".VariableHighlight());
        }

        private GameObject GetTarget(ControlGenerationData data)
        {
            if (!Unit.@object.hasValidConnection && Unit.defaultValues[Unit.@object.key] == null && data.TryGetGameObject(out GameObject gameObject))
                return gameObject;

            if (!Unit.@object.hasValidConnection && Unit.defaultValues[Unit.@object.key] != null)
                return Unit.defaultValues[Unit.@object.key].ConvertTo<GameObject>();

            if (data.TryGetGraphPointer(out GraphPointer graphPointer) &&
                Unit.@object.hasValidConnection &&
                CanPredictConnection(Unit.@object, data))
            {
                try
                {
                    return Flow.Predict<GameObject>(Unit.@object.GetPesudoSource(), graphPointer.AsReference());
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
            }

            return null;
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
