namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.UnbindDelegateUnit")]
    public abstract class UnbindDelegateNode<TDelegateInterface> : Unit where TDelegateInterface : IDelegate
    {
        [Serialize]
        public TDelegateInterface _delegate;
        [DoNotSerialize]
        public ControlInput enter;
        [DoNotSerialize]
        public ControlOutput exit;
        [DoNotSerialize]
        public ValueInput a;
        [DoNotSerialize]
        public ValueInput b;

        public UnbindDelegateNode() : base() { }

        public UnbindDelegateNode(TDelegateInterface @delegate)
        {
            this._delegate = @delegate;
        }

        protected override void Definition()
        {
            enter = ControlInput("enter", (flow) =>
            {
                if (_delegate is IAction)
                {
                    (flow.GetValue<IAction>(a)).Unbind(flow.GetValue<TDelegateInterface>(b));
                }
                else
                {
                    if (_delegate is IFunc)
                    {
                        ((IFunc)_delegate).Unbind(flow.GetValue<TDelegateInterface>(b));
                    }
                }

                return exit;
            });

            exit = ControlOutput("exit");

            if (_delegate != null)
            {
                a = ValueInput(_delegate.GetType(), "a");
                b = ValueInput(_delegate.GetType(), "b");
            }
            else
            {
                a = ValueInput(typeof(object), "a");
                b = ValueInput(typeof(object), "b");
            }

            Requirement(a, enter);
            Requirement(b, enter);
            Succession(enter, exit);
        }
    }
}
