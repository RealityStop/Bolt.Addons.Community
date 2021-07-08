using Unity.VisualScripting;
using System;
using UnityEngine;

namespace Bolt.Addons.Community.Processing
{
    public abstract class GraphProcess
    {
        public virtual Type graphType => typeof(IGraph);
        public abstract void Process(IGraph graph, ICanvas canvas);
        protected virtual void OnBind(IGraph graph, ICanvas canvas) { }
        protected virtual void OnUnbind(IGraph graph, ICanvas canvas) { }
        protected Event @event;

        public void Bind(IGraph graph, ICanvas canvas)
        {
            UnityEditorEvent.onCurrentEvent += SetKeyCode;
        }

        public void Unbind(IGraph graph, ICanvas canvas)
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

        protected sealed override void OnBind(IGraph graph, ICanvas canvas)
        {
            OnBind((TGraph)graph, (TCanvas)canvas);
        }

        protected sealed override void OnUnbind(IGraph graph, ICanvas canvas)
        {
            OnUnbind((TGraph)graph, (TCanvas)canvas);
        }

        public abstract void Process(TGraph graph, TCanvas canvas);
        public virtual void OnBind(TGraph graph, TCanvas canvas) { }
        public virtual void OnUnbind(TGraph graph, TCanvas canvas) { }
    }
}