using System;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    [Widget(typeof(FuzzyLiteral))]
    public sealed class FuzzyLiteralWidget : UnitWidget<FuzzyLiteral>
    {
        public FuzzyLiteralWidget(FlowCanvas canvas, FuzzyLiteral unit) : base(canvas, unit)
        {
    
        }
    
        public override void DrawForeground()
        {
            var Literal = new Literal(unit.type, unit.value);
            var unitPosition = unit.position;
            var preservation = UnitPreservation.Preserve(unit);
            context.BeginEdit();
            UndoUtility.RecordEditedObject("Replace Node");
            var unitgraph = unit.graph;
            Literal.guid = Guid.NewGuid();
            Literal.position = unitPosition;
            unitgraph.units.Add(Literal);
            preservation.RestoreTo(Literal);
            unit.graph.units.Remove(unit);
            selection.Select(Literal);
            GUI.changed = true;
            context.EndEdit();
        }
    } 
}
