using System;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Widget(typeof(ConvertNode))]
    public sealed class ConvertNodeWidget : UnitWidget<ConvertNode>
    {
        public ConvertNodeWidget(FlowCanvas canvas, ConvertNode unit) : base(canvas, unit)
        {
        }

        protected override bool showHeaderAddon => true;
        public override bool foregroundRequiresInput => true;
        private ConversionType lastConversionType;
        private Type lastType;

        protected override void DrawHeaderAddon()
        {
            if (unit.conversion == ConversionType.ToArrayOfObject || unit.conversion == ConversionType.ToListOfObject)
            {
                LudiqGUI.Inspector(metadata["conversion"], new Rect(headerAddonPosition.x, headerAddonPosition.y, GetHeaderAddonWidth(), 18), GUIContent.none);
            }
            else
            {
                LudiqGUI.Inspector(metadata["conversion"], new Rect(headerAddonPosition.x, headerAddonPosition.y, GetHeaderAddonWidth(), 18), GUIContent.none);
                LudiqGUI.Inspector(metadata["type"], new Rect(headerAddonPosition.x, headerAddonPosition.y + 20, GetHeaderAddonWidth(), 18), GUIContent.none);
            }

            if (lastConversionType != unit.conversion)
            {
                lastConversionType = unit.conversion;
                Reposition();
                unit.Define();
            }

            if (lastType == null || lastType != unit.type)
            {
                lastType = unit.type;
                unit.Define();
            }
        }

        protected override float GetHeaderAddonWidth()
        {
            return 130;
        }

        protected override float GetHeaderAddonHeight(float width)
        {
            if (unit.conversion == ConversionType.ToArrayOfObject || unit.conversion == ConversionType.ToListOfObject) return 20;
            return 40;
        }
    }
} 