#if PACKAGE_INPUT_SYSTEM_EXISTS
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using Unity.VisualScripting.InputSystem;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnInputSystemEventButton))]
    public sealed class OnInputSystemEventButtonGenerator : OnInputSystemEventGeneratorBase<OnInputSystemEventButton>
    {
        public OnInputSystemEventButtonGenerator(Unit unit) : base(unit) { }
        
        public override string Name => "OnInputSystemEventButton" + count;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>();

        protected override ValueInput Target => TypedUnit.Target;
        protected override ValueInput InputAction => TypedUnit.InputAction;
        protected override ControlOutput Trigger => TypedUnit.trigger;
        protected override InputActionChangeOption ChangeType => TypedUnit.InputActionChangeType;
        protected override string WasRunningVariablePrefix => "button";
    }
}
#endif
