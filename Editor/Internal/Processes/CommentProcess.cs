using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;
using System.Linq;

namespace Unity.VisualScripting.Community
{
    public sealed class CommentProcess : GraphProcess<FlowGraph, FlowCanvas>
    {
        private int ticks;
        private bool firstSlash;

        protected override void OnBind()
        {
        }

        protected override void OnUnbind()
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
                        if (@event.keyCode == KeyCode.Slash && @event.rawType == EventType.KeyUp && @event.CtrlOrCmd())
                        {
                            firstSlash = true;
                        }
                    }
                }
                else
                {
                    if (@event.keyCode == KeyCode.Slash && @event.rawType == EventType.KeyUp && @event.CtrlOrCmd())
                    {
                        CommentPopup.Open((node) =>
                        {
                            node.connectedElements.AddRange(canvas.selection.Where(e => e != node));
                        });
                        firstSlash = false;
                    }
                }

                HUMFlow.AfterTicks(ref ticks, 800, true, () => { firstSlash = false; });
            }
        }
    }
}