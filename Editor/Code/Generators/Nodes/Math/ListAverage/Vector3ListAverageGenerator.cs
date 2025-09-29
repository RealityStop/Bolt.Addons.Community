namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector3ListAverage))]
    public class Vector3ListAverageGenerator : BaseListAverage<Vector3ListAverage, float>
    {
        public Vector3ListAverageGenerator(Unit unit) : base(unit) { }

        protected override ValueInput GetNumbersInput()
        {
            return Unit.numbers;
        }
    }
}