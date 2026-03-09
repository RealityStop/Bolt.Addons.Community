namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(ScalarListAverage))]
    public class ScalarListAverageGenerator : BaseListAverage<ScalarListAverage, float>
    {
        public ScalarListAverageGenerator(Unit unit) : base(unit) { }

        protected override ValueInput GetNumbersInput()
        {
            return Unit.numbers;
        }
    }
}