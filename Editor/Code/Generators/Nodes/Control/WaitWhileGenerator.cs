using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(WaitWhileUnit))]
    public sealed class WaitWhileGenerator : NodeGenerator<WaitWhileUnit>
    {
        public WaitWhileGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            data.SetHasReturned(true);
            using (data.Expect(typeof(bool)))
            {
                writer.YieldReturn(writer.Action(() => writer.New(typeof(WaitWhile), writer.Action(() =>
                {
                    writer.Write("() => ");
                    GenerateValue(Unit.condition, data, writer);
                }))), WriteOptions.IndentedNewLineAfter);
            }

            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}