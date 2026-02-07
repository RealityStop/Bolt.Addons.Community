using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Unit))]
    public class NodeGenerator : Decorator<NodeGenerator, NodeGeneratorAttribute, Unit>
    {
        public Unit unit;

        public virtual IEnumerable<string> GetNamespaces()
        {
            yield break;
        }

        public string variableName = "";
        private int currentRecursionDepth = CSharpPreviewSettings.RecursionDepth;
        private Recursion recursion { get; set; } = Recursion.New(CSharpPreviewSettings.RecursionDepth);

        public NodeGenerator(Unit unit)
        {
            this.unit = unit;
        }

        private void UpdateRecursion()
        {
            if (currentRecursionDepth != CSharpPreviewSettings.RecursionDepth)
            {
                recursion = Recursion.New(CSharpPreviewSettings.RecursionDepth);
                currentRecursionDepth = CSharpPreviewSettings.RecursionDepth;
            }
        }

        public void GenerateControl(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            UpdateRecursion();

            if (!(recursion?.TryEnter(unit) ?? true))
            {
                using (writer.BeginNode(unit))
                    writer.WriteDiagnostic("This node appears to cause infinite recursion (the flow is leading back to this node). Consider using a While loop instead or increasing recursion depth in preview settings.", "Infinite recursion detected!", CodeDiagnosticKind.Error, WriteOptions.IndentedNewLineAfter);
                return;
            }

            var commentNode = unit.graph.GetComments().FirstOrDefault(commentNode => commentNode.connectedElements.Any(c => c == unit));
            if (commentNode != null)
            {
                using (writer.BeginNode(commentNode))
                {

                    if (!string.IsNullOrEmpty(commentNode.title))
                        writer.Comment(commentNode.title + ":\n" + commentNode.comment, WriteOptions.IndentedNewLineAfter);
                    else
                        writer.Comment(commentNode.comment, WriteOptions.IndentedNewLineAfter);
                }
            }

            try
            {
                using (writer.BeginNode(unit))
                {
                    GenerateControlInternal(input, data, writer);
                }
            }
            finally
            {
                recursion?.Exit(unit);
            }
        }

        public void GenerateValue(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            UpdateRecursion();

            if (!(recursion?.TryEnter(unit) ?? true))
            {
                using (writer.BeginNode(unit))
                    writer.WriteDiagnostic($"{input.key} is infinitely generating itself. Consider reviewing your graph logic or increasing recursion depth in preview settings.", "Infinite recursion detected!", CodeDiagnosticKind.Error);

                return;
            }
            try
            {
                GenerateValueInternal(input, data, writer);
            }
            finally
            {
                recursion?.Exit(unit);
            }
        }

        public void GenerateValue(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            UpdateRecursion();

            if (!(recursion?.TryEnter(unit) ?? true))
            {
                using (writer.BeginNode(unit))
                {
                    writer.WriteDiagnostic($"{output.key} is infinitely generating itself. Consider reviewing your graph logic or increasing recursion depth in preview settings.", "Infinite recursion detected!", CodeDiagnosticKind.Error);
                }
                return;
            }

            try
            {
                using (writer.BeginNode(unit))
                {
                    GenerateValueInternal(output, data, writer);
                }
            }
            finally
            {
                recursion?.Exit(unit);
            }
        }

        private void GenerateValueCasted(ValueOutput output, ControlGenerationData data, CodeWriter writer, Type castType, Func<bool> shouldCast, bool wrapInParentheses)
        {
            if (!(recursion?.TryEnter(unit) ?? true))
            {
                using (writer.BeginNode(unit))
                    writer.WriteDiagnostic($"{output.key} is infinitely generating itself. Consider reviewing your graph logic or increasing recursion depth in preview settings.", "Infinite recursion detected!", CodeDiagnosticKind.Error);

                return;
            }
            try
            {
                using (writer.BeginCastNode(unit, castType, shouldCast, wrapInParentheses))
                {
                    GenerateValueInternal(output, data, writer);
                }
            }
            finally
            {
                recursion?.Exit(unit);
            }
        }

        protected virtual void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteLine($"/* Port '{input?.key}' of '{unit.GetType().Name}' Missing Generator. */".ErrorHighlight());
        }

        protected virtual void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input.hasValidConnection)
            {
                GenerateConnectedValue(input, data, writer);
                return;
            }
            using (writer.BeginNode(input.unit as Unit))
            {
                if (input.hasDefaultValue)
                {
                    WriteDefaultValue(input, data, writer);
                    return;
                }

                writer.Write($"/* \"{input.key} Requires Input\" */".ErrorHighlight());
            }
        }

        protected virtual void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.Write($"/* Port '{output?.key}' of '{unit.GetType().Name}' Missing Generator. */".ErrorHighlight());
        }

        protected void GenerateChildControl(ControlOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (!output.hasValidConnection)
                return;

            Unit nextUnit = output.connection.destination.unit as Unit;
            nextUnit.GenerateControl(output.connection.destination, data, writer);
        }

        protected void GenerateExitControl(ControlOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (!output.hasValidConnection)
                return;

            writer.ExitCurrentNode(unit);

            Unit nextUnit = output.connection.destination.unit as Unit;
            nextUnit.GenerateControl(output.connection.destination, data, writer);
        }

        protected void GenerateConnectedValue(ValueInput input, ControlGenerationData data, CodeWriter writer, bool expectInputType = true)
        {
            if (!input.hasValidConnection)
                return;

            Unit sourceUnit = input.connection.source.unit as Unit;
            if (expectInputType)
            {
                using (data.Expect(input.type))
                {
                    sourceUnit.GenerateValue(input.connection.source, writer, data);
                }
                return;
            }
            sourceUnit.GenerateValue(input.connection.source, writer, data);
        }

        protected void GenerateConnectedValueCasted(ValueInput input, ControlGenerationData data, CodeWriter writer, Type castType, Func<bool> shouldCast, bool wrapInParentheses = true, bool expectInputType = true)
        {
            if (!input.hasValidConnection)
                return;

            Unit sourceUnit = input.connection.source.unit as Unit;
            if (expectInputType)
            {
                using (data.Expect(input.type))
                {
                    sourceUnit.GetGenerator().GenerateValueCasted(input.connection.source, data, writer, castType, shouldCast, wrapInParentheses);
                }
                return;
            }
            sourceUnit.GetGenerator().GenerateValueCasted(input.connection.source, data, writer, castType, shouldCast, wrapInParentheses);
        }

        protected void GenerateConnectedValueCasted(ValueInput input, ControlGenerationData data, CodeWriter writer, Type castType, bool wrapInParentheses = true, bool expectInputType = true)
        {
            GenerateConnectedValueCasted(input, data, writer, castType, () => true, wrapInParentheses, expectInputType);
        }

        protected void GenerateConnectedValue(ValueInput input, ControlGenerationData data, CodeWriter writer, Type type)
        {
            if (!input.hasValidConnection)
                return;

            Unit sourceUnit = input.connection.source.unit as Unit;
            using (data.Expect(type))
            {
                sourceUnit.GenerateValue(input.connection.source, writer, data);
            }
        }

        protected void WriteDefaultValue(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input.nullMeansSelf && input.unit.defaultValues[input.key] == null &&
            ComponentHolderProtocol.IsComponentHolderType(input.type) && typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType))
            {
                if (input.type == typeof(GameObject))
                {
                    writer.Write("gameObject".VariableHighlight());
                    return;
                }

                writer.Write("gameObject".VariableHighlight() + ".GetComponent<" + writer.GetTypeNameHighlighted(input.type) + ">()");
                return;
            }

            writer.Object(input.unit.defaultValues[input.key]);
        }

        public bool ShouldCast(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input.hasValidConnection)
            {
                Type sourceType = GetSourceType(input, data, writer);
                Type targetType = input.type;

                if (data.IsCurrentExpectedTypeMet())
                {
                    return false;
                }

                return TypeConversionUtility.ShouldCast(sourceType, targetType);
            }

            return false;
        }

        public bool ShouldCast(ValueInput input, ControlGenerationData data, Type targetType, CodeWriter writer)
        {
            if (input.hasValidConnection)
            {
                Type sourceType = GetSourceType(input, data, writer);

                if (data.IsCurrentExpectedTypeMet())
                {
                    return false;
                }

                return TypeConversionUtility.ShouldCast(sourceType, targetType);
            }

            return false;
        }

        /// <summary>
        /// <tt>resolveTypes:</tt> If true, the generator connected to the <tt>valueInput</tt> will ensure that it is fully initialized so that types can be resolved properly.
        /// It is not required if you trigger GenerateValue with the <tt>valueInput</tt> before calling this method.
        /// </summary>
        public Type GetSourceType(ValueInput valueInput, ControlGenerationData data, CodeWriter writer, bool resolveTypes = true)
        {
            if (valueInput == null)
            {
                return null;
            }

            if (data == null)
            {
                return valueInput.type;
            }

            // Ensure that the Generator is Initialized so it creates any Symbols, Variables, etc.
            if (resolveTypes)
            {
                using (writer.SuppressRecording(valueInput, out var canSuppress))
                {
                    if (canSuppress)
                        GenerateValueInternal(valueInput, data, writer);
                }
            }

            if (NodeGeneration.IsSourceLiteral(valueInput, out var result))
            {
                return result;
            }

            var pseudoSource = valueInput.GetPesudoSource();
            if (valueInput.hasValidConnection)
            {
                if (data.TryGetSymbol(pseudoSource?.unit as Unit, out var symbol))
                {
                    return symbol.Type;
                }

                if (data != null && pseudoSource != null && data.TryGetVariableType((pseudoSource.unit as Unit).GetGenerator().variableName, out Type type))
                {
                    return type;
                }

                if (pseudoSource != null && (pseudoSource.unit as Unit).GetGenerator() is LocalVariableGenerator localVariable && localVariable.variableType != null)
                {
                    return localVariable.variableType;
                }

                if (pseudoSource != null && pseudoSource?.type != typeof(object))
                {
                    return pseudoSource.type;
                }

                if (valueInput.hasValidConnection)
                {
                    return valueInput.connection.source.type;
                }
            }

            return valueInput.type;
        }

        public static bool CanPredictConnection(ValueInput target, ControlGenerationData data)
        {
            if (data.TryGetGraphPointer(out var graphPointer) && target.GetPesudoSource()?.unit is UnifiedVariableUnit variableUnit)
            {
                // This is one of the main problems so we check this first.
                if (variableUnit.kind == VariableKind.Scene)
                {
                    if (graphPointer.scene != null)
                        return Flow.CanPredict(target, graphPointer.AsReference());
                    else return false;
                }
            }

            return graphPointer != null;
        }
    }

    public class NodeGenerator<TUnit> : NodeGenerator where TUnit : Unit
    {
        public TUnit Unit;

        public NodeGenerator(Unit unit) : base(unit) { this.unit = unit; Unit = (TUnit)unit; }
    }
}