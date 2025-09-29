using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Utility;
using System.Collections;
using System.Linq;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnReturned))]
    [RequiresMethods]
    public class OnReturnedGenerator : AwakeMethodNodeGenerator, IRequireMethods
    {
        private OnReturned Unit => unit as OnReturned;
        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => Unit.Result.Yield().ToList();

        public override List<TypeParam> Parameters => new TypeParam(typeof(GameObject), "result").Yield().ToList();

        string methodName = "OnReturnedRunner";

        public OnReturnedGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit("result".VariableHighlight());
        }

        public override string GenerateAwakeCode(ControlGenerationData data, int indent)
        {
            var builder = Unit.CreateClickableString();
            builder.Indent(indent);
            builder.InvokeMember(typeof(EventBus), "Register", new Type[] { typeof(PoolData) }, p1 => p1.GetMember(typeof(CommunityEvents), "OnReturned"), p2 => p2.Clickable(methodName)).Clickable(";").NewLine();
            return builder;
        }

        protected override string GenerateCode(ControlInput input, ControlGenerationData data, int indent)
        {
            return GetNextUnit(Unit.trigger, data, indent);
        }

        public IEnumerable<MethodGenerator> GetRequiredMethods(ControlGenerationData data)
        {
            if (!Unit.trigger.hasValidConnection) yield break;
            methodName = data.AddMethodName(methodName);
            var method = MethodGenerator.Method(AccessModifier.Private, MethodModifier.None, typeof(void), methodName);
            method.AddParameter(ParameterGenerator.Parameter("args", typeof(PoolData), ParameterModifier.None));
            var builder = Unit.CreateClickableString();
            builder.Clickable("if ".ControlHighlight()).Parentheses(inner => inner.GetMember("args".VariableHighlight(), "pool").Equals(true).Ignore(GenerateValue(Unit.Pool, data)));
            builder.Body(null, (inner, indent) => inner.Indent(indent).Clickable(Unit.coroutine ? $"StartCoroutine({Name}({"args".VariableHighlight()}.{"arg".VariableHighlight()}));" : Name + $"({"args".VariableHighlight()}.{"arg".VariableHighlight()});"), true);
            method.Body(builder);
            yield return method;
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.Pool && !input.hasValidConnection && Unit.defaultValues[input.key] == null)
            {
                return MakeClickableForThisUnit("gameObject".VariableHighlight() + ".GetComponent<" + typeof(ObjectPool).As().CSharpName(false, true) + ">()");
            }
            return base.GenerateValue(input, data);
        }
    }
}