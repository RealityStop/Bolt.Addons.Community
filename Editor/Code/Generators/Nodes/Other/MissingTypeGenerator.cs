#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(MissingType))]
    public class MissingTypeGenerator : NodeGenerator<MissingType>
    {
        public MissingTypeGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.Error($"The type {Unit.formerType} could not be found");
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.Error($"The type {Unit.formerType} could not be found");
        }
    }
}
#endif