#if NEW_UNIT_UI
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Unity.VisualScripting.Community
{
    public sealed class LiteralWidget : UnitWidget<Literal>
    {
        public LiteralWidget(FlowCanvas canvas, Literal unit) : base(canvas, unit) { }

        protected override bool showHeaderAddon => unit.isDefined;
        public override bool foregroundRequiresInput => true;

        protected override float GetHeaderAddonWidth()
        {
            var adaptiveWidthAttribute = unit.type.GetAttribute<InspectorAdaptiveWidthAttribute>();
            return Mathf.Min(metadata.Inspector().GetAdaptiveWidth(), adaptiveWidthAttribute?.width ?? Styles.maxSettingsWidth);
        }

        protected override float GetHeaderAddonHeight(float width)
        {
            return LudiqGUI.GetInspectorHeight(null, metadata, width, GUIContent.none);
        }

        public override void BeforeFrame()
        {
            base.BeforeFrame();
            if (showHeaderAddon &&
                (GetHeaderAddonWidth() != headerAddonPosition.width ||
                GetHeaderAddonHeight(headerAddonPosition.width) != headerAddonPosition.height))
            {
                Reposition();
            }
        }

        protected override void DrawHeaderAddon()
        {
            using (LudiqGUIUtility.labelWidth.Override(75))
            using (Inspector.adaptiveWidth.Override(true))
            {
                EditorGUI.BeginChangeCheck();
                LudiqGUI.Inspector(metadata, headerAddonPosition, GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    unit.EnsureDefined();
                    Reposition();
                }
            }
        }

        public override void CachePosition()
        {
            var edgeOrigin = unit.position;
            var edgeX = edgeOrigin.x;
            var edgeY = edgeOrigin.y;

            iconPosition = new Rect(edgeX, edgeY, 0, 0);
            titlePosition = new Rect(edgeX, edgeY, 0, 0);

            var validOutput = outputs.OfType<ValueOutputWidget>().FirstOrDefault();

            var invalidInputs = inputs.ToList();
            var invalidOutputs = outputs.Where(p => p != validOutput).ToList();

            float addonWidth = 0f;
            float addonHeight = 0f;
            if (showHeaderAddon)
            {
                addonWidth = GetHeaderAddonWidth();
                addonHeight = GetHeaderAddonHeight(addonWidth);
            }

            float inspectorX = edgeX + 5f;
#if !NEW_UNIT_STYLE
            headerAddonPosition = new Rect(inspectorX, edgeY + 4.5f, addonWidth, addonHeight);
#else
            headerAddonPosition = new Rect(inspectorX, edgeY + 2f, addonWidth, addonHeight);
#endif
            if (validOutput != null)
            {
                validOutput.y = headerAddonPosition.y + (addonHeight / 2f) - (validOutput.GetHeight() / 2f);
#if !NEW_UNIT_STYLE
                validOutput.y += 2.5f;
#endif
            }

            bool hasInvalidPorts = false;
            float inputY = headerAddonPosition.y + addonHeight + 10f;
            float maxInputWidth = 0f;
            foreach (var port in invalidInputs)
            {
                hasInvalidPorts = true;

                port.y = inputY;
                inputY += port.GetHeight() + Styles.spaceBetweenPorts;
                maxInputWidth = Mathf.Max(maxInputWidth, port.GetInnerWidth());
            }

            float outputY = (validOutput != null) ? validOutput.y + validOutput.GetHeight() + Styles.spaceBetweenPorts : headerAddonPosition.y;
            float maxOutputWidth = (validOutput != null) ? validOutput.GetInnerWidth() : 0f;

            var invalidOutputY = headerAddonPosition.y + addonHeight + 10f;

            foreach (var port in invalidOutputs)
            {
                hasInvalidPorts = true;

                port.y = invalidOutputY;
                invalidOutputY += port.GetHeight() + Styles.spaceBetweenPorts;
                maxOutputWidth = Mathf.Max(maxOutputWidth, port.GetInnerWidth());
            }

            float totalWidth = 15f + addonWidth + maxInputWidth + maxOutputWidth;

#if NEW_UNIT_STYLE
            const float heightPadding = 0;
#else
            const float heightPadding = 5;
#endif

            float totalHeight = !hasInvalidPorts ? headerAddonPosition.height + heightPadding : Mathf.Max(headerAddonPosition.height + heightPadding, inputY - edgeY, outputY - edgeY);

            _position = new Rect(edgeX, edgeY, totalWidth, totalHeight);
        }
    }
}
#endif