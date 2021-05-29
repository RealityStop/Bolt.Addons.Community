using UnityEngine;
using UnityEngine.UIElements;

namespace Bolt.Addons.Integrations.Continuum.Humility
{
    public class HorizontalSeperator : VisualElement
    {
        public HorizontalSeperator(int thickness, Color color)
        {
            this.Set().Margin(4);
            style.width = thickness; 

            var rectangle = new Rectangle(color);
            rectangle.StretchToParentSize();

            Add(rectangle);
        }
    }
}
