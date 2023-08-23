using UnityEngine;
using Unity.VisualScripting;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.Community;

[Widget(typeof(ValueConnection))]
public sealed class ValueConnectionWidget : UnitConnectionWidget<ValueConnection>
{
    private bool canTrigger = true;
    private Color _color = Color.white;

    public ValueConnectionWidget(FlowCanvas canvas, ValueConnection connection) : base(canvas, connection)
    {
    }

    public override void HandleInput()
    {
        base.HandleInput();

        var outputFlowReroute = (connection.source.unit as ValueReroute);
        var inputFlowReroute = (connection.destination.unit as ValueReroute);

        if (selection.Contains(connection.destination.unit) && outputFlowReroute != null)
        {
            if (selection.Contains(outputFlowReroute))
            {
                if (e.keyCode == KeyCode.Backspace && e.rawType == EventType.KeyDown && canTrigger)
                {
                    canTrigger = false;

                    outputFlowReroute.outputVisible = !outputFlowReroute.outputVisible;

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

                    inputFlowReroute.inputVisible = !inputFlowReroute.inputVisible;

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
        var outputFlowReroute = (connection.source.unit as ValueReroute);
        var inputFlowReroute = (connection.destination.unit as ValueReroute);

        if (outputFlowReroute != null && !outputFlowReroute.outputVisible)
        {
            if (clippingPosition.Contains(mousePosition) && outputFlowReroute.showFlowOnHover)
            {
                base.DrawConnection();
            }
            return;
        }
        else if (inputFlowReroute != null && !inputFlowReroute.inputVisible)
        {
            if (clippingPosition.Contains(mousePosition) && inputFlowReroute.showFlowOnHover)
            {
                base.DrawConnection();
            }
            return;
        }

        base.DrawConnection();
    }

    protected override bool colorIfActive => !BoltFlow.Configuration.animateControlConnections || !BoltFlow.Configuration.animateValueConnections;

    #region Droplets

    public override IEnumerable<IWidget> subWidgets => base.subWidgets;

    protected override bool showDroplets => BoltFlow.Configuration.animateControlConnections;

    public override Color color => Unity.VisualScripting.ValueConnectionWidget.DetermineColor(connection.source.type, connection.destination.type);

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