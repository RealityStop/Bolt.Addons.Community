using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public class CustomFloatField : AdvancedField<FloatField, float>
    {
        public CustomFloatField(string label, float @default, LabelPosition position = LabelPosition.Left, int minFieldWidth = 120, VisualElement prependLabel = null, VisualElement appendLabel = null, Action<FloatField, float> onValueChanged = null) : base(label, @default, position, minFieldWidth, prependLabel, appendLabel, onValueChanged)
        {
        }
    }
}
