#if PACKAGE_INPUT_SYSTEM_EXISTS
using UnityEngine;
using Unity.VisualScripting.InputSystem;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnInputSystemEventVector2))]
    public sealed class OnInputSystemEventVector2Generator : OnInputSystemEventGeneratorBase<OnInputSystemEventVector2>
    {
        public OnInputSystemEventVector2Generator(Unit unit) : base(unit) { }

        public override List<ValueOutput> OutputValues => new List<ValueOutput>();
        public override string Name => "OnInputSystemEventVector2" + count;

        protected override ValueInput Target => TypedUnit.Target;
        protected override ValueInput InputAction => TypedUnit.InputAction;
        protected override ControlOutput Trigger => TypedUnit.trigger;
        protected override InputActionChangeOption ChangeType => TypedUnit.InputActionChangeType;
        protected override string WasRunningVariablePrefix => "vector2";

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.InvokeMember("action".VariableHighlight(), "ReadValue", new CodeWriter.TypeParameter[] { typeof(Vector2) });
        }
    }
}
#endif
