namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ScalarAverage))]
    public class ScalarAverageGenerator : BaseAverageGenerator<float>
    {
        public ScalarAverageGenerator(Unit unit) : base(unit) { }
    }
}