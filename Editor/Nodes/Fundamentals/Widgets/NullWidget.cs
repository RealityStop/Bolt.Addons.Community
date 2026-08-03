#if NEW_UNIT_UI
using UnityEngine;
using System.Linq;

namespace Unity.VisualScripting.Community
{
    [Widget(typeof(Null))]
    public sealed class NullWidget : UnitWidget<Null>
    {
        public NullWidget(FlowCanvas canvas, Null unit) : base(canvas, unit) { }

        private const float IconWidth = 16f;
        private const float IconHeight = 16f;
        private const float HorizontalSpacing = 6f;
        private const float InnerPaddingX = 6f;

        private static readonly GUIContent content = new GUIContent("Null");
        public override void CachePosition()
        {
            var edgeOrigin = unit.position;
            var edgeX = edgeOrigin.x;
            var edgeY = edgeOrigin.y;

            var titleSize = Styles.title.CalcSize(content);
            float titleWidth = titleSize.x;
            float titleHeight = titleSize.y;

            var validOutput = outputs.OfType<ValueOutputWidget>().FirstOrDefault();

            float currentX = edgeX + InnerPaddingX;

            iconPosition = new Rect(currentX, edgeY + 4f, IconWidth, IconHeight);
            currentX += IconWidth + HorizontalSpacing;

            titlePosition = new Rect(currentX, edgeY + 3f, titleWidth, titleHeight);
            currentX += titleWidth + HorizontalSpacing;

            float maxHeaderHeight = Mathf.Max(IconHeight, titleHeight);
            if (validOutput != null)
            {
                validOutput.y = edgeY + (maxHeaderHeight / 2f) - (validOutput.GetHeight() / 2f) + 3f;
                validOutput.port.Description<UnitPortDescription>().showLabel = false;
            }

            var invalidInputs = inputs.ToList();
            var invalidOutputs = outputs.Where(p => p != validOutput).ToList();

            bool hasInvalidPorts = false;
            float inputY = edgeY + maxHeaderHeight + 10f;
            float maxInputWidth = 0f;

            foreach (var port in invalidInputs)
            {
                hasInvalidPorts = true;
                port.y = inputY;
                inputY += port.GetHeight() + Styles.spaceBetweenPorts;
                maxInputWidth = Mathf.Max(maxInputWidth, port.GetInnerWidth());
            }

            float invalidOutputY = edgeY + maxHeaderHeight + 10f;
            float maxOutputWidth = (validOutput != null) ? validOutput.GetInnerWidth() : 0f;

            foreach (var port in invalidOutputs)
            {
                hasInvalidPorts = true;
                port.y = invalidOutputY;
                invalidOutputY += port.GetHeight() + Styles.spaceBetweenPorts;
                maxOutputWidth = Mathf.Max(maxOutputWidth, port.GetInnerWidth());
            }

            float totalWidth = (currentX - edgeX) + maxOutputWidth + InnerPaddingX;


            float totalHeight = !hasInvalidPorts 
                ? maxHeaderHeight
                : Mathf.Max(maxHeaderHeight, inputY - edgeY, invalidOutputY - edgeY);

            _position = new Rect(edgeX, edgeY, totalWidth, totalHeight);
        }
    }
}
#endif