namespace Unity.VisualScripting.Community
{
    [Widget(typeof(Todo))]
    public sealed class TodoWidget : UnitWidget<Todo>
    {
        public TodoWidget(FlowCanvas canvas, Todo unit) : base(canvas, unit)
        {
        }

        protected override NodeColorMix baseColor
        {
            get
            {
                return new NodeColorMix() { red = 0.6578709f, green = 1f };
            }
        }
    }
}