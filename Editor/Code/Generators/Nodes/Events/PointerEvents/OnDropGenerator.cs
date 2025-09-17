using UnityEngine.EventSystems;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnDrop))]
    public class OnDropGenerator : PointerEventUnitGenerator<OnDrop, IDropHandler>
    {
        public OnDropGenerator(Unit unit) : base(unit) { }
    }
}