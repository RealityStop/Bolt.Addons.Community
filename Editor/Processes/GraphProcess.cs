using Unity.VisualScripting;
using System;

namespace Bolt.Addons.Community.Processing
{
    public abstract class GraphProcess
    {
        public virtual Type graphType => typeof(IGraph);
        public abstract void Process(IGraph graph, ICanvas canvas);
        public abstract void OnBind(IGraph graph, ICanvas canvas);
        public abstract void OnUnbind(IGraph graph, ICanvas canvas);
    }

    public abstract class GraphProcess<TGraph, TCanvas> : GraphProcess where TGraph : IGraph
    {
        public sealed override Type graphType => typeof(TGraph);

        public sealed override void Process(IGraph graph, ICanvas canvas)
        {
            Process((TGraph)graph, (TCanvas)canvas);
        }

        public sealed override void OnBind(IGraph graph, ICanvas canvas)
        {
            OnBind((TGraph)graph, (TCanvas)canvas);
        }

        public sealed override void OnUnbind(IGraph graph, ICanvas canvas)
        {
            OnUnbind((TGraph)graph, (TCanvas)canvas);
        }

        public abstract void Process(TGraph graph, TCanvas canvas);
        public abstract void OnBind(TGraph graph, TCanvas canvas);
        public abstract void OnUnbind(TGraph graph, TCanvas canvas);
    }
}