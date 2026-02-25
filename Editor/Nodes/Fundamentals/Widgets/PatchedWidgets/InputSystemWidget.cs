#if PACKAGE_INPUT_SYSTEM_EXISTS
using System;
using JetBrains.Annotations;
using Unity.VisualScripting.InputSystem;

namespace Unity.VisualScripting.Community
{
    [UsedImplicitly]
    public class InputSystemWidget : UnitWidget<OnInputSystemEvent>
    {
        public InputSystemWidget(FlowCanvas canvas, OnInputSystemEvent unit) : base(canvas, unit)
        {
            inputActionInspectorConstructor = metadata => new InputActionInspector(metadata, reference, unit);
        }

        protected override NodeColorMix baseColor => NodeColor.Green;

        private InputActionInspector nameInspector;

        private Func<Metadata, InputActionInspector> inputActionInspectorConstructor;

        public override Inspector GetPortInspector(IUnitPort port, Metadata metadata)
        {
            if (port == unit.InputAction)
            {
                InspectorProvider.instance.Renew(ref nameInspector, metadata, inputActionInspectorConstructor);
                return nameInspector;
            }

            return base.GetPortInspector(port, metadata);
        }
    }
}
#endif