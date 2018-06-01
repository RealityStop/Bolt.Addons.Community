using Ludiq;
using Bolt;
using Lasm.BoltAddons.UnitTools.FlowControl;

namespace Lasm.BoltAddons.UnitTools.FlowControl.Editor
{
    [Descriptor(typeof(CycleValueUnit))]
    public sealed class CycleValueUnitDescriptor : UnitDescriptor<CycleValueUnit>
    {
        public CycleValueUnitDescriptor(CycleValueUnit unit) : base(unit)
        {
        }

        protected override void Ports(UnitPortDescriptionCollection ports)
        {
            base.Ports(ports);

            for (int i = 0; i < unit.valueInputs.Count; i++)
            {
                ports["tag_" + i.ToString()].label = "Tag";
                if (unit.useTags)
                {
                    ports["value_" + i.ToString()].isLabelVisible = false;
                } else
                {
                    ports["value_" + i.ToString()].label = i.ToString();
                }
            }

            for (int i = 0; i < unit.controlInputs.Count; i++)
            {
                ports["enter"].isLabelVisible = false;
            }

            for (int i = 0; i < unit.controlOutputs.Count; i++)
            {
                ports["exit"].isLabelVisible = false;
            }

            for (int i = 0; i < unit.valueOutputs.Count; i++)
            {
                ports["currentValueOut"].label = "Value";
            }
        }
    }
}
