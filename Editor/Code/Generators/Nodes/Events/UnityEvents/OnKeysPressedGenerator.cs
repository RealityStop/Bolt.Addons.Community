using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Utility;
using System.Collections;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnMultiKeyPress))]
    public class OnMultiKeyPressGenerator : UpdateMethodNodeGenerator, IRequireVariables
    {
        private OnMultiKeyPress Unit => unit as OnMultiKeyPress;
        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>();

        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override MethodModifier MethodModifier => MethodModifier.None;

        public override string Name => "OnMultiKeyPress" + count;

        private string name = "onMultiKeyPress";

        public override Type ReturnType => Unit.coroutine ? typeof(IEnumerator) : typeof(void);

        public override List<TypeParam> Parameters => new List<TypeParam>();

        public OnMultiKeyPressGenerator(Unit unit) : base(unit) { }

        public override void GenerateUpdateCode(ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented("if".ControlHighlight());
            writer.Write(" (");
            writer.Write(name.VariableHighlight());
            writer.Write(".Check(");
            writer.Object(Unit.Delay);
            writer.Write(", ");
            GenerateValue(Unit.Key1, data, writer);
            writer.Write(", ");
            GenerateValue(Unit.Key2, data, writer);
            writer.Write(", ");
            GenerateValue(Unit.Action, data, writer);
            writer.Write("))");
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
            GenerateChildControl(Unit.trigger, data, writer);
        }

        public IEnumerable<FieldGenerator> GetRequiredVariables(ControlGenerationData data)
        {
            name = data.AddLocalNameInScope(name.LegalMemberName(), typeof(OnMultiKeyPressLogic));
            var variable = FieldGenerator.Field(AccessModifier.Private, FieldModifier.None, typeof(OnMultiKeyPressLogic), name);
            variable.Default(new OnMultiKeyPressLogic());
            variable.SetNewlineLiteral(false);
            yield return variable;
        }

    }
}