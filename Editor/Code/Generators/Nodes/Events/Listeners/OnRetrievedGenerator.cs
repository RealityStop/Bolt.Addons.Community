using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Utility;
using System.Collections;
using System.Linq;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnRetrieved))]
    public class OnRetrievedGenerator : AwakeMethodNodeGenerator, IRequireMethods
    {
        private OnRetrieved Unit => unit as OnRetrieved;
        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => Unit.Result.Yield().ToList();

        public override List<TypeParam> Parameters => new TypeParam(typeof(GameObject), "result").Yield().ToList();

        string methodName = "OnRetrievedRunner";

        public OnRetrievedGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.GetVariable("result");
        }

        public override void GenerateAwakeCode(ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented();
            writer.InvokeMember(typeof(EventBus), "Register", new CodeWriter.TypeParameter[] { typeof(PoolData) }, 
                writer.Action(() => writer.GetMember(typeof(CommunityEvents), "OnRetrieved")),
                writer.Action(() => writer.Write(methodName.VariableHighlight()))
            );
            writer.Write(";");
            writer.NewLine();
        }

        protected override void GenerateCode(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            GenerateChildControl(Unit.trigger, data, writer);
        }

        public IEnumerable<MethodGenerator> GetRequiredMethods(ControlGenerationData data)
        {
            if (!Unit.trigger.hasValidConnection) yield break;
            methodName = data.AddMethodName(methodName);
            var method = MethodGenerator.Method(AccessModifier.Private, MethodModifier.None, typeof(void), methodName);
            method.AddParameter(ParameterGenerator.Parameter("args", typeof(PoolData), ParameterModifier.None));
            method.Body(writer => {
                writer.WriteIndented("if".ControlHighlight());
                writer.Write(" (");
                writer.GetMember("args".VariableHighlight(), "pool");
                writer.Write(" == ");
                GenerateValue(Unit.Pool, data, writer);
                writer.Write(")");
                writer.NewLine();
                writer.WriteLine("{");
                using (writer.IndentedScope(data))
                {
                    writer.WriteIndented(Unit.coroutine ? $"StartCoroutine({Name}({"args".VariableHighlight()}.{"arg".VariableHighlight()}));" : Name + $"({"args".VariableHighlight()}.{"arg".VariableHighlight()});");
                    writer.NewLine();
                }
                writer.WriteLine("}");
            });
            yield return method;
        }

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.Pool && !input.hasValidConnection && Unit.defaultValues[input.key] == null)
            {
                writer.GetVariable("gameObject").GetComponent(typeof(ObjectPool));
                return;
            }
            base.GenerateValueInternal(input, data, writer);
        }
    }
}