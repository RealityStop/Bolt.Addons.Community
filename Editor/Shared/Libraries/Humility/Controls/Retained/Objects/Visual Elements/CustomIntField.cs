using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public class CustomIntField : AdvancedField<IntegerField, int>
    {
        public CustomIntField(string label, int @default, LabelPosition position = LabelPosition.Left, int minFieldWidth = 120, VisualElement prependLabel = null, VisualElement appendLabel = null, Action<IntegerField, int> onValueChanged = null) : base(label, @default, position, minFieldWidth, prependLabel, appendLabel, onValueChanged)
        {
        }
    }
}
