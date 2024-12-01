namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ScalarPerSecond))]
    public class ScalarPerSecondGenerator : PerSecondGenerator<float>
    {
        public ScalarPerSecondGenerator(Unit unit) : base(unit) { }
    }
}
