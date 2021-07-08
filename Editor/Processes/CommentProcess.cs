using Unity.VisualScripting;
using UnityEngine;
using Bolt.Addons.Libraries.Humility;
using UnityEditor;

namespace Bolt.Addons.Community.Processing
{
    public sealed class CommentProcess : GraphProcess<FlowGraph, FlowCanvas>
    {
        private int ticks;
        private bool firstSlash;

        public override void OnBind(FlowGraph graph, FlowCanvas canvas)
        {
        }

        public override void OnUnbind(FlowGraph graph, FlowCanvas canvas)
        {
        }

        public override void Process(FlowGraph graph, FlowCanvas canvas)
        {
            if (CommentPopup.active == null)
            {
                if (!firstSlash)
                {
                    if (@event != null)
                    {
                        if (@event.keyCode == KeyCode.Slash && @event.rawType == EventType.KeyUp)
                        {
                            firstSlash = true;
                        }
                    }
                }
                else
                {
                    if (@event.keyCode == KeyCode.Slash && @event.rawType == EventType.KeyUp && @event.control)
                    {
                        CommentPopup.Open();
                        firstSlash = false;
                    }
                }

                HUMFlow.AfterTicks(ref ticks, 800, true, () => { firstSlash = false; });
            }
        }
    }
}