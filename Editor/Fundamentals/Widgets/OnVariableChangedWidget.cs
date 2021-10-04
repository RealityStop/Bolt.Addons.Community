using System;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    [Widget(typeof(OnVariableChanged))]
    public sealed class OnVariableChangedWidget : UnitWidget<OnVariableChanged>
    {

        protected override NodeColorMix baseColor => NodeColor.Green;


        public OnVariableChangedWidget(FlowCanvas canvas, OnVariableChanged unit) : base(canvas, unit)
        {
            nameInspectorConstructor = (metadata) => new VariableNameInspector(metadata, GetNameSuggestions);
        }

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
            return EditorVariablesUtility.GetVariableNameSuggestions(unit.kind, reference);
        }
    }
}