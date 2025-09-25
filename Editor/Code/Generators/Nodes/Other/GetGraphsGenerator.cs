using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    public abstract class GetGraphsGenerator<TGraph, TGraphAsset, TMachine> : NodeGenerator<GetGraphs<TGraph, TGraphAsset, TMachine>>
        where TGraph : class, IGraph, new()
        where TGraphAsset : Macro<TGraph>
        where TMachine : Machine<TGraph, TGraphAsset>
    {
        public GetGraphsGenerator(Unit unit) : base(unit)
        {
            NameSpaces = "Unity.VisualScripting.Community";
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (output == Unit.graphList)
            {
                string goExpr = GenerateValue(Unit.gameObject, data);
                var builder = Unit.CreateClickableString();
                builder.InvokeMember(typeof(CSharpUtility), "GetGraphs", new Type[] { typeof(TGraph), typeof(TGraphAsset), typeof(TMachine) }, false, p1 => p1.Ignore(goExpr));
                return builder;
            }

            return base.GenerateValue(output, data);
        }
    }
}