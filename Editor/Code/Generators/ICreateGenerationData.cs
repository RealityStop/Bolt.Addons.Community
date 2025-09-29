namespace Unity.VisualScripting.Community 
{
    public interface ICreateGenerationData
    {
        ControlGenerationData data { get; }
        public ControlGenerationData GetGenerationData();
        public ControlGenerationData CreateGenerationData();
    } 
}
