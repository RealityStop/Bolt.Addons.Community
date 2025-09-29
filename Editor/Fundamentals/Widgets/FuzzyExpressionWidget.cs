namespace Unity.VisualScripting.Community 
{
    [Widget(typeof(FuzzyExpression))]
    public sealed class FuzzyExpressionWidget : UnitWidget<FuzzyExpression>
    {
        public FuzzyExpressionWidget(FlowCanvas canvas, FuzzyExpression unit) : base(canvas, unit)
        {
            graph.elements.Remove(unit);
        }
    } 
}