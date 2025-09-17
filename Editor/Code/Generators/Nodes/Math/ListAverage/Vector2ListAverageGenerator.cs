namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector2ListAverage))]
    public class Vector2ListAverageGenerator : BaseListAverage<Vector2ListAverage, float>
    {
        public Vector2ListAverageGenerator(Unit unit) : base(unit) { }

        protected override ValueInput GetNumbersInput()
        {
            return Unit.numbers;
        }
    }
}