using System;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public abstract class GraphProcess
    {
        public virtual Type graphType => typeof(IGraph);
        public abstract void Process(IGraph graph, ICanvas canvas);
        protected virtual void OnBind() { }
        protected virtual void OnUnbind() { }
        protected Event @event;

        public void Bind()
        {
            UnityEditorEvent.onCurrentEvent += SetKeyCode;
        }

        public void Unbind()
        {
            UnityEditorEvent.onCurrentEvent -= SetKeyCode;
        }

        private void SetKeyCode(Event e)
        {
            @event = e;
        }
    }

    public abstract class GraphProcess<TGraph, TCanvas> : GraphProcess where TGraph : IGraph
    {
        public sealed override Type graphType => typeof(TGraph);

        public sealed override void Process(IGraph graph, ICanvas canvas)
        {
            Process((TGraph)graph, (TCanvas)canvas);
        }

        public abstract void Process(TGraph graph, TCanvas canvas);
    }
}