using Unity.VisualScripting;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using NUnit.Framework;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(SwitchOnString))]
    public sealed class SwitchOnStringGenerator : NodeGenerator<SwitchOnString>
    {
        public SwitchOnStringGenerator(SwitchOnString unit) : base(unit)
        {
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.enter)
            {
                var values = Unit.branches;
                var isLiteral = Unit.selector.hasValidConnection && NodeGeneration.IsSourceLiteral(Unit.selector, out _);
                var localName = string.Empty;

                if (isLiteral)
                {
                    localName = data.AddLocalNameInScope("str", typeof(string));
                    writer.WriteIndented();
                    writer.Write("var".ConstructHighlight());
                    writer.Space();
                    writer.Write(localName.VariableHighlight());
                    writer.Write(" = ");
                    GenerateValue(Unit.selector, data, writer);
                    writer.Write(";");
                    writer.NewLine();
                }

                writer.WriteIndented();
                writer.Write("switch".ControlHighlight());
                writer.Write(" (");
                if (isLiteral)
                    writer.Write(localName.VariableHighlight());
                else
                    GenerateValue(Unit.selector, data, writer);

                if (Unit.ignoreCase)
                {
                    writer.Write(".ToLowerInvariant()");
                }
                writer.Write(")");
                writer.NewLine();
                writer.WriteLine("{");

                using (writer.Indented())
                {
                    for (int i = 0; i < values.Count; i++)
                    {
                        writer.WriteIndented();
                        writer.Write("case".ControlHighlight());
                        writer.Space();
                        if (Unit.ignoreCase)
                            writer.Write($"\"{values[i].Key.ToLowerInvariant()}\"".StringHighlight());
                        else
                            writer.Write($"\"{values[i].Key}\"".StringHighlight());
                        writer.Write(":");
                        writer.NewLine();

                        data.SetMustBreak(true);
                        data.SetHasBroke(false);
                        if (values[i].Value.hasValidConnection)
                        {
                            data.NewScope();
                            GenerateChildControl(values[i].Value, data, writer);
                            data.ExitScope();
                        }

                        if ((data.MustBreak && !data.HasBroke) || (data.MustReturn && !data.HasReturned))
                        {
                            writer.WriteIndented();
                            writer.Write("break".ControlHighlight());
                            writer.Write(";");
                            writer.NewLine();
                        }
                    }

                    writer.WriteIndented();
                    writer.Write("default".ControlHighlight());
                    writer.Write(":");
                    writer.NewLine();

                    data.SetMustBreak(true);
                    data.SetHasBroke(false);
                    if (Unit.@default.hasValidConnection)
                    {
                        data.NewScope();
                        GenerateChildControl(Unit.@default, data, writer);
                        data.ExitScope();
                    }

                    if ((data.MustBreak && !data.HasBroke) || (data.MustReturn && !data.HasReturned))
                    {
                        writer.WriteIndented();
                        writer.Write("break".ControlHighlight());
                        writer.Write(";");
                        writer.NewLine();
                    }
                }

                writer.WriteLine("}");
            }
        }
    }
}
