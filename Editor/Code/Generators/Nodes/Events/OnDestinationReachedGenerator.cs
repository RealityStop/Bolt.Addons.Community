#if MODULE_AI_EXISTS
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Utility;
using System.Collections;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnDestinationReached))]
    public class OnDestinationReachedGenerator : UpdateMethodNodeGenerator
    {
        private OnDestinationReached Unit => unit as OnDestinationReached;
        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>();

        public override Type ReturnType => Unit.coroutine ? typeof(IEnumerator) : typeof(void);

        public override List<TypeParam> Parameters => new List<TypeParam>();

        public OnDestinationReachedGenerator(Unit unit) : base(unit) { }

        public override void GenerateUpdateCode(ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented("if".ControlHighlight());
            writer.Write(" (");
            writer.CallCSharpUtilityMethod("DestinationReached",
                writer.Action(() => writer.Write("gameObject".VariableHighlight())),
                writer.Action(() => GenerateValue(Unit.threshold, data, writer)),
                writer.Action(() => GenerateValue(Unit.requireSuccess, data, writer))
            );
            writer.Write(")");
            writer.NewLine();
            writer.WriteLine("{");
            using (writer.IndentedScope(data))
            {
                writer.WriteIndented(Unit.coroutine ? $"StartCoroutine({Name}())" : Name + "()");
                writer.Write(";");
                writer.NewLine();
            }
            writer.WriteLine("}");
        }

        protected override void GenerateCode(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            GenerateExitControl(Unit.trigger, data, writer);
        }
    }
}
#endif