using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(WaitForSecondsUnit))]
    public sealed class WaitForSecondsUnitGenerator : NodeGenerator<WaitForSecondsUnit>
    {
        public WaitForSecondsUnitGenerator(Unit unit) : base(unit)
        {
        }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "Unity.VisualScripting.Community";
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            data.SetHasReturned(true);

            writer.YieldReturn(writer.Action(() =>
            writer.CallCSharpUtilityMethod("CreateWaitForSeconds",
            writer.Action(() => GenerateValue(Unit.seconds, data, writer)),
            writer.Action(() => GenerateValue(Unit.unscaledTime, data, writer)))), WriteOptions.IndentedNewLineAfter);

            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}
