namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector3Sum))]
    public class Vector3SumGenerator : SumGenerator<Vector3Sum>
    {
        public Vector3SumGenerator(Unit unit) : base(unit)
        {
        }
    }
}