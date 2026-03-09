namespace Unity.VisualScripting.Community 
{
    [Descriptor(typeof(GetMachineNode))]
    public class GetMachineNodeDescriptor : UnitDescriptor<GetMachineNode>
    {
        public GetMachineNodeDescriptor(GetMachineNode target) : base(target)
        {
        }

        protected override string DefinedSubtitle()
        {
            return target.type switch
            {
                GraphSource.Embed => "With Name",
                GraphSource.Macro => "With Graph",
                _ => base.DefinedSubtitle(),
            };
        }
    } 
}