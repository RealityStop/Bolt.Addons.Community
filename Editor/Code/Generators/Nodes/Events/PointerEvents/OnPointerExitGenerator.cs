using UnityEngine.EventSystems;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnPointerExit))]
    public class OnPointerExitGenerator : PointerEventUnitGenerator<OnPointerExit, IPointerExitHandler>
    {
        public OnPointerExitGenerator(Unit unit) : base(unit) { }
    }
}