using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
[RenamedFrom("CorutineConverter")]    
    [UnitTitle("CoroutineToFlow")]//Unit title
    [UnitCategory("Community\\Control")]
    [TypeIcon(typeof(Flow))]//Unit icon
    public class CorutineConverter : Unit
    {
        
        [DoNotSerialize]
        public ControlInput In;
        [DoNotSerialize]
        public ControlOutput Converted;
        [DoNotSerialize]
        public ControlOutput Corutine;
    
        protected override void Definition()
        {
            In = ControlInput("In", Convert);
            Converted = ControlOutput("Flow");
            Corutine = ControlOutput("Coroutine");
    
            Succession(In, Converted);
            Succession(In, Corutine);
        }
    
        private ControlOutput Convert(Flow flow) 
        {
            var GraphRef = flow.stack.ToReference();
    
            if (!flow.isCoroutine)
            {
                Debug.LogWarning("CoroutineToFlow node is used to convert a Corutine flow to a normal flow there is no point in using it in a regular flow", flow.stack.gameObject);
                return Converted;
            }
            else 
            {
                var Convertedflow = Flow.New(GraphRef);
                Convertedflow.Run(Converted);
                return Corutine;
            }
        }
    }
    
}