using System;

namespace Unity.VisualScripting.Community {
    [Widget(typeof(OnUnityEvent))]
    public class OnUnityEventWidget : UnitWidget<OnUnityEvent> {

        protected override NodeColorMix baseColor { get; } = NodeColor.Green;

        private Type _currentType;
        
        public OnUnityEventWidget(FlowCanvas canvas, OnUnityEvent unit) : base(canvas, unit) {
        }

        public override void Update() {
            var type = item.UnityEvent?.connection?.source?.type;

            if (type != _currentType) {
                item.UpdatePorts();
                _currentType = type;
            }
        }
        
    }
}