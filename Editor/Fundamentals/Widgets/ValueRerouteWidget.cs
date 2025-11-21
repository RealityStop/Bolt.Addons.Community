using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Widget(typeof(ValueReroute))]
    public sealed class ValueRerouteWidget : UnitWidget<ValueReroute>
    {
        // Data is stored instead of the unit itself because if the unit is deleted after copying,
        // the data we need from it is also deleted.
        private class RerouteCopyData
        {
            public Guid copyID;
            public IUnitConnection sourceConnection;
            public IGraph graph;
            // Store a reference incase it was not deleted so we can reset its copyID
            public ValueReroute reroute;
        }

        private static List<RerouteCopyData> copyDataList = new List<RerouteCopyData>();
        public ValueRerouteWidget(FlowCanvas canvas, ValueReroute unit) : base(canvas, unit)
        {
            var data = copyDataList.FirstOrDefault(d => d.copyID == unit.copyID);

            if (data != null)
            {
                if (unit.hideConnection && data.sourceConnection != null && data.graph == unit.graph)
                {
                    var source = data.sourceConnection.source;
                    unit.input.ValidlyConnectTo(source);
                }

                if (data.reroute != null)
                {
                    data.reroute.copyID = default;
                }
            }

            unit.copyID = default;
        }

        public override void ExpandCopyGroup(HashSet<IGraphElement> copyGroup)
        {
            var copyID = unit.guid;
            if (!copyDataList.Any(c => c.copyID == copyID))
            {
                var copy = new RerouteCopyData
                {
                    copyID = copyID,
                    graph = unit.graph,
                    sourceConnection = unit.input.hasValidConnection ? unit.input.connection : null
                };
                unit.copyID = copyID;

                copyDataList.Add(copy);
            }

            base.ExpandCopyGroup(copyGroup);
        }

        public override void DrawForeground()
        {
            var inputHasConnection = inputs[0].port.hasAnyConnection;
            var outputHasConnection = outputs[0].port.hasAnyConnection;
            mouseIsOver = new Rect(_position.x - 20, _position.y - 10, mouseIsOver ? 80 : 40, 40).Contains(mousePosition);

#if VISUAL_SCRIPTING_1_7_3
            _position.width = 26;
            GraphGUI.Node(new Rect(position.x, position.y + 3, 26, _position.height - 4), NodeShape.Square, NodeColor.Gray, isSelected);
#else
            if (isSelected || mouseIsOver || !inputHasConnection || !outputHasConnection || unit.hideConnection)
            {
                var width = 26f;
                UnitPortDescription inputDescription = null;
                if (unit.hideConnection && unit.input.hasValidConnection)
                {
                    inputDescription = unit.input.connection.source.Description<UnitPortDescription>();
                    width = UnitPortWidget<ValueInput>.Styles.label.CalcSize(inputDescription.ToGUIContent(IconSize.Small)).x + 50f;
                }
                _position.width = width;
                GraphGUI.Node(new Rect(position.x, position.y + 3, width, _position.height - 4), NodeShape.Square, color, isSelected);

                if (inputDescription != null)
                    GUI.Label(new Rect(position.x + 24, position.y + 5, width, _position.height - 4), inputDescription.label);
            }
            else
            {
                _position.width = -19;
            }
#endif

            Reposition();
        }

        EditorTexture valueIcon;

        protected override bool snapToGrid => unit.SnapToGrid;

        public override bool foregroundRequiresInput => true;

        private bool mouseIsOver;

        public override void CachePosition()
        {
            var inputPort = inputs[0].port as ValueInput;
            var outputPort = outputs[0].port;
            var inputHasConnection = inputPort.hasValidConnection;
            var outputHasConnection = outputPort.hasValidConnection;
            _position.x = unit.position.x;
            _position.y = unit.position.y;

#if VISUAL_SCRIPTING_1_7_3
            _position.width = 26;
#else
            if (unit.hideConnection && inputHasConnection)
            {
                _position.width = VisualScripting.UnitPortWidget<ValueInput>.Styles.label.CalcSize(inputPort.connection.source.Description<UnitPortDescription>().ToGUIContent(IconSize.Small)).x + 50f;
            }
            else
                _position.width = !inputHasConnection || !outputHasConnection || isSelected || mouseIsOver || unit.hideConnection ? 26 : -25;
#endif

            _position.height = 20;

            inputs[0].y = _position.y + 5;
            outputs[0].y = _position.y + 5;

            if (valueIcon == null && (inputPort.Descriptor()).description.icon != null) valueIcon = ((UnitPortDescriptor)inputPort.Descriptor()).description.icon;

#if !VISUAL_SCRIPTING_1_7_3
            if (!unit.hideConnection && !inputHasConnection)
            {
                if (inputHasConnection && !outputHasConnection) { ((UnitPortDescriptor)inputPort.Descriptor()).description.icon = null; }
                ((UnitPortDescriptor)inputPort.Descriptor()).description.icon = !inputHasConnection || isSelected || mouseIsOver ? valueIcon : null;
                ((UnitPortDescriptor)outputPort.Descriptor()).description.icon = !outputHasConnection || isSelected || mouseIsOver ? valueIcon : null;
            }
            else if (unit.hideConnection && inputHasConnection)
            {
                ((UnitPortDescriptor)inputPort.Descriptor()).description.icon = ((UnitPortDescriptor)inputPort.connection.source.Descriptor()).description.icon;
            }
#endif
        }
    }
}