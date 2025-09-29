namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector4Sum))]
    public class Vector4SumGenerator : SumGenerator<Vector4Sum>
    {
        public Vector4SumGenerator(Unit unit) : base(unit)
        {
        }
    }
}