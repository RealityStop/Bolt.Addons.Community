namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(ScalarPerSecond))]
    public class ScalarPerSecondGenerator : PerSecondGenerator<float>
    {
        public ScalarPerSecondGenerator(Unit unit) : base(unit) { }
    }
}
