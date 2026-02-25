namespace Unity.VisualScripting.Community 
{
    [Descriptor(typeof(ElseIfUnit))]
    public class ElseIfUnitDescriptor : UnitDescriptor<ElseIfUnit>
    {
        public ElseIfUnitDescriptor(ElseIfUnit target) : base(target)
        {
        }
    
        protected override void DefinedPort(IUnitPort port, UnitPortDescription description)
        {
            switch (port.key)
            {
                case "elseif0" : description.showLabel = false;
                break; 
                case "elseif1" : description.showLabel = false;
                break; 
                case "elseif2" : description.showLabel = false;
                break; 
                case "elseif3" : description.showLabel = false;
                break; 
                case "elseif4" : description.showLabel = false;
                break; 
                case "elseif5" : description.showLabel = false;
                break; 
                case "elseif6" : description.showLabel = false;
                break; 
                case "elseif7" : description.showLabel = false;
                break; 
                case "elseif8" : description.showLabel = false;
                break; 
                case "elseif9" : description.showLabel = false;
                break; 
                case "0" : description.label = "Condition0";
                break;
                case "1" : description.label = "Condition1";
                break;
                case "2" : description.label = "Condition2";
                break;
                case "3" : description.label = "Condition3";
                break;
                case "4" : description.label = "Condition4";
                break;
                case "5" : description.label = "Condition5";
                break;
                case "6" : description.label = "Condition6";
                break;
                case "7" : description.label = "Condition7";
                break;
                case "8" : description.label = "Condition8";
                break;
                case "9" : description.label = "Condition9";
                break; 
            }
            base.DefinedPort(port, description);
        }
    
        protected override string DefinedSummary()
        {
            return "Else if is used to check multiple conditions in sequence and execute code based on the first condition that is true. If none are true, it goes to a default option (the else)";
        }
    } 
}