using System;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    [Widget(typeof(WindowVariableNode))]
    public class WindowVariableNodeWidget : UnitWidget<WindowVariableNode>
    {
        public WindowVariableNodeWidget(FlowCanvas canvas, WindowVariableNode unit) : base(canvas, unit)
        {
            nameInspectorConstructor = (metadata) => new VisualScripting.VariableNameInspector(metadata, GetNameSuggestions);
        }

        protected override NodeColorMix baseColor => NodeColorMix.TealReadable;

        private VisualScripting.VariableNameInspector nameInspector;
        private Func<Metadata, VisualScripting.VariableNameInspector> nameInspectorConstructor;

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
            if (unit.asset != null)
            {
                var variables = unit.asset.variables.variables.ToArrayPooled();
                for (int i = 0; i < variables.Length; i++)
                {
                    yield return variables[i].name;
                }
            }
        }
    }
}
