using Ludiq;
using Bolt;
using Lasm.BoltAddons.UnitTools.FlowControl;

namespace Lasm.BoltAddons.UnitTools.FlowControl.Editor
{
    [Descriptor(typeof(WaitLoopUnit))]
    public sealed class WaitLoopUnitDescriptor : UnitDescriptor<WaitLoopUnit>
    {
        public WaitLoopUnitDescriptor(WaitLoopUnit unit) : base(unit)
        {
        }

        protected override void Ports(UnitPortDescriptionCollection ports)
        {
            base.Ports(ports);

            //for (int i = 0; i < unit.valueInputs.Count; i++)
            //{
            //    ports["tag_" + i.ToString()].label = i.ToString();
            //}

            //for (int i = 0; i < unit.controlInputs.Count; i++)
            //{
            //    ports["enter"].isLabelVisible = false;
            //}

            for (int i = 0; i < unit.controlOutputs.Count; i++)
            {
                ports["broken"].label = "Break";
            }
        }
    }
}
