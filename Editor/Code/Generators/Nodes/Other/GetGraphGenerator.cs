#if VISUAL_SCRIPTING_1_7
using System;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community.CSharp
{
    public abstract class GetGraphGenerator<TGraph, TGraphAsset, TMachine> : NodeGenerator<GetGraph<TGraph, TGraphAsset, TMachine>>
        where TGraph : class, IGraph, new()
        where TGraphAsset : Macro<TGraph>
        where TMachine : Machine<TGraph, TGraphAsset>
    {
        public GetGraphGenerator(Unit unit) : base(unit)
        {
        }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "Unity.VisualScripting.Community";
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (output == Unit.graphOutput)
            {
                writer.InvokeMember(typeof(CSharpUtility), "GetGraph", new CodeWriter.TypeParameter[] { typeof(TGraph), typeof(TGraphAsset), typeof(TMachine) },
                writer.Action(() => GenerateValue(Unit.gameObject, data, writer)));
                return;
            }
            base.GenerateValueInternal(output, data, writer);
        }
    }
}
#endif