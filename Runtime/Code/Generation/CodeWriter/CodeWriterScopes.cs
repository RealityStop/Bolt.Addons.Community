using System;
using Unity.VisualScripting.Community.CSharp;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    public sealed partial class CodeWriter
    {
        private sealed class CaptureScope : IDisposable
        {
            private readonly CodeWriter writer;
            private readonly int rawStart;
            private readonly int highlightedStart;
            private readonly CaptureResult result;
            private bool disposed;

            public CaptureScope(CodeWriter writer, int rawStart, int highlightedStart, CaptureResult result)
            {
                this.writer = writer;
                this.rawStart = rawStart;
                this.highlightedStart = highlightedStart;
                this.result = result;
            }

            public void Dispose()
            {
                if (disposed)
                    return;

                disposed = true;

                int rawLength = writer.rawBuilder.Length - rawStart;
                int highlightedLength = writer.highlightedBuilder.Length - highlightedStart;

                result.Raw = writer.rawBuilder.ToString(rawStart, rawLength);
                result.Value = writer.highlightedBuilder.ToString(highlightedStart, highlightedLength);

                writer.rawBuilder.Length = rawStart;
                writer.highlightedBuilder.Length = highlightedStart;
            }
        }
        private sealed class CaptureWriteScope : IDisposable
        {
            private readonly CodeWriter writer;
            private readonly int rawStart;
            private readonly int highlightedStart;
            private readonly CaptureResult result;
            private readonly WriteOptions options;
            private bool disposed;

            public CaptureWriteScope(CodeWriter writer, int rawStart, int highlightedStart, CaptureResult result, WriteOptions options)
            {
                this.writer = writer;
                this.rawStart = rawStart;
                this.highlightedStart = highlightedStart;
                this.result = result;
                this.options = options;
            }

            public void Dispose()
            {
                if (disposed)
                    return;

                disposed = true;

                int rawLength = writer.rawBuilder.Length - rawStart;
                int highlightedLength = writer.highlightedBuilder.Length - highlightedStart;

                string raw = writer.rawBuilder.ToString(rawStart, rawLength);
                string highlighted = writer.highlightedBuilder.ToString(highlightedStart, highlightedLength);

                result.Raw = raw;
                result.Value = highlighted;

                writer.rawBuilder.Length = rawStart;
                writer.highlightedBuilder.Length = highlightedStart;

                string code = "";

                if ((options & WriteOptions.NewLineBefore) != 0)
                    code += "\n";

                if ((options & WriteOptions.Indented) != 0)
                    code += CodeBuilder.Indent(writer.IndentLevel);

                code += highlighted;

                if ((options & WriteOptions.NewLineAfter) != 0)
                    code += "\n";

                writer.Write(code);
            }
        }

        private sealed class RecordingSuppressionScope : IDisposable
        {
            private readonly CodeWriter writer;
            private readonly ValueInput input;
            public RecordingSuppressionScope(CodeWriter writer, ValueInput input)
            {
                this.writer = writer;
                this.input = input;
            }

            public void Dispose()
            {
                writer.suppressRecording--;
                writer.resolvingTypes.Remove(input);
            }
        }

        private sealed class SourceNodeScope : IDisposable
        {
            private readonly CodeWriter writer;
            private readonly SourceNode node;
            public bool isDisposed { get; private set; }
            public SourceNodeScope(CodeWriter writer, SourceNode node)
            {
                this.writer = writer;
                this.node = node;
            }

            public void Dispose()
            {
                if (isDisposed) return;

                isDisposed = true;

                if (writer.sourceStack.Count > 0)
                    writer.sourceStack.Pop();

                writer.EndNode(node);
            }
        }

        private sealed class IndentScopeWithScope : IDisposable
        {
            private readonly CodeWriter writer;
            private readonly int amount;
            private readonly ControlGenerationData data;
            private bool disposed;

            public IndentScopeWithScope(CodeWriter writer, int amount, ControlGenerationData data)
            {
                this.writer = writer;
                this.amount = amount;
                this.data = data;
            }

            public void Dispose()
            {
                if (disposed) return;
                disposed = true;

                data?.ExitScope();

                writer.Unindent(amount);
            }
        }

        private sealed class DataScope : IDisposable
        {
            private readonly ControlGenerationData data;
            private bool disposed;

            public DataScope(ControlGenerationData data)
            {
                this.data = data;
            }

            public void Dispose()
            {
                if (disposed) return;
                disposed = true;

                data?.ExitScope();
            }
        }

        private sealed class IndentScope : IDisposable
        {
            private CodeWriter writer;
            private int amount;
            private bool disposed;

            public IndentScope(CodeWriter writer, int amount)
            {
                this.writer = writer;
                this.amount = amount;
            }

            public void Dispose()
            {
                if (disposed)
                    return;

                disposed = true;
                writer.Unindent(amount);
            }
        }

        private sealed class CastScope : IDisposable
        {
            private readonly CodeWriter writer;
            private readonly Type castType;
            private readonly Func<bool> shouldCast;
            private readonly bool wrapInParentheses;
            private readonly int rawStart;
            private readonly int highlightedStart;

            public CastScope(CodeWriter writer, Type castType, Func<bool> shouldCast, bool wrapInParentheses)
            {
                this.writer = writer;
                this.castType = castType;
                this.shouldCast = shouldCast;
                this.wrapInParentheses = wrapInParentheses;
                rawStart = writer.RawLength;
                highlightedStart = writer.HighlightedLength;
            }

            public void Dispose()
            {
                if (castType == null || (shouldCast != null && !shouldCast()))
                    return;

                string typeName = writer.GetTypeNameHighlighted(castType);

                if (wrapInParentheses)
                {
                    writer.InsertRaw(rawStart, "((" + typeName + ")");
                    writer.InsertHighlighted(highlightedStart, "((" + typeName + ")");

                    writer.Write(")");
                }
                else
                {
                    writer.InsertRaw(rawStart, "(" + typeName + ")");
                    writer.InsertHighlighted(highlightedStart, "(" + typeName + ")");
                }
            }
        }

        private sealed class CastNodeScope : IDisposable
        {
            private readonly CodeWriter writer;
            private readonly CastSourceNode node;
            private readonly int rawStart;
            private readonly int highlightedStart;

            public CastNodeScope(CodeWriter writer, CastSourceNode node)
            {
                this.writer = writer;
                this.node = node;
                rawStart = writer.RawLength;
                highlightedStart = writer.HighlightedLength;
            }

            public void Dispose()
            {
                bool shouldCast =
                    node.CastType != null &&
                    (node.ShouldCast == null || node.ShouldCast());

                int offset = 0;

                if (shouldCast)
                {
                    string typeName = writer.GetTypeNameHighlighted(node.CastType);

                    if (node.WrapInParentheses)
                    {
                        offset = 3 + typeName.RemoveHighlights().RemoveMarkdown().Length;
                        writer.InsertRaw(rawStart, "((" + typeName + ")");
                        writer.InsertHighlighted(highlightedStart, "((" + typeName + ")");
                        writer.Write(")");
                    }
                    else
                    {
                        offset = 2 + typeName.RemoveHighlights().RemoveMarkdown().Length;
                        writer.InsertRaw(rawStart, "(" + typeName + ")");
                        writer.InsertHighlighted(highlightedStart, "(" + typeName + ")");
                    }
                }

                writer.EndCastNode(node, offset);
            }
        }

        public sealed class DiagnosticScope : IDisposable
        {
            private readonly CodeWriter writer;
            private readonly int start;
            private readonly string message;
            private readonly CodeDiagnosticKind kind;

            public DiagnosticScope(CodeWriter writer, int start, string message, CodeDiagnosticKind kind)
            {
                this.writer = writer;
                this.start = start;
                this.message = message;
                this.kind = kind;
            }

            public void Dispose()
            {
                int end = writer.RawLength;
                SourceSpan span = new SourceSpan(start, end);
                writer.AddDiagnostic(span, message, kind);
            }
        }

        private sealed class EmptyScope : IDisposable
        {
            public static readonly EmptyScope Instance = new EmptyScope();
            public void Dispose() { }
        }

        private sealed class EmptyIndentScope : IDisposable
        {
            public static readonly EmptyIndentScope Instance = new EmptyIndentScope();
            public void Dispose() { }
        }

        private sealed class EmptySourceNodeScope : IDisposable
        {
            public static readonly EmptySourceNodeScope Instance = new EmptySourceNodeScope();
            private EmptySourceNodeScope() { }
            public void Dispose() { }
        }
    }
}