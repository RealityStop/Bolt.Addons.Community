namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Vector4ListAverage))]
    public class Vector4ListAverageGenerator : BaseListAverage<Vector4ListAverage, float>
    {
        public Vector4ListAverageGenerator(Unit unit) : base(unit) { }

        protected override ValueInput GetNumbersInput()
        {
            return Unit.numbers;
        }
    }
}