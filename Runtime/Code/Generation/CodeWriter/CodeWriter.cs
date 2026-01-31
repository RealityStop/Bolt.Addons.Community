using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.VisualScripting.Community.CSharp;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public sealed partial class CodeWriter
    {
        private readonly StringBuilder rawBuilder = new StringBuilder();
        private readonly StringBuilder highlightedBuilder = new StringBuilder();

        private readonly Stack<(SourceNode, IDisposable)> sourceStack = new Stack<(SourceNode, IDisposable)>();

        private readonly SourceMap SourceMap = new SourceMap();
        private readonly List<CodeDiagnostic> Diagnostics = new List<CodeDiagnostic>();

        public int IndentLevel { get; private set; }

        public int RawLength => rawBuilder.Length;
        public int HighlightedLength => highlightedBuilder.Length;

        /// <summary>
        /// Gets the last text segment written by <see cref="Write"/> or <see cref="WriteLine"/>.
        /// This does not represent a full generated block, only the most recent write.
        /// </summary>
        public string LastGeneratedCode { get; private set; }
        public string LastGeneratedCodeHighlighted { get; private set; }

        public CodeWriter()
        {
        }

        public int Indent()
        {
            return IndentLevel++;
        }

        public int Indent(int amount)
        {
            if (amount > 0)
                IndentLevel += amount;
            return IndentLevel;
        }

        public int Unindent()
        {
            if (IndentLevel > 0)
                IndentLevel--;
            return IndentLevel;
        }

        public int Unindent(int amount)
        {
            if (amount > 0)
                IndentLevel = Mathf.Max(0, IndentLevel - amount);
            return IndentLevel;
        }

        public IDisposable Indented()
        {
            Indent();
            return new IndentScope(this, 1);
        }

        public IDisposable Indented(int amount)
        {
            if (amount <= 0)
                return EmptyIndentScope.Instance;

            Indent(amount);
            return new IndentScope(this, amount);
        }

        public IDisposable IndentedScope(ControlGenerationData data, int amount = 1)
        {
            if (amount <= 0)
                amount = 1;

            Indent(amount);

            data?.NewScope();

            return new IndentScopeWithScope(this, amount, data);
        }

        public IDisposable NewScope(ControlGenerationData data)
        {
            data?.NewScope();

            return new DataScope(data);
        }

        public CodeWriter Write(string text)
        {
            if (IsRecordingSuppressed)
                return this;

            var raw = (text ?? "").RemoveHighlights().RemoveMarkdown();

            LastGeneratedCode = raw;
            LastGeneratedCodeHighlighted = text;

            rawBuilder.Append(raw);
            highlightedBuilder.Append(text);
            return this;
        }

        public CodeWriter Write(Type type)
        {
            return Write(GetTypeNameHighlighted(type));
        }

        public CodeWriter Write(string text, WriteOptions options)
        {
            if (IsRecordingSuppressed)
                return this;

            string newText = string.Empty;

            if ((options & WriteOptions.NewLineBefore) != 0)
                newText += "\n";

            if ((options & WriteOptions.Indented) != 0)
                newText += CodeBuilder.Indent(IndentLevel);

            newText += text;

            if ((options & WriteOptions.NewLineAfter) != 0)
                newText += "\n";

            var rawText = newText.RemoveHighlights().RemoveMarkdown();

            LastGeneratedCode = rawText;
            LastGeneratedCodeHighlighted = newText;

            rawBuilder.Append(rawText);
            highlightedBuilder.Append(newText);

            return this;
        }

        public CodeWriter WriteIndented(string text = "")
        {
            if (IsRecordingSuppressed)
                return this;

            text = CodeBuilder.Indent(IndentLevel) + text;

            var raw = (text ?? "").RemoveHighlights().RemoveMarkdown();

            LastGeneratedCode = raw;
            LastGeneratedCodeHighlighted = text;

            rawBuilder.Append(raw);
            highlightedBuilder.Append(text);
            return this;
        }

        public CodeWriter WriteLine(string text = "")
        {
            if (IsRecordingSuppressed)
                return this;

            string indent = CodeBuilder.Indent(IndentLevel);

            var raw = (text ?? "").RemoveHighlights().RemoveMarkdown();

            LastGeneratedCode = indent + raw + "\n";
            LastGeneratedCodeHighlighted = indent + text + "\n";

            rawBuilder.Append(indent).Append(raw).Append('\n');
            highlightedBuilder.Append(indent).Append(text).Append('\n');
            return this;
        }

        public CodeWriter CSharpUtilityType(WriteOptions options = WriteOptions.None)
        {
            if (IsRecordingSuppressed)
                return this;

            Write(GetTypeNameHighlighted(typeof(CSharpUtility)), options);
            return this;
        }

        private void InsertRaw(int index, string text)
        {
            if (IsRecordingSuppressed)
                return;

            rawBuilder.Insert(index, (text ?? "").RemoveHighlights().RemoveMarkdown());
        }

        private void InsertHighlighted(int index, string text)
        {
            if (IsRecordingSuppressed)
                return;

            highlightedBuilder.Insert(index, text);
        }

        public override string ToString() => rawBuilder.ToString();
        public string ToHighlightedString(bool removeHighlightMarkers = false)
        {
            var highlighted = highlightedBuilder.ToString();

            if (removeHighlightMarkers)
                highlighted = highlighted.RemoveMarkdown();

            return highlighted;
        }

        public IDisposable BeginNode(Unit unit)
        {
            if (IsRecordingSuppressed)
                return EmptySourceNodeScope.Instance;

            if (sourceStack.Count > 0)
            {
                SourceNode current = sourceStack.Peek().Item1;
                if (current.Unit == unit)
                {
                    return EmptySourceNodeScope.Instance;
                }
            }
            SourceNode parent = sourceStack.Count > 0
            ? sourceStack.Peek().Item1
            : null;
            int start = rawBuilder.Length;
            var node = new SourceNode(new SourceSpan(start, start), unit, parent);
            var scope = new SourceNodeScope(this, node);
            sourceStack.Push((node, scope));
            return scope;
        }

        private void EndNode(SourceNode node)
        {
            if (IsRecordingSuppressed)
                return;

            int end = rawBuilder.Length;
            node.Span = new SourceSpan(node.Span.Start, end);

            SourceMap.Register(node);
        }

        public IDisposable BeginCastNode(Unit unit, Type castType, Func<bool> shouldCast, bool wrapInParentheses)
        {
            if (IsRecordingSuppressed)
                return EmptySourceNodeScope.Instance;

            int start = rawBuilder.Length;

            SourceNode parent = sourceStack.Count > 0
                ? sourceStack.Peek().Item1
                : null;

            var node = new CastSourceNode(new SourceSpan(start, start), unit, parent, castType, shouldCast, wrapInParentheses);

            var scope = new CastNodeScope(this, node);
            sourceStack.Push((node, scope));
            return scope;
        }

        private void EndCastNode(CastSourceNode node, int offset)
        {
            if (IsRecordingSuppressed)
            {
                return;
            }

            if (offset > 0)
            {
                foreach (var child in node.Children)
                {
                    child.Offset(offset);
                }
            }

            int end = rawBuilder.Length;
            node.Span = new SourceSpan(node.Span.Start, end);

            SourceMap.Register(node);
            sourceStack.Pop();
        }

        private int suppressRecording;

        private bool IsRecordingSuppressed => suppressRecording > 0;

        private readonly HashSet<ValueInput> resolvingTypes = new HashSet<ValueInput>();

        public IDisposable SuppressRecording(ValueInput input, out bool canSuppress)
        {
            if (!resolvingTypes.Add(input))
            {
                canSuppress = false;
                return EmptyScope.Instance;
            }
            suppressRecording++;
            canSuppress = true;
            return new RecordingSuppressionScope(this, input);
        }

        /// <summary>
        /// Retrieves the Code that was written in this scope without actually writing it to the string builder.
        /// </summary>
        /// <param name="result">The result of the capture</param>
        public IDisposable Capture(out CaptureResult result)
        {
            result = new CaptureResult();
            int rawStart = rawBuilder.Length;
            int highlightedStart = highlightedBuilder.Length;
            return new CaptureScope(this, rawStart, highlightedStart, result);
        }

        /// <summary>
        /// Captures the code and Rewrites it with the CaptureWriteOptions.
        /// </summary>
        /// <param name="result">The result of the capture</param>
        /// <param name="options">The options to rewrite the code with</param>
        public IDisposable CaptureWrite(out CaptureResult result, WriteOptions options = WriteOptions.None)
        {
            result = new CaptureResult();

            int rawStart = rawBuilder.Length;
            int highlightedStart = highlightedBuilder.Length;
            return new CaptureWriteScope(this, rawStart, highlightedStart, result, options);
        }

        public IDisposable CodeDiagnosticScope(string message, CodeDiagnosticKind kind)
        {
            if (IsRecordingSuppressed)
                return EmptyScope.Instance;

            if (!CSharpPreviewSettings.ShouldGenerateTooltips)
                return EmptyScope.Instance;

            if (!CSharpPreviewSettings.ShouldShowRecommendations && kind == CodeDiagnosticKind.Recommendation)
                return EmptyScope.Instance;

            int start = rawBuilder.Length;
            return new DiagnosticScope(this, start, message, kind);
        }

        public CodeWriter WriteDiagnostic(string diagnosticMessage, string displayText, CodeDiagnosticKind kind, bool highlight, WriteOptions writeOptions = WriteOptions.None)
        {
            if (!CSharpPreviewSettings.ShouldShowRecommendations && kind == CodeDiagnosticKind.Recommendation)
            {
                Write("", writeOptions);
                return this;
            }

            if (CSharpPreviewSettings.ShouldGenerateTooltips)
            {
                using (CodeDiagnosticScope(diagnosticMessage, kind))
                {
                    if (highlight)
                    {
                        WriteDiagnosticComment(displayText + " (Hover for more info)", kind, writeOptions);
                        return this;
                    }

                    Write(displayText + " (Hover for more info)", writeOptions);
                }
            }
            else
            {
                if (highlight)
                {
                    WriteDiagnosticComment(displayText, kind, writeOptions);
                    return this;
                }

                Write(displayText, writeOptions);
            }

            return this;
        }

        public void AddDiagnostic(SourceSpan span, string message, CodeDiagnosticKind kind)
        {
            if (IsRecordingSuppressed)
                return;

            if (!CSharpPreviewSettings.ShouldGenerateTooltips)
                return;

            if (!CSharpPreviewSettings.ShouldShowRecommendations && kind == CodeDiagnosticKind.Recommendation)
                return;

            CodeDiagnostic diagnostic = new CodeDiagnostic();
            diagnostic.Span = span;
            diagnostic.Message = message;
            diagnostic.Kind = kind;
            Diagnostics.Add(diagnostic);
        }

        public void ExitCurrentNode(Unit unit)
        {
            if (IsRecordingSuppressed)
                return;

            if (sourceStack.Count == 0)
                throw new InvalidOperationException("No active node to exit.");

            var current = sourceStack.Peek();

            if (current.Item1.Unit != unit)
                throw new InvalidOperationException($"Node mismatch. Tried to exit node owned by {unit}, but current node belongs to {current.Item1.Unit}.");

            current.Item2.Dispose();
        }

        public SourceNode ResolveUnit(int charIndex)
        {
            return SourceMap.Resolve(charIndex);
        }

        public SourceMap GetSourceMap()
        {
            return SourceMap;
        }

        public List<CodeDiagnostic> GetDiagnostics()
        {
            return Diagnostics;
        }

        public HashSet<string> includedNamespaces = new HashSet<string>();

        public string GetTypeNameHighlighted(Type type, bool checkAmbiguity = true)
        {
            if (!checkAmbiguity)
                return type.As().CSharpName(false, true);
            return type.As().CSharpName(false, BuildAmbiguityResolver(), true);
        }

        public Func<Type, bool> BuildAmbiguityResolver()
        {
            return type =>
            {
                string shortName = type.Name;
                string ns = type.Namespace ?? "";

                if (!shortNameNamespaces.TryGetValue(shortName, out HashSet<string> namespaces))
                    return false;

                int visibleCount = 0;

                foreach (string n in namespaces)
                {
                    if (n == ns || includedNamespaces.Contains(n))
                    {
                        visibleCount++;
                        if (visibleCount > 1)
                            return true;
                    }
                }

                return false;
            };
        }

        static Dictionary<string, HashSet<string>> shortNameNamespaces = new Dictionary<string, HashSet<string>>();
        private static bool cached;

        public static void BuildAmbiguityCache()
        {
            if (cached) return;

            Dictionary<string, HashSet<string>> map = new Dictionary<string, HashSet<string>>();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int a = 0; a < assemblies.Length; a++)
            {
                Type[] types;

                try
                {
                    types = assemblies[a].GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types;
                }

                for (int t = 0; t < types.Length; t++)
                {
                    Type type = types[t];
                    if (type == null) continue;

                    string name = type.Name;
                    string ns = type.Namespace ?? "";

                    if (!map.TryGetValue(name, out HashSet<string> set))
                    {
                        set = new HashSet<string>();
                        map[name] = set;
                    }

                    set.Add(ns);
                }
            }

            shortNameNamespaces = map;
            cached = true;
        }
    }

    public sealed class CaptureResult
    {
        public string Raw;
        public string Value;
    }

    public sealed class CodeDiagnostic
    {
        public SourceSpan Span;
        public CodeDiagnosticKind Kind;
        public string Message;
    }

    [Flags]
    public enum WriteOptions
    {
        None = 0,
        Indented = 1 << 0,
        NewLineBefore = 1 << 1,
        NewLineAfter = 1 << 2,
        NewLineBeforeAndAfter = NewLineBefore | NewLineAfter,
        IndentedNewLineAfter = Indented | NewLineAfter
    }

    [Flags]
    public enum EndWriteOptions
    {
        /// <summary>
        /// Adds )
        /// </summary>
        CloseParentheses = 1 << 0,
        /// <summary>
        /// Adds ;
        /// </summary>
        Semicolon = 1 << 1,
        /// <summary>
        /// Adds \n
        /// </summary>
        Newline = 1 << 2,
        /// <summary>
        /// Adds );\n
        /// </summary>
        All = CloseParentheses | Semicolon | Newline,
        /// <summary>
        /// Adds ;\n
        /// </summary>
        LineEnd = Semicolon | Newline
    }

    public enum CodeDiagnosticKind
    {
        Info,
        Warning,
        Error,
        Recommendation
    }
}
