using UnityEngine;

namespace Unity.VisualScripting.Community
{
     [SpecialUnit]
     [UnitCategory("Events/Community/Variables")]
     [RenamedFrom("Bolt.Addons.Community.Fundamentals.OnVariableChanged")]
     public sealed class OnVariableChanged : MachineEventUnit<EmptyEventArgs>, IUnifiedVariableUnit
     {
        public new class Data : EventUnit<EmptyEventArgs>.Data
        {
            public object _previousValue;
            public bool firstExecution;
        }

        public override IGraphElementData CreateData()
        {
            return new Data();
        }

        protected override string hookName => EventHooks.Update;


        /// <summary>
        /// The value of the variable.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput value { get; private set; }

        /// <summary>
        /// Whether a fallback value should be provided if the 
        /// variable is not defined.
        /// </summary>
        [Serialize]
        [Inspectable]
        [InspectorLabel("Initial State")]
        public bool provideInitial { get; set; } = false;

        /// <summary>
        /// The value to return if the variable is not defined.
        /// </summary>
        [DoNotSerialize]
        public ValueInput Initial { get; private set; }


        /// <summary>
		/// The kind of variable.
		/// </summary>
		[Serialize, Inspectable, UnitHeaderInspectable]
        public VariableKind kind { get; set; }

        /// <summary>
        /// The name of the variable.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput name { get; private set; }

        /// <summary>
        /// The source of the variable.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        [NullMeansSelf]
        public ValueInput @object { get; private set; }


        protected override void Definition()
        {
            base.Definition();

            name = ValueInput(nameof(name), string.Empty);
            value = ValueOutput<object>(nameof(value));

            if (provideInitial)
            {
                Initial = ValueInput<object>(nameof(Initial)).AllowsNull();
                Requirement(Initial, value);
            }


            if (kind == VariableKind.Object)
            {
                @object = ValueInput<GameObject>(nameof(@object), null).NullMeansSelf();
            }

            if (kind == VariableKind.Object)
            {
                Requirement(@object, value);
            }
        }

        public override void StartListening(GraphStack stack)
        {
            base.StartListening(stack);
            var data = stack.GetElementData<Data>(this);
            data._previousValue = null;
            data.firstExecution = true;
        }


        protected override bool ShouldTrigger(Flow flow, EmptyEventArgs args)
        {
            var data = flow.stack.GetElementData<Data>(this);

            if (!data.isListening)
                return false;

            var name = flow.GetValue<string>(this.name);

            VariableDeclarations variables;

            switch (kind)
            {
                case VariableKind.Flow:
                    variables = flow.variables;
                    break;
                case VariableKind.Graph:
                    variables = Variables.Graph(flow.stack);
                    break;
                case VariableKind.Object:
                    variables = Variables.Object(flow.GetValue<GameObject>(@object));
                    break;
                case VariableKind.Scene:
                    variables = Variables.Scene(flow.stack.scene);
                    break;
                case VariableKind.Application:
                    variables = Variables.Application;
                    break;
                case VariableKind.Saved:
                    variables = Variables.Saved;
                    break;
                default:
                    throw new UnexpectedEnumValueException<VariableKind>(kind);
            }
            var val = variables.Get(name);

            if (data.firstExecution)
                if (provideInitial)
                    data._previousValue = flow.GetValue<object>(Initial);
                else
                    data._previousValue = val;


            
            bool equal = OperatorUtility.Equal(data._previousValue, val);
            if (!equal)
            {
                data._previousValue = val;
                flow.SetValue(value, val);
            }

            data.firstExecution = false;

            return !equal;
        }
     }
}