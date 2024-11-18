using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
[RenamedFrom("FlowToCoroutine")]    
    [UnitTitle("FlowToCoroutine")]//Unit title
    [UnitCategory("Community\\Control")]
    [TypeIcon(typeof(Coroutine))]//Unit icon
    public class FlowToCoroutine : Unit
    {
        
        [DoNotSerialize]
        public ControlInput In;
        [DoNotSerialize]
        public ControlOutput Converted;
        [DoNotSerialize]
        public ControlOutput _flow;
    
        protected override void Definition()
        {
            In = ControlInput("In", Convert);
            Converted = ControlOutput("Coroutine");
            _flow = ControlOutput("Flow");
    
            Succession(In, Converted);
            Succession(In, _flow);
        }
    
        private ControlOutput Convert(Flow flow) 
        {
            var GraphRef = flow.stack.ToReference();
    
            if (flow.isCoroutine)
            {
                Debug.LogWarning("FlowToCoroutine node is used to convert a normal flow to a Coroutine flow there is no point in using it in a Coroutine flow", flow.stack.gameObject);
                return Converted;
            }
            else 
            {
                var Convertedflow = Flow.New(GraphRef);
                foreach (var variable in flow.variables)
                {
                    Convertedflow.variables.Set(variable.name, variable.value);
                }
                Convertedflow.StartCoroutine(Converted);
                return _flow;
            }
        }
    }
}