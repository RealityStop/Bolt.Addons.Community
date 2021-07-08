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
            if (canvas.isCreatingConnection && canvas.connectionSource.GetType() == typeof(ControlOutput) && canvas.connectionSource.unit.GetType() != typeof(FlowReroute))
            {
                var canSpawn = @event != null && @event.keyCode == KeyCode.Space && @event.rawType == EventType.KeyUp;
                if (!canSpawn) canSpawn = @event != null && @event.keyCode == KeyCode.Space && @event.rawType == EventType.KeyUp;
                if (canSpawn && FlowRerouteWidget.rerouteHotFixed)
                {
                    var reroute = new FlowReroute();
                    FlowRerouteWidget.addedUnit = reroute;
                    canvas.AddUnit(reroute, canvas.connectionEnd.Add(new Vector2(8, 0)));
                    canvas.connectionSource.ValidlyConnectTo(reroute.input);
                    canvas.connectionSource = reroute.output;
                }
            } 
        }

        private void SetKeyCode(Event e)
        {
            @event = e;
        }
    }
}