using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(BindActionNode))]
    public class BindActionNodeGenerator : NodeGenerator<BindActionNode>
    {
        public BindActionNodeGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            var variable = string.Empty;
            bool sourceIsDelgateNode = Unit.a.hasValidConnection && Unit.a.connection.source.unit is DelegateNode;
            if (sourceIsDelgateNode)
            {
                var source = Unit.a.connection.source;
                variable = data.AddLocalNameInScope("@delgate", source.type);
                writer.CreateVariable((source.unit as DelegateNode)._delegate.GetDelegateType(), variable, writer.Action(() => GenerateValue(Unit.a, data, writer)));

                writer.WriteIndented(variable.VariableHighlight() + " += ");
                GenerateValue(Unit.b, data, writer);
                writer.WriteEnd(EndWriteOptions.LineEnd);
            }
            else
            {
                writer.WriteIndented();
                GenerateValue(Unit.a, data, writer);
                writer.Write(" += ");
                GenerateValue(Unit.b, data, writer);
                writer.WriteEnd(EndWriteOptions.LineEnd);
            }

            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}
