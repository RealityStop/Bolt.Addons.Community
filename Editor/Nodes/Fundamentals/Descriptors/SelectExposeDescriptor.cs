namespace Unity.VisualScripting.Community 
{
    [Descriptor(typeof(SelectExpose))]
    public class SelectExposeDescriptor : UnitDescriptor<SelectExpose>
    {
        public SelectExposeDescriptor(SelectExpose target) : base(target)
        {
        }
        protected override string DefinedSummary()
        {
            return "Custom node that exposes only the user-selected members of a type.";
        }

        protected override EditorTexture DefinedIcon()
        {
            return unit.type.Icon();
        }
    } 
}