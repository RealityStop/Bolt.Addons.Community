using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.Humility;
using System.Collections;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(GlobalDefinedEventNode))]
    [RequiresMethods]
    [RequiresVariables]
    public class GlobalDefinedEventNodeGenerator : MethodNodeGenerator, IRequireMethods, IRequireVariables
    {
        private GlobalDefinedEventNode Unit => unit as GlobalDefinedEventNode;

        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => Unit.outputPorts;

        public override List<TypeParam> Parameters => new TypeParam(EventType, "args").Yield().ToList();

        public override string Name => "GlobalDefinedEvent" + count;

        private Type EventType
        {
            get
            {
                if (Unit.IsRestricted)
                {
                    return Unit.RestrictedEventType.type;
                }
                else
                {
                    return Unit.EventType.type;
                }
            }
        }

        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override MethodModifier MethodModifier => MethodModifier.None;

        public override Type ReturnType => Unit.coroutine ? typeof(IEnumerator) : typeof(void);

        private string eventVariableName = "_globalDefinedEvent";

        public GlobalDefinedEventNodeGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (Unit.IsNotRestricted)
                return MakeClickableForThisUnit("args".VariableHighlight());
            else
                return MakeClickableForThisUnit("args".VariableHighlight() + "." + output.key.VariableHighlight());

        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return GetNextUnit(Unit.trigger, data, indent);
        }

        public IEnumerable<MethodGenerator> GetRequiredMethods(ControlGenerationData data)
        {
            var disableMethod = MethodGenerator.Method(AccessModifier.Private, MethodModifier.None, typeof(void), "OnDisable");
            disableMethod.body = MakeClickableForThisUnit(eventVariableName.VariableHighlight() + "." + "Dispose();") + "\n";
            yield return disableMethod;

            var enableMethod = MethodGenerator.Method(AccessModifier.Private, MethodModifier.None, typeof(void), "OnEnable");
            var builder = Unit.CreateClickableString();
            builder.Indent(indent);
            string args = "args".VariableHighlight();
            builder.Clickable(eventVariableName.VariableHighlight()).Equal(true).InvokeMember(
                typeof(DefinedEvent),
                "RegisterGlobalListener",
                new Type[] { EventType },
                p1 => p1.Clickable($"{args} => {(Unit.coroutine ? $"StartCoroutine({Name}({args}))" : $"{Name}({args})")}"));

            builder.Clickable(";").NewLine();
            enableMethod.body = builder;
            yield return enableMethod;
        }

        public IEnumerable<FieldGenerator> GetRequiredVariables(ControlGenerationData data)
        {
            eventVariableName = data.AddLocalNameInScope(eventVariableName);
            var field = FieldGenerator.Field(AccessModifier.Private, FieldModifier.None, typeof(IDisposable), eventVariableName);
            yield return field;
        }
    }
}