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
        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (Unit.IsNotRestricted)
            {
                writer.GetVariable("args");
            }
            else
            {
                writer.GetVariable("args").GetMember(output.key);
            }
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            GenerateChildControl(Unit.trigger, data, writer);
        }

        public IEnumerable<MethodGenerator> GetRequiredMethods(ControlGenerationData data)
        {
            var disableMethod = MethodGenerator.Method(AccessModifier.Private, MethodModifier.None, typeof(void), "OnDisable");
            disableMethod.Body(w =>
            {
                w.WriteIndented(eventVariableName.VariableHighlight() + "." + "Dispose();".ConstructHighlight());
                w.NewLine();
            });
            yield return disableMethod;

            var enableMethod = MethodGenerator.Method(AccessModifier.Private, MethodModifier.None, typeof(void), "OnEnable");
            enableMethod.Body(w =>
            {
                string args = "args".VariableHighlight();
                string lambdaBody = Unit.coroutine 
                    ? $"StartCoroutine({Name.VariableHighlight()}({args}))" 
                    : $"{Name.VariableHighlight()}({args})";
                
                w.WriteIndented(eventVariableName.VariableHighlight());
                w.Write(" = ".ControlHighlight());
                w.InvokeMember(
                    typeof(DefinedEvent),
                    "RegisterGlobalListener",
                    new CodeWriter.TypeParameter[] { EventType },
                    w.Action(inner => inner.Write($"({args}) => {lambdaBody}")));
                w.Write(";".ConstructHighlight());
                w.NewLine();
            });
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