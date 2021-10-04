using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    public sealed class RerouteProcess : GraphProcess<FlowGraph, FlowCanvas>
    {
        public override void Process(FlowGraph graph, FlowCanvas canvas)
        {
            if (FuzzyWindow.instance == null && canvas.isCreatingConnection)
            {
                var canSpawn = @event != null && @event.keyCode == KeyCode.Space && @event.rawType == EventType.KeyUp;
                if (!canSpawn) canSpawn = @event != null && @event.keyCode == KeyCode.Space && @event.rawType == EventType.KeyUp;
                if (canSpawn)
                {
                    var connnectionType = canvas.connectionSource.GetType();
                    if (connnectionType == typeof(ControlOutput))
                    {
                        var reroute = new FlowReroute();
                        canvas.AddUnit(reroute, canvas.connectionEnd.Add(new Vector2(8, 0)));
                        canvas.connectionSource.ValidlyConnectTo(reroute.input);
                        canvas.connectionSource = reroute.output;
                        return;
                    }

                    if (connnectionType == typeof(ControlInput))
                    {
                        var reroute = new FlowReroute();
                        canvas.AddUnit(reroute, canvas.connectionEnd.Add(new Vector2(-8, 0)));
                        canvas.connectionSource.ValidlyConnectTo(reroute.output);
                        canvas.connectionSource = reroute.input;
                        return;
                    }

                    if (connnectionType == typeof(ValueOutput))
                    {
                        var reroute = new ValueReroute();
                        canvas.AddUnit(reroute, canvas.connectionEnd.Add(new Vector2(8, 0)));
                        canvas.connectionSource.ValidlyConnectTo(reroute.input);
                        canvas.connectionSource = reroute.output;
                        return;
                    }

                    if (connnectionType == typeof(ValueInput))
                    {
                        var reroute = new ValueReroute();
                        canvas.AddUnit(reroute, canvas.connectionEnd.Add(new Vector2(-8, 0)));
                        canvas.connectionSource.ValidlyConnectTo(reroute.output);
                        canvas.connectionSource = reroute.input;
                        return;
                    }
                }
            } 
        }
    }
}