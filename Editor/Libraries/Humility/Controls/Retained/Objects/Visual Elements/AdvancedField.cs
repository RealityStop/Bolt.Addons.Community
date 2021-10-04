using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public abstract class AdvancedField<TFieldElement, T> : VisualElement where TFieldElement : BaseField<T>, new()
    {
        protected Label label { get; private set; }

        private VisualElement fieldElement => field.ElementAt(0);

        public TFieldElement field { get; private set; }

        private int _fontSize = 12;
        public int fontSize
        {
            get { return _fontSize; }
            set
            {
                _fontSize = value;
                label.style.fontSize = value;
                field.style.fontSize = value;
            }
        }

        public VisualElement labelContainer { get; private set; }

        private VisualElement _prependLabel;
        public VisualElement prependLabel
        {
            get { return _prependLabel; }

            set
            {
                if (_prependLabel != null && labelContainer.Contains(_prependLabel)) labelContainer.Remove(_prependLabel);
                if (value != null)
                {
                    labelContainer.Add(value);
                    value.PlaceBehind(label);
                }
                _prependLabel = value;
            }
        }

        private VisualElement _appendLabel;
        public VisualElement appendLabel
        {
            get { return _appendLabel; }

            set
            {
                if (_appendLabel != null && labelContainer.Contains(_appendLabel)) labelContainer.Remove(_appendLabel);
                if (value != null)
                {
                    labelContainer.Add(value);
                    value.PlaceInFront(label);
                }
                _appendLabel = value;
            }
        }

        public LabelPosition position;

        public Action<TFieldElement, T> onValueChanged;

        private Color _fieldColor;
        public Color fieldColor { get { return _fieldColor; } set { _fieldColor = value; fieldElement.style.backgroundColor = value; } }

        private Color _fieldTextColor;
        public Color fieldTextColor { get { return _fieldTextColor; } set { _fieldTextColor = value; fieldElement.style.color = value; } }

        private Color _labelColor;
        public Color labelColor { get { return _labelColor; } set { _labelColor = value; label.style.color = value; } }

        public AdvancedField(string label, T @default, LabelPosition position = LabelPosition.Left, int minFieldWidth = 120, VisualElement prependLabel = null, VisualElement appendLabel = null, Action<TFieldElement, T> onValueChanged = null)
        {
            labelContainer = new VisualElement();
            labelContainer.style.position = Position.Relative;
            labelContainer.style.flexDirection = FlexDirection.Row;
            labelContainer.style.height = fontSize;
            labelContainer.style.marginTop = 4;
            labelContainer.style.paddingRight = 4;

            this.label = new Label(label);
            this.label.style.fontSize = fontSize;
            this.label.style.position = Position.Relative;
            this.label.style.flexDirection = FlexDirection.Row;

            labelContainer.Add(this.label);

            this.prependLabel = prependLabel;
            this.appendLabel = appendLabel;
            this.position = position;

            this.StretchToParentWidth();
            style.position = Position.Relative;
            style.unityTextAlign = TextAnchor.MiddleCenter;
            style.flexDirection = (position == LabelPosition.Left || position == LabelPosition.Right) ? FlexDirection.Row : FlexDirection.Column;

            field = System.Activator.CreateInstance(typeof(TFieldElement), @default) as TFieldElement;
            field.style.position = Position.Relative;
            field.style.flexDirection = FlexDirection.Row;
            field.style.height = fontSize + 8;
            field.style.minWidth = minFieldWidth;
            
            field.RegisterValueChangedCallback((e) =>
            {
                onValueChanged?.DynamicInvoke(field, e.newValue);
            });

            if (position == LabelPosition.Left || position == LabelPosition.Top)
            {
                if (position == LabelPosition.Top) this.field.style.marginTop = this.field.style.marginTop.value.value + 8;
                Add(labelContainer);
                Add(field);
            }
            else
            {
                Add(field);
                Add(labelContainer);
            }
        }
    }

    public enum LabelPosition
    {
        Left,
        Right,
        Top,
        Bottom
    }
}
