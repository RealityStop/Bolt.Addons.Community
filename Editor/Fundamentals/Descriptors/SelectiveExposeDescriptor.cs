namespace Unity.VisualScripting.Community 
{
    [Descriptor(typeof(SelectiveExpose))]
    public class SelectiveExposeDescriptor : UnitDescriptor<SelectiveExpose>
    {
        public SelectiveExposeDescriptor(SelectiveExpose target) : base(target)
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