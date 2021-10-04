using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public class BorderedRectangle : Rectangle
    {
        public int thickness;
        public Color borderColor;

        public BorderedRectangle(Color color, Color borderColor, int thickness, int radius = 0) : base(color, radius)
        {
            this.thickness = thickness;
            this.borderColor = borderColor;

            style.borderLeftColor = borderColor;
            style.borderRightColor = borderColor;
            style.borderTopColor = borderColor;
            style.borderBottomColor = borderColor;

            style.borderLeftWidth = thickness;
            style.borderRightWidth = thickness;
            style.borderTopWidth = thickness;
            style.borderBottomWidth = thickness;
        }
    }
}
