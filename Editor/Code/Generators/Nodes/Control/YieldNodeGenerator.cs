using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(YieldNode))]
    public class YieldNodeGenerator : NodeGenerator<YieldNode>
    {
        public YieldNodeGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input != Unit.enter)
                return;

            switch (Unit.type)
            {
                case YieldNode.EnumeratorType.YieldInstruction:
                case YieldNode.EnumeratorType.CustomYieldInstruction:
                    writer.YieldReturn(writer.Action(() => GenerateValue(Unit.instruction, data, writer)), WriteOptions.IndentedNewLineAfter);
                    break;

                case YieldNode.EnumeratorType.Enumerator:
                    {
                        var enumeratorVar = data.AddLocalNameInScope("enumerator", typeof(IEnumerator));
                        writer.CreateVariable(typeof(IEnumerator), enumeratorVar, writer.Action(w =>
                        {
                            using (data.Expect(typeof(IEnumerator)))
                                GenerateValue(Unit.enumerator, data, w);
                        }), WriteOptions.Indented, EndWriteOptions.LineEnd);

                        writer.WriteIndented("while".ControlHighlight());
                        writer.Parentheses(w =>
                        {
                            w.Write(enumeratorVar.VariableHighlight());
                            w.Write(".MoveNext()");
                        });
                        writer.NewLine();
                        writer.WriteLine("{");

                        using (writer.Indented())
                        {
                            writer.YieldReturn(writer.Action(() => writer.GetMember(enumeratorVar.VariableHighlight(), "Current")), WriteOptions.IndentedNewLineAfter);
                        }

                        writer.WriteLine("}");
                        break;
                    }

                case YieldNode.EnumeratorType.Coroutine:
                    writer.YieldReturn(writer.Action(() => GenerateValue(Unit.coroutine, data, writer)), WriteOptions.IndentedNewLineAfter);
                    break;
            }

            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}