namespace Unity.VisualScripting.Community
{
    [UnitCategory("Community/Control/Delegates")]
    [TypeIcon(typeof(Flow))]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.FuncUnit")]
    public sealed class FuncNode : DelegateNode
    {
        public IFunc _func => _delegate as IFunc;

        [DoNotSerialize]
        public ValueInput @return;

        public FuncNode() : base() { }
        public FuncNode(IDelegate del) : base(del)
        {

        }

        protected override void InitializeDelegate(Flow flow, bool instance = false)
        {
            _func.Initialize(flow, this, () => { var _flow = Flow.New(flow.stack.AsReference()); _flow.Invoke(invoke); return _flow.GetValue(@return); });
        }

        protected override void Definition()
        {
            base.Definition();

            if (_func != null)
            {
                @return = ValueInput(_func.ReturnType, "return");
            }
        }
    }
}
