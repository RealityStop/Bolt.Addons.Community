namespace Unity.VisualScripting.Community
{
    [UnitCategory("Community/Control/Delegates")]
    [TypeIcon(typeof(Flow))]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.ActionUnit")]
    public sealed class ActionNode : DelegateNode
    {
        public IAction _action => _delegate as IAction;

        public ActionNode() : base() { }
        public ActionNode(IDelegate del) : base(del)
        {

        }

        protected override void InitializeDelegate(Flow flow, bool instance = false)
        {
            if (instance)
            {
                _action.SetInstance(flow, this, () => { var _flow = Flow.New(flow.stack.ToReference()); _flow.Invoke(invoke); });
                return;
            }
            _action.Initialize(flow, this, () => { var _flow = Flow.New(flow.stack.ToReference()); _flow.Invoke(invoke); });
        }
    }
}
