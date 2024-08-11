namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector2Sum))]
    public class Vector2SumGenerator : SumGenerator<Vector2Sum>
    {
        public Vector2SumGenerator(Unit unit) : base(unit)
        {
        }
    }
}
