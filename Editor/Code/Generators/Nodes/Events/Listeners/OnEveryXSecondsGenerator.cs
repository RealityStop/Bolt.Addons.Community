using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;
using System.Collections;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnEveryXSeconds))]
    public class OnEveryXSecondsGenerator : UpdateMethodNodeGenerator, IRequireVariables
    {
        public OnEveryXSecondsGenerator(Unit unit) : base(unit) { }

        public OnEveryXSeconds Unit => unit as OnEveryXSeconds;

        private string name = "onEveryXSeconds";

        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>();

        public override Type ReturnType => Unit.coroutine ? typeof(IEnumerator) : typeof(void);

        public override List<TypeParam> Parameters => new List<TypeParam>();

        public override void GenerateUpdateCode(ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented("if ".ControlHighlight() + "(" + name.VariableHighlight() + ".Update(");
            GenerateValue(Unit.seconds, data, writer);
            writer.Write(", ");
            GenerateValue(Unit.unscaledTime, data, writer);
            writer.Write("))").NewLine();
            writer.WriteLine("{");
            using (writer.Indented())
            {
                writer.WriteIndented((Unit.coroutine ? $"StartCoroutine({Name}())" : Name + "()") + ";").NewLine();
            }
            writer.WriteLine("}");
        }

        public IEnumerable<FieldGenerator> GetRequiredVariables(ControlGenerationData data)
        {
            name = data.AddLocalNameInScope(name.LegalMemberName(), typeof(OnEveryXSecondsLogic));
            var variable = FieldGenerator.Field(AccessModifier.Private, FieldModifier.None, typeof(OnEveryXSecondsLogic), name);
            variable.Default(new OnEveryXSecondsLogic());
            variable.SetNewlineLiteral(false);
            yield return variable;
        }

        protected override void GenerateCode(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            GenerateChildControl(Unit.trigger, data, writer);
        }
    }
}