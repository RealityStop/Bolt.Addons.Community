using UnityEngine.EventSystems;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnEndDrag))]
    public class OnEndDragGenerator : PointerEventUnitGenerator<OnEndDrag, IEndDragHandler>
    {
        public OnEndDragGenerator(Unit unit) : base(unit) { }
    }
}