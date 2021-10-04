using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [Widget(typeof(MachineVariableNode))]
    public class MachineVariableNodeWidget : UnitWidget<MachineVariableNode>
    {
        public MachineVariableNodeWidget(FlowCanvas canvas, MachineVariableNode unit) : base(canvas, unit)
        {
            nameInspectorConstructor = (metadata) => new VariableNameInspector(metadata, GetNameSuggestions);
        }

        protected override NodeColorMix baseColor => NodeColorMix.TealReadable;

        private VariableNameInspector nameInspector;
        private Func<Metadata, VariableNameInspector> nameInspectorConstructor;

        public override Inspector GetPortInspector(IUnitPort port, Metadata metadata)
        {
            if (port == unit.name)
            {
                // This feels so hacky. The real holy grail here would be to support attribute decorators like Unity does.
                InspectorProvider.instance.Renew(ref nameInspector, metadata, nameInspectorConstructor);

                return nameInspector;
            }

            return base.GetPortInspector(port, metadata);
        }

        private IEnumerable<string> GetNameSuggestions()
        {
            if (unit.asset != null)
            {
                var variables = unit.asset.graph.variables.ToArrayPooled();
                for (int i = 0; i < variables.Length; i++)
                {
                    yield return variables[i].name;
                }
            }
        }
    }
}
