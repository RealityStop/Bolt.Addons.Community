using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community 
{
    public abstract class BaseAngleGenerator<T> : NodeGenerator<Angle<T>>
    {
        public BaseAngleGenerator(Unit unit) : base(unit) { NameSpaces = "UnityEngine"; }
    
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            data.SetExpectedType(typeof(T));
            string a = GenerateValue(Unit.a, data);
            string b = GenerateValue(Unit.b, data);
            data.RemoveExpectedType();
            return typeof(T).As().CSharpName(false, true) + MakeClickableForThisUnit(".Angle(") + a + MakeClickableForThisUnit(", ") + b + MakeClickableForThisUnit(")");
        }
    } 
}