using UnityEngine.EventSystems;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnPointerUp))]
    public class OnPointerUpGenerator : PointerEventUnitGenerator<OnPointerUp, IPointerUpHandler>
    {
        public OnPointerUpGenerator(Unit unit) : base(unit) { }
    }
}