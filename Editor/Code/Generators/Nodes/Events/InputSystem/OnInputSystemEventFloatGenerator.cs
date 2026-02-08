#if PACKAGE_INPUT_SYSTEM_EXISTS
using Unity.VisualScripting.InputSystem;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnInputSystemEventFloat))]
    public sealed class OnInputSystemEventFloatGenerator : OnInputSystemEventGeneratorBase<OnInputSystemEventFloat>
    {
        public OnInputSystemEventFloatGenerator(Unit unit) : base(unit) { }

        public override List<ValueOutput> OutputValues => new List<ValueOutput>();

        public override string Name => "OnInputSystemEventFloat" + count;

        protected override ValueInput Target => TypedUnit.Target;
        protected override ValueInput InputAction => TypedUnit.InputAction;
        protected override ControlOutput Trigger => TypedUnit.trigger;
        protected override InputActionChangeOption ChangeType => TypedUnit.InputActionChangeType;
        protected override string WasRunningVariablePrefix => "float";

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.InvokeMember("action".VariableHighlight(), "ReadValue", new CodeWriter.TypeParameter[] { typeof(float) });
        }
    }
}
#endif
