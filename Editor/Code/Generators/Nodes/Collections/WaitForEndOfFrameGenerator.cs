using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(WaitForEndOfFrameUnit))]
    public class WaitForEndOfFrameGenerator : NodeGenerator<WaitForEndOfFrameUnit>
    {
        public WaitForEndOfFrameGenerator(Unit unit) : base(unit)
        {
            NameSpace = "UnityEngine";
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return base.GenerateValue(output, data);
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return MakeSelectableForThisUnit("yield return".ControlHighlight() + " new ".ConstructHighlight() + "WaitForEndOfFrame".TypeHighlight() + "();");
        }
    }
}
