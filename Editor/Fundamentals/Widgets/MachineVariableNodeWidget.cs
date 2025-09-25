using System;
using System.Collections.Generic;
#if VISUAL_SCRIPTING_1_7
using SMachine = Unity.VisualScripting.ScriptMachine;
#else
using SMachine = Unity.VisualScripting.FlowMachine;
#endif

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
                InspectorProvider.instance.Renew(ref nameInspector, metadata, nameInspectorConstructor);

                return nameInspector;
            }

            return base.GetPortInspector(port, metadata);
        }

        private IEnumerable<string> GetNameSuggestions()
        {
            if (Flow.CanPredict(unit.target, reference))
            {
                var variables = Flow.Predict<SMachine>(unit.target, reference).graph.variables.ToArrayPooled();
                for (int i = 0; i < variables.Length; i++)
                {
                    yield return variables[i].name;
                }
            }
        }
    }
}
