using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(RetrieveObjectNode))]
    public class RetrieveObjectNodeGenerator : LocalVariableGenerator
    {
        private RetrieveObjectNode Unit => unit as RetrieveObjectNode;

        public RetrieveObjectNodeGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            variableName = data.AddLocalNameInScope("poolObject", typeof(PoolObject));

            writer.CreateVariable("var".ConstructHighlight(), variableName, writer.Action(() =>
            {
                GenerateValue(Unit.Pool, data, writer);
                writer.InvokeMember(null, "RetrieveObjectFromPool");
            }), WriteOptions.Indented, EndWriteOptions.LineEnd);

            GenerateExitControl(Unit.Retrieved, data, writer);
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.GetVariable(variableName);
        }

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.Pool && !input.hasValidConnection && Unit.defaultValues[input.key] == null)
            {
                writer.Write("gameObject".VariableHighlight()).GetComponent(typeof(ObjectPool));
                return;
            }

            base.GenerateValueInternal(input, data, writer);
        }
    }
}