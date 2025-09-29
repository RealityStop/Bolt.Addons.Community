using UnityEngine;

namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// This node has no functionality; it's just a Arrow.
    /// </summary>
    [UnitCategory("Community\\Documentation")]
    [UnitTitle("Arrow")]
    public class Arrow : Unit
    {
        [Inspectable]
        public float Length = 20f;

        [Inspectable]
        public float rotationAngle = 0f;

        [Inspectable]
        public Color Color = Color.red;

        [Inspectable]
        public Color ArrowColor = Color.red;

        [Inspectable]
        public string Text;

        public bool ShowSquare = true;

        public bool ShowBottomArrow = true;

        public bool ShowTopArrow = true;

        public LineType lineType = LineType.Normal;

        public override bool isControlRoot { get { return true; } }
        protected override void Definition()
        {
        }
    }

    [RenamedFrom("LineType")]
    public enum LineType
    {
        Normal,
        Dotted
    }
}