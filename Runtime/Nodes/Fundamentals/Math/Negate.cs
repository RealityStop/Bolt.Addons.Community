using System.Linq;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("NegativeValueNode")]
    [RenamedFrom("Unity.VisualScripting.Community.NegativeValueNode")]
    [UnitTitle("Negate")]
    [UnitCategory("Community\\Math")]
    [TypeIcon(typeof(Negate))]
    public class NegateValueNode : Unit
    {
        public NegateType type;
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput Float;
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput Int;
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput Vector2;
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput Vector3;
        [DoNotSerialize]
        [PortLabel("X")]
        public ValueInput Object;

        [DoNotSerialize]
        [PortLabel("-X")]
        public ValueOutput output;

        protected override void Definition()
        {
            Object = ValueInput<object>(nameof(Object));
            output = ValueOutput(nameof(output), GetNegativeValue).PredictableIf(f =>
            {
                var connection = Object.connection;

                return connection != null && Flow.CanPredict(connection.source, f.stack.AsReference());
            });

            UnityThread.EditorAsync(PostDeserialize);
        }

        private object GetNegativeValue(Flow flow)
        {
            object value = flow.GetValue(Object);
            return OperatorUtility.Negate(value);
        }

        private void PostDeserialize()
        {
            foreach (var invalidInput in inputs)
            {
                switch (invalidInput.key)
                {
                    case nameof(Float):
                    case nameof(Int):
                    case nameof(Vector2):
                    case nameof(Vector3):

                        var connection = invalidInput.connections.FirstOrDefault();
                        if (connection == null) break;

                        Object.ValidlyConnectTo(connection.source);

                        connection.destination.Disconnect();
                        break;
                }
            }
        }
    }

    public enum NegateType
    {
        Float,
        Int,
        Vector2,
        Vector3,
    }

}