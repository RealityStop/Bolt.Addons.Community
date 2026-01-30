#if VISUAL_SCRIPTING_1_7
using System.Collections.Generic;

namespace Unity.VisualScripting.Community.CSharp
{
    public abstract class HasGraphGenerator<TGraph, TMacro, TMachine> : LocalVariableGenerator
        where TGraph : class, IGraph, new()
        where TMacro : Macro<TGraph>
        where TMachine : Machine<TGraph, TMacro>
    {
        protected HasGraph<TGraph, TMacro, TMachine> Unit => unit as HasGraph<TGraph, TMacro, TMachine>;
        public HasGraphGenerator(Unit unit) : base(unit)
        {
        }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "Unity.VisualScripting.Community";
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (output == Unit.hasGraphOutput)
            {
                if (!Unit.enter.hasValidConnection)
                {

                    writer.CallCSharpUtilityGenericMethod("HasGraph", new CodeWriter.TypeParameter[] { typeof(TGraph), typeof(TMacro), typeof(TMachine) },
                    writer.Action(() => GenerateValue(Unit.target, data, writer)),
                    writer.Action(() => GenerateValue(Unit.graphInput, data, writer)));
                }
                else writer.GetVariable(variableName);
                return;
            }

            base.GenerateValueInternal(output, data, writer);
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            variableName = data.AddLocalNameInScope("hasGraphValue", typeof(bool));
            variableType = typeof(bool);

            writer.CreateVariable(typeof(bool), variableName, writer.Action(() =>
            {
                writer.CallCSharpUtilityGenericMethod("HasGraph", new CodeWriter.TypeParameter[] { typeof(TGraph), typeof(TMacro), typeof(TMachine) },
                writer.Action(() => GenerateValue(Unit.target, data, writer)),
                writer.Action(() => GenerateValue(Unit.graphInput, data, writer)));
            }), WriteOptions.Indented, EndWriteOptions.LineEnd);

            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}
#endif