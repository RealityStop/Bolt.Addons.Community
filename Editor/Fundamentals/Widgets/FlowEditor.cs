using UnityEngine;
using Unity.VisualScripting;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.Community;

[Widget(typeof(ControlConnection))]
public sealed class ControlConnectionWidget : UnitConnectionWidget<ControlConnection>
{
    private bool canTrigger = true;
    private Color _color = Color.white;

    public ControlConnectionWidget(FlowCanvas canvas, ControlConnection connection) : base(canvas, connection)
    {
    }

    public override void HandleInput()
    {
        base.HandleInput();

        var outputFlowReroute = (connection.source.unit as FlowReroute);
        var inputFlowReroute = (connection.destination.unit as FlowReroute);

        if (selection.Contains(connection.destination.unit) && outputFlowReroute != null)
        {
            if (selection.Contains(outputFlowReroute))
            {
                if (e.keyCode == KeyCode.Backspace && e.rawType == EventType.KeyDown && canTrigger)
                {
                    canTrigger = false;

                    outputFlowReroute.OutputVisible = !outputFlowReroute.OutputVisible;

                }

                if (e.keyCode == KeyCode.Backspace && e.rawType == EventType.KeyUp)
                {
                    canTrigger = true;
                }
            }
        }

        if (selection.Contains(connection.source.unit) && inputFlowReroute != null)
        {
            if (selection.Contains(inputFlowReroute))
            {
                if (e.keyCode == KeyCode.Backspace && e.rawType == EventType.KeyDown && canTrigger)
                {
                    canTrigger = false;

                    inputFlowReroute.InputVisible = !inputFlowReroute.InputVisible;

                }

                if (e.keyCode == KeyCode.Backspace && e.rawType == EventType.KeyUp)
                {
                    canTrigger = true;
                }
            }
        }
    }

    protected override void DrawConnection()
    {
        var outputFlowReroute = (connection.source.unit as FlowReroute);
        var inputFlowReroute = (connection.destination.unit as FlowReroute);

        if (outputFlowReroute != null && !outputFlowReroute.OutputVisible)
        {
            _color = outputFlowReroute.OutputColor;
            if (clippingPosition.Contains(mousePosition) && outputFlowReroute.showFlowOnHover)
            {
                base.DrawConnection();
            }
            return;
        }
        else if (inputFlowReroute != null && !inputFlowReroute.InputVisible)
        {
            _color = inputFlowReroute.InputColor;
            if (clippingPosition.Contains(mousePosition) && inputFlowReroute.showFlowOnHover)
            {
                base.DrawConnection();
            }
            return;
        }

        if (outputFlowReroute != null && outputFlowReroute.OutputVisible)
        {
            _color = outputFlowReroute.OutputColor;
        }
        else if (inputFlowReroute != null && inputFlowReroute.InputVisible)
        {
            _color = inputFlowReroute.InputColor;
        }

        // Draw the connection as usual
        base.DrawConnection();
    }

    public override Color color => _color;

    protected override bool colorIfActive => !BoltFlow.Configuration.animateControlConnections || !BoltFlow.Configuration.animateValueConnections;

    #region Droplets

    public override IEnumerable<IWidget> subWidgets => base.subWidgets;

    protected override bool showDroplets => BoltFlow.Configuration.animateControlConnections;

    protected override Vector2 GetDropletSize()
    {
        return BoltFlow.Icons.valuePortConnected?[12].Size() ?? 13 * Vector2.one;
    }

    protected override void DrawDroplet(Rect position)
    {
        if (BoltFlow.Icons.valuePortConnected != null)
        {
            GUI.DrawTexture(position, BoltFlow.Icons.valuePortConnected[12]);
        }
    }

    #endregion
}