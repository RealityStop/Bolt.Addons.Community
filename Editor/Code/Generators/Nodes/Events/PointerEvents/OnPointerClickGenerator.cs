using UnityEngine.EventSystems;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnPointerClick))]
    public class OnPointerClickGenerator : PointerEventUnitGenerator<OnPointerClick, IPointerClickHandler>
    {
        public OnPointerClickGenerator(Unit unit) : base(unit) { }
    }
}