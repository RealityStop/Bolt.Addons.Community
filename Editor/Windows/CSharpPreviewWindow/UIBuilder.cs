using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    internal static class UIBuilder
    {
        public static Button CreateToolbarButton(string text, System.Action onClick, bool right, bool left)
        {
            var button = new Button(onClick) { text = text };
            button.style.paddingLeft = 6;
            button.style.paddingRight = 6;
            button.style.marginLeft = 0;
            button.style.marginRight = 0;
            button.style.paddingTop = 4;
            button.style.paddingBottom = 4;
            button.style.backgroundColor = new Color(0, 0, 0, 0);
            button.style.borderTopColor = new Color(0, 0, 0, 0);
            button.style.borderBottomColor = new Color(0, 0, 0, 0);
            button.style.borderLeftColor = new Color(0.1f, 0.1f, 0.1f);
            button.style.borderRightColor = new Color(0.1f, 0.1f, 0.1f);
            button.style.borderTopWidth = 0;
            button.style.borderBottomWidth = 0;
            if (left)
                button.style.borderLeftWidth = 1;
            else
                button.style.borderLeftWidth = 0;
            if (right)
                button.style.borderRightWidth = 1;
            else
                button.style.borderRightWidth = 0;
            button.style.color = Color.white;
            button.style.borderTopLeftRadius = 0;
            button.style.borderBottomLeftRadius = 0;
            button.style.borderTopRightRadius = 0;
            button.style.borderBottomRightRadius = 0;

            // Hover effects
            var defaultBackgroundColor = button.style.backgroundColor.value;
            var hoverBackgroundColor = new Color(0.15f, 0.15f, 0.15f); // Darker color on hover

            button.RegisterCallback<MouseEnterEvent>(evt =>
            {
                button.style.backgroundColor = hoverBackgroundColor;
            });

            button.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                button.style.backgroundColor = defaultBackgroundColor;
            });

            return button;
        }
        const float labelsWidth = 200;
        public static void AddColorField(CSharpPreviewSettings settings, System.Action initialize, VisualElement container, string labelText, string tooltip, Color initialColor, System.Action<Color> onColorChanged, System.Action<ColorField> resetToDefault)
        {
            initialize();

            var colorContainer = new VisualElement
            {
                style = { marginTop = 10, flexDirection = FlexDirection.Row }
            };

            var colorLabel = new Label(labelText)
            {
                tooltip = tooltip,
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginRight = 10, width = labelsWidth }
            };

            var colorField = new ColorField
            {
                style = { marginLeft = 10, width = 400 },
                value = initialColor
            };

            colorField.RegisterValueChangedCallback(evt =>
            {
                onColorChanged(evt.newValue);
                settings.SaveAndDirty();
            });

            var defaultButton = new Button(() =>
            {
                resetToDefault(colorField);
                settings.SaveAndDirty();
            })
            {
                text = "Default",
                style = { marginLeft = 10 }
            };

            colorContainer.Add(colorLabel);
            colorContainer.Add(colorField);
            colorContainer.Add(defaultButton);

            container.Add(colorContainer);
        }

        public static void RemovePaddingAndMargin(VisualElement element)
        {
            element.style.paddingLeft = 0;
            element.style.paddingRight = 0;
            element.style.paddingTop = 0;
            element.style.paddingBottom = 0;
            element.style.marginLeft = 0;
            element.style.marginRight = 0;
            element.style.marginTop = 0;
            element.style.marginBottom = 0;
        }

    }
}
