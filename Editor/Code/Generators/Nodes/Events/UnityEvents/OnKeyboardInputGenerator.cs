using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;
using System.Collections;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnKeyboardInput))]
    public class OnKeyboardInputGenerator : UpdateMethodNodeGenerator
    {
        private OnKeyboardInput Unit => unit as OnKeyboardInput;
        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>();

        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override MethodModifier MethodModifier => MethodModifier.None;

        public override string Name => "OnKeyboardInput" + count;

        public override Type ReturnType => Unit.coroutine ? typeof(IEnumerator) : typeof(void);

        public override List<TypeParam> Parameters => new List<TypeParam>();

        public OnKeyboardInputGenerator(Unit unit) : base(unit) { }

        public override void GenerateUpdateCode(ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented("if".ControlHighlight());
            writer.Write(" (");
            writer.CallCSharpUtilityMethod("GetKeyAction",
                writer.Action(() => GenerateValue(Unit.key, data, writer)),
                writer.Action(() => GenerateValue(Unit.action, data, writer))
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
            GenerateChildControl(Unit.trigger, data, writer);
        }
    }
}