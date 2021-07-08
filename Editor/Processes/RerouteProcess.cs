using Unity.VisualScripting;
using UnityEngine;
using Bolt.Addons.Community.Fundamentals.Units.Utility;
using Bolt.Addons.Libraries.Humility;
using Bolt.Addons.Community.Utility.Editor;

namespace Bolt.Addons.Community.Processing
{
    public sealed class RerouteProcess : GraphProcess<FlowGraph, FlowCanvas>
    {
        private int ticks;
        private bool canAdd = true;
        private Event @event;

        public override void OnBind(FlowGraph graph, FlowCanvas canvas)
        {
            UnityEditorEvent.onCurrentEvent += SetKeyCode;
        }

        public override void OnUnbind(FlowGraph graph, FlowCanvas canvas)
        {
            UnityEditorEvent.onCurrentEvent -= SetKeyCode;
        }

        public override void Process(FlowGraph graph, FlowCanvas canvas)
        {
            if (canvas.isCreatingConnection)
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

        private void SetKeyCode(Event e)
        {
            @event = e;
        }
    }
}