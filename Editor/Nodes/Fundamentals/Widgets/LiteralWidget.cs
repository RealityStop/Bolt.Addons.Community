#if ENABLE_VERTICAL_FLOW
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

            const float compactX = 0.8f;

            var titleWidth = Styles.title.CalcSize(titleContent).x;
            var iconSize = Styles.iconSize;
            var innerY = edgeY;
            var innerX = edgeX;

            iconPosition = new Rect(innerX, innerY, iconSize, iconSize);

            titlePosition = new Rect(
                iconPosition.xMax + Styles.spaceAfterIcon * compactX,
                innerY,
                titleWidth,
                iconSize
            );

            var totalWidth = titlePosition.xMax + 20f - edgeX;
            var totalHeight = iconSize;

            if (showHeaderAddon)
            {
                var width = GetHeaderAddonWidth();
                var height = GetHeaderAddonHeight(width);

                headerAddonPosition = new Rect(
                    titlePosition.x,
                    titlePosition.yMax + 2f,
                    width,
                    height
                );

                totalWidth = Mathf.Max(totalWidth, headerAddonPosition.xMax + 20f - edgeX);
                totalHeight = headerAddonPosition.yMax - edgeY;
            }

            var validOutput = outputs.OfType<ValueOutputWidget>().FirstOrDefault();
            var invalidOutputs = outputs
                .Where(p => p is InvalidOutputWidget)
                .Cast<IUnitPortWidget>()
                .ToList();

            float portsStartY = edgeY + totalHeight + Styles.spaceBetweenPorts;

            if (validOutput != null)
            {
                float visualCenterY = Styles.spaceBetweenPorts + edgeY + (totalHeight / 2f) - (validOutput.GetHeight() / 2f);
                validOutput.y = visualCenterY;

                portsStartY += validOutput.GetHeight() + Styles.spaceBetweenPorts;

                totalWidth += validOutput.GetInnerWidth();
            }

            foreach (var port in invalidOutputs)
            {
                port.y = portsStartY;
                portsStartY += port.GetHeight() + Styles.spaceBetweenPorts;

                totalWidth = Mathf.Max(
                    totalWidth,
                    port.GetInnerWidth() + 20f
                );
            }

            if (invalidOutputs.Count > 0)
            {
                totalHeight = portsStartY - edgeY;
            }

            _position = new Rect(edgeX, edgeY, totalWidth, totalHeight);
        }
    }
}
#endif