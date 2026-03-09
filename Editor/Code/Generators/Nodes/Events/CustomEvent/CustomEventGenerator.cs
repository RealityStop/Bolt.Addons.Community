using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(CustomEvent))]
    public class CustomEventGenerator : MethodNodeGenerator
    {
        public CustomEventGenerator(Unit unit) : base(unit)
        {
        }

        private CustomEvent Unit => unit as CustomEvent;
        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => Unit.argumentPorts;

        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override MethodModifier MethodModifier => MethodModifier.None;

        public override string Name => "CustomEvent" + count;

        public override Type ReturnType => Unit.coroutine ? typeof(IEnumerator) : typeof(void);

        public override List<TypeParam> Parameters => new List<TypeParam>() { new TypeParam(typeof(CustomEventArgs), "args") };

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "Unity.VisualScripting.Community";
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            GenerateChildControl(Unit.trigger, data, writer);
        }

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.target && !input.hasValidConnection)
            {
                writer.GetVariable("gameObject");
                return;
            }
            base.GenerateValueInternal(input, data, writer);
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (Unit.argumentPorts.Contains(output))
            {
                using (writer.Cast(data.GetExpectedType(), () => data.GetExpectedType() != null && !data.IsCurrentExpectedTypeMet() && data.GetExpectedType() != typeof(object), true))
                {
                    data.MarkExpectedTypeMet();
                    writer.InvokeMember("args".VariableHighlight(), nameof(CSharpUtility.GetArgument), new CodeWriter.TypeParameter[] { new CodeWriter.TypeParameter(true) { TypeValue = data.GetExpectedType() ?? typeof(object) } },
                    writer.ObjectString(Unit.argumentPorts.IndexOf(output)));
                }
                return;
            }
            base.GenerateValueInternal(output, data, writer);
        }
    }
}